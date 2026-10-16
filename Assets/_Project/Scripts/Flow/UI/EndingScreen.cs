using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// Son ekranı (GamePhase.Ending). Metin/görsel ctx.Endings (EndingSO) içinden gelir; yoksa yedek metin kullanılır.
    /// Oyun bittiği için Completed tetiklenmez; "Ana menüye dön" doğrudan MainMenu sahnesini yükler.
    /// </summary>
    public class EndingScreen : FlowScreen
    {
        [SerializeField] Sprite goodImage;      // ending_good
        [SerializeField] Sprite badImage;       // ending_bad
        [SerializeField] Sprite closedImage;    // ending_closed
        [SerializeField] string menuSceneName = "MainMenu";

        public override GamePhase Phase => GamePhase.Ending;

        Image _bg;
        TextMeshProUGUI _title, _body, _stats;

        protected override void Build()
        {
            var root = UiKit.Stretch(transform, "Root");
            _bg = UiKit.Image(root, "Bg", null, UiKit.Dark, Vector2.zero, Vector2.one);
            UiKit.Image(root, "Shade", null, new Color(0f, 0f, 0f, 0.55f), Vector2.zero, Vector2.one);

            _title = UiKit.Text(root, "Title", "", 84, UiKit.Paper, TextAlignmentOptions.Center,
                new Vector2(0.15f, 0.72f), new Vector2(0.85f, 0.88f), true, 40);
            _title.fontStyle = FontStyles.Bold;
            _body = UiKit.Text(root, "Body", "", 42, UiKit.Paper, TextAlignmentOptions.Top,
                new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.7f), true, 24);
            _stats = UiKit.Text(root, "Stats", "", 32, new Color(UiKit.Paper.r, UiKit.Paper.g, UiKit.Paper.b, 0.85f),
                TextAlignmentOptions.Top, new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.4f), true, 20);

            UiKit.Button(root, "Menu", "Ana menüye dön", () => SceneManager.LoadScene(menuSceneName),
                new Vector2(0.4f, 0.07f), new Vector2(0.6f, 0.14f), 32f);
        }

        protected override void Refresh()
        {
            var s = Context.State;
            var type = s.Ending;

            string title, body;
            Sprite image;
            Fallback(type, out title, out body, out image);

            var so = FindEnding(type);
            if (so != null)
            {
                if (!string.IsNullOrEmpty(so.Title)) title = so.Title;
                if (!string.IsNullOrEmpty(so.Body)) body = so.Body;
                if (so.Image != null) image = so.Image;
            }

            _bg.sprite = image;
            _bg.color = image != null ? Color.white : UiKit.Dark;
            _title.text = title;
            _body.text = body;
            _stats.text =
                $"Geçen gün: {s.Day}\n" +
                $"Onaylanan bina: {s.TotalApproved}   Yıkılan bina: {s.TotalCollapses}\n" +
                $"Alınan rüşvet: {s.TotalBribesTaken}\n" +
                $"Kasa: {UiKit.Money(s.Money)}   Borç: {UiKit.Money(s.Debt)}";
        }

        EndingSO FindEnding(EndingType type)
        {
            if (Context.Endings == null) return null;
            foreach (var e in Context.Endings)
                if (e != null && e.Type == type) return e;
            return null;
        }

        void Fallback(EndingType type, out string title, out string body, out Sprite image)
        {
            switch (type)
            {
                case EndingType.Good:
                    title = "Dürüst İmza";
                    body = "Hedef güne ulaştın. İmzan, bu şehirde ayakta kalan binaların arkasında durdu.";
                    image = goodImage;
                    break;
                case EndingType.Bad:
                    title = "Mahkeme";
                    body = "İmzanın altındaki dosyalar mahkemeye taşındı. Kürsüde anlatılan her şey, masanda verdiğin kararların sonucuydu.";
                    image = badImage;
                    break;
                case EndingType.MiddleClosed:
                    title = "Ofis Kapandı";
                    body = "Borçlar ofisin kapısına dayandı. Mühürlenen kapının ardında kalan imzalar artık başkalarının sorumluluğunda.";
                    image = closedImage;
                    break;
                case EndingType.MiddleCollapsed: // (A, Core: MiddleQuota yerine; kota kaldırıldı)
                    title = "Enkazın Gölgesi";
                    body = "Mahkeme kapına gelmedi. Ama imzanı taşıyan bir bina artık yok ve o binada yaşayanların yokluğu her sabah masanda.";
                    image = badImage;
                    break;
                default:
                    title = "Son";
                    body = "";
                    image = null;
                    break;
            }
        }
    }
}
