using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sadece Sandbox_Desk için sahte IGameActions. Gerçek uygulaması Core'dadır (A);
    // burada masayı denemeye yetecek kadar para/karar güncellemesi yapılır.
    public class FakeGameActions : IGameActions
    {
        private readonly GameContext context;

        public FakeGameActions(GameContext context)
        {
            this.context = context;
        }

        public bool BuyExtraTest(string projectId)
        {
            var project = context.GetTodayProject(projectId);
            int cost = context.Balance.ExtraTestCost;
            if (project == null || project.ExtraTestBought || context.State.Money < cost) return false;

            context.State.Money -= cost;
            project.ExtraTestBought = true;
            foreach (var param in project.Params) param.Tested = true;

            Debug.Log($"[FakeActions] Ek test: {project.Name} (-{cost})");
            context.NotifyStateChanged();
            return true;
        }

        // (A, Core) IGameActions'a Ç4 ile eklendi; sandbox'ta hediyeyi ofis listesine yazar. memory.md'de not var.
        public bool AcceptGift(string projectId)
        {
            var project = context.GetTodayProject(projectId);
            if (project == null || string.IsNullOrEmpty(project.GiftId) || context.State.OfficeGifts.Contains(project.GiftId)) return false;
            context.State.OfficeGifts.Add(project.GiftId);
            context.NotifyStateChanged();
            return true;
        }

        public void Approve(string projectId, bool acceptBribe)
        {
            var project = context.GetTodayProject(projectId);
            if (project == null || project.Verdict != ProjectVerdict.Pending) return;

            var state = context.State;
            project.Verdict = ProjectVerdict.Approved;
            project.DayDecided = state.Day;
            state.Money += project.Fee;
            state.ApprovedToday++;
            state.TotalApproved++;
            state.Buildings.Add(project);

            if (acceptBribe && project.OffersBribe)
            {
                project.BribeAccepted = true;
                state.Money += project.BribeAmount;
                state.TotalBribesTaken++;
            }

            // Gerçekte Core müteahhidin repliklerinden seçer (ClientReactions); sandbox'ta sabit cümle.
            project.ReactionText = project.BribeAccepted ? "Anlaşırız demiştim hocam." : "Sağ olun hocam, hayırlı işler!";

            Debug.Log($"[FakeActions] Onay: {project.Name}{(project.BribeAccepted ? " +zarf" : "")}");
            context.NotifyStateChanged();
        }

        public void Reject(string projectId)
        {
            var project = context.GetTodayProject(projectId);
            if (project == null || project.Verdict != ProjectVerdict.Pending) return;

            project.Verdict = ProjectVerdict.Rejected;
            project.DayDecided = context.State.Day;

            var client = context.GetClient(project.ClientId);
            if (client != null && client.IsThug) context.State.WindowRepairsToday++;
            project.ReactionText = client != null && client.IsThug ? "Bunu unutmam hocam..." : "Peki hocam, eksikleri tamamlayıp gelirim.";

            Debug.Log($"[FakeActions] Red: {project.Name}");
            context.NotifyStateChanged();
        }

        public void SetExpensePaid(string expenseId, bool paid)
        {
            // Masada kullanılmaz.
        }
    }
}
