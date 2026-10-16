using System;
using System.Collections.Generic;
using CivilJam.Core;
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

        public void Show(ProjectData project, ClientSO client, bool canBuyTest, int testCost, bool allDecided)
        {
            clientNameText.text = client != null ? client.DisplayName : "Başvuru sahibi";
            string greeting = PickLine(client?.GreetingLines, project.Id);
            clientLineText.text = string.IsNullOrEmpty(greeting) ? "" : $"“{greeting}”";

            bool pending = project.Verdict == ProjectVerdict.Pending;

            bribeRoot.SetActive(pending && project.OffersBribe);
            if (project.OffersBribe)
            {
                string bribeLine = PickLine(client?.BribeLines, project.Id);
                string quote = string.IsNullOrEmpty(bribeLine) ? "" : $"<i>“{bribeLine}”</i>\n";
                bribeText.text = $"{quote}Dosyanın arasında bir zarf var: <b>{DeskFormat.Money(project.BribeAmount)}</b>";
            }

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

            finishButton.gameObject.SetActive(allDecided);
        }

        // Aynı proje her çizimde aynı repliği göstersin diye proje kimliğine göre seçilir.
        private static string PickLine(List<string> lines, string projectId)
        {
            if (lines == null || lines.Count == 0) return null;
            int hash = 0;
            foreach (char c in projectId) hash = hash * 31 + c;
            return lines[Mathf.Abs(hash) % lines.Count];
        }

        private void SetDecisionButtonsVisible(bool visible)
        {
            extraTestButton.gameObject.SetActive(visible);
            approveButton.gameObject.SetActive(visible);
            rejectButton.gameObject.SetActive(visible);
        }
    }
}
