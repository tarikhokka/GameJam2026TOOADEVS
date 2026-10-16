using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    /// <summary>
    /// Ana menü (MainMenu sahnesi; IGameScreen değil). Yeni Oyun / Devam Et / Emeği Geçenler / Çıkış.
    /// Devam Et sadece SaveSystem.HasSave ise görünür; GameLaunch.ContinueFromSave bayrağıyla Game sahnesi açılır.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] Sprite background;     // menu_bg
        [SerializeField] Sprite logo;           // logo
        [SerializeField] string gameSceneName = "Game";

        // CREDITS_AND_AI.md "Oyun içi Emeği Geçenler metni" ile aynı tutulur (yeni CC BY asset gelince burayı da güncelle).
        const string CreditsText =
            "<b>SESLER</b>\n" +
            "Ortam sesi: \"Suburban House Ambience 02\" — msepitaph (freesound.org), CC BY 4.0\n" +
            "Kağıt sesi: \"paper5\" — florian_reinke (freesound.org), CC0\n" +
            "Arayüz ve kaşe sesleri: Kenney (kenney.nl), CC0";

        GameObject _menuPanel, _creditsPanel;

        void Start()
        {
            var root = UiKit.Stretch(transform, "Root");
            UiKit.Image(root, "Bg", background, background != null ? Color.white : UiKit.Dark, Vector2.zero, Vector2.one);

            _menuPanel = UiKit.Stretch(root, "Menu").gameObject;
            if (logo != null)
            {
                var l = UiKit.Image(_menuPanel.transform, "Logo", logo, Color.white,
                    new Vector2(0.3f, 0.66f), new Vector2(0.7f, 0.92f));
                l.preserveAspect = true;
            }
            else
            {
                var t = UiKit.Text(_menuPanel.transform, "Title", "İMZA", 140, UiKit.Paper, TextAlignmentOptions.Center,
                    new Vector2(0.3f, 0.68f), new Vector2(0.7f, 0.9f));
                t.fontStyle = FontStyles.Bold;
            }

            float y = 0.55f;
            const float h = 0.085f, gap = 0.02f;
            if (SaveSystem.HasSave)
            {
                UiKit.Button(_menuPanel.transform, "Continue", "Devam Et", Continue, Anchor(ref y, h, gap, out var max), max, 38f);
            }
            UiKit.Button(_menuPanel.transform, "New", "Yeni Oyun", NewGame, Anchor(ref y, h, gap, out var max2), max2, 38f);
            UiKit.Button(_menuPanel.transform, "Credits", "Emeği Geçenler", () => SetCredits(true), Anchor(ref y, h, gap, out var max3), max3, 38f);
            UiKit.Button(_menuPanel.transform, "Quit", "Çıkış", Quit, Anchor(ref y, h, gap, out var max4), max4, 38f);

            BuildCredits(root);
            SetCredits(false);
        }

        // Düğmeleri yukarıdan aşağı dizer: min döner, max out ile verilir.
        static Vector2 Anchor(ref float top, float height, float gap, out Vector2 max)
        {
            max = new Vector2(0.4f + 0.2f, top);
            var min = new Vector2(0.4f, top - height);
            top -= height + gap;
            return min;
        }

        void BuildCredits(Transform root)
        {
            _creditsPanel = UiKit.Stretch(root, "Credits").gameObject;
            UiKit.Image(_creditsPanel.transform, "Shade", null, new Color(0f, 0f, 0f, 0.8f), Vector2.zero, Vector2.one);
            UiKit.Image(_creditsPanel.transform, "Panel", null, UiKit.Paper, new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.8f));
            UiKit.Text(_creditsPanel.transform, "Text", CreditsText, 40, UiKit.Ink, TextAlignmentOptions.TopLeft,
                new Vector2(0.23f, 0.38f), new Vector2(0.77f, 0.77f), true, 22);
            UiKit.Button(_creditsPanel.transform, "Back", "Geri", () => SetCredits(false),
                new Vector2(0.42f, 0.28f), new Vector2(0.58f, 0.35f), 32f);
        }

        void SetCredits(bool on)
        {
            _creditsPanel.SetActive(on);
            _menuPanel.SetActive(!on);
        }

        void NewGame()
        {
            GameLaunch.ContinueFromSave = false;
            SceneManager.LoadScene(gameSceneName);
        }

        void Continue()
        {
            GameLaunch.ContinueFromSave = true;
            SceneManager.LoadScene(gameSceneName);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
