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
    // düz ve yan yana: soldan YAPI DENETİM FORMU, LABORATUVAR RAPORU (belge turu B1: müteahhit oturunca masanın solunda ikon olur,
    // ikona tıklanınca açılır) ve hep açık DENETÇİ EL KİTABI; sağda kaşe çekmecesi, masada zarf, lab raporunda ek test. Karar →
    // kaşe çekmecesinden ONAY / RED kaşesi forma basılır → belgeler müteahhide sürüklenip verilir → tepki balonu (+ hediye) →
    // sıradaki gelir. Son müteahhitten sonra mesai zili + "MESAİ BİTTİ" geçiş ekranı, ardından Completed bir kez kendiliğinden
    // tetiklenir ("Mesaiyi bitir" düğmesi kalktı, EEE).
    // (A, belge turu A1 — memory.md "📂 BELGE TURU", D1) Karar iki adım: StampDecision (kaşe forma basılır, değiştirilemez, Core'a
    // gitmez) → HandBack (belgeler müteahhide verildi: karar Core'a gider, AfterDecision sırası). B2: StampDecision'ı kaşe
    // çekmecesi (StampTrayView) ve zarf kabulü, HandBack'i belge iadesi çağırır.
    public class DeskScreen : MonoBehaviour, IGameScreen
    {
        [SerializeField] private DeskHeaderView header; // üst bar (Onur'un bileşeni, poliş B2): gün, kasa, müteahhit · proje, sayaçlar
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
        [SerializeField] private DeskDialogue dialogue;     // uyuşmazlık diyaloğu (denetçi ↔ müteahhit), sürerken masa kilitli
        [SerializeField] private BribeEnvelopeView bribeEnvelope; // (A) zarfa tıklanınca açılan rüşvet ekranı (kabul / ret)
        [SerializeField] private StampTrayView stampTray;   // (belge turu B2) kaşe çekmecesi (Onur): kaşeyi forma sürükle = StampDecision

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

        private bool holdEnvelope; // inceleme diyaloğu bitene kadar teklif edilen zarf masaya düşmez

        // ---------- (A, belge turu A1) Kaşe → iade API'si (memory.md "API sözleşmesi") ----------
        private string stampedId;    // kaşesi basılmış dosya (bu müteahhit); null = kaşe yok
        private bool stampedApprove; // basılan kaşe: true = ONAY

        /// <summary>Formdaki kaşe alanı (kaşe bölmesi buraya bırakılınca basar).</summary>
        public RectTransform StampArea => formView.StampArea;

        /// <summary>Bu dosyaya kaşe basıldı mı (karar henüz Core'a gitmemiş olabilir; HandBack'te gider).</summary>
        public bool IsStamped => stampedId != null && stampedId == selectedId;

        /// <summary>
        /// Kaşe basılabilir mi: dosya bekliyor, form açık, kaşe yok; karar sırası, diyalog ve zarf ekranı yok.
        /// (B2, Oğuzhan) İnceleme modu engel değil: modda kaşe düğmesi aydınlık kalır, çekmece açılınca mod kapanır (poliş K2 gibi).
        /// </summary>
        public bool CanStamp
        {
            get
            {
                var project = selectedId != null && context != null ? context.GetTodayProject(selectedId) : null;
                return project != null && project.Verdict == ProjectVerdict.Pending && formWindow.IsOpen && !IsStamped
                    && !stamping && !envelopeOpen && (dialogue == null || !dialogue.IsPlaying);
            }
        }

        /// <summary>Kaşe basıldı (true = ONAY). Karar henüz Core'da değil.</summary>
        public event Action<bool> DecisionStamped;

        /// <summary>
        /// Kaşeyi forma bas: kaşe izi görünür, ses çalar, karar artık değiştirilemez. Core'a gitmez (D1).
        /// Zarf masadayken ONAY kaşesi = zarfla onay (HandBack'te BribeRevealed'e göre).
        /// </summary>
        public void StampDecision(bool approve)
        {
            if (Inspecting && !stamping) inspect.SetMode(false);
            if (!CanStamp) return;
            stampedId = selectedId;
            stampedApprove = approve;
            formView.SetPendingStamp(approve ? ProjectVerdict.Approved : ProjectVerdict.Rejected);
            Redraw();
            formView.AnimateDecisionStamp();
            if (deskAudio != null) deskAudio.PlayStamp();
            DecisionStamped?.Invoke(approve);
        }

        /// <summary>
        /// Belgeler müteahhide verildi: karar Core'a gider (ONAY: Approve(id, acceptBribe: BribeRevealed); RED: Reject) ve
        /// tepki / hediye / kabadayı / sıradaki sırası başlar. Kaşe yoksa ya da sıra / diyalog sürüyorsa false.
        /// </summary>
        public bool HandBack()
        {
            if (!IsStamped || stamping || (dialogue != null && dialogue.IsPlaying)) return false;
            string id = selectedId;
            if (stampedApprove)
            {
                var p = context.GetTodayProject(id);
                Decide(() => context.Actions.Approve(id, acceptBribe: p != null && p.BribeRevealed));
            }
            else Decide(() => context.Actions.Reject(id));
            return true;
        }


        private void Awake()
        {
            // (Tarık, Ahmet kararı 18 Ekim ~03:10) Zarf teklif edildiyse ONAY = zarfla onay; zarfsız onay yok.
            // Teklif edilmemiş zarf yine alınamaz (Core BribeRevealed değilse kabul etmez) — HandBack'te.
            // (belge turu B2) Karar kaşe çekmecesinden: kaşe forma sürüklenip basılır (StampDecision), belgeler müteahhide verilince
            // Core'a gider (HandBack). ONAY / RED düğmeleri ve geçici köprü kalktı.
            if (stampTray != null)
            {
                stampTray.Bind(() => StampArea, () => CanStamp);
                stampTray.Stamped += StampDecision;
                stampTray.OpenChanged += OnStampTrayOpen;
            }
            labView.ExtraTestClicked += BuyExtraTest;

            props.FinishClicked += Finish;
            props.FormIconClicked += () => OpenDocument(formWindow, props.FormIcon); // (belge turu B1) belge ikonundan açılır
            props.LabIconClicked += () => OpenDocument(labWindow, props.LabIcon);
            formWindow.Sheet.CanHandBack = () => IsStamped; // kaşeden sonra ofis tarafına taşınan belge ikon boyutuna küçülür
            labWindow.Sheet.CanHandBack = () => IsStamped;
            formWindow.HandBackTarget = scene.DropArea; // müteahhidin üstünde bırakılan kaşeli belge verilmiş olur
            labWindow.HandBackTarget = scene.DropArea;
            formWindow.HandedBack += OnHandedBack;
            labWindow.HandedBack += OnHandedBack;
            formWindow.HandBackRefused += StampFirst;
            labWindow.HandBackRefused += StampFirst;
            props.EnvelopeRevealed += PlayPaper; // zarf masaya düştü
            props.EnvelopeClicked += OpenEnvelope; // (A) zarf açılır: para + tutar, RÜŞVETİ KABUL ET / REDDET

            if (rulebook != null) rulebook.Toggled += PlayPaper;
            if (tutorial != null) tutorial.PageChanged += PlayPaper;
            if (deskAudio != null) scene.CharTyped += deskAudio.PlayTyping; // konuşma balonu harf harf yazarken
            if (inspect != null)
            {
                inspect.ModeChanged += OnInspectMode;
                inspect.Clicked += PlayClick;
                // Karar Core'da: bugünkü dosya + El Kitabı'nda oyuncunun seçtiği kat grubu (yanlış grup = ALAKASIZ).
                inspect.Checker = Inspect;
                // Sonuç sesi (uyuşmazlık = kaşe, diğerleri = tık) Onur'un InspectResultView'unda.
                inspect.Compared += OnInspected;
            }
        }

        // (belge turu B1) Form ve lab raporu kendiliğinden açılmaz: müteahhit oturunca masanın solunda ikon olarak durur, ikona
        // tıklanınca açılır (OpenDocument); açık belgenin ikonu masada görünmez. Müteahhit yokken / karar verilmişken belgeler
        // kapanır (karar sırası sürerken kapanışı AfterDecision yapar). El Kitabı hep açık (Show).
        private void SyncDocuments(ProjectData project)
        {
            bool pending = !stamping && project != null && project.Verdict == ProjectVerdict.Pending;
            if (!pending && !stamping)
            {
                formWindow.Close();
                labWindow.Close();
            }
            props.SetDocIcons(pending && !formWindow.IsOpen && !formGiven, pending && !labWindow.IsOpen && !labGiven);
        }

        private bool formGiven, labGiven; // (belge turu B1) bu müteahhide verilen belgeler; ikisi de verilince HandBack

        private void ClearGiven() => formGiven = labGiven = false;

        // Kaşeli belge müteahhide verildi (PaperWindow sürükle-bırak). İkisi de verilince karar Core'a gider (HandBack).
        private void OnHandedBack(PaperWindow window)
        {
            if (window == formWindow) formGiven = true;
            else if (window == labWindow) labGiven = true;
            PlayPaper();
            Redraw();
            if (formGiven && labGiven) HandBack();
        }

        // Kaşeden önce müteahhide uzatılan belge geri döner; müteahhit hatırlatır (EEE metni, memory.md "📂 BELGE TURU" T2).
        private const string StampFirstLine = "Önce kaşeyi basın hocam, sonra alırım.";

        private void StampFirst()
        {
            if (stamping || (dialogue != null && dialogue.IsPlaying)) return;
            scene.Say(StampFirstLine);
        }

        // İkona tıklandı: belge ikonun yerinden büyüyerek bugünkü yerine açılır, ikon masadan kalkar.
        private void OpenDocument(PaperWindow window, RectTransform icon)
        {
            var project = selectedId != null && context != null ? context.GetTodayProject(selectedId) : null;
            if (stamping || window.IsOpen || project == null || project.Verdict != ProjectVerdict.Pending) return;
            window.OpenFrom(icon);
            window.Sheet.BringToFront();
            PlayPaper();
            Redraw();
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
            holdEnvelope = false;
            ClearStamp();

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
            if (dialogue != null) dialogue.Stop();
            if (inspect != null) inspect.SetMode(false);
            if (stampTray != null) stampTray.Close(true);
            CloseEnvelope();
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

        private void OnDestroy()
        {
            Unsubscribe();
            if (stampTray != null) // (Onur'un rehberi) ekran yok edilirken abonelik kalkar
            {
                stampTray.Stamped -= StampDecision;
                stampTray.OpenChanged -= OnStampTrayOpen;
            }
        }

        private void Unsubscribe()
        {
            if (context != null) context.StateChanged -= Redraw;
        }

        private void Select(string projectId)
        {
            selectedId = projectId;
            if (inspect != null) inspect.SetMode(false); // yeni müteahhit: inceleme modu kapanır, seçim temizlenir
            if (dialogue != null) dialogue.Stop(); // yeni müteahhit: diyalog sıfırlanır
            if (stampTray != null) stampTray.Close(true); // (belge turu B2) yeni müteahhit: kaşe çekmecesi kapalı
            CloseEnvelope();
            holdEnvelope = false;
            ClearStamp(); // (A, belge turu A1) yeni müteahhit: kaşe yok
            if (rulebook != null) rulebook.ShowProject(context.GetTodayProject(projectId)); // (A, poliş K1) kat grubu otomatik
            Redraw();
        }

        private void BuyExtraTest()
        {
            if (stamping || Inspecting) return;
            if (deskAudio != null) deskAudio.PlayClick();
            context.Actions.BuyExtraTest(selectedId);
            Redraw();
        }

        private void ClearStamp()
        {
            stampedId = null;
            formView.SetPendingStamp(ProjectVerdict.Pending);
            ClearGiven(); // (belge turu B1) yeni müteahhit: belgeler henüz verilmedi
        }

        // Karar Core'a gider (HandBack; kaşe izi ve sesi StampDecision'da); müteahhit tepki verir, sıradaki gelir.
        // Karar Core'da cam tamiri doğurduysa (kabadayı reddedildi) önce olay kartı.
        private void Decide(Action decision)
        {
            if (stamping || (dialogue != null && dialogue.IsPlaying)) return;
            if (Inspecting) inspect.SetMode(false);
            stamping = true;

            int repairsBefore = context.State.WindowRepairsToday;
            var project = context.GetTodayProject(selectedId);

            decision();
            Redraw();

            bool windowBroken = context.State.WindowRepairsToday > repairsBefore;
            StartCoroutine(AfterDecision(project, windowBroken));
        }

        private IEnumerator AfterDecision(ProjectData project, bool windowBroken)
        {
            yield return new WaitForSeconds(stampDelay);
            // (belge turu B1) Belgeler müteahhide verildiyse zaten masada değil. Hâlâ açık olan (geçici köprü: düğmeyle karar,
            // B2'de kalkar) kaşeli form ve lab raporu masadan kayıp gider.
            if (formWindow.IsOpen || labWindow.IsOpen)
            {
                formWindow.Close();
                labWindow.Close();
                PlayPaper();
            }

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
                var client = context.GetClient(project.ClientId);
                string name = client != null ? client.DisplayName : "Müteahhit";
                int cost = context.Balance != null ? context.Balance.WindowRepairCost : 0;
                var damage = PickThugDamage();
                if (deskAudio != null)
                {
                    if (damage.Sound == 1) deskAudio.PlayGlassBreak();
                    else if (damage.Sound == 2) deskAudio.PlayStamp(); // ahşap darbe
                }
                eventCard.Open(damage.Title,
                    string.Format(damage.Body, name) + "\n\n" +
                    $"Tamir: <b>{DeskFormat.Money(cost)}</b>. Bugünün giderlerine eklendi.",
                    AdvanceToNext);
                yield break;
            }

            AdvanceToNext();
        }

        // (EEE, 17 Ekim) Kabadayı (Rüstem) reddedilince her seferinde cam kırılmaz: 4 olaydan biri, bir öncekinden farklı.
        // Core'da para aynı (WindowRepairCost, gider satırı "Hasar tamiri"); sadece olay ve ses değişir. {0} = müteahhidin adı.
        // Sound: 0 sessiz, 1 cam kırılması, 2 ahşap darbe.
        private struct ThugDamage { public string Title, Body; public int Sound; }
        private static readonly ThugDamage[] ThugDamages =
        {
            new ThugDamage { Title = "Camın kırıldı!", Sound = 1,
                Body = "{0} kararını beğenmedi. Biraz sonra ofisinin camına bir taş geldi." },
            new ThugDamage { Title = "Masan kırıldı!", Sound = 2,
                Body = "{0} kalkarken masana öyle bir tekme attı ki bir ayağı çatladı. Masa artık sallanıyor." },
            new ThugDamage { Title = "Kapının kilidi kırıldı!", Sound = 2,
                Body = "{0} çıkarken kapıyı öyle bir çarptı ki kilit yerinden fırladı." },
            new ThugDamage { Title = "Arabanın lastikleri!", Sound = 0,
                Body = "Akşam otoparka indiğinde arabanın dört lastiğinin de indirildiğini gördün. Kimin yaptığını tahmin etmek zor değil: {0}." },
        };
        private int lastThugDamage = -1;

        private ThugDamage PickThugDamage()
        {
            int i = UnityEngine.Random.Range(0, ThugDamages.Length - (lastThugDamage >= 0 ? 1 : 0));
            if (lastThugDamage >= 0 && i >= lastThugDamage) i++; // bir öncekiyle aynı olmasın
            lastThugDamage = i;
            return ThugDamages[i];
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

        // İnceleme modu açıkken belgeler sürüklenmez, ZARF / EK TEST pasif (El Kitabı sekmeleri ve kat grubu çalışır).
        // Kaşe düğmesi modda da aydınlık ve basılabilir (B2, poliş K2 gibi): basınca mod kapanır, çekmece açılır (OnStampTrayOpen).
        // İnceleme ↔ açık çekmece birlikte olmaz: mod açılınca çekmece kapanır.
        private void OnInspectMode(bool on)
        {
            formWindow.Sheet.Locked = on;
            labWindow.Sheet.Locked = on;
            if (rulebook != null && rulebook.Window != null) rulebook.Window.Sheet.Locked = on;
            labView.SetLocked(on);
            if (stampTray != null && inspect != null)
            {
                if (on) stampTray.Close(false);
                InspectLift.Set(stampTray.gameObject, on, inspect.LiftOrder);
            }
            PlayClick();
        }

        // (belge turu B2) Kaşe çekmecesi açıkken İNCELE'deki hafif karartma gelir; kaşe alanı (parlar) ve çekmecenin bütün kökü
        // (sürüklenen kaşe kökteki DragLayer'da) karartmanın üstüne alınır, çekmece kaşe alanının da üstünde. Kapanınca geri.
        // İnceleme modu açıkken çekmece açılamaz (CanStamp); mod açılınca çekmece kapanır (OnInspectMode).
        private void OnStampTrayOpen(bool open)
        {
            if (open && Inspecting) inspect.SetMode(false);
            if (inspect != null)
            {
                inspect.SetFocus(open);
                if (StampArea != null) InspectLift.Set(StampArea.gameObject, open, inspect.LiftOrder);
                InspectLift.Set(stampTray.gameObject, open, inspect.LiftOrder + 1);
            }
            formView.SetStampFocus(open);
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
        // Rüşvet selamlamada söylenmez (Ahmet kararı 17 Ekim ~19:20): sadece ilk uyuşmazlıktan sonra diyalogda teklif edilir.
        private static string Greeting(ProjectData project, ClientSO client) =>
            !string.IsNullOrEmpty(project.GreetingText)
                ? project.GreetingText
                : DeskFormat.PickLine(client?.GreetingLines, project.Id);

        // Masadaki zarf: Core teklif ettiyse (BribeRevealed), dosya bekliyorsa ve diyalog bittiyse.
        // holdEnvelope: Inspect teklif edince Core ekranı hemen yeniden çizdirir; zarf diyalogdan önce düşmesin diye bekletilir.
        private bool BribeVisible(ProjectData project) =>
            project.Verdict == ProjectVerdict.Pending && project.BribeRevealed && !IsStamped // kaşeden sonra zarf masada değil
            && !holdEnvelope && !envelopeOpen && (dialogue == null || !dialogue.IsPlaying);

        // (A, Ahmet isteği) Zarfa tıklanınca: masadaki zarf kalkar, zarf ekranı öne gelir. Kabul → zarfla onay (ONAY kaşesi);
        // ret → zarf masadaki yerine döner, dosya karar bekler (oyuncu sonra kaşe basar; zarfı tekrar açabilir).
        private bool envelopeOpen;

        private void OpenEnvelope()
        {
            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (bribeEnvelope == null || project == null || envelopeOpen || stamping || Inspecting || !BribeVisible(project)) return;
            var from = props.EnvelopeWorld;
            envelopeOpen = true;
            Redraw(); // masadaki zarf kalkar
            bribeEnvelope.Open(project.BribeAmount, from,
                // (belge turu D4) Kabul = zarf alınır + ONAY kaşesi forma kendiliğinden basılır; belgeleri oyuncu müteahhide verir.
                // Zarf teklif edildiği için HandBack zarfla onaylar (BribeRevealed).
                accept: () => { envelopeOpen = false; AcceptBribeStamp(); },
                decline: () => { envelopeOpen = false; Redraw(); });
        }

        // Kaşe forma basılır: form kapalıysa önce ikonundan açılır (CanStamp formun açık olmasını ister); çekmece açıksa kapanır.
        private void AcceptBribeStamp()
        {
            if (stampTray != null) stampTray.Close(true);
            if (!formWindow.IsOpen) OpenDocument(formWindow, props.FormIcon);
            StampDecision(true);
            if (!IsStamped) Redraw(); // basılamadıysa (beklenmez) zarf masaya geri dönsün
        }

        private void CloseEnvelope()
        {
            envelopeOpen = false;
            if (bribeEnvelope != null) bribeEnvelope.Close(instant: true);
        }

        // İncelemenin kararı ve diyaloğu Core'da (memory.md "💬"): kaşe + sırayla satırlar + bu çağrıda teklif edildi mi.
        private InspectOutcome Inspect(InspectTarget a, InspectTarget b)
        {
            holdEnvelope = true;
            return context.Actions.Inspect(selectedId, a, b,
                rulebook != null ? rulebook.SelectedFloorGroup : Inspection.NoFloorGroup);
        }

        // Kaşe gösterildi. Satır varsa (uyuşmazlık, bu alan çifti ilk kez) diyalog; teklif olduysa diyalog bitince zarf düşer.
        private void OnInspected(InspectOutcome outcome)
        {
            var lines = new List<DeskDialogue.Line>();
            if (outcome != null)
                foreach (var line in outcome.Lines)
                    lines.Add(new DeskDialogue.Line(line.Speaker == DialogueSpeaker.Inspector, line.Text));

            // (EEE) Zarf teklif edildiyse diyalog bitince inceleme modu kendiliğinden kapanır: modda ZARF pasif olduğu için
            // oyuncu zarfa tıklayamıyordu.
            bool bribeOffered = outcome != null && outcome.BribeOffered;
            System.Action done = () =>
            {
                if (bribeOffered && inspect != null) inspect.SetMode(false);
                ReleaseEnvelope();
            };

            if (lines.Count == 0 || dialogue == null)
            {
                done();
                return;
            }
            dialogue.Play(lines, done);
        }

        private void ReleaseEnvelope()
        {
            holdEnvelope = false;
            Redraw();
        }

        private bool CanBuyExtraTest(ProjectData project) =>
            project.Verdict == ProjectVerdict.Pending
            && !project.ExtraTestBought
            && context.Balance != null
            && context.State.Money >= context.Balance.ExtraTestCost;

        private void Redraw()
        {
            if (context == null) return;

            if (header != null) header.Refresh(context);

            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (project == null)
            {
                scene.Leave(); // son müteahhit sola çıkar, koltuk boş kalır
                props.ShowEmpty(AllDecided());
                if (header != null) header.SetDayDone(AllDecided());
                SyncDocuments(null);
                if (inspect != null) inspect.SetAvailable(false);
                if (stampTray != null && stampTray.gameObject.activeSelf) stampTray.SetVisible(false); // müteahhit yok: çekmece yok
                if (AllDecided() && !completed && !endingShift && !stamping) StartCoroutine(EndShift());
                return;
            }

            var client = context.GetClient(project.ClientId);
            scene.Seat(project.Id, client, Greeting(project, client));
            bool bribeVisible = BribeVisible(project);
            props.Show(project, client, bribeVisible);
            if (header != null) header.SetClient(client != null ? client.DisplayName : "Başvuru sahibi", project.Name);
            formView.Show(context, project);
            labView.Show(context, project, CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0);
            SyncDocuments(project);
            if (inspect != null) inspect.SetAvailable(!stamping && !IsStamped && project.Verdict == ProjectVerdict.Pending);
            // (belge turu B2) Müteahhit varken kaşe düğmesi masada; basılabilir mi (form açık, kaşe yok...) CanStamp'e göre kendisi bakar.
            if (stampTray != null && !stampTray.gameObject.activeSelf) stampTray.SetVisible(true);
        }
    }
}
