using System.Collections.Generic;
using System.Globalization;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Yapı Denetim Formu (Documents[0]), yeni hedef arayüz (Onur'un referansı, EEE görselleri; içerik El Kitabı v2):
    // başlık (proje no, tarih), PROJE BİLGİLERİ ("etiket : değer" satırları + kat sayısına göre bina resmi), TEKNİK BİLGİLER
    // tablosu (beyan edilen değerler), belediye kaşesi ve altta kaşe alanı (belge turu B2: ONAY / RED düğmeleri kalktı, karar
    // kaşe çekmecesinden kaşe buraya sürüklenerek verilir; StampTrayView → DeskScreen.StampDecision).
    // Tutarsızlıkları işaretlemez: oyuncu Lab Raporu ve El Kitabı ile karşılaştırır (parsel, tarih, mühendis imzası formda).
    public class FormView : MonoBehaviour
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        [Header("Başlık")]
        [SerializeField] private TMP_Text fileTabText; // "Proje : YD-2026-0412"
        [SerializeField] private TMP_Text dateText;    // "Tarih : 12.10.2026"

        [Header("Proje bilgileri")]
        [SerializeField] private Transform infoContainer;
        [SerializeField] private DottedRowView infoTemplate;
        [SerializeField] private Image buildingImage;
        [Tooltip("Kat sayısına göre bina resmi: 1-3, 4-6, 7+ (EEE: bld_low, bld_mid, bld_high)")]
        [SerializeField] private Sprite[] buildingSprites;

        [Header("Teknik bilgiler")]
        [SerializeField] private Transform techContainer;
        [SerializeField] private DottedRowView techTemplate;

        [Header("Kaşe")]
        [SerializeField] private GameObject municipalStamp;
        [SerializeField] private TMP_Text municipalStampText;

        [Header("Karar")]
        [SerializeField] private RectTransform stampArea; // kaşe alanı (eski ONAY / RED satırının yeri; boş, hafif çerçeveli)
        [SerializeField] private Image stampGlow;         // kaşe alanının ilk çocuğu: çekmece açıkken kağıt rengi zemin, hafif parlar
        [SerializeField] private DecisionStampView decisionStamp;

        private readonly List<DottedRowView> infoRows = new List<DottedRowView>();
        private readonly List<DottedRowView> techRows = new List<DottedRowView>();
        private ProjectVerdict pendingStamp = ProjectVerdict.Pending; // (A, belge turu A1) basılmış ama henüz verilmemiş kaşe

        // Kaşenin basılacağı alan: kaşe çekmecesi (StampTrayView) kaşeyi buraya bırakınca basılır.
        public RectTransform StampArea => stampArea;

        // Kaşe basıldı ama karar henüz Core'a gitmedi (belgeler müteahhide verilince gider, memory.md "📂 BELGE TURU" D1):
        // form bu kaşeyi gösterir. Pending = kaşe yok (yeni müteahhit).
        public void SetPendingStamp(ProjectVerdict verdict) => pendingStamp = verdict;

        public void AnimateDecisionStamp() => decisionStamp.Animate();

        // (belge turu B2) Kaşe çekmecesi açıkken kaşe alanı karartmanın üstüne alınır (DeskScreen); altındaki kağıt kararmış
        // kalmasın diye alan kağıt rengi bir zeminle aydınlanır ve sıcak bir tonla yavaşça nabız gibi parlar ("buraya bas").
        private static readonly Color GlowPaper = new Color(0.97f, 0.94f, 0.86f);
        private static readonly Color GlowWarm = new Color(1f, 0.9f, 0.62f);
        private const float GlowPeriod = 1.1f; // sn

        public void SetStampFocus(bool on)
        {
            if (stampGlow == null) return;
            stampGlow.gameObject.SetActive(on);
            if (on) stampGlow.color = GlowPaper;
        }

        private void Update()
        {
            if (stampGlow == null || !stampGlow.gameObject.activeSelf) return;
            float k = 0.5f - 0.5f * Mathf.Cos(Time.unscaledTime * 2f * Mathf.PI / GlowPeriod);
            stampGlow.color = Color.Lerp(GlowPaper, GlowWarm, 0.6f * k);
        }

        public void Show(GameContext ctx, ProjectData project)
        {
            var doc = project.Documents.Count > 0 ? project.Documents[0] : null;
            var client = ctx.GetClient(project.ClientId);

            fileTabText.text = $"Proje : YD-{project.FileNo}";
            dateText.text = $"Tarih : {(doc != null ? doc.Date : "")}";

            Fill(infoRows, infoTemplate, infoContainer, new[]
            {
                ("Proje Adı", project.Name),
                ("Ada / Parsel", doc != null ? doc.ParcelNo : ""),
                ("Adres", project.Address),
                ("Yapı Sınıfı", DeskFormat.BuildingName(project.BuildingType)),
                ("Kat Adedi", project.Floors.ToString(Tr)),
                ("Yükseklik", $"{project.HeightMeters.ToString("0.00", Tr)} m"),
                ("Kat başı kişi", project.PeoplePerFloor.ToString(Tr)),
                ("Müteahhit", client != null ? client.DisplayName : ""),
                ("Mühendis", doc != null ? doc.SignatureName : ""), // lab raporunda aynı başlık: iki belgede aynı kişi imzalar (sahtede farklı); kısa: etiket sütunu 150 px
                ("Onay ücreti", DeskFormat.Money(project.Fee)),
            });

            // İnceleme modunda tıklanabilir alanlar (memory.md "Uygulama notu" tablosu; başka hiçbir yer).
            InspectSpot.Mark(dateText, InspectTarget.Of(InspectDoc.Form, InspectField.Date));
            InspectSpot.Mark(infoRows[1], InspectTarget.Of(InspectDoc.Form, InspectField.Parcel));
            InspectSpot.Mark(infoRows[8], InspectTarget.Of(InspectDoc.Form, InspectField.Signature));

            int group = project.Floors <= 3 ? 0 : project.Floors <= 6 ? 1 : 2;
            var sprite = buildingSprites != null && group < buildingSprites.Length ? buildingSprites[group] : null;
            buildingImage.sprite = sprite;
            buildingImage.enabled = sprite != null;

            // Teknik bilgiler: beyan edilen değerler, lab raporuyla aynı sırada (Core ParamType sırası).
            var tech = new (string, string)[project.Params.Count];
            for (int i = 0; i < project.Params.Count; i++)
            {
                var p = project.Params[i];
                tech[i] = (DeskFormat.ParamName(ctx.Rules, project.BuildingType, p.Type), DeskFormat.Value(ctx.Rules, p.Type, p.Reported));
            }
            Fill(techRows, techTemplate, techContainer, tech);
            for (int i = 0; i < project.Params.Count; i++)
                InspectSpot.Mark(techRows[i], InspectTarget.OfParam(InspectDoc.Form, project.Params[i].Type));

            bool hasStamp = doc != null && !string.IsNullOrEmpty(doc.StampId);
            municipalStamp.SetActive(hasStamp);
            if (hasStamp && municipalStampText != null) municipalStampText.text = doc.StampId;

            decisionStamp.Bind(project.Verdict != ProjectVerdict.Pending ? project.Verdict : pendingStamp);
            DocTextBoost.Apply(transform, 1.4f); // (EEE) yazılar sığdığı kadar %40 büyük, koyu ve kalın
        }

        private static void Fill(List<DottedRowView> rows, DottedRowView template, Transform container, (string, string)[] lines)
        {
            UiPool.Resize(rows, template, container, lines.Length);
            for (int i = 0; i < lines.Length; i++) rows[i].Bind(lines[i].Item1, lines[i].Item2);
        }
    }
}
