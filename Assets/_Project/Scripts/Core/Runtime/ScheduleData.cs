using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Programdaki tek ziyaret: kim gelir, hangi tür proje getirir (tür gizli; masada gösterilmez).</summary>
    [Serializable]
    public class ScheduleSlot
    {
        public string ClientId;
        public ProjectKind Kind;
    }

    /// <summary>Bir günün ziyaretleri, geliş sırasıyla. GameState.Schedule'da kayıtla saklanır (memory.md "📅").</summary>
    [Serializable]
    public class ScheduledDay
    {
        public int Day;
        public List<ScheduleSlot> Slots = new List<ScheduleSlot>();
    }
}
