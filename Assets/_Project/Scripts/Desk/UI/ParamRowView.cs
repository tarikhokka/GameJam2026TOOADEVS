using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Rapor tablosunda tek satır: parametre | gerekli | beyan | test.
    public class ParamRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text requiredText;
        [SerializeField] private TMP_Text reportedText;
        [SerializeField] private TMP_Text actualText;

        [SerializeField] private Color okColor = new Color(0.18f, 0.5f, 0.22f);
        [SerializeField] private Color failColor = new Color(0.75f, 0.15f, 0.12f);
        [SerializeField] private Color hiddenColor = new Color(0.55f, 0.55f, 0.55f);

        public void Bind(string displayName, string unit, ParamEntry param)
        {
            string suffix = string.IsNullOrEmpty(unit) ? "" : " " + unit;

            nameText.text = displayName;
            requiredText.text = $"min. {DeskFormat.Number(param.Required)}{suffix}";
            reportedText.text = $"{DeskFormat.Number(param.Reported)}{suffix}";

            if (param.Tested)
            {
                actualText.text = $"{DeskFormat.Number(param.Actual)}{suffix}";
                actualText.color = param.ActualPasses ? okColor : failColor;
            }
            else
            {
                actualText.text = "?";
                actualText.color = hiddenColor;
            }
        }
    }
}
