using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Deprem anında ayaktaki tüm binaları değerlendirir.
    /// Kural: Strength >= 1 ASLA yıkılmaz. Aksi halde Strength + gürültü(±noise) &lt; Intensity ise yıkılır.
    /// Yıkım varsa JailRisk artar ve hapis zarı atılır.
    /// </summary>
    public static class EarthquakeSimulator
    {
        public static bool ShouldCollapse(float strength, float intensity, float noise, Random rng)
        {
            if (strength >= 1f) return false;
            float jitter = ((float)rng.NextDouble() * 2f - 1f) * noise;
            return strength + jitter < intensity;
        }

        /// <summary>GameState'i günceller (bina durumları, sayaçlar, JailRisk) ve sonucu döner.</summary>
        public static QuakeResult Run(GameState state, float intensity, BalanceSO balance, Random rng)
        {
            var result = new QuakeResult
            {
                Day = state.Day,
                Occurred = true,
                Intensity = intensity,
                JailRiskBefore = state.JailRisk
            };

            foreach (var b in state.Buildings)
            {
                if (b.Status == BuildingStatus.Collapsed) continue;

                float strength = b.Strength;
                bool collapsed = ShouldCollapse(strength, intensity, balance.StrengthNoise, rng);

                var outcome = new BuildingOutcome
                {
                    ProjectId = b.Id,
                    BuildingName = b.Name,
                    Collapsed = collapsed,
                    WasBribed = b.BribeAccepted,
                    Strength = strength
                };
                foreach (var p in b.Params)
                    if (!p.ActualPasses) outcome.FailedParams.Add(p);
                result.Outcomes.Add(outcome);

                if (!collapsed) continue;

                b.Status = BuildingStatus.Collapsed;
                b.DayCollapsed = state.Day;
                state.TotalCollapses++;
                if (b.BribeAccepted)
                {
                    state.BribedCollapses++;
                    state.JailRisk += balance.JailRiskPerBribedCollapse;
                }
                else
                {
                    state.JailRisk += balance.JailRiskPerCollapse;
                }
            }

            state.JailRisk = Math.Min(1f, state.JailRisk);
            result.JailRiskAfter = state.JailRisk;

            if (result.CollapsedCount > 0)
            {
                result.JailRolled = true;
                result.Jailed = rng.NextDouble() < state.JailRisk;
            }

            return result;
        }
    }
}
