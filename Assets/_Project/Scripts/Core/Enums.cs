namespace CivilJam.Core
{
    /// <summary>
    /// Oyun akışındaki ekran/durum. GameManager bunu değiştirir, ekranlar okur.
    /// Akış: DayIntro → Desk → Construction → (afet varsa Result/gazete, yoksa DaySummary) → DailyExpenses → Save → DayIntro | Ending.
    /// Deprem ayrı ekran değildir; oyuncu gazeteden öğrenir.
    /// </summary>
    public enum GamePhase
    {
        MainMenu,
        DayIntro,
        Desk,
        Construction,
        Result,        // Gazete (sadece afet olan gün)
        DaySummary,    // Gazetesiz gün özeti (afet olmayan gün)
        DailyExpenses,
        Save,
        Ending
    }

    /// <summary>Projede ölçülen yapı parametresi. Required/Reported/Actual bu türe göre karşılaştırılır.</summary>
    public enum ParamType
    {
        ConcreteStrength, // Beton dayanımı
        RebarRatio,       // Donatı oranı
        SoilQuality,      // Zemin etüdü
        SeismicDesign,    // Deprem dayanım katsayısı
        FireSafety,       // Yangın güvenliği (fabrika; kapsam dışı)
        // Ç1 (el kitabı) — yeni parametreler. Sona eklendi: kayıtlı int değerleri bozulmasın.
        ConcreteGrade,    // Beton sınıfı (harfli: C20, C25, ... → Labels)
        BarDiameter,      // Donatı çapı (mm)
        ColumnSection,    // Kolon kesiti (cm²)
        SoilClass,        // Zemin sınıfı (harfli: ZE ... ZA → Labels)
        ShearWallRatio    // Perde duvar oranı (%)
    }

    public enum BuildingType
    {
        Residential,
        Factory
    }

    public enum DocumentKind
    {
        Application, // Proje başvurusu
        LabReport    // Laboratuvar raporu
    }

    /// <summary>Bedava ipucu türleri: belgeler arası tutarsızlık.</summary>
    public enum ClueType
    {
        ParcelMismatch,
        DateMismatch,
        UnknownLab,
        WrongStamp,
        WrongSignature
    }

    /// <summary>Projenin gizli türü (oyuncuya gösterilmez; test, simülasyon ve gazete için).</summary>
    public enum ProjectKind
    {
        Compliant,   // Uygun
        Sloppy,      // Rüşvetsiz, bir-iki değeri gerekenin biraz altında (dikkatsizlik); belgeler tutarlı
        Bribe,       // Rüşvet teklif eder, değerleri yetersiz
        Fake,        // Rüşvetsiz, sahte belge: değerler geçiyor gibi yazılır, belgelerde ipucu var
        BribeFake    // Hem rüşvet hem sahte belge
    }

    public enum ProjectVerdict
    {
        Pending,
        Approved,
        Rejected
    }

    public enum BuildingStatus
    {
        Standing,
        Collapsed
    }

    public enum EndingType
    {
        None,
        Good,            // Son güne ulaştı, hiç bina yıkılmadı, hapis yok, ofis açık
        Bad,             // Mahkeme / hapis
        MiddleClosed,    // Borç limiti aşıldı, ofis kapandı
        MiddleCollapsed  // Son güne ulaştı ama imzalı bina(lar) yıkıldı; hapis çıkmadı
    }
}
