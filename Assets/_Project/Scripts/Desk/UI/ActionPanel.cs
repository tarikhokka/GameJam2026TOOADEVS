using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Açık dosyanın sağ sütunu: başvuru sahibi, rüşvet zarfı, ek test ve onay/red butonları.
    // Selamlama ve rüşvet repliği sahnedeki konuşma balonunda; "Mesaiyi bitir" masada (DeskPropsView).
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

        public event Action ExtraTestClicked;
        public event Action ApproveClicked;
        public event Action TakeBribeClicked;
        public event Action RejectClicked;

        private void Awake()
        {
            extraTestButton.onClick.AddListener(() => ExtraTestClicked?.Invoke());
            approveButton.onClick.AddListener(() => ApproveClicked?.Invoke());
            takeBribeButton.onClick.AddListener(() => TakeBribeClicked?.Invoke());
            rejectButton.onClick.AddListener(() => RejectClicked?.Invoke());
        }

        public void ShowEmpty()
        {
            clientNameText.text = "";
            clientLineText.text = "";
            bribeRoot.SetActive(false);
            SetDecisionButtonsVisible(false);
            verdictText.text = "";
        }

        public void Show(ProjectData project, ClientSO client, bool canBuyTest, int testCost)
        {
            clientNameText.text = client != null ? client.DisplayName : "Başvuru sahibi";
            clientLineText.text = project.Name;

            bool pending = project.Verdict == ProjectVerdict.Pending;

            bribeRoot.SetActive(pending && project.OffersBribe);
            if (project.OffersBribe)
                bribeText.text = $"Dosyanın arasında bir zarf var: <b>{DeskFormat.Money(project.BribeAmount)}</b>";

            SetDecisionButtonsVisible(pending);
            extraTestButton.interactable = canBuyTest;
            extraTestLabel.text = project.ExtraTestBought
                ? "Ek test yapıldı"
                : $"Ek test iste ({DeskFormat.Money(testCost)})";

            verdictText.text = project.Verdict switch
            {
                ProjectVerdict.Approved => project.BribeAccepted ? "ONAYLANDI <size=70%>(zarf alındı)</size>" : "ONAYLANDI",
                ProjectVerdict.Rejected => "REDDEDİLDİ",
                _ => "",
            };
        }

        private void SetDecisionButtonsVisible(bool visible)
        {
            extraTestButton.gameObject.SetActive(visible);
            approveButton.gameObject.SetActive(visible);
            rejectButton.gameObject.SetActive(visible);
        }
    }
}
