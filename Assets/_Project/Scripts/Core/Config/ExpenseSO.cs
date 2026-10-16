using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Haftalık gider kalemi (kira, kredi, sigorta...).</summary>
    [CreateAssetMenu(menuName = "CivilJam/Expense", fileName = "Expense")]
    public class ExpenseSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public int Amount;
        [Tooltip("Ödenmezse borca eklenir")]
        public bool Mandatory = true;
    }
}
