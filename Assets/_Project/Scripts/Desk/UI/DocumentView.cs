using System;
using System.Collections.Generic;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Ortadaki kağıt: seçili projenin belge sekmeleri, künye bilgileri, kaşe ve (lab raporunda) parametre tablosu.
    // Tutarsızlıkları (bedava ipuçları) işaretlemez; oyuncu belgeleri karşılaştırarak bulur.
    public class DocumentView : MonoBehaviour
    {
        [Header("Sekmeler")]
        [SerializeField] private Transform tabsContainer;
        [SerializeField] private Button tabTemplate;
        [SerializeField] private Color tabColor = new Color(0.75f, 0.68f, 0.52f);
        [SerializeField] private Color tabSelectedColor = new Color(0.95f, 0.92f, 0.85f);

        [Header("Kağıt")]
        [SerializeField] private GameObject paperRoot;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text metaText;
        [SerializeField] private GameObject stampRoot;
        [SerializeField] private TMP_Text stampText;

        [Header("Tablo")]
        [SerializeField] private GameObject paramHeader;
        [SerializeField] private Transform paramContainer;
        [SerializeField] private ParamRowView paramRowTemplate;

        public event Action<int> TabSelected;

        private readonly List<Button> tabs = new List<Button>();
        private readonly List<ParamRowView> rows = new List<ParamRowView>();

        public void ShowEmpty()
        {
            UiPool.Resize(tabs, tabTemplate, tabsContainer, 0);
            paperRoot.SetActive(false);
            emptyState.SetActive(true);
        }

        public void Show(GameContext context, ProjectData project, int documentIndex)
        {
            emptyState.SetActive(false);
            paperRoot.SetActive(true);

            BindTabs(project, documentIndex);

            var doc = project.Documents[documentIndex];
            titleText.text = doc.Title;
            metaText.text = BuildMeta(project, doc);

            bool hasStamp = !string.IsNullOrEmpty(doc.StampId);
            stampRoot.SetActive(hasStamp);
            if (hasStamp) stampText.text = $"ONAYLIDIR\n<size=60%>{doc.StampId}</size>";

            // Parametreler projede tutulur; lab raporu onları "beyan" olarak gösterir.
            bool showParams = doc.Kind == DocumentKind.LabReport;
            int rowCount = showParams ? project.Params.Count : 0;
            paramHeader.SetActive(showParams);
            UiPool.Resize(rows, paramRowTemplate, paramContainer, rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                var param = project.Params[i];
                var (name, unit) = DeskFormat.Param(context.Rules, project.BuildingType, param.Type);
                rows[i].Bind(name, unit, param);
            }
        }

        private void BindTabs(ProjectData project, int selectedIndex)
        {
            UiPool.Resize(tabs, tabTemplate, tabsContainer, project.Documents.Count);
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                tabs[i].GetComponentInChildren<TMP_Text>().text = project.Documents[i].Title;
                tabs[i].image.color = i == selectedIndex ? tabSelectedColor : tabColor;
                tabs[i].onClick.RemoveAllListeners();
                tabs[i].onClick.AddListener(() => TabSelected?.Invoke(index));
            }
        }

        private static string BuildMeta(ProjectData project, DocumentData doc)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>Proje:</b> {project.Name}");
            sb.AppendLine($"<b>Parsel:</b> {doc.ParcelNo}");
            sb.AppendLine($"<b>Tarih:</b> {doc.Date}");

            if (doc.Kind == DocumentKind.Application)
            {
                sb.AppendLine($"<b>Adres:</b> {project.Address}");
                sb.AppendLine($"<b>Yapı:</b> {DeskFormat.BuildingName(project.BuildingType)} · {project.Floors} kat");
                sb.AppendLine($"<b>Onay ücreti:</b> {DeskFormat.Money(project.Fee)}");
            }
            else
            {
                sb.AppendLine($"<b>Laboratuvar:</b> {doc.LabName}");
            }

            sb.Append($"<b>İmza:</b> <i>{doc.SignatureName}</i>");
            return sb.ToString();
        }
    }
}
