using UnityEngine;

namespace CivilJam.Desk
{
    // Kağıt pencerelerinde iki yazılık satır (Ahmet'in piksel tasarımı, Docs/Tasarim/*_GORUNUM.html):
    // noktalı satırda "etiket ........ DEĞER" (DottedRowView), el kitabında "1.1  kural metni" (RuleRowView).
    public abstract class RowView : MonoBehaviour
    {
        public abstract void Bind(string first, string second);
    }
}
