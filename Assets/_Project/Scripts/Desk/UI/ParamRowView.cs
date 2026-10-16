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

        public void Bind(DeskParam param, bool revealActual)
        {
            nameText.text = param.Name;
            requiredText.text = $"min. {DeskFormat.Number(param.Required)} {param.Unit}";
            reportedText.text = $"{DeskFormat.Number(param.Reported)} {param.Unit}";

            if (revealActual)
            {
                actualText.text = $"{DeskFormat.Number(param.Actual)} {param.Unit}";
                actualText.color = param.Actual >= param.Required ? okColor : failColor;
            }
            else
            {
                actualText.text = "?";
                actualText.color = hiddenColor;
            }
        }
    }
}
