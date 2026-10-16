using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Yıkılan bina için kurgusal kayıp verisi üretir. İsimler NamePoolSO'dan rastgele birleştirilir.</summary>
    public static class CasualtyGenerator
    {
        public static void Fill(BuildingOutcome outcome, int floors, BalanceSO balance, NamePoolSO names, Random rng)
        {
            int residents = 0;
            for (int i = 0; i < Math.Max(1, floors); i++)
                residents += rng.RangeInclusive(balance.ResidentsPerFloor.x, balance.ResidentsPerFloor.y);

            int deaths = Math.Max(1, (int)Math.Round(residents * rng.Range(balance.DeathRatio.x, balance.DeathRatio.y)));
            int injured = (int)Math.Round(residents * rng.Range(balance.InjuryRatio.x, balance.InjuryRatio.y));
            injured = Math.Min(injured, residents - deaths);

            outcome.Residents = residents;
            outcome.Deaths = deaths;
            outcome.Injured = injured;
            outcome.VictimNames = MakeNames(deaths, names, rng);
        }

        static List<string> MakeNames(int count, NamePoolSO names, Random rng)
        {
            var list = new List<string>();
            var used = new HashSet<string>();
            int guard = count * 10;
            while (list.Count < count && guard-- > 0)
            {
                string first = rng.Pick(names != null ? names.FirstNames : null, "Ayşe");
                string last = rng.Pick(names != null ? names.LastNames : null, "Kaya");
                string full = $"{first} {last}";
                if (used.Add(full)) list.Add(full);
            }
            // Havuz küçükse tekrar eden isimle tamamla
            int unique = list.Count;
            while (list.Count < count) list.Add(unique > 0 ? list[list.Count % unique] : "İsimsiz");
            return list;
        }
    }
}
