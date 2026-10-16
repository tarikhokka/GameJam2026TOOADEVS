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
        [Tooltip("Dürüst usta: rüşvet teklif etmez, projeleri uygun gelir")]
        public bool IsHonest;
        public bool IsThug;          // Reddedilirse cam kırar
        public Vector2Int BribeRange = new Vector2Int(500, 2000);
        [TextArea] public List<string> GreetingLines = new List<string>();
        [TextArea] public List<string> BribeLines = new List<string>();
    }
}
