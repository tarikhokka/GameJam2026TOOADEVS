using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Müteahhit / müşteri. İsimler tamamen kurgusal olmalı.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Client", fileName = "Client")]
    public class ClientSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Portrait;
        [Tooltip("Dürüst usta: rüşvet teklif etmez, sahte belge getirmez")]
        public bool IsHonest;
        public bool IsThug;          // Reddedilirse cam kırar
        public Vector2Int BribeRange = new Vector2Int(500, 2000);

        [Header("Replikler")]
        [TextArea] public List<string> GreetingLines = new List<string>();
        [TextArea] public List<string> BribeLines = new List<string>();

        [Header("Tepkiler (Ç4) — karardan sonra; Core seçer, ProjectData.ReactionText'e yazar")]
        [TextArea] public List<string> ApproveLines = new List<string>();      // Onaylanınca (teşekkür)
        [TextArea] public List<string> RejectLines = new List<string>();       // Reddedilince (söylenme / kavga)
        [TextArea] public List<string> BribeRefusedLines = new List<string>(); // Zarf geri çevrilip reddedilince
        [Tooltip("Daha önce geldiyse selamlama yerine; {last} = \"onaylamıştınız\" / \"reddetmiştiniz\" yerine geçer")]
        [TextArea] public List<string> ReturnLines = new List<string>();

        [Header("Hediye (Ç4) — sadece kabul edilebilir, para/risk etkisi yok")]
        public GiftSO Gift;
        [Range(0f, 1f)] public float GiftChance = 0.3f;   // Onaylanan projesi başına

        [Header("Etkiler (Ç4)")]
        [Tooltip("Her reddedilişte sonraki zarf bu kadar büyür (Nuri)")]
        public int BribeRaiseOnReject;

        /// <summary>Rüşvet teklif eder mi: dürüst değil ve BribeRange 0-0 değil. (Selim: 0-0 → asla teklif etmez.)</summary>
        public bool OffersBribes => !IsHonest && BribeRange.y > 0;
    }
}
