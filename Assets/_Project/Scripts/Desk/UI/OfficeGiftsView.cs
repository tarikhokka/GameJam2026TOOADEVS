using System.Collections;
using CivilJam.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Kabul edilen hediyeler (ctx.State.OfficeGifts, kayıtla saklanır) masanın uzak kenarında kalıcı durur (EEE: ~140-180 px,
    // kitaplar gibi alt gölgeli → DeskBookProp). Yerler builder'da sabit (form klasörü, zarf, isimlik açıkta kalır); kabul sırasıyla dolar.
    // Yeni kabul edilen hediye masaya hafif yaylanarak konur.
    public class OfficeGiftsView : MonoBehaviour
    {
        [SerializeField] private GameObject[] slots;     // DeskBookProp kökleri (gölge + görsel)
        [SerializeField] private Image[] slotImages;

        public void Refresh(GameContext ctx, string newGiftId = null)
        {
            var owned = ctx.State != null ? ctx.State.OfficeGifts : null;
            for (int i = 0; i < slots.Length; i++)
            {
                string id = owned != null && i < owned.Count ? owned[i] : null;
                var gift = ctx.GetGift(id);
                bool show = gift != null && gift.Sprite != null;
                slots[i].SetActive(show);
                if (!show) continue;

                slotImages[i].sprite = gift.Sprite;
                if (id == newGiftId && isActiveAndEnabled) StartCoroutine(Place(slots[i].transform));
                else slots[i].transform.localScale = Vector3.one;
            }
        }

        // Masaya konma: küçükten, hafif yukarıdan, yaylanarak (DeskMotion).
        private IEnumerator Place(Transform slot)
        {
            var rt = (RectTransform)slot;
            Vector2 rest = rt.anchoredPosition;
            float duration = DeskMotion.Base * 1.4f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / duration;
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, DeskMotion.EaseOutBack(k, 1.7f));
                rt.anchoredPosition = rest + new Vector2(0, 40f * (1f - DeskMotion.EaseOut(k)));
                yield return null;
            }
            rt.localScale = Vector3.one;
            rt.anchoredPosition = rest;
        }
    }
}
