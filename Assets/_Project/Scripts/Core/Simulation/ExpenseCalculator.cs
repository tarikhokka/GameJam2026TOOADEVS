using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Günlük giderler.
    /// Kural (karar 2026-10-17): kasada yetmeyen gider ödenemez; zorunluysa borca yazılır.
    /// Borç BalanceSO.DebtLimit'i aşarsa ofis kapanır (GameSession.EvaluateEnding → MiddleClosed).
    /// Borç varsa isteğe bağlı "Borç ödemesi" kalemi eklenir; kasa çeşitli kalemleri sırayla karşılar.
    /// </summary>
    public static class ExpenseCalculator
    {
        public const string DebtPaymentId = "debt_payment";
        public const string WindowRepairId = "window_repair";

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
                        Mandatory = e.Mandatory
                    });
                }
            }

            if (state.WindowRepairsToday > 0)
            {
                result.Lines.Add(new ExpenseLine
                {
                    ExpenseId = WindowRepairId,
                    DisplayName = $"Hasar tamiri x{state.WindowRepairsToday}", // kabadayı: cam, masa, kapı ya da araba (EEE)
                    Amount = balance.WindowRepairCost * state.WindowRepairsToday,
                    Mandatory = true
                });
            }

            if (state.Debt > 0)
            {
                result.Lines.Add(new ExpenseLine
                {
                    ExpenseId = DebtPaymentId,
                    DisplayName = "Borç ödemesi",
                    Amount = state.Debt,
                    Mandatory = false
                });
            }

            // Varsayılan seçim: zorunlu kalemler, kasa yettiği sürece işaretli.
            int budget = state.Money;
            foreach (var l in result.Lines)
            {
                l.Paid = l.Mandatory && l.Amount <= budget;
                if (l.Paid) budget -= l.Amount;
            }

            return result;
        }

        /// <summary>
        /// Oyuncu seçimini uygular. Bir kez çalışır (Applied).
        /// Seçili kalem kasa yetmiyorsa ödenmez (Paid = false olur). Ödenmeyen zorunlu kalem borca yazılır.
        /// Kasa hiçbir zaman eksiye düşmez.
        /// </summary>
        /// <summary>
        /// Ödeme ekranı için önizleme: oyuncunun şu anki seçimi onaylansa ne olurdu? Gerçek state'e ve pending'e dokunmaz;
        /// Apply ile birebir aynı sıra ve kurallar (zorunlular önce, borç ödemesi kalanla kısmen). Sonuçta
        /// MoneyAfter, DebtAfter, PaidTotal, AddedToDebt, DebtPaid ve her kalemin gerçekten ödenip ödenmediği (Lines[i].Paid) var.
        /// Seçim her değiştiğinde yeniden çağır.
        /// </summary>
        public static DailyExpenseResult Preview(GameState state, DailyExpenseResult pending)
        {
            if (pending == null) return null;
            if (pending.Applied) return pending.Clone();
            var copy = pending.Clone();
            var scratch = new GameState { Money = state.Money, Debt = state.Debt };
            Apply(scratch, copy);
            return copy;
        }

        public static void Apply(GameState state, DailyExpenseResult result)
        {
            if (result == null || result.Applied) return;
            int moneyStart = state.Money;
            result.AddedToDebt = 0;
            result.DebtPaid = 0;

            // Önce zorunlular, sonra isteğe bağlılar (borç ödemesi en sonda kalanla yapılır).
            foreach (bool mandatoryPass in new[] { true, false })
            {
                foreach (var line in result.Lines)
                {
                    if (line.Mandatory != mandatoryPass) continue;

                    if (line.ExpenseId == DebtPaymentId)
                    {
                        // Borç kısmen de ödenebilir: kasada ne kaldıysa.
                        int pay = line.Paid ? System.Math.Min(System.Math.Min(line.Amount, state.Money), state.Debt) : 0;
                        state.Money -= pay;
                        state.Debt -= pay;
                        line.Amount = pay;
                        line.Paid = pay > 0;
                        result.DebtPaid = pay;
                        continue;
                    }

                    if (line.Paid && line.Amount <= state.Money)
                    {
                        state.Money -= line.Amount;
                    }
                    else
                    {
                        line.Paid = false;
                        if (line.Mandatory)
                        {
                            state.Debt += line.Amount;
                            result.AddedToDebt += line.Amount;
                        }
                    }
                }
            }

            result.PaidTotal = moneyStart - state.Money;
            result.MoneyAfter = state.Money;
            result.DebtAfter = state.Debt;
            result.Applied = true;
        }
    }
}
