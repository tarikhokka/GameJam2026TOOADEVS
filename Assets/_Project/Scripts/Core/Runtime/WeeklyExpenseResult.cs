using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>Haftalık hesaplaşma. Flow kalemleri gösterir, oyuncu seçimini IGameActions.SetExpensePaid ile bildirir.</summary>
    [Serializable]
    public class WeeklyExpenseResult
    {
        public int Week;
        public List<ExpenseLine> Lines = new List<ExpenseLine>();
        public int MoneyBefore;
        public int MoneyAfter;
        public int DebtBefore;
        public int DebtAfter;
        public bool OfficeClosed;      // Borç limiti aşıldı → Orta son

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
        public bool Mandatory;         // Ödenmezse borca eklenir (kira, kredi)
        public bool Paid;
    }
}
