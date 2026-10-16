using System.Collections;
using CivilJam.Core;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CivilJam.Flow.Tests
{
    public class PauseMenuTests
    {
        Keyboard _keyboard;
        InputSettings.BackgroundBehavior _previousBackground;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode _previousEditorInput;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameLaunch.ContinueFromSave = false;
            // Batch tests have no focused Game view; still deliver synthetic keyboard events.
            _previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            _previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            _keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var menu = Object.FindAnyObjectByType<PauseMenu>();
            if (menu != null) menu.Resume();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            InputSystem.settings.backgroundBehavior = _previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = _previousEditorInput;
#endif
            yield return null;
        }

        IEnumerator PressEscape()
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.IsTrue(_keyboard.escapeKey.isPressed, "Synthetic Escape was not delivered to the Game view.");
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
        }

        [UnityTest]
        public IEnumerator Escape_BlocksDayIntroAndRestoresTimeAndInput()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            var manager = Object.FindAnyObjectByType<GameManager>();
            var menu = Object.FindAnyObjectByType<PauseMenu>();
            var intro = Object.FindAnyObjectByType<DayIntroScreen>();
            var start = intro.GetComponentInChildren<Button>();
            var background = intro.GetComponentInParent<GraphicRaycaster>();
            Time.timeScale = 0.65f;
            int day = manager.Context.State.Day;
            int money = manager.Context.State.Money;

            yield return PressEscape();
            Assert.IsTrue(PauseMenu.IsOpen);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(AudioListener.pause);
            Assert.IsFalse(background.enabled);
            Assert.IsFalse(manager.enabled, "Debug keys and fallback GUI must stop while paused.");
            start.onClick.Invoke();
            Assert.AreEqual(GamePhase.DayIntro, manager.Context.Phase);
            Assert.IsTrue(start.interactable);
            Assert.AreEqual(day, manager.Context.State.Day);
            Assert.AreEqual(money, manager.Context.State.Money);

            yield return PressEscape();
            Assert.IsFalse(PauseMenu.IsOpen);
            Assert.AreEqual(0.65f, Time.timeScale);
            Assert.IsFalse(AudioListener.pause);
            Assert.IsTrue(background.enabled);
            Assert.IsTrue(manager.enabled);
            yield return null;
            start.onClick.Invoke();
            Assert.AreEqual(GamePhase.Desk, manager.Context.Phase);
        }

        [UnityTest]
        public IEnumerator PausingIntro_FreezesTypewriterAndResumeDoesNotAdvancePage()
        {
            var screen = Object.FindAnyObjectByType<MainMenuScreen>();
            screen.transform.Find("Root/Menu/New").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.6f);
            var intro = Object.FindAnyObjectByType<IntroSequence>();
            var story = intro.transform.Find("Content/Story").GetComponent<TMP_Text>();
            var hint = intro.transform.Find("Hint").GetComponent<TMP_Text>();
            var menu = Object.FindAnyObjectByType<PauseMenu>();
            Assert.Greater(story.maxVisibleCharacters, 0);
            Assert.Less(story.maxVisibleCharacters, story.textInfo.characterCount);

            menu.Open();
            int visible = story.maxVisibleCharacters;
            string page = story.text;
            string pageHint = hint.text;
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.AreEqual(visible, story.maxVisibleCharacters);
            Assert.AreEqual(pageHint, hint.text);

            menu.transform.Find("PausePanel/Resume").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(PauseMenu.InputBlocked, "The resume frame must not reach the intro.");
            yield return new WaitForSecondsRealtime(0.15f);
            Assert.IsFalse(PauseMenu.IsOpen);
            Assert.Greater(story.maxVisibleCharacters, visible);
            Assert.AreEqual(page, story.text);
            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator ReturningToMenu_RestoresGlobalsAndKeepsOneOverlay()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            var menu = Object.FindAnyObjectByType<PauseMenu>();
            menu.Open();
            menu.ReturnToMenu();
            yield return null;
            yield return null;
            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);
            Assert.IsFalse(PauseMenu.IsOpen);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(AudioListener.pause);
            Assert.AreEqual(1, Object.FindObjectsByType<PauseMenu>(FindObjectsInactive.Exclude).Length);
            Assert.IsNotNull(Object.FindAnyObjectByType<MainMenuScreen>());
        }
    }
}
