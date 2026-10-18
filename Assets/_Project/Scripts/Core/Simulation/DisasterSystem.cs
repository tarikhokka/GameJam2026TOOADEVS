using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Gün sonu afet kontrolü.
    /// - DisasterChance ile zar atılır.
    /// - Afet olursa: EarthquakeSimulator çalışır, LastDisasterDay = bugün, olasılık tabana döner (ResetChanceAfterDisaster).
    /// - Olmazsa: olasılık Increment kadar artar, Cap'i geçmez.
    /// - Jüri vitrini (BalanceSO.GuaranteedDisasterByDay): o güne kadar hiç afet olmadıysa o gece afet kesin. Zar yine atılır
    ///   (rastgele akış ve önceki gecelerin olasılığı değişmez).
    /// </summary>
    public static class DisasterSystem
    {
        public static QuakeResult EndOfDay(GameState state, DayConfigSO config, BalanceSO balance, NamePoolSO names, Random rng)
        {
            float chance = state.DisasterChance;
            QuakeResult result;

            double roll = rng.NextDouble();
            bool guaranteed = balance.GuaranteedDisasterByDay > 0 && state.LastDisasterDay == 0 && state.Day >= balance.GuaranteedDisasterByDay;
            if (guaranteed || roll < chance)
            {
                float intensity = Math.Min(1f, rng.Range(config.IntensityMin, config.IntensityMax));
                result = EarthquakeSimulator.Run(state, intensity, balance, names, rng);
                state.LastDisasterDay = state.Day;
                state.DisasterChance = balance.ResetChanceAfterDisaster ? balance.DisasterChanceStart : chance;
            }
            else
            {
                result = new QuakeResult { Day = state.Day, Occurred = false, JailRiskBefore = state.JailRisk, JailRiskAfter = state.JailRisk };
                state.DisasterChance = Math.Min(balance.DisasterChanceCap, chance + balance.DisasterChanceIncrement);
            }

            result.ChanceBefore = chance;
            return result;
        }
    }
}
