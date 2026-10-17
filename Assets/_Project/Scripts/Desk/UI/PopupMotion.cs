using System;
using System.Collections;
using UnityEngine;

namespace CivilJam.Desk
{
    // Ortada açılan kartların (olay kartı, öğretici) açılış/kapanışı (DeskMotion, ui-ux-pro-max §7):
    // perde yumuşakça kararır, kart biraz aşağıdan ve küçükten hafif yaylanarak gelir; kapanış kısa ve hızlanarak.
    // Kapanırken tıklamayı engellemez (arkadaki masa hemen kullanılabilir). Çalışırken eklenir (prefab değişmez):
    // panelin altında "Card" adlı çocuk aranır; yoksa sadece perde/saydamlık oynar.
    public class PopupMotion : MonoBehaviour
    {
        private const float CardRise = 24f;

        private CanvasGroup group;
        private RectTransform card;
        private Vector2 cardRest;
        private Coroutine anim;

        public static PopupMotion On(GameObject panel)
        {
            var motion = panel.GetComponent<PopupMotion>();
            if (motion == null) motion = panel.AddComponent<PopupMotion>();
            motion.Init();
            return motion;
        }

        private void Init()
        {
            if (group != null) return;
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            card = transform.Find("Card") as RectTransform;
            if (card != null) cardRest = card.anchoredPosition;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Play(Enter());
        }

        public void Hide(Action hidden = null)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                hidden?.Invoke();
                return;
            }
            Play(Exit(hidden));
        }

        public void HideInstant()
        {
            if (anim != null) StopCoroutine(anim);
            anim = null;
            Rest(1f);
            gameObject.SetActive(false);
        }

        private void Play(IEnumerator routine)
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(routine);
        }

        private IEnumerator Enter()
        {
            group.blocksRaycasts = true;
            float duration = DeskMotion.Base;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / duration;
                group.alpha = DeskMotion.EaseOut(k);
                SetCard(DeskMotion.EaseOutBack(k, 1.4f));
                yield return null;
            }
            Rest(1f);
            anim = null;
        }

        private IEnumerator Exit(Action hidden)
        {
            group.blocksRaycasts = false; // kapanırken arkası tıklanabilir
            float duration = DeskMotion.Exit(DeskMotion.Base);
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseIn(t / duration);
                group.alpha = 1f - k;
                if (card != null) card.localScale = Vector3.one * Mathf.Lerp(1f, 0.96f, k);
                yield return null;
            }
            Rest(1f);
            gameObject.SetActive(false);
            anim = null;
            hidden?.Invoke();
        }

        // p: 0 = başlangıç (aşağıda, küçük), 1 = yerinde.
        private void SetCard(float p)
        {
            if (card == null) return;
            card.anchoredPosition = cardRest + new Vector2(0, -CardRise * (1f - p));
            card.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, p);
        }

        private void Rest(float alpha)
        {
            group.alpha = alpha;
            group.blocksRaycasts = true;
            if (card != null)
            {
                card.anchoredPosition = cardRest;
                card.localScale = Vector3.one;
            }
        }
    }
}
