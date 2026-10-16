using System.Collections.Generic;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// Günlük hesaplaşma (GamePhase.DailyExpenses). Kalemleri gösterir; seçimi ctx.Actions.SetExpensePaid ile iletir.
    /// Para/borç hesabı Core'dadır; buradaki "ödeme sonrası kasa" sadece önizlemedir.
    /// </summary>
    public class DailyExpensesScreen : FlowScreen
    {
        [SerializeField] Sprite background;     // payment_bg

        public override GamePhase Phase => GamePhase.DailyExpenses;

        TextMeshProUGUI _title, _totals;
        RectTransform _list;
        Button _confirm;

        protected override void Build()
        {
            var root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Bg", background, background != null ? Color.white : UiKit.Dark, Vector2.zero, Vector2.one);
            UiKit.Image(root, "Panel", null, new Color(UiKit.Paper.r, UiKit.Paper.g, UiKit.Paper.b, 0.95f),
                new Vector2(0.2f, 0.12f), new Vector2(0.8f, 0.88f));

            _title = UiKit.Text(root, "Title", "", 56, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.22f, 0.78f), new Vector2(0.78f, 0.86f), true, 28);
            _title.fontStyle = FontStyles.Bold;

            _list = UiKit.Rect(root, "List", new Vector2(0.23f, 0.42f), new Vector2(0.77f, 0.76f));
            var layout = _list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _totals = UiKit.Text(root, "Totals", "", 34, UiKit.Ink, TextAlignmentOptions.TopLeft,
                new Vector2(0.23f, 0.22f), new Vector2(0.77f, 0.41f), true, 20);

            _confirm = UiKit.Button(root, "Confirm", "Ödemeyi onayla  →", Finish,
                new Vector2(0.38f, 0.13f), new Vector2(0.62f, 0.2f), 30f);
        }

        protected override void Refresh()
        {
            var pending = Context.PendingExpenses;
            _title.text = pending != null ? $"{pending.Day}. Gün Hesaplaşması" : "Günlük Hesaplaşma";

            UiKit.ClearChildren(_list);
            if (pending != null)
            {
                foreach (var line in pending.Lines) AddRow(line);
            }
            UpdateTotals();
        }

        void AddRow(ExpenseLine line)
        {
            var row = UiKit.Rect(_list, "Row_" + line.ExpenseId, Vector2.zero, Vector2.one);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;

            var frame = row.gameObject.AddComponent<Image>();
            frame.color = new Color(1f, 1f, 1f, 0.35f);

            // Onay kutusu
            var box = UiKit.Image(row, "Box", null, UiKit.Ink, new Vector2(0.02f, 0.18f), new Vector2(0.07f, 0.82f));
            var mark = UiKit.Image(box.transform, "Mark", null, UiKit.Paper, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f));
            var toggle = box.gameObject.AddComponent<Toggle>();
            box.raycastTarget = true;
            toggle.targetGraphic = box;
            toggle.graphic = mark;
            toggle.isOn = line.Paid;
            toggle.onValueChanged.AddListener(on =>
            {
                Context.Actions.SetExpensePaid(line.ExpenseId, on);
                UpdateTotals();
            });

            string name = line.DisplayName + (line.Mandatory ? "  <size=60%>(zorunlu)</size>" : "");
            UiKit.Text(row, "Name", name, 34, UiKit.Ink, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.10f, 0f), new Vector2(0.68f, 1f), true, 20);
            UiKit.Text(row, "Amount", UiKit.Money(line.Amount), 34, UiKit.Ink, TextAlignmentOptions.MidlineRight,
                new Vector2(0.68f, 0f), new Vector2(0.98f, 1f), true, 20);
        }

        void UpdateTotals()
        {
            var s = Context.State;
            var pending = Context.PendingExpenses;
            int selected = pending != null ? pending.TotalSelected : 0;
            int unpaid = 0;
            if (pending != null)
                foreach (var l in pending.Lines) if (!l.Paid) unpaid += l.Amount;

            string text =
                $"Ödenecek toplam: {UiKit.Money(selected)}\n" +
                $"Kasa: {UiKit.Money(s.Money)}  →  ödeme sonrası: {UiKit.Money(s.Money - selected)}\n" +
                $"Mevcut borç: {UiKit.Money(s.Debt)}";
            if (unpaid > 0)
                text += $"\n<size=75%>Ödenmeyen giderler ({UiKit.Money(unpaid)}) borç olarak birikebilir.</size>";
            _totals.text = text;
        }
    }
}
