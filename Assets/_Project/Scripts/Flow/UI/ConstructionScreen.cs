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
        }

        protected override void Refresh()
        {
            var state = Context.State;
            constructionTitle.text = $"{state.Day}. GÜN · İNŞAATLAR";
            buildingInfo.text = state.ApprovedToday == 0
                ? "Bugün yeni yapı onaylanmadı."
                : $"Bugün onaylanan bina: {state.ApprovedToday}\n\nOnayladığın yapıların inşaatı tamamlandı.";
            continueButton.interactable = true;
        }

        void Continue()
        {
            if (PauseMenu.InputBlocked) return;
            continueButton.interactable = false;
            Finish();
        }
    }
}
