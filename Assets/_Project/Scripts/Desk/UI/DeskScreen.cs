using System;
using System.Collections;
using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranı. GameContext'ten okur, oyuncu kararlarını sadece context.Actions ile Core'a iletir.
    // Akış: müteahhit gelip karşıdaki koltuğa oturur (selamlama balonu); masada ortada YAPI DENETİM FORMU klasörü, solda
    // LAB RAPORU kitabı belirir, sağda EL KİTABI hep durur. Her biri kendi kağıt penceresini açar (Papers, Please gibi yan yana):
    // formda ONAY / RED (ve zarf), lab raporunda ek test. Karar → kaşe forma basılır → form kaşeli klasör olarak müteahhide
    // gider, lab raporu kitabına döner → tepki balonu → sıradaki gelir. Oyuncu "Mesaiyi bitir"e basınca Completed bir kez tetiklenir.
    public class DeskScreen : MonoBehaviour, IGameScreen
    {
        // Pencere yerleşimi (1920x1080 tuval): soldan sağa form, lab, el kitabı; açık olanlar yan yana ortalanır.
        private const float SheetWidth = 680f, SheetGap = 20f, SheetY = -40f, ThreeOpenScale = 0.9f;

        [SerializeField] private DeskHud hud;
        [SerializeField] private ClientSceneView scene;
        [SerializeField] private DeskPropsView props;
        [SerializeField] private PaperWindow formWindow;
        [SerializeField] private FormView formView;
        [SerializeField] private PaperWindow labWindow;
        [SerializeField] private LabReportView labView;
        [SerializeField] private DeskAudio deskAudio;
        [SerializeField] private RulebookView rulebook;
        [SerializeField] private TutorialView tutorial;
        [SerializeField] private EventCardView eventCard;
        [SerializeField] private FolderHandoffView handoff; // karar sonrası kaşeli klasör müteahhide gider
        [SerializeField] private GiftOfferView giftOffer;   // onaydan sonra hediye kartı (sadece "Kabul et")
        [SerializeField] private OfficeGiftsView officeGifts; // kabul edilen hediyeler masada

        [Tooltip("Karar kaşesi basıldıktan sonra form kapanmadan önce beklenen süre (sn)")]
        [SerializeField] private float stampDelay = 0.6f;
        [Tooltip("Müteahhidin tepki balonu yazılıp bittikten sonra sıradakine geçmeden önce bu kadar beklenir (sn)")]
        [SerializeField] private float reactionHold = 1.5f;

        public GamePhase Phase => GamePhase.Desk;
        public event Action Completed;

        private GameContext context;
        private string selectedId;
        private bool completed;
        private bool stamping; // karar sonrası sıra (kaşe, tepki, olay kartı) sürerken yeni girdi alınmaz

        private PaperWindow RulebookWindow => rulebook != null ? rulebook.Window : null;

        private void Awake()
        {
            formView.ApproveClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: false));
            formView.TakeBribeClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: true));
            formView.RejectClicked += () => Decide(() => context.Actions.Reject(selectedId));
            labView.ExtraTestClicked += BuyExtraTest;

            props.FormClicked += () => ToggleWindow(formWindow);
            props.LabBookClicked += () => ToggleWindow(labWindow);
            props.FinishClicked += Finish;

            foreach (var window in Windows())
            {
                var w = window;
                w.OpenChanged += () => { if (w.IsOpen) ArrangeSheets(); };
                if (w.Sheet != null) w.Sheet.Dropped += PlayPaper; // kağıt masaya bırakılınca
            }
            formWindow.CloseClicked += () => CloseWindow(formWindow);
            labWindow.CloseClicked += () => CloseWindow(labWindow);

            if (rulebook != null) rulebook.Toggled += PlayPaper;
            if (tutorial != null) tutorial.PageChanged += PlayPaper;
            if (deskAudio != null) scene.CharTyped += deskAudio.PlayTyping; // konuşma balonu harf harf yazarken
        }

        private IEnumerable<PaperWindow> Windows()
        {
            yield return formWindow;
            yield return labWindow;
            if (RulebookWindow != null) yield return RulebookWindow;
        }

        // Bir pencere açılınca açık olanlar soldan sağa (form, lab, el kitabı) yan yana dizilir; üçü açıksa biraz küçülür.
        // Kapanınca diğerleri yerinde kalır (karardan sonra el kitabı müteahhidin önüne kaymasın).
        private void ArrangeSheets()
        {
            var open = new List<PaperWindow>();
            foreach (var w in Windows()) if (w.IsOpen) open.Add(w);
            if (open.Count == 0) return;

            float scale = open.Count >= 3 ? ThreeOpenScale : 1f;
            float width = SheetWidth * scale;
            float total = open.Count * width + (open.Count - 1) * SheetGap;
            float x = -total * 0.5f + width * 0.5f;
            foreach (var w in open)
            {
                w.SetHome(new Vector2(x, SheetY), scale);
                x += width + SheetGap;
            }
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
            formWindow.Close(instant: true);
            labWindow.Close(instant: true);
            if (giftOffer != null) giftOffer.Close();
            if (officeGifts != null) officeGifts.Refresh(context); // önceki günlerde kabul edilenler (kayıttan da)
            scene.Clear();
            Select(FirstPendingId());
            if (tutorial != null && context.State.Day == 1) tutorial.Open();
        }

        public void Hide()
        {
            Unsubscribe();
            StopAllCoroutines();
            stamping = false;
            formWindow.Close(instant: true);
            labWindow.Close(instant: true);
            if (rulebook != null) rulebook.Close(instant: true);
            if (tutorial != null) tutorial.Close();
            if (eventCard != null) eventCard.Close();
            if (giftOffer != null) giftOffer.Close();
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
            Redraw();
        }

        // Masadaki form klasörü / LAB RAPORU: sadece bekleyen dosyada ve karar sırası sürmüyorken açılır.
        private void ToggleWindow(PaperWindow window)
        {
            if (window.IsOpen)
            {
                CloseWindow(window);
                return;
            }
            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (stamping || project == null || project.Verdict != ProjectVerdict.Pending) return;
            PlayPaper();
            Redraw();
            window.Open();
        }

        private void CloseWindow(PaperWindow window)
        {
            if (stamping || !window.IsOpen) return;
            PlayPaper();
            window.Close();
        }

        private void BuyExtraTest()
        {
            if (stamping) return;
            if (deskAudio != null) deskAudio.PlayClick();
            context.Actions.BuyExtraTest(selectedId);
            Redraw();
        }

        // Karar kaşesi forma basılır; sonra form kaşeli klasör olarak müteahhide gider, lab raporu kitabına döner,
        // müteahhit tepki verir, sıradaki gelir. Karar Core'da cam tamiri doğurduysa (kabadayı reddedildi) önce olay kartı.
        private void Decide(Action decision)
        {
            if (stamping) return;
            stamping = true;

            int repairsBefore = context.State.WindowRepairsToday;
            var project = context.GetTodayProject(selectedId);

            decision();
            Redraw();
            formView.AnimateDecisionStamp();
            if (deskAudio != null) deskAudio.PlayStamp();

            bool windowBroken = context.State.WindowRepairsToday > repairsBefore;
            StartCoroutine(AfterDecision(project, windowBroken));
        }

        private IEnumerator AfterDecision(ProjectData project, bool windowBroken)
        {
            yield return new WaitForSeconds(stampDelay);
            labWindow.Close();
            if (project != null)
            {
                formWindow.HandTo(handoff, project.Verdict == ProjectVerdict.Approved, project.Name);
                if (handoff != null) PlayPaper();
                while (handoff != null && handoff.Playing) yield return null; // müteahhit klasörü alınca konuşur
            }
            else formWindow.Close();

            // Tepki: Core karardan sonra doldurur (ClientReactions); boşsa beklemeden geçilir.
            bool reacted = project != null && !string.IsNullOrEmpty(project.ReactionText);
            if (reacted) scene.Say(project.ReactionText);

            // Hediye (sadece onayda, Core dürüst ustaya bazen verir): tepki balonuyla aynı anda kart; "Kabul et" → masaya konur.
            var gift = project != null && project.Verdict == ProjectVerdict.Approved ? context.GetGift(project.GiftId) : null;
            if (gift != null && giftOffer != null)
            {
                var giver = context.GetClient(project.ClientId);
                giftOffer.Open(gift, giver != null ? giver.DisplayName : "Müteahhit", () =>
                {
                    if (context.Actions.AcceptGift(project.Id) && officeGifts != null) officeGifts.Refresh(context, gift.Id);
                    PlayPaper();
                });
                while (giftOffer.IsOpen) yield return null;
            }

            if (reacted)
            {
                while (scene.IsTyping) yield return null; // uzun replik yarıda kesilmesin (tıklayınca anında biter)
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
                scene.Leave(); // son müteahhit sola çıkar, koltuk boş kalır
                props.ShowEmpty(AllDecided());
                return;
            }

            var client = context.GetClient(project.ClientId);
            scene.Seat(project.Id, client, Greeting(project, client));
            props.Show(project, client);
            formView.Show(context, project);
            labView.Show(context, project, CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0);
        }
    }
}
