using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sandbox_Desk'i besleyen sabit, sahte gün. Belge kuralları Core'daki ProjectGenerator ile aynıdır:
    // iki belgeyi aynı mühendis imzalar, lab kaşesi "<LAB ADI> · AKREDİTE"; ipuçları bu düzeni bozar.
    // Dört tip: dürüst usta, rüşvetçi müteahhit, sahte belgeli proje, kabadayı. İsimler tamamen kurgusaldır.
    public static class SandboxDeskContext
    {
        private const string MunicipalityStamp = "BELEDİYE · İMAR MÜDÜRLÜĞÜ";
        private const string GoodLab = "Betonya Yapı Lab.";

        // portraits: sahte müşterilere sırayla verilen portreler (Sandbox_Desk kurulurken Art'tan bağlanır); boş olabilir.
        // rules: gerçek yönetmelik (Data/RuleSet.asset); boşsa aşağıdaki sahte 3 kural.
        public static GameContext Create(IReadOnlyList<Sprite> portraits = null, RuleSetSO rules = null)
        {
            var balance = ScriptableObject.CreateInstance<BalanceSO>();

            var context = new GameContext
            {
                Phase = GamePhase.Desk,
                Balance = balance,
                Rules = rules != null ? rules : CreateRules(),
                Names = CreateNames(),
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
                    Client("rustem", "Balyoz Rüstem", honest: false, thug: true,
                        greetings: new[] { "Hocam selamünaleyküm. Ben uzatmayı sevmem." },
                        bribes: new[] { "Ofisinizin camları çok güzel bu arada... Kırılsa yazık olur." }),
                },
                TodayProjects = new List<ProjectData> { HonestProject(), BribeProject(), FakeDocProject(), ThugProject() },
            };

            if (portraits != null && portraits.Count > 0)
                for (int i = 0; i < context.Clients.Count; i++)
                    context.Clients[i].Portrait = portraits[i % portraits.Count];

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

        // Kabadayı: zarf var, değerler düşük. Reddedilirse cam kırılır (olay kartı).
        private static ProjectData ThugProject()
        {
            const string parcel = "301/9", date = "02.10.2026", engineer = "Müh. O. Tezel";
            return new ProjectData
            {
                Id = "D1-P4",
                Name = "Yıldız Pasajı",
                Address = "Çarşı Mah. Demirci Sk. No: 11",
                ClientId = "rustem",
                BuildingType = BuildingType.Residential,
                Floors = 4,
                Fee = 500,
                OffersBribe = true,
                BribeAmount = 2000,
                Params =
                {
                    Param(ParamType.ConcreteStrength, 30, 22, 21),
                    Param(ParamType.RebarRatio, 1.0f, 0.8f, 0.8f),
                    Param(ParamType.SoilQuality, 200, 210, 205),
                },
                Documents =
                {
                    Application(parcel, date, engineer),
                    LabReport(parcel, date, GoodLab, LabStamp(GoodLab), engineer),
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

        // El kitabındaki akredite lab listesi. "Hızlı Sonuç Lab. Ltd." bilerek listede yok (UnknownLab ipucu).
        private static NamePoolSO CreateNames()
        {
            var names = ScriptableObject.CreateInstance<NamePoolSO>();
            names.LabNames.AddRange(new[] { GoodLab, "Merkez Yapı Lab.", "Anadolu Malzeme Test" });
            return names;
        }

        private static ParamRule Rule(ParamType type, string name, string unit, float required) => new ParamRule
        {
            BuildingType = BuildingType.Residential,
            Param = type,
            DisplayName = name,
            Unit = unit,
            Required = required,
        };

        private static ClientSO Client(string id, string name, bool honest, string[] greetings, string[] bribes = null,
            bool thug = false)
        {
            var client = ScriptableObject.CreateInstance<ClientSO>();
            client.Id = id;
            client.DisplayName = name;
            client.IsHonest = honest;
            client.IsThug = thug;
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
