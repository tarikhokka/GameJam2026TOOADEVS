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
    // (A, belge turu A1 — memory.md "📂 BELGE TURU", D1) Karar iki adım: StampDecision (kaşe forma basılır, değiştirilemez, Core'a
    // gitmez) → HandBack (belgeler müteahhide verildi: karar Core'a gider, AfterDecision sırası). Geçici köprü: ONAY / RED ve zarf
    // kabulü ikisini arka arkaya çağırır; B2'de kaşe bölmesi StampDecision'ı, belge iadesi HandBack'i çağırır.
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

        /// <summary>Kaşe basılabilir mi: dosya bekliyor, form açık, kaşe yok; karar sırası, diyalog, zarf ekranı ve inceleme modu yok.</summary>
        public bool CanStamp
        {
            get
            {
                var project = selectedId != null && context != null ? context.GetTodayProject(selectedId) : null;
                return project != null && project.Verdict == ProjectVerdict.Pending && formWindow.IsOpen && !IsStamped
                    && !stamping && !envelopeOpen && !Inspecting && (dialogue == null || !dialogue.IsPlaying);
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
                Decide(() => context.Actions.Approve(id, acceptBribe: p != null && p.BribeRevealed), stampNow: false);
            }
            else Decide(() => context.Actions.Reject(id), stampNow: false);
            return true;
        }

        // Geçici köprü (B2'de kalkar): eski ONAY / RED düğmesi = kaşe + hemen iade.
        private void StampAndHandBack(bool approve)
        {
            StampDecision(approve);
            if (IsStamped) HandBack();
        }


        private void Awake()
        {
            // (Tarık, Ahmet kararı 18 Ekim ~03:10) Zarf teklif edildiyse ONAY = zarfla onay; zarfsız onay yok.
            // Teklif edilmemiş zarf yine alınamaz (Core BribeRevealed değilse kabul etmez) — HandBack'te.
            // (A, belge turu A1) Geçici köprü: düğmeler kaşe + hemen iade; B2'de düğmeler kalkar.
            formView.ApproveClicked += () => StampAndHandBack(true);
            formView.TakeBribeClicked += () => StampAndHandBack(true);
            formView.RejectClicked += () => StampAndHandBack(false);
            labView.ExtraTestClicked += BuyExtraTest;

            props.FinishClicked += Finish;
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

        private void OnDestroy() => Unsubscribe();

        private void Unsubscribe()
        {
            if (context != null) context.StateChanged -= Redraw;
        }

        private void Select(string projectId)
        {
            selectedId = projectId;
            if (inspect != null) inspect.SetMode(false); // yeni müteahhit: inceleme modu kapanır, seçim temizlenir
            if (dialogue != null) dialogue.Stop(); // yeni müteahhit: diyalog sıfırlanır
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
        }

        // Karar Core'a gider (HandBack); sonra form ve lab raporu kalkar, müteahhit tepki verir, sıradaki gelir.
        // Karar Core'da cam tamiri doğurduysa (kabadayı reddedildi) önce olay kartı. stampNow: kaşe animasyonu ve sesi burada mı
        // (belge turunda kaşe StampDecision'da basıldı → false).
        private void Decide(Action decision, bool stampNow = true)
        {
            if (stamping || (dialogue != null && dialogue.IsPlaying)) return;
            if (Inspecting) inspect.SetMode(false); // poliş K2: inceleme modunda ONAY / RED → mod kapanır, karar bugünkü gibi
            stamping = true;

            int repairsBefore = context.State.WindowRepairsToday;
            var project = context.GetTodayProject(selectedId);

            decision();
            Redraw();
            if (stampNow)
            {
                formView.AnimateDecisionStamp();
                if (deskAudio != null) deskAudio.PlayStamp();
            }

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
        // Rüşvet selamlamada söylenmez (Ahmet kararı 17 Ekim ~19:20): sadece ilk uyuşmazlıktan sonra diyalogda teklif edilir.
        private static string Greeting(ProjectData project, ClientSO client) =>
            !string.IsNullOrEmpty(project.GreetingText)
                ? project.GreetingText
                : DeskFormat.PickLine(client?.GreetingLines, project.Id);

        // Zarf ve "ZARFI AL VE ONAYLA": Core teklif ettiyse (BribeRevealed), dosya bekliyorsa ve diyalog bittiyse.
        // holdEnvelope: Inspect teklif edince Core ekranı hemen yeniden çizdirir; zarf diyalogdan önce düşmesin diye bekletilir.
        private bool BribeVisible(ProjectData project) =>
            project.Verdict == ProjectVerdict.Pending && project.BribeRevealed
            && !holdEnvelope && !envelopeOpen && (dialogue == null || !dialogue.IsPlaying);

        // (A, Ahmet isteği) Zarfa tıklanınca: masadaki zarf kalkar, zarf ekranı öne gelir. Kabul → zarfla onay (karar kaşesi);
        // ret → zarf masadaki yerine döner, dosya karar bekler (oyuncu sonra ONAY / RED verir; zarfı tekrar açabilir).
        private bool envelopeOpen;

        private void OpenEnvelope()
        {
            var project = selectedId != null ? context.GetTodayProject(selectedId) : null;
            if (bribeEnvelope == null || project == null || envelopeOpen || stamping || Inspecting || !BribeVisible(project)) return;
            var from = props.EnvelopeWorld;
            envelopeOpen = true;
            Redraw(); // masadaki zarf kalkar
            bribeEnvelope.Open(project.BribeAmount, from,
                // (A, belge turu D4) Kabul = zarf alınır + ONAY kaşesi kendiliğinden basılır. Geçici köprü: hemen iade (B2'de kalkar,
                // oyuncu belgeleri müteahhide verir). Zarf teklif edildiği için HandBack zarfla onaylar (BribeRevealed).
                accept: () => { envelopeOpen = false; StampAndHandBack(true); },
                decline: () => { envelopeOpen = false; Redraw(); });
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

            // (EEE) Zarf teklif edildiyse diyalog bitince inceleme modu kendiliğinden kapanır: modda ONAY / RED / ZARF
            // pasif olduğu için oyuncu zarfa tıklayamıyordu.
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
                if (AllDecided() && !completed && !endingShift && !stamping) StartCoroutine(EndShift());
                return;
            }

            var client = context.GetClient(project.ClientId);
            scene.Seat(project.Id, client, Greeting(project, client));
            bool bribeVisible = BribeVisible(project);
            props.Show(project, client, bribeVisible);
            if (header != null) header.SetClient(client != null ? client.DisplayName : "Başvuru sahibi", project.Name);
            formView.Show(context, project, bribeVisible);
            labView.Show(context, project, CanBuyExtraTest(project),
                context.Balance != null ? context.Balance.ExtraTestCost : 0);
            SyncDocuments(project);
            if (inspect != null) inspect.SetAvailable(!stamping && !IsStamped && project.Verdict == ProjectVerdict.Pending);
        }
    }
}
