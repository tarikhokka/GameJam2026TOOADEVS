using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Sağ panel: başvuru sahibi, rüşvet zarfı, ek test ve onay/red butonları.
    public class ActionPanel : MonoBehaviour
    {
        [Header("Başvuru sahibi")]
        [SerializeField] private TMP_Text clientNameText;
        [SerializeField] private TMP_Text clientLineText;

        [Header("Rüşvet")]
        [SerializeField] private GameObject bribeRoot;
        [SerializeField] private TMP_Text bribeText;
        [SerializeField] private Button takeBribeButton;

        [Header("Kararlar")]
        [SerializeField] private Button extraTestButton;
        [SerializeField] private TMP_Text extraTestLabel;
        [SerializeField] private Button approveButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private TMP_Text verdictText;
        [SerializeField] private Button finishButton;

        public event Action ExtraTestClicked;
        public event Action ApproveClicked;
        public event Action TakeBribeClicked;
        public event Action RejectClicked;
        public event Action FinishClicked;

        private void Awake()
        {
            extraTestButton.onClick.AddListener(() => ExtraTestClicked?.Invoke());
            approveButton.onClick.AddListener(() => ApproveClicked?.Invoke());
            takeBribeButton.onClick.AddListener(() => TakeBribeClicked?.Invoke());
            rejectButton.onClick.AddListener(() => RejectClicked?.Invoke());
            finishButton.onClick.AddListener(() => FinishClicked?.Invoke());
        }

        public void ShowEmpty(bool allDecided)
        {
            clientNameText.text = "";
            clientLineText.text = allDecided ? "Masada bekleyen dosya kalmadı." : "";
            bribeRoot.SetActive(false);
            SetDecisionButtonsVisible(false);
            verdictText.text = "";
            finishButton.gameObject.SetActive(allDecided);
        }

        public void Show(DeskProject project, DeskDecision decision, bool canBuyTest, int testCost, bool allDecided)
        {
            clientNameText.text = project.ClientName;
            clientLineText.text = $"“{project.ClientLine}”";

            bool pending = decision.Verdict == DeskVerdict.Pending;

            bribeRoot.SetActive(pending && project.HasBribe);
            if (project.HasBribe)
                bribeText.text = $"Dosyanın arasından bir zarf çıktı.\nİçinde <b>{DeskFormat.Money(project.BribeAmount)}</b> var.";

            SetDecisionButtonsVisible(pending);
            extraTestButton.interactable = canBuyTest;
            extraTestLabel.text = decision.ExtraTestBought
                ? "Ek test yapıldı"
                : $"Ek test iste ({DeskFormat.Money(testCost)})";

            verdictText.text = decision.Verdict switch
            {
                DeskVerdict.Approved => decision.TookBribe ? "ONAYLANDI <size=70%>(zarf alındı)</size>" : "ONAYLANDI",
                DeskVerdict.Rejected => "REDDEDİLDİ",
                _ => "",
            };

            finishButton.gameObject.SetActive(allDecided);
        }

        private void SetDecisionButtonsVisible(bool visible)
        {
            extraTestButton.gameObject.SetActive(visible);
            approveButton.gameObject.SetActive(visible);
            rejectButton.gameObject.SetActive(visible);
        }
    }
}
