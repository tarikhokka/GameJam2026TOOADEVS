using System;
using System.Collections.Generic;
using System.Globalization;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Yapı Denetim Formu (Documents[0]; Ahmet'in tasarımı Docs/Tasarim/yapi_denetim_formu_GORUNUM.html, içerik El Kitabı v2):
    // dosya sekmesi, künye, bölümler (ParamRule.Section: "A · YAPI", "B · ZEMİN") ve beyan edilen değerler, imza + belediye kaşesi,
    // ONAY / RED kutuları ve (varsa) zarf. Tutarsızlıkları işaretlemez: oyuncu Lab Raporu ve El Kitabı ile karşılaştırır.
    public class FormView : MonoBehaviour
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        [Header("Başlık")]
        [SerializeField] private TMP_Text fileTabText;
        [SerializeField] private TMP_Text dateText;

        [Header("Künye")]
        [SerializeField] private Transform wideRowsContainer; // tam genişlik: proje, adres
        [SerializeField] private DottedRowView wideRowTemplate;
        [SerializeField] private Transform gridRowsContainer; // iki sütun: kullanım, parsel, kat, yükseklik, kişi, ücret
        [SerializeField] private DottedRowView gridRowTemplate;

        [Header("Bölümler")]
        [SerializeField] private Transform sectionsContainer;
        [SerializeField] private SectionBlockView sectionTemplate;

        [Header("İmza ve kaşe")]
        [SerializeField] private TMP_Text signatureText;
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

        private readonly List<DottedRowView> wideRows = new List<DottedRowView>();
        private readonly List<DottedRowView> gridRows = new List<DottedRowView>();
        private readonly List<SectionBlockView> sections = new List<SectionBlockView>();

        private void Awake()
        {
            approveButton.onClick.AddListener(() => ApproveClicked?.Invoke());
            rejectButton.onClick.AddListener(() => RejectClicked?.Invoke());
            takeBribeButton.onClick.AddListener(() => TakeBribeClicked?.Invoke());
        }

        public void AnimateDecisionStamp() => decisionStamp.Animate();

        public void Show(GameContext ctx, ProjectData project)
        {
            var doc = project.Documents.Count > 0 ? project.Documents[0] : null;

            fileTabText.text = $"DOSYA · {project.FileNo}";
            dateText.text = doc != null ? doc.Date : "";

            Fill(wideRows, wideRowTemplate, wideRowsContainer, new[]
            {
                ("Proje", Upper(project.Name)),
                ("Adres", Upper(project.Address)),
            });
            Fill(gridRows, gridRowTemplate, gridRowsContainer, new[]
            {
                ("Kullanım", Upper(DeskFormat.BuildingName(project.BuildingType))),
                ("Parsel no", doc != null ? doc.ParcelNo : ""),
                ("Kat sayısı", project.Floors.ToString(Tr)),
                ("Yükseklik", $"{project.HeightMeters.ToString("0.00", Tr)} M"),
                ("Kat başı kişi", project.PeoplePerFloor.ToString(Tr)),
                ("Onay ücreti", Upper(DeskFormat.Money(project.Fee))),
            });

            BindSections(ctx, project);

            signatureText.text = doc != null ? $"İmza: <i>{doc.SignatureName}</i>" : "";
            bool hasStamp = doc != null && !string.IsNullOrEmpty(doc.StampId);
            municipalStamp.SetActive(hasStamp);
            if (hasStamp) municipalStampText.text = $"ONAYLIDIR\n<size=60%>{doc.StampId}</size>";

            bool pending = project.Verdict == ProjectVerdict.Pending;
            approveButton.interactable = pending;
            rejectButton.interactable = pending;
            bribeRoot.SetActive(pending && project.OffersBribe);
            if (project.OffersBribe) bribeLabel.text = $"ZARFI AL VE ONAYLA · {Upper(DeskFormat.Money(project.BribeAmount))}";
            decisionStamp.Bind(project.Verdict);
        }

        // Parametreleri bölümlerine göre toplar (bölüm El Kitabı v2'de kurala bağlı); sıra lab raporuyla aynı.
        private void BindSections(GameContext ctx, ProjectData project)
        {
            var order = new List<string>();
            var lines = new Dictionary<string, List<(string, string)>>();
            foreach (var p in project.Params)
            {
                var rule = ctx.Rules != null ? ctx.Rules.FindRule(project.BuildingType, p.Type, project.Floors) : null;
                string section = rule != null && !string.IsNullOrWhiteSpace(rule.Section) ? rule.Section : "A · YAPI";
                if (!lines.TryGetValue(section, out var list))
                {
                    list = new List<(string, string)>();
                    lines[section] = list;
                    order.Add(section);
                }
                list.Add((DeskFormat.ParamName(ctx.Rules, project.BuildingType, p.Type),
                    Upper(DeskFormat.Value(ctx.Rules, p.Type, p.Reported))));
            }
            order.Sort(StringComparer.Ordinal);

            UiPool.Resize(sections, sectionTemplate, sectionsContainer, order.Count);
            for (int i = 0; i < order.Count; i++) sections[i].Bind(order[i], lines[order[i]]);
        }

        private static void Fill(List<DottedRowView> rows, DottedRowView template, Transform container, (string, string)[] lines)
        {
            UiPool.Resize(rows, template, container, lines.Length);
            for (int i = 0; i < lines.Length; i++) rows[i].Bind(lines[i].Item1, lines[i].Item2);
        }

        private static string Upper(string s) => string.IsNullOrEmpty(s) ? "" : s.ToUpper(Tr);
    }
}
