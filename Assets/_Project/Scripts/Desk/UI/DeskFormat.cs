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

        // Parametre adı/birimi yönetmelikten (RuleSetSO) okunur; kural yoksa yedek ad kullanılır.
        public static (string name, string unit) Param(RuleSetSO rules, BuildingType building, ParamType type)
        {
            if (rules != null)
            {
                foreach (var rule in rules.RulesFor(building))
                    if (rule.Param == type)
                        return (rule.DisplayName, rule.Unit);
            }

            return type switch
            {
                ParamType.ConcreteStrength => ("Beton dayanımı", ""),
                ParamType.RebarRatio => ("Donatı oranı", ""),
                ParamType.SoilQuality => ("Zemin etüdü", ""),
                ParamType.SeismicDesign => ("Deprem yönetmeliği uyumu", ""),
                ParamType.FireSafety => ("Yangın güvenliği", ""),
                _ => (type.ToString(), ""),
            };
        }
    }
}
