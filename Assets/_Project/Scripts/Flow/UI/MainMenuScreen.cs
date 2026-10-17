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
        // İki sütun: sol = oyun/ekip/yapay zekâ, sağ = sesler/görseller/yazı tipleri. (EEE, 17 Ekim)
        const string CreditsLeft =
            "<size=130%><b>İMZA</b></size>\n" +
            "TOOADEVS · Civil Game Jam 2026\n\n" +
            "<b>EKİP</b>\n" +
            "Ahmet İlman — Oyun mantığı ve sistemler\n" +
            "Oğuzhan Yurt — Masa ekranı\n" +
            "Onur Kabadayılar — Akış ekranları ve menü\n" +
            "Tarık Hokkaömeroğlu — Görsel, ses ve içerik\n\n" +
            "<b>YAPAY ZEKÂ</b>\n" +
            "Bu oyunun kodunda, metinlerinde ve bazı görsellerinde yapay zekâ araçları " +
            "(Claude, ChatGPT/Codex, Gemini) kullanıldı. Ayrıntılı liste: CREDITS_AND_AI.md";

        const string CreditsRight =
            "<b>SESLER VE MÜZİKLER</b>\n" +
            "Ortam sesi: \"Suburban House Ambience 02\" — msepitaph (freesound.org), CC BY 4.0\n" +
            "Menü müziği: \"Ambient Paulstretched Piano\" — waveplaysfx (freesound.org), CC0\n" +
            "Son müziği: \"The Sad Piano\" — dasgoat (freesound.org), CC0\n" +
            "Kağıt sesi: \"paper5\" — florian_reinke (freesound.org), CC0\n" +
            "Cam kırılma sesi: \"Breaking Glass\" — hifijohn (freesound.org), CC0\n" +
            "Arayüz ve kaşe sesleri: Kenney (kenney.nl), CC0\n" +
            "Yazı sesi: \"Medium Text Blip\" — malakme (freesound.org), CC0\n\n" +
            "<b>GÖRSELLER</b>\n" +
            "Müteahhit karakterleri ve ofis: yapay zekâ (ChatGPT)\n" +
            "Gazete şablonu: yapay zekâ (Gemini)\n\n" +
            "<b>YAZI TİPLERİ</b>\n" +
            "Liberation Sans — SIL Open Font License 1.1\n" +
            "Lilita One (logo) — Juan Montoreano, SIL Open Font License 1.1\n" +
            "Paytone One — The Paytone Project Authors, SIL OFL 1.1\n" +
            "Fira Sans — Mozilla Foundation / Telefonica, SIL OFL 1.1";

        GameObject _menuPanel, _creditsPanel;
        Transform _root;
        bool _launching;

        void Start()
        {
            var root = UiKit.Stretch(transform, "Root");
            _root = root;
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
            UiKit.Image(_creditsPanel.transform, "Panel", null, UiKit.Paper, new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.93f));
            var title = UiKit.Text(_creditsPanel.transform, "Title", "Emeği Geçenler", 60, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.91f), true, 36);
            title.fontStyle = FontStyles.Bold;
            UiKit.Image(_creditsPanel.transform, "Rule", null, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.35f),
                new Vector2(0.12f, 0.81f), new Vector2(0.88f, 0.814f));
            UiKit.Text(_creditsPanel.transform, "Left", CreditsLeft, 30, UiKit.Ink, TextAlignmentOptions.TopLeft,
                new Vector2(0.12f, 0.2f), new Vector2(0.47f, 0.78f), true, 18);
            UiKit.Text(_creditsPanel.transform, "Right", CreditsRight, 30, UiKit.Ink, TextAlignmentOptions.TopLeft,
                new Vector2(0.51f, 0.2f), new Vector2(0.88f, 0.78f), true, 18);
            UiKit.Button(_creditsPanel.transform, "Back", "Geri", () => SetCredits(false),
                new Vector2(0.42f, 0.09f), new Vector2(0.58f, 0.16f), 32f);
        }

        void SetCredits(bool on)
        {
            _creditsPanel.SetActive(on);
            _menuPanel.SetActive(!on);
        }

        void NewGame()
        {
            if (_launching) return;
            _launching = true;
            GameLaunch.ContinueFromSave = false;
            _menuPanel.SetActive(false);
            _creditsPanel.SetActive(false);

            var intro = UiKit.Stretch(_root, "StoryIntro");
            UiKit.Image(intro, "Background", null, UiKit.Dark, Vector2.zero, Vector2.one);
            var content = UiKit.Stretch(intro, "Content");
            var fade = content.gameObject.AddComponent<CanvasGroup>();
            fade.alpha = 0f;
            var story = UiKit.Text(content, "Story", "", 48f, UiKit.Paper,
                TextAlignmentOptions.Center, new Vector2(0.16f, 0.25f), new Vector2(0.84f, 0.8f), true, 32f);
            var hint = UiKit.Text(intro, "Hint", "", 26f, UiKit.Paper,
                TextAlignmentOptions.Center, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.16f));
            UiKit.Text(intro, "FictionNotice", "Bu hikâyedeki ülke, kişiler ve olaylar kurgusaldır.",
                20f, UiKit.Paper, TextAlignmentOptions.Center,
                new Vector2(0.12f, 0.02f), new Vector2(0.88f, 0.07f));
            intro.gameObject.AddComponent<IntroSequence>().Initialize(story, fade, hint, gameSceneName);
        }

        void Continue()
        {
            if (_launching) return;
            _launching = true;
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
