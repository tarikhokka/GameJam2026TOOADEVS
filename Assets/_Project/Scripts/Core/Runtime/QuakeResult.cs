using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Gün sonu afet kontrolünün sonucu. Occurred = false ise gazete gösterilmez (DaySummary).
    /// Outcomes sadece son afetten bu yana onaylanan binaları içerir. Flow bunu sadece gösterir.
    /// </summary>
    [Serializable]
    public class QuakeResult
    {
        public int Day;
        public bool Occurred;
        public float Intensity;        // Oyuncuya GÖSTERİLMEZ (debug için)
        public float ChanceBefore;     // Debug
        public List<BuildingOutcome> Outcomes = new List<BuildingOutcome>();

        // Hapis zarı (yıkım varsa atılır)
        public float JailRiskBefore;
        public float JailRiskAfter;
        public bool JailRolled;
        public bool Jailed;

        public int CollapsedCount => Outcomes.FindAll(o => o.Collapsed).Count;

        public int TotalDeaths
        {
            get { int n = 0; foreach (var o in Outcomes) n += o.Deaths; return n; }
        }

        public int TotalInjured
        {
            get { int n = 0; foreach (var o in Outcomes) n += o.Injured; return n; }
        }
    }

    [Serializable]
    public class BuildingOutcome
    {
        public string ProjectId;
        public string BuildingName;
        public string Address;
        public int DayApproved;        // Oyuncunun imza attığı gün
        public bool Collapsed;
        public bool WasBribed;
        public float Strength;

        // Kayıplar (isimler tamamen kurgusal, NamePoolSO'dan)
        public int Residents;
        public int Deaths;
        public int Injured;
        public List<string> VictimNames = new List<string>();

        /// <summary>Oyuncunun imzasının sorumluluğu, ör. "Bu binanın onayında sizin imzanız vardı."</summary>
        public string ResponsibilityText;

        /// <summary>Actual &lt; Required olan parametreler; yıkım nedeni cümlesi bunlardan üretilir.</summary>
        public List<ParamEntry> FailedParams = new List<ParamEntry>();
    }
}
