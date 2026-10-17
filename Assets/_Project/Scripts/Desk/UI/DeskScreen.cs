using System;
using System.Collections;
using System.Collections.Generic;
using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa ekranı. GameContext'ten okur, oyuncu kararlarını sadece context.Actions ile Core'a iletir.
    // Yeni hedef arayüz (Ahmet kararı): üstte ofis, müteahhit masanın arkasında oturur (selamlama balonu); masada üç belge
    // düz ve yan yana: soldan YAPI DENETİM FORMU, LABORATUVAR RAPORU (müteahhit oturunca aşağıdan kayarak gelir) ve hep açık
    // DENETÇİ EL KİTABI. Formda ONAY / RED (ve zarf), lab raporunda ek test. Karar → kaşe forma basılır → form ve lab raporu
    // masadan kayıp gider → tepki balonu (+ hediye) → sıradaki gelir. Son müteahhitten sonra mesai zili + "MESAİ BİTTİ"
    // geçiş ekranı, ardından Completed bir kez kendiliğinden tetiklenir ("Mesaiyi bitir" düğmesi kalktı, EEE).
    public class DeskScreen : MonoBehaviour, IGameScreen
    {
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
        [SerializeField] private GiftOfferView giftOffer;   // onaydan sonra hediye kartı (sadece "Kabul et")
        [SerializeField] private OfficeGiftsView officeGifts; // kabul edilen hediyeler masada
        [SerializeField] private InspectController inspect; // İnceleme modu: İNCELE düğmesi, tıklanabilir alanlar, çizgi

        [Tooltip("Karar kaşesi basıldıktan sonra form kapanmadan önce beklenen süre (sn)")]
        [SerializeField] private float stampDelay = 0.6f;
        [Tooltip("Müteahhidin tepki balonu yazılıp bittikten sonra sıradakine geçmeden önce bu kadar beklenir (sn)")]
        [SerializeField] private float reactionHold = 1.5f;

        // (EEE, 17 Ekim; A ve B onayladı) "Mesaiyi bitir" düğmesi kalktı: son müteahhit gidince kısa bir bekleme,
        // mesai zili ve "MESAİ BİTTİ" geçiş ekranı, sonra Completed kendiliğinden tetiklenir.
        [Tooltip("Son karar sırası bittikten sonra geçiş ekranından önce masada beklenen süre (sn)")]
        [SerializeField] private float shiftEndDelay = 1.2f;
        [SerializeField] private float shiftEndFade = 0.7f;
        [SerializeField] private float shiftEndHold = 1.8f;
        [SerializeField] private TMP_FontAsset shiftTitleFont; // PaytoneOne-Regular SDF
        [SerializeField] private TMP_FontAsset shiftInfoFont;  // FiraSans-Bold SDF

        public GamePhase Phase => GamePhase.Desk;
        public event Action Completed;

        private GameContext context;
        private string selectedId;
        private bool completed;
        private bool stamping; // karar sonrası sıra (kaşe, tepki, olay kartı) sürerken yeni girdi alınmaz
        private bool endingShift;
        private ShiftEndOverlay shiftOverlay;


        private void Awake()
        {
            formView.ApproveClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: false));
            formView.TakeBribeClicked += () => Decide(() => context.Actions.Approve(selectedId, acceptBribe: true));
            formView.RejectClicked += () => Decide(() => context.Actions.Reject(selectedId));
            labView.ExtraTestClicked += BuyExtraTest;

            props.FinishClicked += Finish;

            if (rulebook != null) rulebook.Toggled += PlayPaper;
            if (tutorial != null) tutorial.PageChanged += PlayPaper;
            if (deskAudio != null) scene.CharTyped += deskAudio.PlayTyping; // konuşma balonu harf harf yazarken
            if (inspect != null)
            {
                inspect.ModeChanged += OnInspectMode;
                inspect.Clicked += PlayClick;
                // Karar Core'da: bugünkü dosya + El Kitabı'nda oyuncunun seçtiği kat grubu (yanlış grup = ALAKASIZ).
                inspect.Checker = (a, b) => Inspection.Check(context,
                    selectedId != null ? context.GetTodayProject(selectedId) : null, a, b,
                    rulebook != null ? rulebook.SelectedFloorGroup : Inspection.NoFloorGroup);
                // Sonuç sesi (uyuşmazlık = kaşe, diğerleri = tık) Onur'un InspectResultView'unda.
            }
        }

        // Yeni hedef arayüz: bekleyen bir dosya varken form ve lab raporu masada açık (sabit yerlerinde, yan yana);
        // karar sırası (kaşe, tepki) sürerken ve müteahhit yokken kapalı. El Kitabı hep açık (Show).
        private void SyncDocuments(ProjectData project)
        {
            bool show = !stamping && project != null && project.Verdict == ProjectVerdict.Pending;
            if (show && !formWindow.IsOpen)
            {
                formWindow.Open();
                labWindow.Open();
                PlayPaper();
            }
            else if (!show && formWindow.IsOpen && !stamping)
            {
                formWindow.Close();
                labWindow.Close();
            }
        }

        public void Show(GameContext gameContext)
        {
            Unsubscribe();
            context = gameContext;
            context.StateChanged += Redraw;
            completed = false;
            stamping = false;
            endingShift = false;
            if (shiftOverlay != null) shiftOverlay.Hide();

            gameObject.SetActive(true);
            if (rulebook != null)
            {
                rulebook.Bind(context);
                rulebook.Open(); // El Kitabı masada hep açık
            }
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
            if (inspect != null) inspect.SetMode(false);
            StopAllCoroutines();
            stamping = false;
            endingShift = false;
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
            if (inspect != null) inspect.SetMode(false); // yeni müteahhit: inceleme modu kapanır, seçim temizlenir
            if (rulebook != null) rulebook.ResetFloorGroup(); // yeni müteahhit: kat grubunu oyuncu yeniden seçer (Ahmet kararı)
            Redraw();
        }

        private void BuyExtraTest()
        {
            if (stamping || Inspecting) return;
            if (deskAudio != null) deskAudio.PlayClick();
            context.Actions.BuyExtraTest(selectedId);
            Redraw();
        }

        // Karar kaşesi forma basılır; sonra form kaşeli klasör olarak müteahhide gider, lab raporu kitabına döner,
        // müteahhit tepki verir, sıradaki gelir. Karar Core'da cam tamiri doğurduysa (kabadayı reddedildi) önce olay kartı.
        private void Decide(Action decision)
        {
            if (stamping || Inspecting) return;
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
            // Kaşeli form ve lab raporu masadan kayıp gider (yeni düzen: klasörün müteahhide uçması kalktı).
            formWindow.Close();
            labWindow.Close();
            PlayPaper();

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

        private IEnumerator EndShift()
        {
            endingShift = true;
            yield return new WaitForSeconds(shiftEndDelay); // "Bugünkü başvurular bitti." masada kısa görünsün
            if (completed) yield break;
            if (deskAudio != null) deskAudio.PlayShiftEnd();
            if (shiftOverlay == null) shiftOverlay = ShiftEndOverlay.Create(transform, shiftTitleFont, shiftInfoFont);
            yield return shiftOverlay.Play("MESAİ BİTTİ", ShiftSummary(), shiftEndFade, shiftEndHold);
            completed = true;
            Completed?.Invoke();
        }

        private string ShiftSummary()
        {
            int approved = 0, rejected = 0;
            foreach (var p in context.TodayProjects)
            {
                if (p.Verdict == ProjectVerdict.Approved) approved++;
                else if (p.Verdict == ProjectVerdict.Rejected) rejected++;
            }
            const string gold = "#FFD27A";
            return $"{context.State.Day}. gün  ·  <color={gold}>{context.TodayProjects.Count}</color> başvuru  ·  " +
                   $"<color={gold}>{approved}</color> onay  ·  <color={gold}>{rejected}</color> ret";
        }

        private bool Inspecting => inspect != null && inspect.IsOn;

        // İnceleme modu açıkken belgeler sürüklenmez, ONAY / RED / ZARF / EK TEST pasif (El Kitabı sekmeleri ve kat grubu çalışır).
        private void OnInspectMode(bool on)
        {
            formWindow.Sheet.Locked = on;
            labWindow.Sheet.Locked = on;
            if (rulebook != null && rulebook.Window != null) rulebook.Window.Sheet.Locked = on;
            formView.SetLocked(on);
            labView.SetLocked(on);
            PlayClick();
        }

        private void PlayClick()
        {
            if (deskAudio != null) deskAudio.PlayClick();
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
                SyncDocuments(null);
                if (inspect != null) inspect.SetAvailable(false);
                if (AllDecided() && !completed && !endingShift && !stamping) StartCoroutine(EndShift());
                return;
            }

            var client = context.GetClient(project.ClientId);
            scene.Seat(project.Id, client, Greeting(project, client));
            props.Show(project, client);
            formView.Show(context, project);
            labView.Show(context, project, CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0);
            SyncDocuments(project);
            if (inspect != null) inspect.SetAvailable(!stamping && project.Verdict == ProjectVerdict.Pending);
        }
    }
}
