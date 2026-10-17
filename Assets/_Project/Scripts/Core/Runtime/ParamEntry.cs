using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Tek bir yapı parametresi. Oyuncu Reported'ı görür; Actual sadece ek testle açılır.
    /// Required masada GÖSTERİLMEZ (Ç1): oyuncu Denetçi El Kitabı'nda bulur.
    /// Seçenekli parametrede (Labeled) değerler RuleSetSO etiketlerindeki sıra numarasıdır (ör. Nervürlü = 1); ekrana RuleSetSO.FormatValue ile yazılır.
    /// </summary>
    [Serializable]
    public class ParamEntry
    {
        public ParamType Type;
        public float Required; // Yönetmelik değeri (kat grubuna göre)
        public float Reported; // Belgede yazan değer
        public float Actual;   // Gerçek değer (gizli)
        public bool Tested;    // Ek test yapıldı mı (Actual görünür mü)
        public bool Labeled;   // Seçenekli parametre mi (değerler seçenek sıra numarası)

        /// <summary>Seçenekli parametrede her eksik kademenin dayanımdan düşürdüğü pay (denge simülasyonuyla seçildi: tek zarf ~yazı-tura).</summary>
        public const float LabelStepPenalty = 0.25f;

        /// <summary>
        /// Dayanıma katkı: 1 ve üstü yeterli. Seçenekli parametrede her eksik kademe %25 düşürür
        /// (ör. Düz demir = 0,75; iki kademe düşük beton = 0,50); fazla kademe 1'in üstüne çıkar.
        /// </summary>
        public float Ratio
        {
            get
            {
                if (Labeled) return 1f - LabelStepPenalty * (Required - Actual);
                return Required <= 0f ? 1f : Actual / Required;
            }
        }

        public bool ReportedPasses => Reported >= Required;
        public bool ActualPasses => Actual >= Required;
    }
}
