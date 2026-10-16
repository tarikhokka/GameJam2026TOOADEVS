using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Günlük hesaplaşma (her gün sonunda). Flow kalemleri gösterir, oyuncu seçimini IGameActions.SetExpensePaid ile bildirir.</summary>
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

        public int TotalSelected
        {
            get
            {
                int sum = 0;
                foreach (var l in Lines) if (l.Paid) sum += l.Amount;
                return sum;
            }
        }
    }

    [Serializable]
    public class ExpenseLine
    {
        public string ExpenseId;       // ExpenseSO.Id
        public string DisplayName;
        public int Amount;
        public bool Mandatory;         // Ödenmezse sonucu henüz kararlaştırılmadı (geçici: borca eklenir)
        public bool Paid;
    }
}
