using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Üst bar: gün, kasa, onay kotası.
    public class DeskHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelTitleText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text quotaText;

        public void Refresh(GameContext context)
        {
            var state = context.State;
            levelTitleText.text = $"Gün {state.Day}";

            moneyText.text = state.Debt > 0
                ? $"Kasa: {DeskFormat.Money(state.Money)}  <size=80%>Borç: {DeskFormat.Money(state.Debt)}</size>"
                : $"Kasa: {DeskFormat.Money(state.Money)}";

            int quota = context.Balance != null ? context.Balance.ApprovalQuota : 0;
            quotaText.text = $"Bugün onay: {state.ApprovedToday} / {quota}";
        }
    }
}
