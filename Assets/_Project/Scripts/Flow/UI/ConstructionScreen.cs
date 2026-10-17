using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>Core'un onay sayısını gösterir; inşaat veya afet hesabı yapmaz.</summary>
    public class ConstructionScreen : FlowScreen
    {
        [SerializeField] TextMeshProUGUI constructionTitle;
        [SerializeField] TextMeshProUGUI buildingInfo;
        [SerializeField] Button continueButton;

        public override GamePhase Phase => GamePhase.Construction;

        protected override void Build()
        {
            continueButton.onClick.AddListener(Continue);
            // (EEE, 17 Ekim) Gün başı ekranıyla aynı yazı stili.
            UiKit.StyleTitle(constructionTitle, titleFont, 84);
            UiKit.StyleInfo(buildingInfo, infoFont, 40);
        }

        protected override void Refresh()
        {
            var state = Context.State;
            constructionTitle.text = $"{state.Day}. GÜN · İNŞAATLAR";
            buildingInfo.text = state.ApprovedToday == 0
                ? "Bugün yeni yapı onaylanmadı."
                : $"Bugün onaylanan bina: <color={UiKit.GoldHex}>{state.ApprovedToday}</color>\n\nOnayladığın yapıların inşaatı tamamlandı.";
            continueButton.interactable = true;
            StartCoroutine(FadeIn()); // masadaki "MESAİ BİTTİ" karartmasından yumuşak geçiş (EEE)
        }

        Image _fadeCover;

        // Ekranın kendisi saydamlaşmaz (arkadan kameranın mavi zemini görünüyordu); üstündeki koyu örtü açılır.
        System.Collections.IEnumerator FadeIn()
        {
            if (_fadeCover == null)
            {
                _fadeCover = UiKit.Image(transform, "FadeCover", null, Color.black, Vector2.zero, Vector2.one);
                _fadeCover.raycastTarget = false;
            }
            var dark = new Color(0.07f, 0.05f, 0.04f, 1f); // masadaki "MESAİ BİTTİ" ekranıyla aynı ton
            _fadeCover.transform.SetAsLastSibling();
            _fadeCover.gameObject.SetActive(true);
            const float duration = 0.6f;
            for (float t = 0f; t < duration; t += PauseMenu.AnimationDeltaTime)
            {
                float k = t / duration;
                _fadeCover.color = new Color(dark.r, dark.g, dark.b, 1f - k * k * (3f - 2f * k));
                yield return null;
            }
            _fadeCover.gameObject.SetActive(false);
        }

        void Continue()
        {
            if (PauseMenu.InputBlocked) return;
            continueButton.interactable = false;
            Finish();
        }
    }
}
