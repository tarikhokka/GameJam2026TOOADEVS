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

        // Aynı proje için tekrar çağrılırsa bir şey yapmaz (her Redraw'da çağrılabilir).
        public void Seat(string projectId, ClientSO client, string line)
        {
            if (projectId == seatedKey) return;
            RememberSeat();
            seatedKey = projectId;

            var sprite = client != null ? client.Portrait : null;
            portraitImage.sprite = sprite;
            portraitImage.enabled = sprite != null;
            portraitRoot.gameObject.SetActive(true);

            pendingLine = line;
            balloonRoot.SetActive(false);
            if (move != null) StopCoroutine(move);
            move = StartCoroutine(Arrive());
        }

        // Konuşma balonu; boş metin balonu gizler. Müteahhit henüz yerine oturmadıysa oturunca gösterilir.
        public void Say(string line)
        {
            if (move != null)
            {
                pendingLine = line;
                return;
            }
            bool has = !string.IsNullOrEmpty(line);
            balloonRoot.SetActive(has);
            if (has) balloonText.text = line;
        }

        // Koltuk boş kalır.
        public void Clear()
        {
            RememberSeat();
            if (move != null) StopCoroutine(move);
            move = null;
            seatedKey = null;
            pendingLine = null;
            portraitRoot.anchoredPosition = seatedPosition;
            portraitRoot.gameObject.SetActive(false);
            balloonRoot.SetActive(false);
        }

        private void RememberSeat()
        {
            if (seatedPositionKnown) return;
            seatedPosition = portraitRoot.anchoredPosition;
            seatedPositionKnown = true;
        }

        private IEnumerator Arrive()
        {
            var from = seatedPosition + new Vector2(arriveOffset, 0f);
            for (float t = 0f; t < arriveDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / arriveDuration;
                float eased = 1f - (1f - k) * (1f - k) * (1f - k); // ease-out
                portraitRoot.anchoredPosition = Vector2.LerpUnclamped(from, seatedPosition, eased);
                yield return null;
            }
            portraitRoot.anchoredPosition = seatedPosition;
            move = null;
            Say(pendingLine);
        }
    }
}
