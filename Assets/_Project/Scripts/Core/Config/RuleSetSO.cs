using System;
using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Yönetmelik: bina türüne göre hangi parametre hangi değeri istiyor. Masadaki yönetmelik kitapçığı da buradan okunur.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Rule Set", fileName = "RuleSet")]
    public class RuleSetSO : ScriptableObject
    {
        public List<ParamRule> Rules = new List<ParamRule>();

        public IEnumerable<ParamRule> RulesFor(BuildingType type)
        {
            foreach (var r in Rules)
                if (r.BuildingType == type) yield return r;
        }
    }

    [Serializable]
    public class ParamRule
    {
        public BuildingType BuildingType;
        public ParamType Param;
        public string DisplayName;   // Ör. "Beton sınıfı"
        public string Unit;          // Ör. "MPa"
        public float Required;
        [Tooltip("Uygunsuz projede Actual = Required * bu aralıktan bir oran")]
        public Vector2 FailRatioRange = new Vector2(0.55f, 0.9f);
    }
}
