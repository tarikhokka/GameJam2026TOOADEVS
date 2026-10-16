using System;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranının kökü. Show ile bir oturum başlatır, oyuncu mesaiyi bitirince Completed atar.
    // Core'daki IGameScreen gelince ona bir adapter ile bağlanacak.
    public class DeskScreen : MonoBehaviour
    {
        [SerializeField] private DeskHud hud;
        [SerializeField] private ProjectQueueView queue;
        [SerializeField] private DocumentView documentView;
        [SerializeField] private ActionPanel actionPanel;

        public event Action<DeskResult> Completed;

        public DeskSession Session { get; private set; }

        private string selectedId;
        private int selectedDocIndex;

        private void Awake()
        {
            queue.Selected += Select;
            documentView.TabSelected += i => { selectedDocIndex = i; Redraw(); };

            actionPanel.ExtraTestClicked += () => Session.BuyExtraTest(selectedId);
            actionPanel.ApproveClicked += () => Decide(() => Session.Approve(selectedId, takeBribe: false));
            actionPanel.TakeBribeClicked += () => Decide(() => Session.Approve(selectedId, takeBribe: true));
            actionPanel.RejectClicked += () => Decide(() => Session.Reject(selectedId));
            actionPanel.FinishClicked += () => Completed?.Invoke(Session.BuildResult());
        }

        public void Show(DeskSessionConfig config)
        {
            if (Session != null) Session.Changed -= Redraw;

            Session = new DeskSession(config);
            Session.Changed += Redraw;

            gameObject.SetActive(true);
            Select(Session.FirstPendingId());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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
            string next = Session.FirstPendingId();
            if (next != null) Select(next);
        }

        private void Redraw()
        {
            hud.Refresh(Session);
            queue.Refresh(Session, selectedId);

            if (selectedId == null)
            {
                documentView.ShowEmpty();
                actionPanel.ShowEmpty(Session.AllDecided);
                return;
            }

            var project = Session.GetProject(selectedId);
            var decision = Session.GetDecision(selectedId);
            documentView.Show(project, selectedDocIndex, decision.ExtraTestBought);
            actionPanel.Show(project, decision, Session.CanBuyExtraTest(selectedId),
                Session.Config.ExtraTestCost, Session.AllDecided);
        }
    }
}
