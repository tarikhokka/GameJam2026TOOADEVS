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
        // gifts: Data/Gifts (EEE); dürüst usta (Hilmi) henüz kabul edilmemiş ilk hediyeyi getirir (FakeGameActions onayda verir).
        // officeGifts: kabul edilen hediyeler; sandbox günleri boyunca aynı liste (gerçek oyunda kayıtla saklanır).
        public static GameContext Create(IReadOnlyList<Sprite> portraits = null, RuleSetSO rules = null,
            IReadOnlyList<GiftSO> gifts = null, List<string> officeGifts = null)
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

            if (officeGifts != null) context.State.OfficeGifts = officeGifts;
            if (gifts != null && gifts.Count > 0)
            {
                context.Gifts = gifts;
                foreach (var gift in gifts)
                {
                    if (gift == null || context.State.OfficeGifts.Contains(gift.Id)) continue;
                    context.Clients[0].Gift = gift; // Hilmi Usta (dürüst)
                    break;
                }
            }

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
                Address = "Çınar Sokak No: 4",
                ClientId = "hilmi",
                BuildingType = BuildingType.Residential,
                FileNo = "2026-0412",
                PeoplePerFloor = 12,
                Floors = 5, // 4-6 kat: kolon 35, temel 2,0, C25, B500, Sert kil → hepsi geçer
                Fee = 600,
                Params =
                {
                    Labeled(ParamType.ConcreteGrade, C25, C30, C30),
                    Param(ParamType.ColumnThickness, 35, 40, 40),
                    Param(ParamType.FoundationDepth, 2.0f, 2.0f, 2.0f),
                    Labeled(ParamType.RebarType, B500, B500, B500),
                    Labeled(ParamType.SoilType, StiffClay, StiffClay, StiffClay),
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
                Address = "Köprü Caddesi No: 18",
                ClientId = "cevdet",
                BuildingType = BuildingType.Residential,
                FileNo = "2026-0377",
                PeoplePerFloor = 20,
                Floors = 8, // 7+ kat: beyanda bile beton C25 (C30 gerekli) ve demir B500 (B500S gerekli)
                Fee = 900,
                OffersBribe = true,
                BribeAmount = 1500,
                Params =
                {
                    Labeled(ParamType.ConcreteGrade, C30, C25, C20),
                    Param(ParamType.ColumnThickness, 40, 40, 40),
                    Param(ParamType.FoundationDepth, 2.5f, 2.5f, 2.5f),
                    Labeled(ParamType.RebarType, B500S, B500, B420),
                    Labeled(ParamType.SoilType, Rock, Rock, Rock),
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
                Address = "Sahil Yolu No: 2",
                ClientId = "nurettin",
                BuildingType = BuildingType.Residential,
                FileNo = "2026-0451",
                PeoplePerFloor = 14,
                Floors = 6, // 4-6 kat: beyan temiz, ek test beton/kolon/demir/zeminde eksik çıkarır
                Fee = 700,
                Params =
                {
                    Labeled(ParamType.ConcreteGrade, C25, C25, C16),
                    Param(ParamType.ColumnThickness, 35, 35, 30),
                    Param(ParamType.FoundationDepth, 2.0f, 2.0f, 2.0f),
                    Labeled(ParamType.RebarType, B500, B500, B220),
                    Labeled(ParamType.SoilType, StiffClay, StiffClay, SoftClay),
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
                Address = "Demirci Sokak No: 11",
                ClientId = "rustem",
                BuildingType = BuildingType.Residential,
                FileNo = "2026-0398",
                PeoplePerFloor = 10,
                Floors = 4, // 4-6 kat: beton C20 (C25 gerekli) ve kolon 30 cm (35 gerekli)
                Fee = 500,
                OffersBribe = true,
                BribeAmount = 2000,
                Params =
                {
                    Labeled(ParamType.ConcreteGrade, C25, C20, C20),
                    Param(ParamType.ColumnThickness, 35, 30, 30),
                    Param(ParamType.FoundationDepth, 2.0f, 2.0f, 2.0f),
                    Labeled(ParamType.RebarType, B500, B500, B500),
                    Labeled(ParamType.SoilType, StiffClay, StiffClay, StiffClay),
                },
                Documents =
                {
                    Application(parcel, date, engineer),
                    LabReport(parcel, date, GoodLab, LabStamp(GoodLab), engineer),
                },
            };
        }

        // Seçenekli parametrelerde değer = seçenek sıra numarası (RuleSet.asset'teki Labels ile aynı sıra, kötüden iyiye).
        private const int C16 = 0, C20 = 1, C25 = 2, C30 = 3;
        private const int B220 = 0, B420 = 1, B500 = 2, B500S = 3;     // demir: B220 düz, hiçbir binada kabul edilmez
        private const int SoftClay = 1, StiffClay = 2, Rock = 3;        // (Dolgu = 0), Yumuşak kil, Sert kil, Kaya
        private static readonly string[] ConcreteLabels = { "C16", "C20", "C25", "C30", "C35" };
        private static readonly string[] RebarLabels = { "B220 (düz)", "B420", "B500", "B500S" };
        private static readonly string[] SoilLabels = { "Dolgu", "Yumuşak kil", "Sert kil", "Kaya" };

        // Yedek yönetmelik (Data/RuleSet.asset bağlı değilse): el kitabı v2 ile aynı 5 parametre × 3 kat grubu.
        private static RuleSetSO CreateRules()
        {
            var rules = ScriptableObject.CreateInstance<RuleSetSO>();
            var groups = new[] { (1, 3), (4, 6), (7, 0) };
            float[] column = { 30, 35, 40 }, depth = { 1.5f, 2.0f, 2.5f };
            int[] concrete = { C20, C25, C30 }, rebar = { B420, B500, B500S }, soil = { SoftClay, StiffClay, Rock };
            for (int i = 0; i < groups.Length; i++)
            {
                var (min, max) = groups[i];
                rules.Rules.Add(Rule(ParamType.ConcreteGrade, "Beton sınıfı", "", concrete[i], min, max, ConcreteLabels));
                rules.Rules.Add(Rule(ParamType.ColumnThickness, "Kolon kalınlığı", "cm", column[i], min, max));
                rules.Rules.Add(Rule(ParamType.FoundationDepth, "Temel derinliği", "m", depth[i], min, max));
                rules.Rules.Add(Rule(ParamType.RebarType, "Demir tipi", "", rebar[i], min, max, RebarLabels));
                rules.Rules.Add(Rule(ParamType.SoilType, "Zemin türü", "", soil[i], min, max, SoilLabels));
            }
            return rules;
        }

        // El kitabındaki akredite lab listesi. "Hızlı Sonuç Lab. Ltd." bilerek listede yok (UnknownLab ipucu).
        private static NamePoolSO CreateNames()
        {
            var names = ScriptableObject.CreateInstance<NamePoolSO>();
            names.LabNames.AddRange(new[] { GoodLab, "Merkez Yapı Lab.", "Anadolu Malzeme Test" });
            return names;
        }

        private static ParamRule Rule(ParamType type, string name, string unit, float required, int minFloors, int maxFloors,
            string[] labels = null) => new ParamRule
        {
            BuildingType = BuildingType.Residential,
            Param = type,
            DisplayName = name,
            Unit = unit,
            Required = required,
            MinFloors = minFloors,
            MaxFloors = maxFloors,
            Labels = labels != null ? new List<string>(labels) : new List<string>(),
            // El Kitabı sekmeleri (RuleSet.asset ile aynı, Ahmet f583077): beton → BETON, demir → DONATI, kolon → YAPI, temel ve zemin → ZEMİN.
            Section = type == ParamType.ConcreteGrade ? "BETON" : type == ParamType.RebarType ? "DONATI"
                : type == ParamType.ColumnThickness ? "YAPI" : "ZEMİN",
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
            // Uyuşmazlık diyaloğu (Core InspectionDialogue): sandbox deneme replikleri; oyundakileri EEE yazar (Data/Clients).
            client.MismatchLines = new List<string>
            {
                honest ? "Haklısınız hocam, gözden kaçmış. Hemen düzelttiririm." : "Hocam o kadar da önemli bir şey değil bence.",
            };
            client.BribeCloseLines = new List<string> { "Ben bunu masaya bırakıyorum. Siz bilirsiniz." };
            return client;
        }

        private static ParamEntry Param(ParamType type, float required, float reported, float actual) =>
            new ParamEntry { Type = type, Required = required, Reported = reported, Actual = actual };

        // Seçenekli parametre: değerler seçenek sıra numarası (ParamEntry.Labeled → dayanım kademe cezasıyla hesaplanır).
        private static ParamEntry Labeled(ParamType type, int required, int reported, int actual) =>
            new ParamEntry { Type = type, Required = required, Reported = reported, Actual = actual, Labeled = true };

        private static DocumentData Application(string parcel, string date, string signature) => new DocumentData
        {
            Kind = DocumentKind.Application,
            Title = "Yapı Denetim Formu",
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
