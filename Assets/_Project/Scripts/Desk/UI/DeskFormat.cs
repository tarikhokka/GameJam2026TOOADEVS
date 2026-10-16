using System.Globalization;

namespace CivilJam.Desk
{
    internal static class DeskFormat
    {
        public static string Money(int value) =>
            value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + " TL";

        public static string Number(float value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
