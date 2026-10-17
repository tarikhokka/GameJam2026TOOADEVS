using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CivilJam.Desk
{
    // Belgelerin arkasında, masa yüzeyini kaplayan görünmez alan. İnceleme modunda boş masaya tıklamak seçimi temizler.
    // Raycast'i InspectController açıp kapatır (mod kapalıyken masaya hiçbir şey yapmaz).
    public class InspectCatcher : MonoBehaviour, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke();
    }
}
