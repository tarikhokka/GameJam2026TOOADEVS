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
        public enum LineKind { Greeting, Approve, Reject, Mismatch, Bribe, BribeClose, BribeRefused }

        /// <summary>
        /// Müteahhidin replik listesi. Kişiliksiz karakterde (IsGeneric) kendi listesi boşsa Dialogue.asset'teki ortak havuz
        /// (memory.md "📅"). Ana karakterler sadece kendi listelerini kullanır.
        /// </summary>
        public static List<string> LinesOf(GameContext ctx, ClientSO client, LineKind kind)
        {
            if (client == null) return null;
            List<string> own;
            switch (kind)
            {
                case LineKind.Greeting: own = client.GreetingLines; break;
                case LineKind.Approve: own = client.ApproveLines; break;
                case LineKind.Reject: own = client.RejectLines; break;
                case LineKind.Mismatch: own = client.MismatchLines; break;
                case LineKind.Bribe: own = client.BribeLines; break;
                case LineKind.BribeClose: own = client.BribeCloseLines; break;
                default: own = client.BribeRefusedLines; break;
            }
            if ((own != null && own.Count > 0) || !client.IsGeneric || ctx == null || ctx.Dialogue == null) return own;
            var d = ctx.Dialogue;
            switch (kind)
            {
                case LineKind.Greeting: return d.GenericGreetingLines;
                case LineKind.Approve: return d.GenericApproveLines;
                case LineKind.Reject: return d.GenericRejectLines;
                case LineKind.Mismatch: return d.GenericMismatchLines;
                case LineKind.Bribe: return d.GenericBribeLines;
                case LineKind.BribeClose: return d.GenericBribeCloseLines;
                default: return d.GenericBribeRefusedLines;
            }
        }

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
                    p.GreetingText = rng.Pick(LinesOf(ctx, client, LineKind.Greeting), "");

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
            p.ReactionText = rng.Pick(LinesOf(ctx, client, LineKind.Approve), "");

            var gift = client.Gift;
            bool giftAlreadyToday = ctx.TodayProjects.Exists(x => x != p && !string.IsNullOrEmpty(x.GiftId) && x.GiftId == gift?.Id);
            if (client.IsHonest && gift != null && !string.IsNullOrEmpty(gift.Id) && !ctx.State.OfficeGifts.Contains(gift.Id)
                && !giftAlreadyToday && (p.GiftGuaranteed || rng.Chance(client.GiftChance))) // vitrin: kesin
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
            var refused = LinesOf(ctx, client, LineKind.BribeRefused);
            var lines = refusedBribe && refused != null && refused.Count > 0 ? refused : LinesOf(ctx, client, LineKind.Reject);
            p.ReactionText = rng.Pick(lines, "");

            var rec = RecordOf(ctx.State, client.Id, create: true);
            rec.Rejected++;
            if (refusedBribe) rec.BribesRefused++;
            rec.LastVerdict = 2;
        }
    }
}
