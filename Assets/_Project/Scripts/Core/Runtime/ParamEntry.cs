using System;

namespace CivilJam.Core
{
    /// <summary>Tek bir yapı parametresi. Oyuncu Reported'ı görür; Actual sadece ek testle açılır.</summary>
    [Serializable]
    public class ParamEntry
    {
        public ParamType Type;
        public float Required; // Yönetmelik değeri
        public float Reported; // Belgede yazan değer
        public float Actual;   // Gerçek değer (gizli)
        public bool Tested;    // Ek test yapıldı mı (Actual görünür mü)

        /// <summary>Actual/Required. 1 ve üstü yeterli.</summary>
        public float Ratio => Required <= 0f ? 1f : Actual / Required;

        public bool ReportedPasses => Reported >= Required;
        public bool ActualPasses => Actual >= Required;
    }
}
