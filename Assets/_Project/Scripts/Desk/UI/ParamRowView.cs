using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Rapor tablosunda tek satır: parametre | beyan | ek test.
    // Gerekli değer masada yazmaz, test sonucu renklenmez (Ç1): oyuncu El Kitabı ile karşılaştırır.
    public class ParamRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text reportedText;
        [SerializeField] private TMP_Text actualText;

        [SerializeField] private Color testedColor = new Color(0.17f, 0.17f, 0.17f);
        [SerializeField] private Color hiddenColor = new Color(0.55f, 0.55f, 0.55f);

        // actual null ise ek test yapılmamıştır ("?").
        public void Bind(string displayName, string reported, string actual)
        {
            nameText.text = displayName;
            reportedText.text = reported;
            actualText.text = actual ?? "?";
            actualText.color = actual != null ? testedColor : hiddenColor;
        }
    }
}
