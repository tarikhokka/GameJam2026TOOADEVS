using System.Collections;
using CivilJam.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Ofis sahnesi: masanın karşısındaki koltuğa müteahhit gelip oturur, başının üstünde konuşma balonu çıkar.
    // Çizim sırası (EEE): desk_bg (oda + koltuk + masa) → portre → desk_front (masa) → balon. Gelen giden portredir.
    public class ClientSceneView : MonoBehaviour
    {
        [SerializeField] private RectTransform portraitRoot;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject balloonRoot;
        [SerializeField] private TMP_Text balloonText;

        [Tooltip("Müteahhidin yandan kayarak gelip oturma süresi (sn)")]
        [SerializeField] private float arriveDuration = 0.5f;
        [Tooltip("Geliş başlangıcının oturduğu yere uzaklığı (px, sağdan)")]
        [SerializeField] private float arriveOffset = 1100f;

        private Vector2 seatedPosition;
        private bool seatedPositionKnown;
        private string seatedKey;
        private string pendingLine;
        private Coroutine move;
        private Coroutine balloonAnim;
        private CanvasGroup balloonGroup;

        // Aynı proje için tekrar çağrılırsa bir şey yapmaz (her Redraw'da çağrılabilir).
        // Koltukta biri varsa önce o sola çıkar, sonra yenisi sağdan gelir (sıra akışı tek yönde).
        public void Seat(string projectId, ClientSO client, string line)
        {
            if (projectId == seatedKey) return;
            RememberSeat();
            bool someoneSeated = seatedKey != null && portraitRoot.gameObject.activeSelf;
            seatedKey = projectId;

            pendingLine = line;
            HideBalloon();
            if (move != null) StopCoroutine(move);
            move = StartCoroutine(SwapClient(client, someoneSeated));
        }

        // Konuşma balonu; boş metin balonu gizler. Müteahhit henüz yerine oturmadıysa oturunca gösterilir.
        // Açıkken metin değişirse balon kısa bir "pop" ile yenilenir.
        public void Say(string line)
        {
            if (move != null)
            {
                pendingLine = line;
                return;
            }
            if (string.IsNullOrEmpty(line))
            {
                HideBalloon();
                return;
            }
            balloonText.text = line;
            PlayBalloon(BalloonIn());
        }

        // Gün sonu: oturan müteahhit sola çıkar, koltuk boş kalır. Kimse yoksa bir şey yapmaz (her Redraw'da çağrılabilir).
        public void Leave()
        {
            if (seatedKey == null) return;
            seatedKey = null;
            pendingLine = null;
            HideBalloon();
            if (move != null) StopCoroutine(move);
            move = portraitRoot.gameObject.activeSelf && isActiveAndEnabled ? StartCoroutine(SwapClient(null, true, arrive: false)) : null;
            if (move == null) portraitRoot.gameObject.SetActive(false);
        }

        // Koltuk boş kalır (gün başı): animasyonsuz.
        public void Clear()
        {
            RememberSeat();
            if (move != null) StopCoroutine(move);
            if (balloonAnim != null) StopCoroutine(balloonAnim);
            move = balloonAnim = null;
            seatedKey = null;
            pendingLine = null;
            portraitRoot.anchoredPosition = seatedPosition;
            portraitRoot.gameObject.SetActive(false);
            balloonRoot.SetActive(false);
        }

        private void HideBalloon()
        {
            if (balloonRoot.activeSelf) PlayBalloon(BalloonOut());
        }

        private void PlayBalloon(IEnumerator routine)
        {
            if (balloonAnim != null) StopCoroutine(balloonAnim);
            balloonAnim = StartCoroutine(routine);
        }

        // Balon konuşanın başından (alt kenar pivotu) hafif yaylanarak açılır.
        private IEnumerator BalloonIn()
        {
            var group = BalloonGroup();
            balloonRoot.SetActive(true);
            var rt = balloonRoot.transform;
            for (float t = 0f; t < DeskMotion.Fast; t += Time.unscaledDeltaTime)
            {
                float k = t / DeskMotion.Fast;
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, DeskMotion.EaseOutBack(k, 1.6f));
                group.alpha = DeskMotion.EaseOut(k);
                yield return null;
            }
            rt.localScale = Vector3.one;
            group.alpha = 1f;
            balloonAnim = null;
        }

        // Kapanış girişten kısa ve hızlanarak.
        private IEnumerator BalloonOut()
        {
            var group = BalloonGroup();
            var rt = balloonRoot.transform;
            float duration = DeskMotion.Exit(DeskMotion.Fast);
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseIn(t / duration);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.9f, k);
                group.alpha = 1f - k;
                yield return null;
            }
            balloonRoot.SetActive(false);
            rt.localScale = Vector3.one;
            group.alpha = 1f;
            balloonAnim = null;
        }

        private CanvasGroup BalloonGroup()
        {
            if (balloonGroup == null)
            {
                balloonGroup = balloonRoot.GetComponent<CanvasGroup>();
                if (balloonGroup == null) balloonGroup = balloonRoot.AddComponent<CanvasGroup>();
                balloonGroup.blocksRaycasts = false;
            }
            return balloonGroup;
        }

        private void RememberSeat()
        {
            if (seatedPositionKnown) return;
            seatedPosition = portraitRoot.anchoredPosition;
            seatedPositionKnown = true;
        }

        private IEnumerator SwapClient(ClientSO client, bool someoneSeated, bool arrive = true)
        {
            // Önceki müteahhit sola, hızlanarak çıkar (girişten kısa).
            if (someoneSeated)
            {
                var start = portraitRoot.anchoredPosition;
                var exit = seatedPosition - new Vector2(arriveOffset, 0f);
                float duration = DeskMotion.Exit(arriveDuration);
                for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
                {
                    portraitRoot.anchoredPosition = Vector2.Lerp(start, exit, DeskMotion.EaseIn(t / duration));
                    yield return null;
                }
            }

            if (!arrive)
            {
                portraitRoot.gameObject.SetActive(false);
                portraitRoot.anchoredPosition = seatedPosition;
                move = null;
                yield break;
            }

            var sprite = client != null ? client.Portrait : null;
            portraitImage.sprite = sprite;
            portraitImage.enabled = sprite != null;
            portraitRoot.gameObject.SetActive(true);

            // Yenisi sağdan gelir, yavaşlayıp koltuğa hafif yaylanarak oturur.
            var from = seatedPosition + new Vector2(arriveOffset, 0f);
            for (float t = 0f; t < arriveDuration; t += Time.unscaledDeltaTime)
            {
                float k = DeskMotion.EaseOutBack(t / arriveDuration, 0.9f);
                portraitRoot.anchoredPosition = Vector2.LerpUnclamped(from, seatedPosition, k);
                yield return null;
            }
            portraitRoot.anchoredPosition = seatedPosition;
            move = null;
            Say(pendingLine);
        }
    }
}
