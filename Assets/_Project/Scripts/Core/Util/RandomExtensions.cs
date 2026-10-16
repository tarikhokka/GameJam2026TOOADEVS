using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>System.Random yardımcıları. Core UnityEngine.Random kullanmaz (seed ile tekrarlanabilirlik + test).</summary>
    public static class RandomExtensions
    {
        public static float Range(this Random rng, float min, float max) =>
            min + (float)rng.NextDouble() * (max - min);

        /// <summary>min ve max dahil.</summary>
        public static int RangeInclusive(this Random rng, int min, int max) =>
            rng.Next(min, max + 1);

        public static bool Chance(this Random rng, float p) => rng.NextDouble() < p;

        public static T Pick<T>(this Random rng, IReadOnlyList<T> list, T fallback = default)
        {
            if (list == null || list.Count == 0) return fallback;
            return list[rng.Next(list.Count)];
        }

        public static void Shuffle<T>(this Random rng, IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
