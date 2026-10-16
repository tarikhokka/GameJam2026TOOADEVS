using System;
using System.Text;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Denetçi el kitabı: üst bardaki butonla açılan pencere. Kontrol listesi, yönetmelikteki alt sınırlar (ctx.Rules)
    // ve akredite laboratuvarlar (ctx.Names.LabNames). Sadece mevcut kuralları gösterir; hiçbir şeyi değiştirmez.
    public class RulebookView : MonoBehaviour
    {
        [SerializeField] private Button openButton;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        public event Action Toggled;

        private GameContext context;

        private void Awake()
        {
            openButton.onClick.AddListener(() => SetOpen(!panelRoot.activeSelf));
            closeButton.onClick.AddListener(() => SetOpen(false));
            backdropButton.onClick.AddListener(() => SetOpen(false));
        }

        public void Bind(GameContext gameContext)
        {
            context = gameContext;
            panelRoot.SetActive(false);
        }

        public void Close() => panelRoot.SetActive(false);

        private void SetOpen(bool open)
        {
            if (panelRoot.activeSelf == open) return;
            if (open && context != null) bodyText.text = Build(context);
            panelRoot.SetActive(open);
            Toggled?.Invoke();
        }

        private static string Build(GameContext context)
        {
            var sb = new StringBuilder();
            int testCost = context.Balance != null ? context.Balance.ExtraTestCost : 0;

            sb.AppendLine("<b>Kontrol listesi</b>");
            sb.AppendLine("1. Beyan edilen her değer yönetmelikteki alt sınırı karşılamalı.");
            sb.AppendLine("2. Başvuru ile lab raporunda parsel no, tarih ve imza aynı olmalı.");
            sb.AppendLine("3. Raporun başlığındaki laboratuvar ile kaşedeki aynı olmalı ve aşağıdaki listede bulunmalı.");
            sb.AppendLine($"4. Beyana güvenmiyorsan ek test iste ({DeskFormat.Money(testCost)}): gerçek değerleri gösterir.");

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (!IsRelevant(context, type) || context.Rules == null) continue;

                bool header = false;
                foreach (var rule in context.Rules.RulesFor(type))
                {
                    if (!header)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"<b>Yönetmelik · {DeskFormat.BuildingName(type)}</b>");
                        header = true;
                    }
                    string unit = string.IsNullOrEmpty(rule.Unit) ? "" : " " + rule.Unit;
                    sb.AppendLine($"• {rule.DisplayName}: en az {DeskFormat.Number(rule.Required)}{unit}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("<b>Akredite laboratuvarlar</b>");
            var labs = context.Names != null ? context.Names.LabNames : null;
            if (labs == null || labs.Count == 0) sb.AppendLine("• (liste yok)");
            else foreach (var lab in labs) sb.AppendLine($"• {lab}");

            return sb.ToString().TrimEnd();
        }

        // Konut her gün; diğer türler o gün masada varsa ya da günün ayarı izin veriyorsa gösterilir.
        private static bool IsRelevant(GameContext context, BuildingType type)
        {
            if (type == BuildingType.Residential) return true;
            if (type == BuildingType.Factory && context.DayConfig != null && context.DayConfig.AllowFactories) return true;
            return context.TodayProjects.Exists(p => p.BuildingType == type);
        }
    }
}
