using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sahnedeki pasif bir şablon objeden gereken sayıda kopya üretir / fazlasını siler.
    internal static class UiPool
    {
        public static void Resize<T>(List<T> items, T template, Transform parent, int count) where T : Component
        {
            template.gameObject.SetActive(false);

            while (items.Count < count)
            {
                var item = Object.Instantiate(template, parent);
                item.gameObject.SetActive(true);
                items.Add(item);
            }

            while (items.Count > count)
            {
                Object.Destroy(items[items.Count - 1].gameObject);
                items.RemoveAt(items.Count - 1);
            }
        }
    }
}
