using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Oyun başındaki hikaye anlatımı: sayfaları daktilo efektiyle yazar,
// tıklama / Space / Enter ile ilerler, son sayfadan sonra Game sahnesine geçer.
namespace CivilJam.Flow
{
    public class IntroSequence : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TMP_Text storyText;
        [SerializeField] private CanvasGroup fadeGroup;      // opsiyonel: sayfa geçişlerinde kararma
        [SerializeField] private AudioSource typeSound;      // opsiyonel: daktilo sesi

        [Header("Hikaye")]
        [SerializeField, TextArea(3, 8)] private string[] pages =
        {
            "<size=140%><b>BETONYA CUMHURİYETİ</b></size>\n\nGüzel ülke. Sıcak insanlar.\nÇok hızlı müteahhitler.",
            "Betonya'da yeni binalar hızla yükseldi.\n\nAncak birçoğu\n<b>gerekli denetimler yapılmadan</b> inşa edildi.",
            "Depremlerde denetimsiz binalar yıkıldı.\nİnsanlar hayatını kaybetti.\n\n<b>Güvenli yapılar ve özenli denetim,\nhayatları koruyabilir.</b>",
            "Halk sokaklara döküldü.\nHükümet bir karar aldı:\n\n<b>ULUSAL YAPI DENETİM KURUMU</b> kuruldu.",
            "<b>TEBRİKLER.</b>\n\nEkim ayı memur kurası tamamlandı.\nAdınız çekildi.\n\nKuruma <b>Baş Denetçi</b> olarak atandınız.",
            "Her imzanız bir binayı ayağa kaldıracak.\n\nYa da yıkacak.\n\n<size=130%><b>Hazır mısın?</b></size>"
        };

        [Header("Ayarlar")]
        [SerializeField] private float charDelay = 0.03f;
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private string nextSceneName = "Game";

        private TMP_Text advanceHint;
        private bool skipRequested;

        public void Initialize(TMP_Text text, CanvasGroup fade, TMP_Text hint, string sceneName)
        {
            storyText = text;
            fadeGroup = fade;
            advanceHint = hint;
            nextSceneName = sceneName;
        }

        private void Start()
        {
            if (storyText == null)
            {
                Debug.LogError("[IntroSequence] Hikâye metni atanmamış.", this);
                enabled = false;
                return;
            }
            StartCoroutine(PlayStory());
        }

        private void Update()
        {
            if (!PauseMenu.InputBlocked && AdvancePressed())
                skipRequested = true;
        }

        private IEnumerator PlayStory()
        {
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                while (PauseMenu.InputBlocked) yield return null;
                storyText.text = pages[pageIndex];
                storyText.maxVisibleCharacters = 0;
                SetHint(pageIndex, "Metni tamamlamak için tıkla · Space / Enter");
                yield return Fade(1f);
                yield return TypePage(pages[pageIndex]);
                while (PauseMenu.InputBlocked) yield return null;
                SetHint(pageIndex, pageIndex == pages.Length - 1
                    ? "Mesaiye başlamak için tıkla · Space / Enter"
                    : "Devam etmek için tıkla · Space / Enter");

                // sayfa bitti, oyuncunun devam etmesini bekle
                skipRequested = false;
                yield return new WaitUntil(() => !PauseMenu.InputBlocked && skipRequested);
                skipRequested = false;

                yield return Fade(0f);
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private IEnumerator TypePage(string page)
        {
            storyText.text = page;
            storyText.ForceMeshUpdate();
            int total = storyText.textInfo.characterCount;
            storyText.maxVisibleCharacters = 0;

            skipRequested = false;

            for (int i = 1; i <= total; i++)
            {
                while (PauseMenu.InputBlocked) yield return null;
                // yazarken tıklanırsa sayfanın tamamını göster
                if (skipRequested)
                {
                    storyText.maxVisibleCharacters = total;
                    break;
                }

                storyText.maxVisibleCharacters = i;
                if (typeSound != null && !char.IsWhiteSpace(storyText.textInfo.characterInfo[i - 1].character))
                    typeSound.Play();

                for (float elapsed = 0; elapsed < charDelay; elapsed += PauseMenu.AnimationDeltaTime)
                    yield return null;
            }

        }

        private void SetHint(int pageIndex, string text)
        {
            if (advanceHint != null)
                advanceHint.text = $"{pageIndex + 1} / {pages.Length}  ·  {text}";
        }

        private IEnumerator Fade(float target)
        {
            if (fadeGroup == null) yield break;
            while (PauseMenu.InputBlocked) yield return null;

            float start = fadeGroup.alpha;
            for (float t = 0; t < fadeDuration; t += PauseMenu.AnimationDeltaTime)
            {
                fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            while (PauseMenu.InputBlocked) yield return null;
            fadeGroup.alpha = target;
        }

        private static bool AdvancePressed()
        {
            bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool key = Keyboard.current != null &&
                       (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
            return click || key;
        }
    }
}
