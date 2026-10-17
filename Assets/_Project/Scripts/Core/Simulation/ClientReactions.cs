using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Bir müteahhidin bu oyundaki geçmişi (Ç4). GameState.ClientHistory'de, kayıtla saklanır.</summary>
    [Serializable]
    public class ClientRecord
    {
        public string ClientId;
        public int Approved;
        public int Rejected;
        public int BribesRefused;   // Zarf teklif etti, proje reddedildi
        public int LastVerdict;     // 0 = henüz yok, 1 = onay, 2 = ret
    }

    /// <summary>
    /// Ç4: müteahhit konuşmaları ve etkileri. Saf C#; GameSession çağırır.
    ///  - Selamlama: daha önce karar verilmişse ReturnLines ({last} → "onaylamıştınız" / "reddetmiştiniz"), yoksa GreetingLines.
    ///  - Tepki: onay → ApproveLines; ret → zarf geri çevrildiyse BribeRefusedLines, değilse RejectLines.
    ///  - Hediye: SADECE dürüst usta (IsHonest) verir (Ahmet kararı); onayda ClientSO.Gift tanımlıysa, bu hediye henüz alınmadıysa
    ///    ve bugün başka projede çıkmadıysa GiftChance ihtimalle.
    ///  - Zarf artışı: BribeRaiseOnReject × şimdiye kadarki ret sayısı (Nuri).
    /// Replik listesi boşsa metin boş kalır; masa bu durumda balon göstermez.
    /// </summary>
    public static class ClientReactions
    {
        public static ClientRecord RecordOf(GameState state, string clientId, bool create = false)
        {
            if (string.IsNullOrEmpty(clientId)) return null;
            var r = state.ClientHistory.Find(x => x.ClientId == clientId);
            if (r == null && create)
            {
                r = new ClientRecord { ClientId = clientId };
                state.ClientHistory.Add(r);
            }
            return r;
        }

        /// <summary>BeginDay'de: selamlama metni + geçmişe göre zarf artışı.</summary>
        public static void PrepareArrivals(GameContext ctx, Func<string, Random> rngFor)
        {
            foreach (var p in ctx.TodayProjects)
            {
                var client = ctx.GetClient(p.ClientId);
                if (client == null) continue;
                var rec = RecordOf(ctx.State, client.Id);
                var rng = rngFor(p.Id + ":greet");

                if (rec != null && rec.LastVerdict != 0 && client.ReturnLines != null && client.ReturnLines.Count > 0)
                    p.GreetingText = rng.Pick(client.ReturnLines, "")
                        .Replace("{last}", rec.LastVerdict == 1 ? "onaylamıştınız" : "reddetmiştiniz");
                else
                    p.GreetingText = rng.Pick(client.GreetingLines, "");

                if (p.OffersBribe && client.BribeRaiseOnReject > 0 && rec != null && rec.Rejected > 0)
                    p.BribeAmount += client.BribeRaiseOnReject * rec.Rejected;
            }
        }

        /// <summary>Approve'da: tepki + hediye + geçmiş.</summary>
        public static void OnApproved(GameContext ctx, ProjectData p, Func<string, Random> rngFor)
        {
            var client = ctx.GetClient(p.ClientId);
            if (client == null) return;
            var rng = rngFor(p.Id + ":react");
            p.ReactionText = rng.Pick(client.ApproveLines, "");

            var gift = client.Gift;
            bool giftAlreadyToday = ctx.TodayProjects.Exists(x => x != p && !string.IsNullOrEmpty(x.GiftId) && x.GiftId == gift?.Id);
            if (client.IsHonest && gift != null && !string.IsNullOrEmpty(gift.Id) && !ctx.State.OfficeGifts.Contains(gift.Id)
                && !giftAlreadyToday && rng.Chance(client.GiftChance))
                p.GiftId = gift.Id;

            var rec = RecordOf(ctx.State, client.Id, create: true);
            rec.Approved++;
            rec.LastVerdict = 1;
        }

        /// <summary>Reject'te: tepki + geçmiş.</summary>
        public static void OnRejected(GameContext ctx, ProjectData p, Func<string, Random> rngFor)
        {
            var client = ctx.GetClient(p.ClientId);
            if (client == null) return;
            var rng = rngFor(p.Id + ":react");
            bool refusedBribe = p.OffersBribe && p.BribeRevealed; // teklif edilmemiş zarf geri çevrilemez
            var lines = refusedBribe && client.BribeRefusedLines != null && client.BribeRefusedLines.Count > 0
                ? client.BribeRefusedLines : client.RejectLines;
            p.ReactionText = rng.Pick(lines, "");

            var rec = RecordOf(ctx.State, client.Id, create: true);
            rec.Rejected++;
            if (refusedBribe) rec.BribesRefused++;
            rec.LastVerdict = 2;
        }
    }
}
