using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Üst bar: gün, kasa, bugün onaylanan ve bekleyen müşteri sayısı.
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

            // Kota yok (karar A); sadece bilgi. Müşteriler sırayla gelir; bekleyen = henüz karar verilmemiş dosya.
            int waiting = context.TodayProjects.FindAll(p => p.Verdict == ProjectVerdict.Pending).Count;
            quotaText.text = $"Onaylanan: {state.ApprovedToday}  ·  Bekleyen: {waiting}";
        }
    }
}
