using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Deprem anında son afetten bu yana onaylanmış ve ayakta olan binaları değerlendirir.
    /// Kural: Strength >= 1 ASLA yıkılmaz. Aksi halde Strength + gürültü(±noise) &lt; Intensity ise yıkılır.
    /// Yıkım varsa kayıplar üretilir, JailRisk artar ve hapis zarı atılır.
    /// LastDisasterDay'i DisasterSystem günceller.
    /// </summary>
    public static class EarthquakeSimulator
    {
        public static bool ShouldCollapse(float strength, float intensity, float noise, Random rng)
        {
            if (strength >= 1f) return false;
            float jitter = ((float)rng.NextDouble() * 2f - 1f) * noise;
            return strength + jitter < intensity;
        }

        /// <summary>Bu afette değerlendirilecek bina mı: son afetten sonra onaylanmış ve ayakta.</summary>
        public static bool IsInScope(ProjectData b, int lastDisasterDay) =>
            b.Status == BuildingStatus.Standing && b.DayDecided > lastDisasterDay;

        /// <summary>GameState'i günceller (bina durumları, sayaçlar, JailRisk) ve sonucu döner.</summary>
        public static QuakeResult Run(GameState state, float intensity, BalanceSO balance, NamePoolSO names, Random rng)
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
                if (!IsInScope(b, state.LastDisasterDay)) continue;

                float strength = b.Strength;
                bool collapsed = ShouldCollapse(strength, intensity, balance.StrengthNoise, rng);

                var outcome = new BuildingOutcome
                {
                    ProjectId = b.Id,
                    BuildingName = b.Name,
                    Address = b.Address,
                    DayApproved = b.DayDecided,
                    Collapsed = collapsed,
                    WasBribed = b.BribeAccepted,
                    Strength = strength
                };
                foreach (var p in b.Params)
                    if (!p.ActualPasses) outcome.FailedParams.Add(p);

                if (collapsed)
                {
                    CasualtyGenerator.Fill(outcome, b.Floors, balance, names, rng);

                    b.Status = BuildingStatus.Collapsed;
                    b.DayCollapsed = state.Day;
                    state.TotalCollapses++;
                    state.TotalDeaths += outcome.Deaths;
                    state.TotalInjured += outcome.Injured;
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

                outcome.ResponsibilityText = ResponsibilityText(outcome);
                result.Outcomes.Add(outcome);
            }

            state.JailRisk = Math.Min(1f, state.JailRisk);
            result.JailRiskAfter = state.JailRisk;

            if (result.CollapsedCount > 0)
            {
                result.JailRolled = true;
                result.Jailed = rng.NextDouble() < state.JailRisk;
                if (result.Jailed) state.Jailed = true;
            }

            return result;
        }

        static string ResponsibilityText(BuildingOutcome o)
        {
            if (!o.Collapsed)
                return $"{o.BuildingName} ayakta kaldı. Onayında sizin imzanız vardı ({o.DayApproved}. gün).";
            if (o.WasBribed)
                return $"{o.BuildingName} yıkıldı. Rüşvet alarak imzaladığınız bina ({o.DayApproved}. gün).";
            return $"{o.BuildingName} yıkıldı. Onayında sizin imzanız vardı ({o.DayApproved}. gün).";
        }
    }
}
