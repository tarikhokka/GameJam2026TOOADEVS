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
        // (EEE, 17 Ekim) Başlık Paytone One (krem→altın, koyu kontur, gölge); kasa/borç Fira Sans Bold. Boşsa prefab fontu kalır.
        [SerializeField] TMP_FontAsset titleFont;   // PaytoneOne-Regular SDF
        [SerializeField] TMP_FontAsset infoFont;    // FiraSans-Bold SDF

        static readonly Color Cream = new Color(1f, 0.96f, 0.86f, 1f);
        static readonly Color Gold = new Color(0.98f, 0.74f, 0.36f, 1f);
        static readonly Color Ink = new Color(0.23f, 0.16f, 0.15f, 1f);

        public override GamePhase Phase => GamePhase.DayIntro;

        protected override void Build()
        {
            startDayButton.onClick.AddListener(StartDay);
            if (titleFont != null)
            {
                dayTitle.font = titleFont;
                dayTitle.fontSize = 124;
                dayTitle.textWrappingMode = TextWrappingModes.NoWrap;
                dayTitle.color = Color.white;
                dayTitle.enableVertexGradient = true;
                dayTitle.colorGradient = new VertexGradient(Cream, Cream, Gold, Gold);
                Decorate(dayTitle, 0.24f, 0.9f);
            }
            if (infoFont != null)
            {
                moneyInfo.font = infoFont;
                moneyInfo.fontSize = 42;
                moneyInfo.lineSpacing = 12;
                moneyInfo.color = Cream;
                Decorate(moneyInfo, 0.18f, 0.7f);
            }
        }

        // Koyu kahve kontur + aşağı düşen yumuşak gölge (malzeme kopyası sadece bu metne).
        static void Decorate(TMP_Text text, float outline, float shadow)
        {
            text.outlineWidth = outline;
            text.outlineColor = Ink;
            var mat = text.fontMaterial;
            mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, shadow));
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.8f);
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.35f);
        }

        protected override void Refresh()
        {
            var state = Context.State;

            dayTitle.text = $"{state.Day}. GÜN";

            // Rakamlar altın; borç varsa kırmızımsı.
            moneyInfo.text =
                $"Kasa: <color=#FFD27A>{UiKit.Money(state.Money)}</color>\n" +
                $"Borç: <color={(state.Debt > 0 ? "#FF9A8A" : "#FFD27A")}>{UiKit.Money(state.Debt)}</color>";

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
