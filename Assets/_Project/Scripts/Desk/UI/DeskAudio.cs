using System.Collections;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masa sesleri: efektler tek kaynaktan PlayOneShot ile, ofis müziği ayrı kaynakta döngüde.
    // Klipler prefab'da referansla bağlıdır (dosya adı/uzantısına güvenilmez).
    public class DeskAudio : MonoBehaviour
    {
        [Header("Kaynaklar")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Klipler")]
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip paper;
        [SerializeField] private AudioClip stamp;
        [SerializeField] private AudioClip officeMusic;

        [Header("Seviye")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
        [SerializeField] private float musicFadeIn = 1.5f;

        private Coroutine fade;

        public void PlayClick() => Play(click);
        public void PlayPaper() => Play(paper);
        public void PlayStamp() => Play(stamp);

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
