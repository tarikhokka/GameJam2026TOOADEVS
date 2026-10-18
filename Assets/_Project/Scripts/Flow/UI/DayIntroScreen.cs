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
            // (EEE, 17 Ekim) Başlık Paytone One, kasa/borç Fira Sans Bold (FlowScreen.titleFont / infoFont).
            UiKit.StyleTitle(dayTitle, titleFont, 124);
            UiKit.StyleInfo(moneyInfo, infoFont, 42);
            moneyInfo.lineSpacing = 12;
        }

        protected override void Refresh()
        {
            var state = Context.State;

            dayTitle.text = $"{state.Day}. GÜN";

            // Rakamlar altın; borç varsa kırmızımsı.
            moneyInfo.text =
                $"Kasa: <color={UiKit.GoldHex}>{UiKit.Money(state.Money)}</color>\n" +
                $"Borç: <color={(state.Debt > 0 ? UiKit.RedHex : UiKit.GoldHex)}>{UiKit.Money(state.Debt)}</color>";

            // (Tarık, Onur'un incelemesi P1) "Devam Et" istendi ama kayıt okunamadıysa oyuncu neden yeni oyunda olduğunu bilsin.
            if (Context.LoadFailed && state.Day == 1)
                moneyInfo.text += $"\n<size=55%><color={UiKit.RedHex}>Kayıt okunamadı, yeni oyun başladı.</color>" +
                    (string.IsNullOrEmpty(Context.LoadError) ? "" : $"\n({Context.LoadError})") + "</size>";

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
