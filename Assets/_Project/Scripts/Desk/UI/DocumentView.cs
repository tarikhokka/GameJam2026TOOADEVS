using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Ortadaki kağıt: seçili projenin belge sekmeleri, künye bilgileri, kaşe ve parametre tablosu.
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

        public void Show(DeskProject project, int documentIndex, bool revealActual)
        {
            emptyState.SetActive(false);
            paperRoot.SetActive(true);

            BindTabs(project, documentIndex);

            var doc = project.Documents[documentIndex];
            titleText.text = doc.Title;
            metaText.text = BuildMeta(project, doc);
            stampRoot.SetActive(doc.HasStamp);

            paramHeader.SetActive(doc.Params.Count > 0);
            UiPool.Resize(rows, paramRowTemplate, paramContainer, doc.Params.Count);
            for (int i = 0; i < doc.Params.Count; i++)
                rows[i].Bind(doc.Params[i], revealActual);
        }

        private void BindTabs(DeskProject project, int selectedIndex)
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

        private static string BuildMeta(DeskProject project, DeskDocument doc)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>Proje:</b> {project.Name}");
            sb.AppendLine($"<b>Parsel:</b> {doc.ParcelNo}");
            sb.AppendLine($"<b>Tarih:</b> {doc.Date}");
            sb.AppendLine($"<b>Düzenleyen:</b> {doc.Issuer}");
            sb.AppendLine($"<b>İmza:</b> <i>{doc.Signature}</i>");
            foreach (var line in doc.Lines)
                sb.AppendLine(line);
            return sb.ToString().TrimEnd();
        }
    }
}
