using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masanın üstündekiler (dosya kapalıyken): müteahhitle gelen LAB RAPORU kitabı (tıklayınca dosya açılır),
    // rüşvet varsa zarf, alt kenarda müteahhit/proje adı, gün bitince "Mesaiyi bitir".
    // EL KİTABI kitabı da masada durur ama onu RulebookView dinler.
    public class DeskPropsView : MonoBehaviour
    {
        [SerializeField] private Button labBook;
        [SerializeField] private GameObject envelopeRoot;
        [SerializeField] private TMP_Text envelopeText;
        [SerializeField] private TMP_Text clientTagText;
        [SerializeField] private Button finishButton;

        public event Action LabBookClicked;
        public event Action FinishClicked;

        private void Awake()
        {
            labBook.onClick.AddListener(() => LabBookClicked?.Invoke());
            finishButton.onClick.AddListener(() => FinishClicked?.Invoke());
        }

        public void Show(ProjectData project, ClientSO client)
        {
            bool pending = project.Verdict == ProjectVerdict.Pending;
            labBook.gameObject.SetActive(pending);
            envelopeRoot.SetActive(pending && project.OffersBribe);
            if (project.OffersBribe) envelopeText.text = $"ZARF\n<b>{DeskFormat.Money(project.BribeAmount)}</b>";

            string name = client != null ? client.DisplayName : "Başvuru sahibi";
            clientTagText.text = $"{name}  ·  {project.Name}";
            finishButton.gameObject.SetActive(false);
        }

        public void ShowEmpty(bool allDecided)
        {
            labBook.gameObject.SetActive(false);
            envelopeRoot.SetActive(false);
            clientTagText.text = allDecided ? "Bugünkü başvurular bitti." : "";
            finishButton.gameObject.SetActive(allDecided);
        }
    }
}
