using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Günlük giderler (v0). Faz 2'de EconomyCalculator'a genişleyecek.
    /// Ödenmeyen giderin sonucu henüz kararlaştırılmadı; GEÇİCİ: zorunlu kalem ödenmezse borca eklenir.
    /// </summary>
    public static class ExpenseCalculator
    {
        public static DailyExpenseResult Build(GameState state, IReadOnlyList<ExpenseSO> expenses, BalanceSO balance)
        {
            var result = new DailyExpenseResult
            {
                Day = state.Day,
                MoneyBefore = state.Money,
                DebtBefore = state.Debt
            };

            if (expenses != null)
            {
                foreach (var e in expenses)
                {
                    if (e == null) continue;
                    result.Lines.Add(new ExpenseLine
                    {
                        ExpenseId = e.Id,
                        DisplayName = e.DisplayName,
                        Amount = e.Amount,
                        Mandatory = e.Mandatory,
                        Paid = e.Mandatory // Varsayılan: zorunlular işaretli, oyuncu kaldırabilir
                    });
                }
            }

            if (state.WindowRepairsToday > 0)
            {
                result.Lines.Add(new ExpenseLine
                {
                    ExpenseId = "window_repair",
                    DisplayName = $"Cam tamiri x{state.WindowRepairsToday}",
                    Amount = balance.WindowRepairCost * state.WindowRepairsToday,
                    Mandatory = true,
                    Paid = true
                });
            }

            return result;
        }

        /// <summary>Oyuncu seçimini uygular. Bir kez çalışır (Applied).</summary>
        public static void Apply(GameState state, DailyExpenseResult result)
        {
            if (result == null || result.Applied) return;

            foreach (var line in result.Lines)
            {
                if (line.Paid) state.Money -= line.Amount;
                else if (line.Mandatory) state.Debt += line.Amount; // GEÇİCİ kural
            }

            result.MoneyAfter = state.Money;
            result.DebtAfter = state.Debt;
            result.Applied = true;
        }
    }
}
