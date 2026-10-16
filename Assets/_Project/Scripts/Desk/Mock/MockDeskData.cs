using System.Collections.Generic;

namespace CivilJam.Desk
{
    // Core'daki ProjectGenerator gelene kadar sandbox'ı besleyen sabit Level 1 verisi.
    // Üç tip: dürüst usta, rüşvetçi müteahhit, sahte belgeli (bedava ipuçlu) proje.
    // İsimler tamamen kurgusaldır.
    public static class MockDeskData
    {
        public static DeskSessionConfig CreateLevel1()
        {
            return new DeskSessionConfig
            {
                LevelTitle = "Ekim · 1. Ay",
                StartMoney = 100000,
                ExtraTestCost = 15000,
                Quota = 1,
                Projects = new List<DeskProject> { HonestProject(), BribeProject(), FakeDocProject() },
            };
        }

        private static DeskProject HonestProject()
        {
            const string parcel = "Ada 112 / Parsel 7";
            return new DeskProject
            {
                Id = "L1-P1",
                Name = "Papatya Apartmanı",
                BuildingType = "Konut · 5 kat",
                ClientName = "Hilmi Usta",
                ClientLine = "Hocam, dosyalar tam. Bir eksik görürseniz söyleyin, hemen düzeltiriz.",
                Documents =
                {
                    Application(parcel, "02.10.2026", "Mimar Selin Aydemir", "S. Aydemir", 5),
                    new DeskDocument
                    {
                        Title = "Malzeme Laboratuvar Raporu",
                        Kind = DeskDocumentKind.LabReport,
                        ParcelNo = parcel,
                        Date = "09.10.2026",
                        Issuer = "Betonya Yapı Malzemeleri Lab. (Akredite)",
                        Signature = "Dr. Kerem Uçar",
                        HasStamp = true,
                        Params =
                        {
                            Concrete(30, 32, 32),
                            Rebar(420, 430, 428),
                            Soil(200, 220, 215),
                        },
                    },
                },
            };
        }

        private static DeskProject BribeProject()
        {
            const string parcel = "Ada 64 / Parsel 3";
            return new DeskProject
            {
                Id = "L1-P2",
                Name = "Lale Sitesi B Blok",
                BuildingType = "Konut · 8 kat",
                ClientName = "Cevdet Bey (Müteahhit)",
                ClientLine = "Mühendis bey, işler biraz acele. Masanıza ufak bir zarf bıraktım; siz rakamlara takılmayın, bir imza yeter.",
                BribeAmount = 50000,
                Documents =
                {
                    Application(parcel, "01.10.2026", "Mimar Okan Tezel", "O. Tezel", 8),
                    new DeskDocument
                    {
                        Title = "Malzeme Laboratuvar Raporu",
                        Kind = DeskDocumentKind.LabReport,
                        ParcelNo = parcel,
                        Date = "06.10.2026",
                        Issuer = "Betonya Yapı Malzemeleri Lab. (Akredite)",
                        Signature = "Dr. Kerem Uçar",
                        HasStamp = true,
                        Params =
                        {
                            Concrete(30, 20, 19),
                            Rebar(420, 420, 410),
                            Soil(200, 180, 175),
                        },
                    },
                },
            };
        }

        private static DeskProject FakeDocProject()
        {
            return new DeskProject
            {
                Id = "L1-P3",
                Name = "Menekşe Konutları",
                BuildingType = "Konut · 6 kat",
                ClientName = "Nurettin Kalfa",
                ClientLine = "Raporlar laboratuvardan bu sabah çıktı, sıcağı sıcağına getirdim.",
                Documents =
                {
                    Application("Ada 87 / Parsel 14", "05.10.2026", "Mimar Deniz Karaca", "D. Karaca", 6),
                    new DeskDocument
                    {
                        // Bedava ipuçları: parsel numarası ters, tarih geçersiz, kaşe yok.
                        Title = "Malzeme Laboratuvar Raporu",
                        Kind = DeskDocumentKind.LabReport,
                        ParcelNo = "Ada 87 / Parsel 41",
                        Date = "31.09.2026",
                        Issuer = "Yıldız Yapı Laboratuvarı",
                        Signature = "(okunaksız)",
                        HasStamp = false,
                        Params =
                        {
                            Concrete(30, 31, 18),
                            Rebar(420, 425, 400),
                            Soil(200, 210, 160),
                        },
                    },
                },
            };
        }

        private static DeskDocument Application(string parcel, string date, string architect, string signature, int floors)
        {
            return new DeskDocument
            {
                Title = "Yapı Ruhsatı Başvurusu",
                Kind = DeskDocumentKind.Project,
                ParcelNo = parcel,
                Date = date,
                Issuer = architect,
                Signature = signature,
                HasStamp = true,
                Lines =
                {
                    "Yapı türü: Konut",
                    $"Kat sayısı: {floors}",
                    "Taşıyıcı sistem: Betonarme çerçeve",
                },
            };
        }

        private static DeskParam Concrete(float req, float rep, float act) =>
            new DeskParam { Name = "Beton basınç dayanımı", Unit = "MPa", Required = req, Reported = rep, Actual = act };

        private static DeskParam Rebar(float req, float rep, float act) =>
            new DeskParam { Name = "Donatı akma dayanımı", Unit = "MPa", Required = req, Reported = rep, Actual = act };

        private static DeskParam Soil(float req, float rep, float act) =>
            new DeskParam { Name = "Zemin taşıma gücü", Unit = "kPa", Required = req, Reported = rep, Actual = act };
    }
}
