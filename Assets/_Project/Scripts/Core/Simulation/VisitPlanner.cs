using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Oyun başında bütün günlerin ziyaret programını kurar (Ahmet kararı 17 Ekim ~23:30, memory.md "📅").
    ///  - Gün başına proje sayısı DayConfig.ProjectsPerDay (7 gün: 3-3-4-4-4-5-5).
    ///  - Ana müteahhitler (IsGeneric değil) BalanceSO.MainClientVisits kez, farklı günlerde; kabadayı sadece AllowThugs günlerinde.
    ///  - Kalan yerler kişiliksiz karakterler (her biri bir kez). Yetmezse ana müteahhitler doldurur (yedek plan).
    ///  - Projelerin tam CompliantShare'i uygun (28'de 14 / 14); 1. gün en çok 1 uygunsuz. Uygunsuz olma ihtimali role göre
    ///    ağırlıklı ama kesin değil: yüz kararı ele vermez.
    ///  - Uygunsuz türü gün kurallarına uyar: zarf AllowBribes, sahte belge AllowFakeDocuments günlerinde.
    ///  - Aynı müteahhit aynı gün iki kez gelmez; dünün sonuncusu bugünün ilki olmaz.
    ///  - Jüri vitrini (BalanceSO.ShowcaseEarlyDays, Ahmet kararı 18 Ekim ~03:30): 1. gün bir dürüst usta uygun projeyle (onaylanırsa
    ///    hediye kesin), 2. gün bir rüşvetçi zarflı projeyle ve kabadayı uygunsuz dosyayla gelir. Bu ziyaretler 3'er ziyaretin
    ///    içindedir; kabadayının diğer ziyaretleri yine sadece AllowThugs günlerinde. 14/14 ve 1. gün kuralı aynen kalır.
    /// Saf fonksiyon; aynı seed → aynı program.
    /// </summary>
    public static class VisitPlanner
    {
        const float BribeAlsoFakeChance = 0.25f;

        public static List<ScheduledDay> Plan(int totalDays, Func<int, DayConfigSO> configFor, IReadOnlyList<ClientSO> clients,
            BalanceSO balance, Random rng)
        {
            var plan = new List<ScheduledDay>();
            if (totalDays <= 0 || configFor == null || clients == null) return plan;

            var mains = new List<ClientSO>();
            var generics = new List<ClientSO>();
            foreach (var c in clients)
                if (c != null) (c.IsGeneric ? generics : mains).Add(c);

            var counts = new int[totalDays + 1];
            for (int d = 1; d <= totalDays; d++)
            {
                var cfg = configFor(d);
                counts[d] = cfg != null ? Math.Max(0, cfg.ProjectsPerDay) : 0;
            }

            var show = Showcase.Pick(balance, totalDays, counts, mains, rng);
            var days = AssignClients(totalDays, counts, configFor, mains, generics, Math.Max(0, balance.MainClientVisits), show, rng);
            AssignKinds(totalDays, days, configFor, balance, show, rng);

            for (int d = 1; d <= totalDays; d++)
            {
                var day = new ScheduledDay { Day = d };
                foreach (var (client, kind) in days[d])
                    day.Slots.Add(new ScheduleSlot { ClientId = client.Id, Kind = kind, Gift = show.IsGift(d, client) });
                plan.Add(day);
            }
            return plan;
        }

        // ---------- Jüri vitrini ----------

        /// <summary>Vitrin ziyaretleri: kim, hangi gün, uygun mu uygunsuz mu. Kapalıysa boş.</summary>
        sealed class Showcase
        {
            public readonly Dictionary<ClientSO, int> FixedDay = new Dictionary<ClientSO, int>();
            public readonly HashSet<(int day, ClientSO client)> ForcedBad = new HashSet<(int, ClientSO)>();
            public (int day, ClientSO client)? GiftSlot;

            public bool IsGift(int day, ClientSO c) => GiftSlot.HasValue && GiftSlot.Value.day == day && GiftSlot.Value.client == c;
            public bool IsForcedGood(int day, ClientSO c) => IsGift(day, c);

            public static Showcase Pick(BalanceSO balance, int totalDays, int[] counts, List<ClientSO> mains, Random rng)
            {
                var show = new Showcase();
                if (!balance.ShowcaseEarlyDays || totalDays < 2 || counts[1] < 1 || counts[2] < 2) return show;
                var honest = mains.FindAll(c => c.IsHonest && c.Gift != null && !string.IsNullOrEmpty(c.Gift.Id));
                var bribers = mains.FindAll(c => c.OffersBribes && !c.IsThug && !c.IsHonest);
                var thug = mains.Find(c => c.IsThug);
                if (honest.Count > 0)
                {
                    var h = honest[rng.Next(honest.Count)];
                    show.FixedDay[h] = 1;
                    show.GiftSlot = (1, h);
                }
                if (bribers.Count > 0)
                {
                    var b = bribers[rng.Next(bribers.Count)];
                    show.FixedDay[b] = 2;
                    show.ForcedBad.Add((2, b));
                }
                if (thug != null)
                {
                    show.FixedDay[thug] = 2;
                    show.ForcedBad.Add((2, thug));
                }
                return show;
            }
        }

        // ---------- Kim hangi gün ----------

        static List<(ClientSO client, ProjectKind kind)>[] AssignClients(int totalDays, int[] counts, Func<int, DayConfigSO> configFor,
            List<ClientSO> mains, List<ClientSO> generics, int visits, Showcase show, Random rng)
        {
            List<ClientSO>[] best = null;
            for (int attempt = 0; attempt < 200 && best == null; attempt++)
                best = TryPlaceMains(totalDays, counts, configFor, mains, visits, show, rng);
            if (best == null) best = TryPlaceMains(totalDays, counts, configFor, mains, 0, show, rng); // imkânsızsa ana ziyaret yok, aşağıda doldurulur

            // Kalan yerler: kişiliksizler (her biri bir kez, karışık sırayla), yetmezse o gün olmayan ana müteahhitler
            var pool = new List<ClientSO>(generics);
            rng.Shuffle(pool);
            int next = 0;
            var visitCount = new Dictionary<ClientSO, int>();
            foreach (var m in mains) visitCount[m] = 0;
            for (int d = 1; d <= totalDays; d++) foreach (var c in best[d]) if (visitCount.ContainsKey(c)) visitCount[c]++;

            for (int d = 1; d <= totalDays; d++)
            {
                while (best[d].Count < counts[d])
                {
                    ClientSO pick = null;
                    if (next < pool.Count) pick = pool[next++];
                    else
                    {
                        // Yedek plan: o gün gelmemiş, en az gelen ana müteahhit (kabadayı sadece izinli günde)
                        foreach (var m in Shuffled(mains, rng))
                            if (!best[d].Contains(m) && Allowed(m, d, configFor) && (pick == null || visitCount[m] < visitCount[pick])) pick = m;
                        if (pick == null && mains.Count > 0) pick = mains[rng.Next(mains.Count)];
                        if (pick != null && visitCount.ContainsKey(pick)) visitCount[pick]++;
                    }
                    if (pick == null) break;
                    best[d].Add(pick);
                }
            }

            // Geliş sırası: karışık; dünün sonuncusu bugünün ilki olmasın
            var result = new List<(ClientSO, ProjectKind)>[totalDays + 1];
            string last = null;
            for (int d = 1; d <= totalDays; d++)
            {
                var order = Shuffled(best[d], rng);
                if (order.Count > 1 && order[0].Id == last) { var t = order[0]; order[0] = order[order.Count - 1]; order[order.Count - 1] = t; }
                result[d] = new List<(ClientSO, ProjectKind)>();
                foreach (var c in order) result[d].Add((c, ProjectKind.Compliant));
                if (order.Count > 0) last = order[order.Count - 1].Id;
            }
            return result;
        }

        /// <summary>Ana müteahhitleri farklı günlere yerleştirir (en kısıtlı önce: kabadayı). Sığmazsa null.</summary>
        static List<ClientSO>[] TryPlaceMains(int totalDays, int[] counts, Func<int, DayConfigSO> configFor, List<ClientSO> mains,
            int visits, Showcase show, Random rng)
        {
            var days = new List<ClientSO>[totalDays + 1];
            for (int d = 0; d <= totalDays; d++) days[d] = new List<ClientSO>();

            // Jüri vitrini: sabit günler önce (bu ziyaretler 3'er ziyaretin içinde sayılır)
            foreach (var kv in show.FixedDay) if (visits > 0) days[kv.Value].Add(kv.Key);

            var order = Shuffled(mains, rng);
            order.Sort((a, b) => AllowedDays(a, totalDays, configFor).CompareTo(AllowedDays(b, totalDays, configFor)));
            foreach (var m in order)
            {
                bool fixedVisit = visits > 0 && show.FixedDay.ContainsKey(m);
                int remaining = fixedVisit ? visits - 1 : visits;
                var candidates = new List<int>();
                for (int d = 1; d <= totalDays; d++)
                    if (Allowed(m, d, configFor) && days[d].Count < counts[d] && !days[d].Contains(m)) candidates.Add(d);
                if (candidates.Count < remaining) return null;
                // Boş yeri çok olan güne biraz daha meyilli rastgele seçim
                for (int v = 0; v < remaining; v++)
                {
                    int total = 0;
                    foreach (int d in candidates) total += counts[d] - days[d].Count;
                    int roll = rng.Next(Math.Max(1, total)), pickIndex = 0;
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        roll -= counts[candidates[i]] - days[candidates[i]].Count;
                        if (roll < 0) { pickIndex = i; break; }
                    }
                    days[candidates[pickIndex]].Add(m);
                    candidates.RemoveAt(pickIndex);
                }
            }
            return days;
        }

        // Kabadayının sabit olmayan ziyaretleri sadece AllowThugs günlerinde (vitrin günü FixedDay ile ayrıca eklenir).
        static bool Allowed(ClientSO c, int day, Func<int, DayConfigSO> configFor)
        {
            if (!c.IsThug) return true;
            var cfg = configFor(day);
            return cfg != null && cfg.AllowThugs;
        }

        static int AllowedDays(ClientSO c, int totalDays, Func<int, DayConfigSO> configFor)
        {
            int n = 0;
            for (int d = 1; d <= totalDays; d++) if (Allowed(c, d, configFor)) n++;
            return n;
        }

        // ---------- Hangi proje uygun, hangisi değil ----------

        static void AssignKinds(int totalDays, List<(ClientSO client, ProjectKind kind)>[] days, Func<int, DayConfigSO> configFor,
            BalanceSO balance, Showcase show, Random rng)
        {
            var slots = new List<(int day, int index, double key)>();
            int total = 0;
            for (int d = 1; d <= totalDays; d++) total += days[d].Count;
            int bad = total - (int)Math.Round(total * balance.CompliantShare);

            // Ağırlıklı rastgele seçim (Efraimidis–Spirakis): anahtar = u^(1/ağırlık), büyük anahtarlar uygunsuz olur
            for (int d = 1; d <= totalDays; d++)
                for (int i = 0; i < days[d].Count; i++)
                {
                    double u = Math.Max(1e-9, rng.NextDouble());
                    slots.Add((d, i, Math.Pow(u, 1.0 / BadWeight(days[d][i].client))));
                }
            slots.Sort((a, b) => b.key.CompareTo(a.key));

            // 1. gün öğrenme günü: en çok 1 uygunsuz
            var chosen = new HashSet<(int, int)>();
            int dayOneBad = 0;
            // Jüri vitrini: rüşvetçi ve kabadayının 2. gün ziyareti uygunsuz (toplam 14'ün içinde), hediye getiren usta uygun
            for (int d = 1; d <= totalDays; d++)
                for (int i = 0; i < days[d].Count; i++)
                    if (show.ForcedBad.Contains((d, days[d][i].client)) && chosen.Count < bad)
                    {
                        chosen.Add((d, i));
                        if (d == 1) dayOneBad++;
                    }
            foreach (var s in slots)
            {
                if (chosen.Count >= bad) break;
                if (chosen.Contains((s.day, s.index)) || show.IsForcedGood(s.day, days[s.day][s.index].client)) continue;
                if (s.day == 1)
                {
                    if (dayOneBad >= 1) continue;
                    dayOneBad++;
                }
                chosen.Add((s.day, s.index));
            }

            for (int d = 1; d <= totalDays; d++)
            {
                var cfg = configFor(d);
                for (int i = 0; i < days[d].Count; i++)
                {
                    var c = days[d][i].client;
                    days[d][i] = (c, chosen.Contains((d, i)) ? BadKind(c, cfg, rng) : ProjectKind.Compliant);
                }
            }
        }

        /// <summary>Uygunsuz gelme ağırlığı: rol eğilim verir ama kesinlik vermez (Ç3).</summary>
        static double BadWeight(ClientSO c)
        {
            if (c.IsGeneric) return 0.5;
            if (c.IsHonest) return 0.2;
            return 0.8; // rüşvetçi, sahteci, kabadayı
        }

        static ProjectKind BadKind(ClientSO c, DayConfigSO cfg, Random rng)
        {
            bool bribes = cfg != null && cfg.AllowBribes;
            bool fakes = cfg != null && cfg.AllowFakeDocuments;
            ProjectKind Bribe() => fakes && rng.Chance(BribeAlsoFakeChance) ? ProjectKind.BribeFake : ProjectKind.Bribe;

            if (c.IsGeneric)
            {
                var options = new List<ProjectKind> { ProjectKind.Sloppy };
                if (bribes) options.Add(ProjectKind.Bribe);
                if (fakes) options.Add(ProjectKind.Fake);
                var k = options[rng.Next(options.Count)];
                return k == ProjectKind.Bribe ? Bribe() : k;
            }
            if (c.IsHonest) return ProjectKind.Sloppy;                       // dürüst usta asla zarf / sahte belge getirmez
            if (c.OffersBribes) return bribes ? Bribe() : ProjectKind.Sloppy;  // rüşvetçi, kabadayı
            return fakes ? ProjectKind.Fake : ProjectKind.Sloppy;            // sahteci (Selim)
        }

        static List<T> Shuffled<T>(List<T> list, Random rng)
        {
            var copy = new List<T>(list);
            rng.Shuffle(copy);
            return copy;
        }
    }
}
