using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // "Kolon kalınlığı ............ 35 CM": etiket solda, değer sağda (değer fontu), arası noktalı (builder kurar).
    public class DottedRowView : RowView
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        public override void Bind(string label, string value)
        {
            labelText.text = label;
            valueText.text = value;
        }
    }
}
