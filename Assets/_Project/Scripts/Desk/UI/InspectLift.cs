using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // İnceleme modunda karartmanın üstünde kalacak öğeyi öne alır (poliş B1). uGUI'de karartmaya delik açılamadığı için öğeye
    // geçici bir Canvas (overrideSorting, verilen sortingOrder) ve tıklama alsın diye GraphicRaycaster eklenir; mod kapanınca
    // ikisi de kaldırılır (kalıcı iç içe Canvas, kartların arkasından tıklama sızdırabilirdi). Sadece kendi eklediklerini kaldırır.
    public static class InspectLift
    {
        private static readonly HashSet<GameObject> lifted = new HashSet<GameObject>();

        public static void Set(GameObject target, bool on, int sortingOrder)
        {
            if (target == null) return;
            if (on)
            {
                var canvas = target.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = target.AddComponent<Canvas>();
                    target.AddComponent<GraphicRaycaster>();
                    lifted.Add(target);
                }
                if (!lifted.Contains(target)) return; // öğenin kendi Canvas'ı varsa dokunma
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
            }
            else if (lifted.Remove(target))
            {
                var raycaster = target.GetComponent<GraphicRaycaster>();
                if (raycaster != null) Object.DestroyImmediate(raycaster);
                var canvas = target.GetComponent<Canvas>();
                if (canvas != null) Object.DestroyImmediate(canvas);
            }
        }
    }
}
