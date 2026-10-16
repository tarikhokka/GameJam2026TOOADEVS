using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Masaya gelen tek belge. Desk bu alanları gösterir; tutarsızlıklar bedava ipucudur.</summary>
    [Serializable]
    public class DocumentData
    {
        public DocumentKind Kind;
        public string Title;
        public string ParcelNo;
        public string Date;          // Gösterim metni, ör. "12.10.2026"
        public string LabName;       // Sadece LabReport
        public string StampId;       // Kaşe görseli/kimliği
        public string SignatureName;
        // Parametre değerleri ProjectData.Params'tadır; LabReport onları Reported olarak gösterir.

        /// <summary>Bu belgede kasıtlı konmuş tutarsızlıklar. Desk göstermez; sonuç metni ve debug için.</summary>
        public List<ClueType> Clues = new List<ClueType>();
    }
}
