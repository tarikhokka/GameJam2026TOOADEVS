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
            UiKit.Text(root, "Title", "Oyun menüsü", 58f, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.28f, 0.68f), new Vector2(0.72f, 0.78f));
            _notice = UiKit.Text(root, "Notice", "", 26f, UiKit.Ink, TextAlignmentOptions.Center,
                new Vector2(0.29f, 0.53f), new Vector2(0.71f, 0.67f), true, 22f);
            _resumeButton = UiKit.Button(root, "Resume", "Devam", Resume,
                new Vector2(0.36f, 0.42f), new Vector2(0.64f, 0.5f), 34f, true);
            _menuButton = UiKit.Button(root, "MainMenu", "Ana menüye dön", ReturnToMenu,
                new Vector2(0.36f, 0.31f), new Vector2(0.64f, 0.39f), 34f, true).gameObject;
            UiKit.Button(root, "Quit", "Çıkış", Quit,
                new Vector2(0.36f, 0.2f), new Vector2(0.64f, 0.28f), 34f, true);
            _panel.SetActive(false);
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
            _menuButton.SetActive(!atMenu || inIntro);
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
