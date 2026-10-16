using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Soldaki dosya listesinde tek bir proje klasörü.
    public class ProjectQueueItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;

        [SerializeField] private Color normalColor = new Color(0.86f, 0.78f, 0.6f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.93f, 0.72f);
        [SerializeField] private Color approvedColor = new Color(0.25f, 0.55f, 0.3f);
        [SerializeField] private Color rejectedColor = new Color(0.7f, 0.22f, 0.2f);
        [SerializeField] private Color pendingColor = new Color(0.3f, 0.3f, 0.3f);

        public void Bind(DeskProject project, DeskDecision decision, bool selected, Action onClick)
        {
            nameText.text = $"<b>{project.Name}</b>\n<size=80%>{project.BuildingType}</size>";
            background.color = selected ? selectedColor : normalColor;

            switch (decision.Verdict)
            {
                case DeskVerdict.Approved:
                    statusText.text = "ONAYLANDI";
                    statusText.color = approvedColor;
                    break;
                case DeskVerdict.Rejected:
                    statusText.text = "REDDEDİLDİ";
                    statusText.color = rejectedColor;
                    break;
                default:
                    statusText.text = decision.ExtraTestBought ? "Bekliyor · test yapıldı" : "Bekliyor";
                    statusText.color = pendingColor;
                    break;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }
}
