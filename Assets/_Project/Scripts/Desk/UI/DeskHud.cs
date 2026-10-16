using TMPro;
using UnityEngine;

namespace CivilJam.Desk
{
    // Üst bar: ay başlığı, kasa, kota.
    public class DeskHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelTitleText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text quotaText;

        public void Refresh(DeskSession session)
        {
            levelTitleText.text = session.Config.LevelTitle;
            moneyText.text = $"Kasa: {DeskFormat.Money(session.Money)}";
            quotaText.text = $"Onay: {session.ApprovedCount} / {session.Config.Quota}";
        }
    }
}
