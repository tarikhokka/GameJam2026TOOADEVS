using System.Collections;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa sesleri: efektler tek kaynaktan PlayOneShot ile, ofis müziği ayrı kaynakta döngüde, konuşma balonunun
    // yazı sesi kendi kaynağında (perdesi her harfte rastgele; diğer efektlere bulaşmasın). Klipler prefab'da referansla bağlıdır.
    public class DeskAudio : MonoBehaviour
    {
        [Header("Kaynaklar")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource typingSource;

        [Header("Klipler")]
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip paper;
        [SerializeField] private AudioClip stamp;
        [SerializeField] private AudioClip glassBreak;
        [SerializeField] private AudioClip officeMusic;
        [SerializeField] private AudioClip typing; // EEE: sfx_typing.ogg (malakme "Medium Text Blip", CC0; döngüde çalar), karaktere özel ses yok

        [Header("Seviye")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
        [SerializeField, Range(0f, 1f)] private float typingVolume = 0.45f;
        [SerializeField] private Vector2 typingPitch = new Vector2(0.95f, 1.05f); // tekdüze olmasın (EEE)
        [SerializeField] private float musicFadeIn = 1.5f;

        private Coroutine fade;

        // Yazı sesi ön yüklemesiz içe aktarılmış; ilk harfte gecikme olmasın diye baştan belleğe alınır.
        private void Awake()
        {
            if (typing != null) typing.LoadAudioData();
        }

        public void PlayClick() => Play(click);
        public void PlayPaper() => Play(paper);
        public void PlayStamp() => Play(stamp);
        public void PlayGlassBreak() => Play(glassBreak);

        // EEE (17 Ekim): sfx_typing artık 8,8 sn'lik blip dizisi. Her harfte baştan çalmak yerine harfler akarken
        // döngüde çalar; typingStopDelay boyunca yeni harf gelmezse (yazı bitti / cümle duraklaması) kendiliğinden durur.
        [SerializeField] private float typingStopDelay = 0.25f;
        private float lastTypedTime = -1f;

        public void PlayTyping()
        {
            if (typing == null || typingSource == null) return;
            lastTypedTime = Time.unscaledTime;
            if (typingSource.isPlaying) return;
            typingSource.clip = typing;
            typingSource.loop = true;
            typingSource.volume = typingVolume;
            typingSource.pitch = Random.Range(typingPitch.x, typingPitch.y);
            typingSource.time = Random.Range(0f, Mathf.Max(0f, typing.length - 0.5f)); // her seferinde farklı yerden
            typingSource.Play();
        }

        private void Update()
        {
            if (typingSource != null && typingSource.isPlaying && typingSource.clip == typing
                && Time.unscaledTime - lastTypedTime > typingStopDelay)
                typingSource.Stop();
        }

        public void StartMusic()
        {
            if (musicSource == null || officeMusic == null) return;

            musicSource.clip = officeMusic;
            musicSource.loop = true;
            if (!musicSource.isPlaying)
            {
                musicSource.volume = 0f;
                musicSource.Play();
            }

            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeIn());
        }

        // Ekran kapanırken obje pasifleşeceği için müzik beklemeden durur.
        public void StopMusic()
        {
            if (fade != null) StopCoroutine(fade);
            fade = null;
            if (musicSource != null) musicSource.Stop();
        }

        private void Play(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
                sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private IEnumerator FadeIn()
        {
            float start = musicSource.volume;
            for (float t = 0f; t < musicFadeIn; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(start, musicVolume, t / musicFadeIn);
                yield return null;
            }
            musicSource.volume = musicVolume;
            fade = null;
        }
    }
}
