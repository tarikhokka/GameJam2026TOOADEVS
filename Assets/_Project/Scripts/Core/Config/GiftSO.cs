using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Ofis hediyesi (Ç4): belirli bir müteahhit, projesi onaylanınca bazen verir (saat, tablo, masa isimliği...).
    /// Para ya da risk etkisi yoktur; kabul edilince ofiste (masa ekranında) kalıcı olarak görünür. Geri çevrilemez.
    /// Asset'ler: Assets/_Project/Data/Gifts/ (EEE). Hangi müteahhidin vereceği: ClientSO.Gift.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Gift", fileName = "Gift")]
    public class GiftSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;          // Ör. "Duvar saati"
        public Sprite Sprite;               // Ofiste görünen görsel
        [TextArea] public string Note;      // Hediye kartındaki not, ör. "Rıfat Usta'dan, emeğinize..."
    }
}
