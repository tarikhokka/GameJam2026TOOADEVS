using CivilJam.Core;
using TMPro;
using UnityEngine;

namespace CivilJam.Flow
{
    /// <summary>Gazetesiz gün özeti (GamePhase.DaySummary). Afet olmayan günün sonunda açılır.</summary>
    public class DaySummaryScreen : FlowScreen
    {
        [SerializeField] Sprite background;     // daysummary_bg

        public override GamePhase Phase => GamePhase.DaySummary;

        TextMeshProUGUI _title, _body;

        protected override void Build()
        {
            var root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Bg", background, background != null ? Color.white : UiKit.Dark, Vector2.zero, Vector2.one);
            UiKit.Image(root, "Panel", null, new Color(UiKit.Paper.r, UiKit.Paper.g, UiKit.Paper.b, 0.94f),
                new Vector2(0.27f, 0.2f), new Vector2(0.73f, 0.84f));

            _title = UiKit.Text(root, "Title", "", 60, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.29f, 0.72f), new Vector2(0.71f, 0.82f), true, 30);
            _title.fontStyle = FontStyles.Bold;
            _body = UiKit.Text(root, "Body", "", 38, UiKit.Ink, TextAlignmentOptions.TopLeft,
                new Vector2(0.30f, 0.30f), new Vector2(0.70f, 0.70f), true, 22);

            UiKit.Button(root, "Continue", "Günlük hesaplaşmaya geç  →", Finish,
                new Vector2(0.38f, 0.22f), new Vector2(0.62f, 0.29f), 30f);
        }

        protected override void Refresh()
        {
            var s = Context.State;
            int standing = 0;
            foreach (var b in s.Buildings) if (b.Status == BuildingStatus.Standing) standing++;

            _title.text = $"{s.Day}. Gün Sona Erdi";
            _body.text =
                "Şehir bugün sakin geçti.\n\n" +
                $"Bugün onaylanan: {s.ApprovedToday}\n" +
                $"Ayakta duran bina: {standing}\n" +
                $"Kasa: {UiKit.Money(s.Money)}\n" +
                $"Borç: {UiKit.Money(s.Debt)}";
        }
    }
}
