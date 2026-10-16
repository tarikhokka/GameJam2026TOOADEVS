using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Gün sonu afet kontrolü.
    /// - DisasterChance ile zar atılır.
    /// - Afet olursa: EarthquakeSimulator çalışır, LastDisasterDay = bugün, olasılık tabana döner (ResetChanceAfterDisaster).
    /// - Olmazsa: olasılık Increment kadar artar, Cap'i geçmez.
    /// </summary>
    public static class DisasterSystem
    {
        public static QuakeResult EndOfDay(GameState state, DayConfigSO config, BalanceSO balance, NamePoolSO names, Random rng)
        {
            float chance = state.DisasterChance;
            QuakeResult result;

            if (rng.NextDouble() < chance)
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
