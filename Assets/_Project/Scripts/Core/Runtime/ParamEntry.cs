using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Tek bir yapı parametresi. Oyuncu Reported'ı görür; Actual sadece ek testle açılır.
    /// Required masada GÖSTERİLMEZ (Ç1): oyuncu Denetçi El Kitabı'nda bulur.
    /// Harfli parametrede (Labeled) değerler RuleSetSO etiketlerindeki sıra numarasıdır (ör. C25 = 2); ekrana RuleSetSO.FormatValue ile yazılır.
    /// </summary>
    [Serializable]
    public class ParamEntry
    {
        public ParamType Type;
        public float Required; // Yönetmelik değeri (bölge + kata göre)
        public float Reported; // Belgede yazan değer
        public float Actual;   // Gerçek değer (gizli)
        public bool Tested;    // Ek test yapıldı mı (Actual görünür mü)
        public bool Labeled;   // Harfli parametre mi (değerler etiket sıra numarası)

        /// <summary>
        /// Dayanıma katkı: 1 ve üstü yeterli. Harfli parametrede kademe farkı (sıra+1)/(gerekli+1) ile ölçülür
        /// (ör. C25 yerine C30 istenmişse 3/4 = 0,75).
        /// </summary>
        public float Ratio
        {
            get
            {
                if (Labeled) return (Actual + 1f) / (Required + 1f);
                return Required <= 0f ? 1f : Actual / Required;
            }
        }

        public bool ReportedPasses => Reported >= Required;
        public bool ActualPasses => Actual >= Required;
    }
}
