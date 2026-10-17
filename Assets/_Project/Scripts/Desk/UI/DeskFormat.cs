using System.Collections.Generic;
using System.Globalization;
using CivilJam.Core;

namespace CivilJam.Desk
{
    internal static class DeskFormat
    {
        public static string Money(int value) =>
            value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + " TL";

        public static string Number(float value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string BuildingName(BuildingType type) => type switch
        {
            BuildingType.Factory => "Fabrika",
            _ => "Konut",
        };

        // Parametre adı yönetmelikten (RuleSetSO) okunur; kural yoksa yedek ad kullanılır.
        public static string ParamName(RuleSetSO rules, BuildingType building, ParamType type)
        {
            if (rules != null)
            {
                foreach (var rule in rules.RulesFor(building))
                    if (rule.Param == type && !string.IsNullOrEmpty(rule.DisplayName))
                        return rule.DisplayName;
            }

            return type switch
            {
                ParamType.ConcreteStrength => "Beton dayanımı",
                ParamType.RebarRatio => "Donatı oranı",
                ParamType.SoilQuality => "Zemin taşıma gücü",
                ParamType.SeismicDesign => "Deprem dayanım katsayısı",
                ParamType.FireSafety => "Yangın güvenliği",
                ParamType.ConcreteGrade => "Beton sınıfı",
                ParamType.BarDiameter => "Donatı çapı",
                ParamType.ColumnSection => "Kolon kesiti",
                ParamType.SoilClass => "Zemin sınıfı",
                ParamType.ShearWallRatio => "Perde duvar oranı",
                ParamType.ColumnThickness => "Kolon kalınlığı",
                ParamType.FoundationDepth => "Temel derinliği",
                ParamType.RebarType => "Demir tipi",
                ParamType.SoilType => "Zemin türü",
                _ => type.ToString(),
            };
        }

        // Aynı proje her çizimde aynı repliği göstersin diye proje kimliğine göre seçilir.
        public static string PickLine(List<string> lines, string projectId)
        {
            if (lines == null || lines.Count == 0) return null;
            int hash = 0;
            foreach (char c in projectId) hash = hash * 31 + c;
            return lines[(hash & int.MaxValue) % lines.Count];
        }

        // Değer metni Core'dan (RuleSetSO.FormatValue): harfli parametrede etiket (C25), değilse sayı + birim.
        public static string Value(RuleSetSO rules, ParamType type, float value) =>
            rules != null ? rules.FormatValue(type, value) : Number(value);
    }
}
