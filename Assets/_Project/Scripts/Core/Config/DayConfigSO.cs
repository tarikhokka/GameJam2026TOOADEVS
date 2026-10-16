using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Belli bir günden itibaren geçerli içerik ayarı. GameManager, Day >= FromDay olan en büyük config'i kullanır.</summary>
    [CreateAssetMenu(menuName = "CivilJam/Day Config", fileName = "DayConfig")]
    public class DayConfigSO : ScriptableObject
    {
        [Min(1)] public int FromDay = 1;

        [Header("Günlük projeler")]
        [Min(1)] public int ProjectsPerDay = 3;
        [Range(0f, 1f)] public float NonCompliantRatio = 0.4f;

        [Header("Açılan içerik")]
        public bool AllowBribes;
        public bool AllowFakeDocuments;
        public bool AllowThugs;      // Kabadayılar (cam tamiri gideri)
        public bool AllowFactories;  // Fabrika + yangın parametresi

        [Header("Afet şiddeti (gizli)")]
        [Range(0f, 1f)] public float IntensityMin = 0.5f;
        [Range(0f, 1f)] public float IntensityMax = 0.9f;
    }
}
