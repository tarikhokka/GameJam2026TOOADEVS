using System;
using System.Collections.Generic;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Laboratuvar Raporu (Documents[1]), yeni hedef arayüz: künye (laboratuvar, rapor tarihi, proje, ada/parsel), TEST SONUÇLARI
    // tablosu (Parametre | Beyan | Ek test), karar vermeyen değerlendirme cümlesi (builder), yetkili + imza, kırmızı lab kaşesi ve
    // altında kaşedeki lab adı, EK TEST düğmesi (EEE görseli) ve ücreti. Sahte belge ipuçları (parsel / tarih / imza / lab adı)
    // formla karşılaştırılarak bulunur; işaretlenmez.
    public class LabReportView : MonoBehaviour
    {
        [Header("Künye")]
        [SerializeField] private Transform infoContainer;
        [SerializeField] private DottedRowView infoTemplate;

        [Header("Tablo")]
        [SerializeField] private Transform paramContainer;
        [SerializeField] private ParamRowView paramRowTemplate;

        [Header("Kaşe ve imza")]
        [SerializeField] private GameObject stampRoot;
        [SerializeField] private TMP_Text stampText;      // kaşedeki lab adı ("<LAB ADI> · AKREDİTE")
        [SerializeField] private TMP_Text signatureText;  // raporu imzalayan

        [Header("Ek test")]
        [SerializeField] private Button extraTestButton;
        [SerializeField] private TMP_Text extraTestLabel; // ücret ya da "YAPILDI"

        public event Action ExtraTestClicked;

        private readonly List<DottedRowView> infoRows = new List<DottedRowView>();
        private readonly List<ParamRowView> rows = new List<ParamRowView>();
        private bool canBuy, locked;

        private void Awake() => extraTestButton.onClick.AddListener(() => ExtraTestClicked?.Invoke());

        // İnceleme modu açıkken EK TEST pasif.
        public void SetLocked(bool on)
        {
            locked = on;
            extraTestButton.interactable = canBuy && !locked;
        }

        public void Show(GameContext ctx, ProjectData project, bool canBuyTest, int testCost)
        {
            var doc = project.Documents.Count > 1 ? project.Documents[1] : null;

            var info = new[]
            {
                ("Laboratuvar", doc != null ? doc.LabName : ""),
                ("Rapor tarihi", doc != null ? doc.Date : ""),
                ("Proje", project.Name),
                ("Ada / Parsel", doc != null ? doc.ParcelNo : ""),
            };
            UiPool.Resize(infoRows, infoTemplate, infoContainer, info.Length);
            for (int i = 0; i < info.Length; i++) infoRows[i].Bind(info[i].Item1, info[i].Item2);

            // İnceleme modunda tıklanabilir alanlar (Core hedefleri adım 3'te).
            InspectSpot.Mark(infoRows[1], "Lab: Rapor tarihi");
            InspectSpot.Mark(infoRows[3], "Lab: Ada / Parsel");
            InspectSpot.Mark(signatureText, "Lab: Yetkili imzası");

            UiPool.Resize(rows, paramRowTemplate, paramContainer, project.Params.Count);
            for (int i = 0; i < project.Params.Count; i++)
            {
                var p = project.Params[i];
                rows[i].Bind(
                    DeskFormat.ParamName(ctx.Rules, project.BuildingType, p.Type),
                    DeskFormat.Value(ctx.Rules, p.Type, p.Reported),
                    p.Tested ? DeskFormat.Value(ctx.Rules, p.Type, p.Actual) : null);
            }

            bool hasStamp = doc != null && !string.IsNullOrEmpty(doc.StampId);
            stampRoot.SetActive(hasStamp);
            if (hasStamp) stampText.text = doc.StampId;
            signatureText.text = doc != null ? $"<i>{doc.SignatureName}</i>" : "";

            bool pending = project.Verdict == ProjectVerdict.Pending;
            extraTestButton.gameObject.SetActive(pending || project.ExtraTestBought);
            canBuy = canBuyTest;
            extraTestButton.interactable = canBuy && !locked;
            extraTestLabel.text = project.ExtraTestBought ? "YAPILDI" : DeskFormat.Money(testCost);
        }
    }
}
