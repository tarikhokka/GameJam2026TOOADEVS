using System;
using System.Collections;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranı. GameContext'ten okur, oyuncu kararlarını sadece context.Actions ile Core'a iletir.
    // Akış: müteahhit gelip karşıdaki koltuğa oturur (selamlama balonu), masada LAB RAPORU kitabı belirir →
    // oyuncu kitaba tıklayınca dosya ortada açılır → karar → kaşe → dosya kapanır, tepki balonu → sıradaki gelir.
    // Oyuncu "Mesaiyi bitir"e basınca Completed bir kez tetiklenir.
    public class DeskScreen : MonoBehaviour, IGameScreen
    {
        [SerializeField] private DeskHud hud;
        [SerializeField] private ClientSceneView scene;
        [SerializeField] private DeskPropsView props;
        [SerializeField] private DossierView dossier;
        [SerializeField] private DocumentView documentView;
        [SerializeField] private ActionPanel actionPanel;
        [SerializeField] private DeskAudio deskAudio;
        [SerializeField] private RulebookView rulebook;
        [SerializeField] private TutorialView tutorial;
        [SerializeField] private EventCardView eventCard;

        [Tooltip("Karar kaşesi basıldıktan sonra dosya kapanmadan önce beklenen süre (sn)")]
        [SerializeField] private float stampDelay = 0.6f;
        [Tooltip("Müteahhidin tepki balonu varsa sıradakine geçmeden önce bu kadar beklenir (sn)")]
        [SerializeField] private float reactionHold = 1.5f;

        public GamePhase Phase => GamePhase.Desk;
        public event Action Completed;

        private GameContext context;
        private string selectedId;
        private int selectedDocIndex;
        private bool completed;
        private bool stamping; // karar sonrası sıra (kaşe, tepki, olay kartı) sürerken yeni girdi alınmaz

        private void Awake()
        {
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

            props.LabBookClicked += OpenDossier;
            props.FinishClicked += Finish;
            dossier.CloseClicked += CloseDossier;

            if (rulebook != null) rulebook.Toggled += PlayPaper;
            if (tutorial != null) tutorial.PageChanged += PlayPaper;
        }

        public void Show(GameContext gameContext)
        {
            Unsubscribe();
            context = gameContext;
            context.StateChanged += Redraw;
            completed = false;
            stamping = false;

            gameObject.SetActive(true);
            if (rulebook != null) rulebook.Bind(context);
            if (deskAudio != null) deskAudio.StartMusic();
            dossier.Close();
            scene.Clear();
            Select(FirstPendingId());
            if (tutorial != null && context.State.Day == 1) tutorial.Open();
        }

        public void Hide()
        {
            Unsubscribe();
            StopAllCoroutines();
            stamping = false;
            dossier.Close();
            if (rulebook != null) rulebook.Close();
            if (tutorial != null) tutorial.Close();
            if (eventCard != null) eventCard.Close();
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

        private void OpenDossier()
        {
            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (stamping || project == null || project.Verdict != ProjectVerdict.Pending) return;
            PlayPaper();
            dossier.Open();
            Redraw();
        }

        private void CloseDossier()
        {
            if (stamping || !dossier.IsOpen) return;
            PlayPaper();
            dossier.Close();
        }

        private void BuyExtraTest()
        {
            if (stamping) return;
            if (deskAudio != null) deskAudio.PlayClick();
            context.Actions.BuyExtraTest(selectedId);
            Redraw();
        }

        // Karar kaşesi açık dosyaya basılır; sonra dosya kapanır, müteahhit tepki verir, sıradaki gelir.
        // Karar Core'da cam tamiri doğurduysa (kabadayı reddedildi) sıradakinden önce olay kartı gösterilir.
        private void Decide(Action decision)
        {
            if (stamping) return;
            stamping = true;

            int repairsBefore = context.State.WindowRepairsToday;
            var project = context.GetTodayProject(selectedId);

            decision();
            Redraw();
            documentView.AnimateDecisionStamp();
            if (deskAudio != null) deskAudio.PlayStamp();

            bool windowBroken = context.State.WindowRepairsToday > repairsBefore;
            StartCoroutine(AfterDecision(project, windowBroken));
        }

        private IEnumerator AfterDecision(ProjectData project, bool windowBroken)
        {
            yield return new WaitForSeconds(stampDelay);
            dossier.Close();

            // Tepki: Core karardan sonra doldurur (ClientReactions); boşsa beklemeden geçilir.
            if (project != null && !string.IsNullOrEmpty(project.ReactionText))
            {
                scene.Say(project.ReactionText);
                yield return new WaitForSeconds(reactionHold);
            }

            if (windowBroken && eventCard != null)
            {
                if (deskAudio != null) deskAudio.PlayGlassBreak();
                var client = context.GetClient(project.ClientId);
                string name = client != null ? client.DisplayName : "Müteahhit";
                int cost = context.Balance != null ? context.Balance.WindowRepairCost : 0;
                eventCard.Open("Camın kırıldı!",
                    $"{name} kararını beğenmedi. Biraz sonra ofisinin camına bir taş geldi.\n\n" +
                    $"Cam tamiri: <b>{DeskFormat.Money(cost)}</b>. Bugünün giderlerine eklendi.",
                    AdvanceToNext);
                yield break;
            }

            AdvanceToNext();
        }

        private void AdvanceToNext()
        {
            stamping = false;

            string next = FirstPendingId();
            if (next != null) Select(next);
            else
            {
                selectedId = null;
                Redraw();
            }
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

        // Core'un seçtiği selamlama (tekrar gelen müteahhitte "geçen sefer..."); yoksa müteahhidin replik listesinden.
        // Zarf varsa rüşvet repliği de balonda söylenir.
        private static string Greeting(ProjectData project, ClientSO client)
        {
            string greeting = !string.IsNullOrEmpty(project.GreetingText)
                ? project.GreetingText
                : DeskFormat.PickLine(client?.GreetingLines, project.Id);
            if (!project.OffersBribe) return greeting;

            string bribe = DeskFormat.PickLine(client?.BribeLines, project.Id);
            if (string.IsNullOrEmpty(bribe)) return greeting;
            return string.IsNullOrEmpty(greeting) ? bribe : $"{greeting}\n{bribe}";
        }

        private bool CanBuyExtraTest(ProjectData project) =>
            project.Verdict == ProjectVerdict.Pending
            && !project.ExtraTestBought
            && context.Balance != null
            && context.State.Money >= context.Balance.ExtraTestCost;

        private void Redraw()
        {
            if (context == null) return;

            hud.Refresh(context);

            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (project == null)
            {
                scene.Clear();
                props.ShowEmpty(AllDecided());
                documentView.ShowEmpty();
                actionPanel.ShowEmpty();
                return;
            }

            var client = context.GetClient(project.ClientId);
            scene.Seat(project.Id, client, Greeting(project, client));
            props.Show(project, client);
            documentView.Show(context, project, selectedDocIndex);
            actionPanel.Show(project, client, CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0);
        }
    }
}
