using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Günlük hesaplaşma (her gün sonunda). Flow kalemleri gösterir, oyuncu seçimini IGameActions.SetExpensePaid ile bildirir.
    /// Ödeme sonrası rakamlar için ekran kendisi çıkarma yapmaz: ExpenseCalculator.Preview(ctx.State, ctx.PendingExpenses).</summary>
    [Serializable]
    public class DailyExpenseResult
    {
        public int Day;
        public List<ExpenseLine> Lines = new List<ExpenseLine>();
        public int MoneyBefore;
        public int MoneyAfter;
        public int DebtBefore;
        public int DebtAfter;
        public bool Applied;           // Core uyguladı mı (çift ödeme koruması)

        // Apply (ya da Preview) sonrası dolar:
        public int PaidTotal;          // Kasadan gerçekten çıkan (borç ödemesi dahil)
        public int AddedToDebt;        // Kasa yetmediği ya da seçilmediği için borca yazılan zorunlu giderler
        public int DebtPaid;           // Borç ödemesi kaleminden gerçekten ödenen (kısmi olabilir)

        /// <summary>Seçili kalemlerin toplamı (kasanın yetip yetmediğine bakmaz). Ödeme sonrası için Preview kullan.</summary>
        public int TotalSelected
        {
            get
            {
                int sum = 0;
                foreach (var l in Lines) if (l.Paid) sum += l.Amount;
                return sum;
            }
        }

        public DailyExpenseResult Clone()
        {
            var c = (DailyExpenseResult)MemberwiseClone();
            c.Lines = new List<ExpenseLine>();
            foreach (var l in Lines) c.Lines.Add(l.Clone());
            return c;
        }
    }

    [Serializable]
    public class ExpenseLine
    {
        public string ExpenseId;       // ExpenseSO.Id
        public string DisplayName;
        public int Amount;
        public bool Mandatory;         // Ödenmezse borca yazılır
        public bool Paid;

        public ExpenseLine Clone() => (ExpenseLine)MemberwiseClone();
    }
}
