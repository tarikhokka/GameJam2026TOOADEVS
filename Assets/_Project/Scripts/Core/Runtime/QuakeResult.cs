using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Gün sonu afet kontrolünün sonucu. Flow bunu sadece gösterir.</summary>
    [Serializable]
    public class QuakeResult
    {
        public int Day;
        public bool Occurred;          // false ise DaySummary gösterilir
        public float Intensity;        // Oyuncuya GÖSTERİLMEZ (debug için)
        public List<BuildingOutcome> Outcomes = new List<BuildingOutcome>();

        // Hapis zarı (yıkım varsa atılır)
        public float JailRiskBefore;
        public float JailRiskAfter;
        public bool JailRolled;
        public bool Jailed;

        public int CollapsedCount => Outcomes.FindAll(o => o.Collapsed).Count;
    }

    [Serializable]
    public class BuildingOutcome
    {
        public string ProjectId;
        public string BuildingName;
        public bool Collapsed;
        public bool WasBribed;
        public float Strength;
        /// <summary>Actual &lt; Required olan parametreler; yıkım nedeni cümlesi bunlardan üretilir.</summary>
        public List<ParamEntry> FailedParams = new List<ParamEntry>();
    }
}
