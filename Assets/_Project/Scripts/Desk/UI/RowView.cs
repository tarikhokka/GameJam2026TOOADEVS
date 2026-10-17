using UnityEngine;

namespace CivilJam.Desk
{
    // Belgelerde iki yazılık satır: form / lab raporunda "etiket : DEĞER" ve tablo satırı (DottedRowView).
    public abstract class RowView : MonoBehaviour
    {
        public abstract void Bind(string first, string second);
    }
}
