using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sandbox_Desk'i besleyen sabit, sahte gün. Belge kuralları Core'daki ProjectGenerator ile aynıdır:
    // iki belgeyi aynı mühendis imzalar, lab kaşesi "<LAB ADI> · AKREDİTE"; ipuçları bu düzeni bozar.
    // Üç tip: dürüst usta, rüşvetçi müteahhit, sahte belgeli proje. İsimler tamamen kurgusaldır.
    public static class SandboxDeskContext
    {
        private const string MunicipalityStamp = "BELEDİYE · İMAR MÜDÜRLÜĞÜ";
        private const string GoodLab = "Betonya Yapı Lab.";

        public static GameContext Create()
        {
            var balance = ScriptableObject.CreateInstance<BalanceSO>();

            var context = new GameContext
            {
                Phase = GamePhase.Desk,
                Balance = balance,
                Rules = CreateRules(),
                State = new GameState { Day = 1, Money = balance.StartMoney, DisasterChance = balance.DisasterChanceStart },
                Clients = new List<ClientSO>
                {
                    Client("hilmi", "Hilmi Usta", honest: true,
                        greetings: new[] { "Hocam, dosyalar tam. Bir eksik görürseniz söyleyin, hemen düzeltiriz." }),
                    Client("cevdet", "Cevdet Bey (Müteahhit)", honest: false,
                        greetings: new[] { "Mühendis bey, işler biraz acele. Bir imza yeter." },
                        bribes: new[] { "Rakamlara takılmayın, bu da çay parası." }),
                    Client("nurettin", "Nurettin Kalfa", honest: false,
                        greetings: new[] { "Raporlar laboratuvardan bu sabah çıktı, sıcağı sıcağına getirdim." }),
                },
                TodayProjects = new List<ProjectData> { HonestProject(), BribeProject(), FakeDocProject() },
            };

            context.Actions = new FakeGameActions(context);
            return context;
        }

        private static ProjectData HonestProject()
        {
            const string parcel = "112/7", date = "09.10.2026", engineer = "Müh. S. Aydemir";
            return new ProjectData
            {
                Id = "D1-P1",
                Name = "Papatya Apartmanı",
                Address = "Lale Mah. Çınar Sk. No: 4",
                ClientId = "hilmi",
                BuildingType = BuildingType.Residential,
                Floors = 5,
                Fee = 600,
                Params =
                {
                    Param(ParamType.ConcreteStrength, 30, 32, 32),
                    Param(ParamType.RebarRatio, 1.0f, 1.1f, 1.1f),
                    Param(ParamType.SoilQuality, 200, 220, 215),
                },
                Documents =
                {
                    Application(parcel, date, engineer),
                    LabReport(parcel, date, GoodLab, LabStamp(GoodLab), engineer),
                },
            };
        }

        private static ProjectData BribeProject()
        {
            const string parcel = "64/3", date = "06.10.2026", engineer = "Müh. O. Tezel";
            return new ProjectData
            {
                Id = "D1-P2",
                Name = "Lale Sitesi B Blok",
                Address = "Gül Mah. Köprü Cd. No: 18",
                ClientId = "cevdet",
                BuildingType = BuildingType.Residential,
                Floors = 8,
                Fee = 900,
                OffersBribe = true,
                BribeAmount = 1500,
                Params =
                {
                    Param(ParamType.ConcreteStrength, 30, 20, 19),
                    Param(ParamType.RebarRatio, 1.0f, 1.0f, 0.8f),
                    Param(ParamType.SoilQuality, 200, 180, 175),
                },
                Documents =
                {
                    Application(parcel, date, engineer),
                    LabReport(parcel, date, GoodLab, LabStamp(GoodLab), engineer),
                },
            };
        }

        private static ProjectData FakeDocProject()
        {
            // Bedava ipuçları (Core'daki ApplyClue ile aynı): parsel ve tarih başvuruyla uyuşmaz,
            // başlıktaki lab bilinmiyor ama kaşe hâlâ başka lab'ın, raporu başka mühendis imzalamış.
            var lab = LabReport("87/41", "28.11.2026", "Hızlı Sonuç Lab. Ltd.", LabStamp(GoodLab), "Müh. K. Durmaz");
            lab.Clues.AddRange(new[] { ClueType.ParcelMismatch, ClueType.DateMismatch, ClueType.UnknownLab, ClueType.WrongSignature });

            return new ProjectData
            {
                Id = "D1-P3",
                Name = "Menekşe Konutları",
                Address = "Yeni Mah. Sahil Yolu No: 2",
                ClientId = "nurettin",
                BuildingType = BuildingType.Residential,
                Floors = 6,
                Fee = 700,
                Params =
                {
                    Param(ParamType.ConcreteStrength, 30, 31, 18),
                    Param(ParamType.RebarRatio, 1.0f, 1.05f, 0.7f),
                    Param(ParamType.SoilQuality, 200, 210, 160),
                },
                Documents =
                {
                    Application("87/14", "05.10.2026", "Müh. D. Karaca"),
                    lab,
                },
            };
        }

        private static RuleSetSO CreateRules()
        {
            var rules = ScriptableObject.CreateInstance<RuleSetSO>();
            rules.Rules.Add(Rule(ParamType.ConcreteStrength, "Beton basınç dayanımı", "MPa", 30));
            rules.Rules.Add(Rule(ParamType.RebarRatio, "Donatı oranı", "%", 1.0f));
            rules.Rules.Add(Rule(ParamType.SoilQuality, "Zemin taşıma gücü", "kPa", 200));
            return rules;
        }

        private static ParamRule Rule(ParamType type, string name, string unit, float required) => new ParamRule
        {
            BuildingType = BuildingType.Residential,
            Param = type,
            DisplayName = name,
            Unit = unit,
            Required = required,
        };

        private static ClientSO Client(string id, string name, bool honest, string[] greetings, string[] bribes = null)
        {
            var client = ScriptableObject.CreateInstance<ClientSO>();
            client.Id = id;
            client.DisplayName = name;
            client.IsHonest = honest;
            client.GreetingLines = new List<string>(greetings);
            client.BribeLines = bribes != null ? new List<string>(bribes) : new List<string>();
            return client;
        }

        private static ParamEntry Param(ParamType type, float required, float reported, float actual) =>
            new ParamEntry { Type = type, Required = required, Reported = reported, Actual = actual };

        private static DocumentData Application(string parcel, string date, string signature) => new DocumentData
        {
            Kind = DocumentKind.Application,
            Title = "Yapı Ruhsatı Başvurusu",
            ParcelNo = parcel,
            Date = date,
            StampId = MunicipalityStamp,
            SignatureName = signature,
        };

        private static string LabStamp(string labName) =>
            $"{labName.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant()} · AKREDİTE";

        private static DocumentData LabReport(string parcel, string date, string lab, string stamp, string signature) => new DocumentData
        {
            Kind = DocumentKind.LabReport,
            Title = "Laboratuvar Raporu",
            ParcelNo = parcel,
            Date = date,
            LabName = lab,
            StampId = stamp,
            SignatureName = signature,
        };
    }
}
