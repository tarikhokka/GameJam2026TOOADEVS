using System;
using System.Collections.Generic;
using System.Globalization;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Laboratuvar Raporu (Documents[1]): ayrı belge, Yapı Denetim Formu ile aynı görsel dil (mavi-gri klasör).
    // Başlıkta laboratuvar, künyede parsel ve tarih, tablo Parametre | Beyan | Ek test, kaşe + imza, ücretli ek test.
    // Sahte belge ipuçları (parsel/tarih/imza/lab uyuşmazlığı) iki belgenin karşılaştırılmasıyla bulunur; işaretlenmez.
    public class LabReportView : MonoBehaviour
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        [Header("Başlık")]
        [SerializeField] private TMP_Text labNameText;
        [SerializeField] private TMP_Text dateText;

        [Header("Künye")]
        [SerializeField] private Transform infoContainer;
        [SerializeField] private DottedRowView infoTemplate;

        [Header("Tablo")]
        [SerializeField] private Transform paramContainer;
        [SerializeField] private ParamRowView paramRowTemplate;

        [Header("Kaşe ve imza")]
        [SerializeField] private GameObject stampRoot;
        [SerializeField] private TMP_Text stampText;
        [SerializeField] private TMP_Text signatureText;

        [Header("Ek test")]
        [SerializeField] private Button extraTestButton;
        [SerializeField] private TMP_Text extraTestLabel;

        public event Action ExtraTestClicked;

        private readonly List<DottedRowView> infoRows = new List<DottedRowView>();
        private readonly List<ParamRowView> rows = new List<ParamRowView>();

        private void Awake() => extraTestButton.onClick.AddListener(() => ExtraTestClicked?.Invoke());

        public void Show(GameContext ctx, ProjectData project, bool canBuyTest, int testCost)
        {
            var doc = project.Documents.Count > 1 ? project.Documents[1] : null;

            labNameText.text = doc != null ? Upper(doc.LabName) : "";
            dateText.text = doc != null ? doc.Date : "";

            var info = new[]
            {
                ("Proje", Upper(project.Name)),
                ("Parsel no", doc != null ? doc.ParcelNo : ""),
            };
            UiPool.Resize(infoRows, infoTemplate, infoContainer, info.Length);
            for (int i = 0; i < info.Length; i++) infoRows[i].Bind(info[i].Item1, info[i].Item2);

            UiPool.Resize(rows, paramRowTemplate, paramContainer, project.Params.Count);
            for (int i = 0; i < project.Params.Count; i++)
            {
                var p = project.Params[i];
                rows[i].Bind(
                    DeskFormat.ParamName(ctx.Rules, project.BuildingType, p.Type),
                    Upper(DeskFormat.Value(ctx.Rules, p.Type, p.Reported)),
                    p.Tested ? Upper(DeskFormat.Value(ctx.Rules, p.Type, p.Actual)) : null);
            }

            bool hasStamp = doc != null && !string.IsNullOrEmpty(doc.StampId);
            stampRoot.SetActive(hasStamp);
            if (hasStamp) stampText.text = doc.StampId;
            signatureText.text = doc != null ? $"Raporu düzenleyen: <i>{doc.SignatureName}</i>" : "";

            bool pending = project.Verdict == ProjectVerdict.Pending;
            extraTestButton.gameObject.SetActive(pending || project.ExtraTestBought);
            extraTestButton.interactable = canBuyTest;
            extraTestLabel.text = project.ExtraTestBought
                ? "EK TEST YAPILDI"
                : $"EK TEST İSTE · {Upper(DeskFormat.Money(testCost))}";
        }

        private static string Upper(string s) => string.IsNullOrEmpty(s) ? "" : s.ToUpper(Tr);
    }
}
