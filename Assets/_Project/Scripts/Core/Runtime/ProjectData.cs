using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Bir inşaat projesi. Onaylanınca GameState.Buildings listesine girer ve sonraki afetlerde de değerlendirilir.</summary>
    [Serializable]
    public class ProjectData
    {
        public string Id;
        public string Name;          // Ör. "Papatya Apartmanı"
        public string Address;
        public string ClientId;      // ClientSO.Id
        public BuildingType BuildingType;
        public int Floors;
        public int Fee;              // Onay ücreti (oyuncuya ödenir)
        public ProjectKind Kind;     // Gizli tür (oyuncuya gösterilmez)

        public List<ParamEntry> Params = new List<ParamEntry>();
        public List<DocumentData> Documents = new List<DocumentData>();

        // Rüşvet: teklif eden her proje uygunsuzdur.
        public bool OffersBribe;
        public int BribeAmount;

        // Oyuncu kararı
        public ProjectVerdict Verdict = ProjectVerdict.Pending;
        public bool BribeAccepted;
        public bool ExtraTestBought;
        public int DayDecided;

        // Yapıldıktan sonra
        public BuildingStatus Status = BuildingStatus.Standing;
        public int DayCollapsed;

        /// <summary>min(Actual/Required). >= 1 ise bina asla yıkılmaz.</summary>
        public float Strength
        {
            get
            {
                if (Params.Count == 0) return 1f;
                float min = float.MaxValue;
                foreach (var p in Params) min = Math.Min(min, p.Ratio);
                return min;
            }
        }

        /// <summary>Gerçekte yönetmeliğe uygun mu (gizli gerçek).</summary>
        public bool IsCompliant => Strength >= 1f;

        public ParamEntry GetParam(ParamType type) => Params.Find(p => p.Type == type);
    }
}
