using System;
using System.Collections;
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
        [SerializeField] private DeskAudio deskAudio;

        [Tooltip("Karar kaşesi basıldıktan sonra sıradaki dosyaya geçmeden önce beklenen süre (sn)")]
        [SerializeField] private float stampDelay = 0.6f;

        public GamePhase Phase => GamePhase.Desk;
        public event Action Completed;

        private GameContext context;
        private string selectedId;
        private int selectedDocIndex;
        private bool completed;
        private bool stamping; // kaşe animasyonu sürerken yeni girdi alınmaz

        private void Awake()
        {
            queue.Selected += id =>
            {
                if (stamping) return;
                PlayPaper();
                Select(id);
            };
            documentView.TabSelected += i =>
            {
                if (stamping) return;
                PlayPaper();
                selectedDocIndex = i;
                Redraw();
            };

            actionPanel.ExtraTestClicked += BuyExtraTest;
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
            stamping = false;

            gameObject.SetActive(true);
            if (deskAudio != null) deskAudio.StartMusic();
            Select(FirstPendingId());
        }

        public void Hide()
        {
            Unsubscribe();
            StopAllCoroutines();
            stamping = false;
            if (deskAudio != null) deskAudio.StopMusic();
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

        private void BuyExtraTest()
        {
            if (stamping) return;
            if (deskAudio != null) deskAudio.PlayClick();
            context.Actions.BuyExtraTest(selectedId);
            Redraw();
        }

        // Karar kaşesi basılır, kısa bir beklemeden sonra sıradaki bekleyen dosyaya geçilir.
        private void Decide(Action decision)
        {
            if (stamping) return;
            stamping = true;

            decision();
            Redraw();
            documentView.AnimateDecisionStamp();
            if (deskAudio != null) deskAudio.PlayStamp();

            StartCoroutine(AdvanceAfterStamp());
        }

        private IEnumerator AdvanceAfterStamp()
        {
            yield return new WaitForSeconds(stampDelay);
            stamping = false;

            string next = FirstPendingId();
            if (next != null)
            {
                PlayPaper();
                Select(next);
            }
            else Redraw();
        }

        private void Finish()
        {
            if (stamping || completed || !AllDecided()) return;
            if (deskAudio != null) deskAudio.PlayClick();
            completed = true;
            Completed?.Invoke();
        }

        private void PlayPaper()
        {
            if (deskAudio != null) deskAudio.PlayPaper();
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
