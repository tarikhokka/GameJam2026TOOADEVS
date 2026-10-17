using System;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Masanın üstündekiler: müteahhit oturunca ortada (isimliğin önünde) YAPI DENETİM FORMU klasörü ve solda LAB RAPORU
    // kitabı belirir (tıklayınca kendi pencereleri açılır), rüşvet varsa zarf, üst barda müteahhit/proje adı,
    // gün bitince "Mesaiyi bitir". EL KİTABI kitabı da masada durur ama onu RulebookView dinler.
    public class DeskPropsView : MonoBehaviour
    {
        [SerializeField] private Button formFolder;
        [SerializeField] private GameObject formFolderRoot; // klasör + gölgesi (DeskBookProp)
        [SerializeField] private Button labBook;
        [SerializeField] private GameObject labBookRoot;    // kitap + gölgesi (DeskBookProp)
        [SerializeField] private GameObject envelopeRoot;
        [SerializeField] private TMP_Text envelopeText;
        [SerializeField] private TMP_Text clientTagText;
        [SerializeField] private Button finishButton;

        public event Action FormClicked;
        public event Action LabBookClicked;
        public event Action FinishClicked;

        private void Awake()
        {
            if (formFolder != null) formFolder.onClick.AddListener(() => FormClicked?.Invoke());
            if (labBook != null) labBook.onClick.AddListener(() => LabBookClicked?.Invoke());
            finishButton.onClick.AddListener(() => FinishClicked?.Invoke());
        }

        private GameObject FormObject => formFolderRoot != null ? formFolderRoot : formFolder != null ? formFolder.gameObject : null;
        private GameObject LabBookObject => labBookRoot != null ? labBookRoot : labBook != null ? labBook.gameObject : null;

        public void Show(ProjectData project, ClientSO client)
        {
            bool pending = project.Verdict == ProjectVerdict.Pending;
            FormObject?.SetActive(pending);
            LabBookObject?.SetActive(pending);
            envelopeRoot.SetActive(pending && project.OffersBribe);
            if (project.OffersBribe) envelopeText.text = $"ZARF\n<b>{DeskFormat.Money(project.BribeAmount)}</b>";

            string name = client != null ? client.DisplayName : "Başvuru sahibi";
            clientTagText.text = $"{name}  ·  {project.Name}";
            finishButton.gameObject.SetActive(false);
        }

        public void ShowEmpty(bool allDecided)
        {
            FormObject?.SetActive(false);
            LabBookObject?.SetActive(false);
            envelopeRoot.SetActive(false);
            clientTagText.text = allDecided ? "Bugünkü başvurular bitti." : "";
            // (EEE, A ve B onayı) "Mesaiyi bitir" artık gösterilmez: DeskScreen mesai zili + geçiş ekranıyla kendisi geçer.
            finishButton.gameObject.SetActive(false);
        }
    }
}
