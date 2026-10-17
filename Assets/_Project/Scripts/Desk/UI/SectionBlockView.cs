using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Kağıtta bölüm: kutulu harf + başlık ("[A] YAPI") ve altında satırlar (şablondan çoğaltılır).
    public class SectionBlockView : MonoBehaviour
    {
        [SerializeField] private TMP_Text letterText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Transform rowsContainer;
        [SerializeField] private RowView rowTemplate;

        private readonly List<RowView> rows = new List<RowView>();

        // section: Core'daki ParamRule.Section, ör. "A · YAPI" → harf "A", başlık "YAPI".
        public void Bind(string section, IReadOnlyList<(string first, string second)> lines)
        {
            Split(section, out string letter, out string title);
            letterText.text = letter;
            titleText.text = title;
            UiPool.Resize(rows, rowTemplate, rowsContainer, lines.Count);
            for (int i = 0; i < lines.Count; i++) rows[i].Bind(lines[i].first, lines[i].second);
        }

        public static void Split(string section, out string letter, out string title)
        {
            section = string.IsNullOrWhiteSpace(section) ? "A · YAPI" : section.Trim();
            int dot = section.IndexOf('·');
            if (dot < 0)
            {
                letter = section.Substring(0, 1);
                title = section;
                return;
            }
            letter = section.Substring(0, dot).Trim();
            title = section.Substring(dot + 1).Trim();
        }
    }
}
