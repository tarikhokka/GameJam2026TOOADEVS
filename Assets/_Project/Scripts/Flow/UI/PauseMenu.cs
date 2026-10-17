using System.Collections.Generic;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CivilJam.Flow
{
    // Runs before the intro and EventSystem so Escape blocks this frame's input too.
    [DefaultExecutionOrder(-10000)]
    public sealed class PauseMenu : MonoBehaviour
    {
        static PauseMenu _instance;
        static int _resumeFrame = -1;

        public static bool IsOpen => _instance != null && _instance._open;
        public static bool InputBlocked => IsOpen || Time.frameCount <= _resumeFrame;
        public static float AnimationDeltaTime => InputBlocked ? 0f : Time.unscaledDeltaTime;

        readonly List<GraphicRaycaster> _blockedRaycasters = new List<GraphicRaycaster>();
        readonly List<MonoBehaviour> _blockedDebug = new List<MonoBehaviour>();
        GameObject _panel, _menuButton;
        Button _resumeButton;
        TMP_Text _notice;
        // (A, 17 Ekim gece — Ahmet isteği) Ayarlar: ana ses + tam ekran. Ana grup (başlık, not, düğmeler) ile ayarlar grubu yer değiştirir.
        readonly List<GameObject> _mainItems = new List<GameObject>();
        GameObject _settings;
        RectTransform _quit;
        Slider _volume;
        TMP_Text _volumeValue, _fullscreenLabel;
        bool _menuButtonWanted;
        const string VolumeKey = "imza.masterVolume";
        GraphicRaycaster _raycaster;
        bool _open, _supportedScene;
        float _previousTimeScale;
        bool _previousAudioPause;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _resumeFrame = -1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (_instance == null)
                new GameObject("FlowPauseMenu", typeof(RectTransform), typeof(PauseMenu));
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Build();
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _raycaster = gameObject.AddComponent<GraphicRaycaster>();

            var root = UiKit.Stretch(transform, "PausePanel");
            _panel = root.gameObject;
            var shield = UiKit.Image(root, "Shield", null, new Color(0, 0, 0, 0.8f), Vector2.zero, Vector2.one);
            shield.raycastTarget = true;
            UiKit.Image(root, "Paper", null, UiKit.Paper, new Vector2(0.25f, 0.17f), new Vector2(0.75f, 0.83f));
            _mainItems.Add(UiKit.Text(root, "Title", "Oyun menüsü", 58f, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.28f, 0.69f), new Vector2(0.72f, 0.79f)).gameObject);
            _notice = UiKit.Text(root, "Notice", "", 26f, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.29f, 0.57f), new Vector2(0.71f, 0.68f), true, 20f);
            _mainItems.Add(_notice.gameObject);
            _resumeButton = UiKit.Button(root, "Resume", "Devam", Resume,
                new Vector2(0.36f, 0.475f), new Vector2(0.64f, 0.545f), 34f, true);
            _mainItems.Add(_resumeButton.gameObject);
            _mainItems.Add(UiKit.Button(root, "Settings", "Ayarlar", ShowSettings,
                new Vector2(0.36f, 0.385f), new Vector2(0.64f, 0.455f), 34f, true).gameObject);
            _menuButton = UiKit.Button(root, "MainMenu", "Ana menüye dön", ReturnToMenu,
                new Vector2(0.36f, 0.295f), new Vector2(0.64f, 0.365f), 34f, true).gameObject;
            _mainItems.Add(_menuButton);
            _quit = (RectTransform)UiKit.Button(root, "Quit", "Çıkış", Quit,
                new Vector2(0.36f, 0.205f), new Vector2(0.64f, 0.275f), 34f, true).transform;
            _mainItems.Add(_quit.gameObject);
            BuildSettings(root);
            ApplySavedVolume();
            FixFullscreenResolution();
            _panel.SetActive(false);
        }

        // ---------- Ayarlar (A, Ahmet isteği) ----------

        void BuildSettings(RectTransform root)
        {
            var group = UiKit.Stretch(root, "SettingsGroup");
            _settings = group.gameObject;
            UiKit.Text(group, "Title", "Ayarlar", 58f, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.28f, 0.69f), new Vector2(0.72f, 0.79f));

            UiKit.Text(group, "VolumeLabel", "Ses", 34f, UiKit.Ink, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.31f, 0.56f), new Vector2(0.45f, 0.63f));
            _volumeValue = UiKit.Text(group, "VolumeValue", "", 34f, UiKit.Ink, TextAlignmentOptions.MidlineRight,
                new Vector2(0.55f, 0.56f), new Vector2(0.69f, 0.63f));
            _volume = BuildSlider(group, new Vector2(0.31f, 0.5f), new Vector2(0.69f, 0.55f));
            _volume.onValueChanged.AddListener(SetVolume);

            var fullscreen = UiKit.Button(group, "Fullscreen", "", ToggleFullscreen,
                new Vector2(0.36f, 0.36f), new Vector2(0.64f, 0.43f), 30f, true);
            _fullscreenLabel = fullscreen.GetComponentInChildren<TMP_Text>();

            UiKit.Button(group, "Back", "Geri", ShowMain,
                new Vector2(0.36f, 0.22f), new Vector2(0.64f, 0.29f), 34f, true);
            _settings.SetActive(false);
        }

        static Slider BuildSlider(RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = UiKit.Rect(parent, "VolumeSlider", anchorMin, anchorMax);
            var background = UiKit.Image(rt, "Background", null, new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.25f),
                new Vector2(0f, 0.3f), new Vector2(1f, 0.7f));
            background.raycastTarget = true;
            var fillArea = UiKit.Rect(rt, "Fill Area", new Vector2(0f, 0.3f), new Vector2(1f, 0.7f));
            var fill = UiKit.Image(fillArea, "Fill", null, UiKit.Accent, Vector2.zero, Vector2.one);
            var handleArea = UiKit.Rect(rt, "Handle Slide Area", Vector2.zero, Vector2.one);
            var handle = UiKit.Image(handleArea, "Handle", null, UiKit.Dark, Vector2.zero, Vector2.one);
            handle.rectTransform.sizeDelta = new Vector2(28f, 0f);
            handle.raycastTarget = true;

            var slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        void ShowSettings()
        {
            foreach (var item in _mainItems) item.SetActive(false);
            _settings.SetActive(true);
            _volume.SetValueWithoutNotify(AudioListener.volume);
            RefreshSettings();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_volume.gameObject);
        }

        void ShowMain()
        {
            _settings.SetActive(false);
            foreach (var item in _mainItems) item.SetActive(true);
            _menuButton.SetActive(_menuButtonWanted);
            // Ana menüde "Ana menüye dön" yok: Çıkış onun yerine çıkar, arada boşluk kalmaz
            _quit.anchorMin = new Vector2(0.36f, _menuButtonWanted ? 0.205f : 0.295f);
            _quit.anchorMax = new Vector2(0.64f, _menuButtonWanted ? 0.275f : 0.365f);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
        }

        void SetVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
            PlayerPrefs.Save();
            RefreshSettings();
        }

        void ToggleFullscreen()
        {
            bool target = !CurrentFullscreen();
            // Sadece Screen.fullScreen değiştirmek eski (küçük) çözünürlüğü tam ekrana gererdi → bulanık görüntü.
            // Tam ekran: ekranın gerçek çözünürlüğü; pencere: ekranın %80'i, 16:9.
            int sw = Display.main.systemWidth, sh = Display.main.systemHeight;
            if (target) Screen.SetResolution(sw, sh, FullScreenMode.FullScreenWindow);
            else
            {
                int w = Mathf.RoundToInt(sw * 0.8f), h = Mathf.RoundToInt(w * 9f / 16f);
                if (h > sh * 0.85f) { h = Mathf.RoundToInt(sh * 0.8f); w = Mathf.RoundToInt(h * 16f / 9f); }
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
            }
            _pendingFullscreen = target ? 1 : 0; // Unity değişikliği bir sonraki karede uygular; etiket hemen yeni hali göstersin
            RefreshSettings();
        }

        int _pendingFullscreen = -1; // -1: Screen.fullScreen'i oku; 0/1: az önce kapattık/açtık

        bool CurrentFullscreen() => _pendingFullscreen < 0 ? Screen.fullScreen : _pendingFullscreen == 1;

        void RefreshSettings()
        {
            _volumeValue.text = $"%{Mathf.RoundToInt(AudioListener.volume * 100f)}";
            bool full = CurrentFullscreen();
            _fullscreenLabel.text = full ? "Tam ekran: Açık" : "Tam ekran: Kapalı";
        }

        // Açılışta tam ekransa ama çözünürlük ekranınkinden düşükse (eski kayıtlı ayar), gerçek çözünürlüğe çek: bulanıklık olmasın.
        static void FixFullscreenResolution()
        {
            if (Application.isEditor || Screen.fullScreenMode == FullScreenMode.Windowed) return;
            int sw = Display.main.systemWidth, sh = Display.main.systemHeight;
            if (Screen.width < sw || Screen.height < sh) Screen.SetResolution(sw, sh, Screen.fullScreenMode);
        }

        static void ApplySavedVolume()
        {
            if (PlayerPrefs.HasKey(VolumeKey)) AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        }

        void Update()
        {
            if (!_supportedScene || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;
            if (_open) Resume();
            else Open();
        }

        public void Open()
        {
            if (_open || !_supportedScene) return;
            _previousTimeScale = Time.timeScale;
            _previousAudioPause = AudioListener.pause;
            _open = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;

            foreach (var caster in FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Exclude))
            {
                if (caster == _raycaster || !caster.enabled) continue;
                _blockedRaycasters.Add(caster);
                caster.enabled = false;
            }
            // Disables demo keys and IMGUI fallback/debug buttons without changing Core/Desk.
            BlockDebug(FindAnyObjectByType<GameManager>());
            BlockDebug(FindAnyObjectByType<FlowSandboxBootstrap>());

            bool atMenu = SceneManager.GetActiveScene().name == "MainMenu";
            bool inIntro = FindAnyObjectByType<IntroSequence>() != null;
            _menuButtonWanted = !atMenu || inIntro;
            _pendingFullscreen = -1;
            ShowMain(); // menü her açılışta ana gruptan başlar (ayarlar açık kalmaz)
            _notice.text = atMenu
                ? "Devam etmek için Devam'a veya Esc tuşuna bas."
                : "Oyun duraklatıldı.\nAna menüye döner veya çıkarsan gün içindeki ilerleme kaydedilmez. Son gün sonu kaydın korunur.";
            _panel.SetActive(true);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
        }

        void BlockDebug(MonoBehaviour behaviour)
        {
            if (behaviour == null || !behaviour.enabled) return;
            _blockedDebug.Add(behaviour);
            behaviour.enabled = false;
        }

        public void Resume()
        {
            if (!_open) return;
            _open = false;
            _resumeFrame = Time.frameCount;
            Time.timeScale = _previousTimeScale;
            AudioListener.pause = _previousAudioPause;
            foreach (var caster in _blockedRaycasters)
                if (caster != null) caster.enabled = true;
            foreach (var behaviour in _blockedDebug)
                if (behaviour != null) behaviour.enabled = true;
            _blockedRaycasters.Clear();
            _blockedDebug.Clear();
            _panel.SetActive(false);
            // The Resume click/Enter must not also submit a button behind the menu.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void ReturnToMenu()
        {
            Resume();
            GameLaunch.ContinueFromSave = false;
            SceneManager.LoadScene("MainMenu");
        }

        void Quit()
        {
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Resume();
            _supportedScene = scene.name == "MainMenu" || scene.name == "Game"
                || scene.name == "Sandbox_Flow" || scene.name == "Sandbox_Desk";
        }

        void OnDisable() => Resume();

        void OnDestroy()
        {
            Resume();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this) _instance = null;
        }
    }
}
