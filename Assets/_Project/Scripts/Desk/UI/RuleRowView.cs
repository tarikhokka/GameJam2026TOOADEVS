using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // El kitabında numaralı kural: "1.1" + metin (metin alt satıra sarabilir).
    public class RuleRowView : RowView
    {
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text bodyText;

        public override void Bind(string number, string body)
        {
            numberText.text = number;
            bodyText.text = body;
        }
    }
}
