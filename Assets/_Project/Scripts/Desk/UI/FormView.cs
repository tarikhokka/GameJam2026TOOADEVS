using System;
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
    // tablosu (beyan edilen değerler), belediye kaşesi, ONAY / RED düğmeleri (EEE görselleri) ve varsa zarf.
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
        [SerializeField] private Button approveButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private GameObject bribeRoot;
        [SerializeField] private Button takeBribeButton;
        [SerializeField] private TMP_Text bribeLabel;
        [SerializeField] private DecisionStampView decisionStamp;

        public event Action ApproveClicked;
        public event Action RejectClicked;
        public event Action TakeBribeClicked;

        private readonly List<DottedRowView> infoRows = new List<DottedRowView>();
        private readonly List<DottedRowView> techRows = new List<DottedRowView>();
        private bool pending, locked;

        private void Awake()
        {
            approveButton.onClick.AddListener(() => ApproveClicked?.Invoke());
            rejectButton.onClick.AddListener(() => RejectClicked?.Invoke());
            takeBribeButton.onClick.AddListener(() => TakeBribeClicked?.Invoke());
        }

        // İnceleme modu açıkken ZARF pasif. ONAY / RED açık kalır (poliş K2: modda basılınca mod kapanır, karar verilir).
        public void SetLocked(bool on)
        {
            locked = on;
            ApplyButtons();
        }

        private void ApplyButtons()
        {
            approveButton.interactable = pending;
            rejectButton.interactable = pending;
            takeBribeButton.interactable = pending && !locked;
        }

        public void AnimateDecisionStamp() => decisionStamp.Animate();

        // bribeVisible: "ZARFI AL VE ONAYLA" görünsün mü (DeskScreen: zarf teklif edildi ve diyalog bitti).
        public void Show(GameContext ctx, ProjectData project, bool bribeVisible)
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

            pending = project.Verdict == ProjectVerdict.Pending;
            ApplyButtons();
            // (A, Ahmet kararı) "ZARFI AL VE ONAYLA" kalktı: zarf masada tıklanarak açılır ve orada kabul / ret edilir.
            bribeRoot.SetActive(false);
            if (project.OffersBribe) bribeLabel.text = $"ZARFI AL VE ONAYLA · {DeskFormat.Money(project.BribeAmount)}";
            decisionStamp.Bind(project.Verdict);
            DocTextBoost.Apply(transform, 1.4f); // (EEE) yazılar sığdığı kadar %40 büyük, koyu ve kalın
        }

        private static void Fill(List<DottedRowView> rows, DottedRowView template, Transform container, (string, string)[] lines)
        {
            UiPool.Resize(rows, template, container, lines.Length);
            for (int i = 0; i < lines.Length; i++) rows[i].Bind(lines[i].Item1, lines[i].Item2);
        }
    }
}
