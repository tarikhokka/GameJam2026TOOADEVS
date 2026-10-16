using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Gazete manşet şablonları. Yer tutucular: {building}, {count}, {day}.
    /// Depremzedeler asla olumsuz gösterilmez; gerçek olaylara atıf yok.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Headlines", fileName = "Headlines")]
    public class HeadlineSO : ScriptableObject
    {
        [TextArea] public List<string> NoCollapse = new List<string>();
        [TextArea] public List<string> SingleCollapse = new List<string>();
        [TextArea] public List<string> MultiCollapse = new List<string>();
        [TextArea] public List<string> BribedCollapse = new List<string>();
    }
}
