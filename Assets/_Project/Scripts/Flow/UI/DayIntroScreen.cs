using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    public class DayIntroScreen : FlowScreen
    {
        [SerializeField] TextMeshProUGUI dayTitle;
        [SerializeField] TextMeshProUGUI moneyInfo;
        [SerializeField] Button startDayButton;

        public override GamePhase Phase => GamePhase.DayIntro;

        protected override void Build()
        {
            startDayButton.onClick.AddListener(StartDay);
        }

        protected override void Refresh()
        {
            var state = Context.State;

            dayTitle.text = $"{state.Day}. GÜN";

            moneyInfo.text =
                $"Kasa: {UiKit.Money(state.Money)}\n" +
                $"Borç: {UiKit.Money(state.Debt)}";

            startDayButton.interactable = true;
        }

        void StartDay()
        {
            if (PauseMenu.InputBlocked) return;
            startDayButton.interactable = false;
            Finish();
        }
    }
}
