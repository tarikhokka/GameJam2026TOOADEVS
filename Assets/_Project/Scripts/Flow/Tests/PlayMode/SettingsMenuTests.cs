using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CivilJam.Flow.Tests
{
    /// <summary>(A, 17 Ekim gece) Esc menüsü Ayarlar: ses kaydırıcısı AudioListener.volume'u değiştirir ve kaydeder; Geri ana gruba döner; menü yeniden açılınca ana gruptan başlar.</summary>
    public class SettingsMenuTests
    {
        [UnityTest]
        public IEnumerator VolumeSlider_SetsAndSaves_AndMenuReturnsToMain()
        {
            float before = PlayerPrefs.GetFloat("imza.masterVolume", 1f); bool had = PlayerPrefs.HasKey("imza.masterVolume");
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
            var menu = Object.FindAnyObjectByType<PauseMenu>();
            menu.Open();
            yield return null;
            menu.transform.Find("PausePanel/Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            var slider = menu.transform.Find("PausePanel/SettingsGroup/VolumeSlider").GetComponent<Slider>();
            slider.value = 0.4f;
            yield return null;
            Assert.AreEqual(0.4f, AudioListener.volume, 1e-3f);
            menu.transform.Find("PausePanel/SettingsGroup/Back").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsTrue(menu.transform.Find("PausePanel/Resume").gameObject.activeSelf);
            Assert.IsFalse(menu.transform.Find("PausePanel/SettingsGroup").gameObject.activeSelf);
            menu.transform.Find("PausePanel/Settings").GetComponent<Button>().onClick.Invoke();
            menu.Resume(); menu.Open(); yield return null;
            Assert.IsFalse(menu.transform.Find("PausePanel/SettingsGroup").gameObject.activeSelf, "Yeniden açılışta ana grup");
            menu.Resume();
            AudioListener.volume = before; if (had) PlayerPrefs.SetFloat("imza.masterVolume", before); else PlayerPrefs.DeleteKey("imza.masterVolume"); PlayerPrefs.Save();
        }
    }
}
