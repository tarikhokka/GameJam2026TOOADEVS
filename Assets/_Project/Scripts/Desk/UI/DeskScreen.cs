using System;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranı. GameContext'ten okur, oyuncu kararlarını sadece context.Actions ile Core'a iletir.
    // Oyuncu "Mesaiyi bitir"e basınca Completed bir kez tetiklenir.
    public class DeskScreen : MonoBehaviour, IGameScreen
    {
        [SerializeField] private DeskHud hud;
        [SerializeField] private ProjectQueueView queue;
        [SerializeField] private DocumentView documentView;
        [SerializeField] private ActionPanel actionPanel;

        public GamePhase Phase => GamePhase.Desk;
        public event Action Completed;

        private GameContext context;
        private string selectedId;
        private int selectedDocIndex;
        private bool completed;

        private void Awake()
        {
            queue.Selected += Select;
            documentView.TabSelected += i => { selectedDocIndex = i; Redraw(); };

            actionPanel.ExtraTestClicked += () => { context.Actions.BuyExtraTest(selectedId); Redraw(); };
            actionPanel.ApproveClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: false));
            actionPanel.TakeBribeClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: true));
            actionPanel.RejectClicked += () => Decide(() => context.Actions.Reject(selectedId));
            actionPanel.FinishClicked += Finish;
        }

        public void Show(GameContext gameContext)
        {
            Unsubscribe();
            context = gameContext;
            context.StateChanged += Redraw;
            completed = false;

            gameObject.SetActive(true);
            Select(FirstPendingId());
        }

        public void Hide()
        {
            Unsubscribe();
            gameObject.SetActive(false);
        }

        private void OnDestroy() => Unsubscribe();

        private void Unsubscribe()
        {
            if (context != null) context.StateChanged -= Redraw;
        }

        private void Select(string projectId)
        {
            selectedId = projectId;
            selectedDocIndex = 0;
            Redraw();
        }

        // Karar verilince sıradaki bekleyen dosyaya geç; hepsi bittiyse son dosyada kal.
        private void Decide(Action decision)
        {
            decision();
            string next = FirstPendingId();
            if (next != null) Select(next);
            else Redraw();
        }

        private void Finish()
        {
            if (completed || !AllDecided()) return;
            completed = true;
            Completed?.Invoke();
        }

        private string FirstPendingId() =>
            context.TodayProjects.Find(p => p.Verdict == ProjectVerdict.Pending)?.Id;

        private bool AllDecided() =>
            context.TodayProjects.TrueForAll(p => p.Verdict != ProjectVerdict.Pending);

        private bool CanBuyExtraTest(ProjectData project) =>
            project.Verdict == ProjectVerdict.Pending
            && !project.ExtraTestBought
            && context.Balance != null
            && context.State.Money >= context.Balance.ExtraTestCost;

        private void Redraw()
        {
            if (context == null) return;

            hud.Refresh(context);
            queue.Refresh(context.TodayProjects, selectedId);

            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (project == null)
            {
                documentView.ShowEmpty();
                actionPanel.ShowEmpty(AllDecided());
                return;
            }

            documentView.Show(context, project, selectedDocIndex);
            actionPanel.Show(project, context.GetClient(project.ClientId), CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0, AllDecided());
        }
    }
}
