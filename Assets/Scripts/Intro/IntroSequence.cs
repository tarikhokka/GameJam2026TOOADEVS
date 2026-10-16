using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Oyun başındaki hikaye anlatımı: sayfaları daktilo efektiyle yazar,
// tıklama / Space / Enter ile ilerler, son sayfadan sonra ofis sahnesine geçer.
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
        "Son 10 yılda Betonya'da\n<b>1.250.000</b> yeni bina yapıldı.\n\nBunların <color=#E04040><b>400.000</b></color>'i hiç denetlenmedi.",
        "Aynı 10 yılda depremler, seller ve yangınlar\n<color=#E04040><b>38.000</b></color> vatandaşın hayatını aldı.\n\nBu hayatların çoğunu afetler değil,\n<b>denetlenmeyen binalar</b> aldı.",
        "Halk sokaklara döküldü.\nHükümet bir karar aldı:\n\n<b>ULUSAL YAPI DENETİM KURUMU</b> kuruldu.",
        "<b>TEBRİKLER.</b>\n\nEkim ayı memur kurası tamamlandı.\nAdınız çekildi.\n\nKuruma <b>Baş Denetçi</b> olarak atandınız.",
        "Her imzanız bir binayı ayağa kaldıracak.\n\nYa da yıkacak.\n\n<size=130%><b>Hazır mısın?</b></size>"
    };

    [Header("Ayarlar")]
    [SerializeField] private float charDelay = 0.03f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private string nextSceneName = "Office";

    private bool isTyping;
    private bool skipRequested;

    private void Start()
    {
        StartCoroutine(PlayStory());
    }

    private void Update()
    {
        if (AdvancePressed())
            skipRequested = true;
    }

    private IEnumerator PlayStory()
    {
        foreach (string page in pages)
        {
            yield return Fade(1f);
            yield return TypePage(page);

            // sayfa bitti, oyuncunun devam etmesini bekle
            skipRequested = false;
            yield return new WaitUntil(() => skipRequested);
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

        isTyping = true;
        skipRequested = false;

        for (int i = 1; i <= total; i++)
        {
            // yazarken tıklanırsa sayfanın tamamını göster
            if (skipRequested)
            {
                storyText.maxVisibleCharacters = total;
                break;
            }

            storyText.maxVisibleCharacters = i;
            if (typeSound != null && !char.IsWhiteSpace(storyText.textInfo.characterInfo[i - 1].character))
                typeSound.Play();

            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    private IEnumerator Fade(float target)
    {
        if (fadeGroup == null) yield break;

        float start = fadeGroup.alpha;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadeGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
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
