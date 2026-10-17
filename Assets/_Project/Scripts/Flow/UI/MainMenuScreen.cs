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
        // (EEE, 17 Ekim) menu_bg'de İMZA logosu ve 3 tahta düğme çizili. Gerçek düğmeler btn_menu ile tam üstlerine oturur;
        // kayıt varsa "Devam Et" en üste girer, diğerleri bir aşağı kayar (resimdeki düğmeler yine kapalı kalır).
        [SerializeField] bool logoInBackground = true;
        [SerializeField] Sprite buttonSprite;   // btn_menu
        [SerializeField] TMP_FontAsset buttonFont; // FiraSans-Bold SDF
        // (EEE, 17 Ekim) Menü canlandırma (MenuAmbience): pencere ışıkları, güneş halesi, toz, açılış.
        [SerializeField] Sprite[] lightLayers;  // menu_lights_0..3 (ilk yarısı söner, ikinci yarısı parlar)
        [SerializeField] Sprite sunGlow;        // menu_glow
        [SerializeField] Sprite dustSprite;     // menu_dust
        [SerializeField] Sprite craneLight;     // menu_redlight (vinç ikaz lambası)
        [SerializeField] Sprite craneLoad;      // menu_load (sallanan kiriş)

        // Resimdeki düğme yuvaları (1600x900 menu_bg'den ölçüldü).
        const float SlotLeft = 0.389f, SlotRight = 0.6026f, SlotTop = 0.498f, SlotHeight = 0.0878f, SlotGap = 0.0155f;

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
            "Müteahhit karakterleri, ofis, belgeler ve ana menü: yapay zekâ (ChatGPT)\n" +
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
            if (logoInBackground && background != null)
            {
                // logo arka planın içinde
            }
            else if (logo != null)
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

            bool slots = buttonSprite != null;
            float y = slots ? SlotTop : 0.55f;
            if (SaveSystem.HasSave)
            {
                MenuButton("Continue", "Devam Et", Continue, ref y);
            }
            MenuButton("New", "Yeni Oyun", NewGame, ref y);
            MenuButton("Credits", "Emeği Geçenler", () => SetCredits(true), ref y);
            MenuButton("Quit", "Çıkış", Quit, ref y);

            BuildCredits(root);
            SetCredits(false);

            // Arka planın üstüne, menünün altına canlandırma katmanları; açılışta karanlıktan gelir.
            var ambience = root.gameObject.AddComponent<MenuAmbience>();
            ambience.Build(root, _menuPanel.transform, lightLayers, sunGlow, dustSprite, craneLight, craneLoad);
            ambience.PlayIntro(_menuPanel.transform);
        }

        // Düğmeleri yukarıdan aşağı dizer (btn_menu varsa resimdeki yuvalara, yoksa eski düz düğmeler).
        void MenuButton(string name, string label, System.Action onClick, ref float top)
        {
            bool slots = buttonSprite != null;
            float left = slots ? SlotLeft : 0.4f, right = slots ? SlotRight : 0.6f;
            float height = slots ? SlotHeight : 0.085f, gap = slots ? SlotGap : 0.02f;
            var btn = UiKit.Button(_menuPanel.transform, name, label, onClick,
                new Vector2(left, top - height), new Vector2(right, top), slots ? 40f : 38f);
            Style(btn);
            top -= height + gap;
        }

        void Style(Button btn)
        {
            if (buttonSprite != null && btn.targetGraphic is Image img)
            {
                img.sprite = buttonSprite;
                img.color = Color.white;
            }
            if (buttonFont != null) btn.GetComponentInChildren<TextMeshProUGUI>().font = buttonFont;
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
            Style(UiKit.Button(_creditsPanel.transform, "Back", "Geri", () => SetCredits(false),
                new Vector2(0.42f, 0.09f), new Vector2(0.58f, 0.16f), 32f));
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
