using System;
using System.Collections.Generic;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Masaya gelen proje klasörlerinin listesi.
    public class ProjectQueueView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private ProjectQueueItem itemTemplate;

        public event Action<string> Selected;

        private readonly List<ProjectQueueItem> items = new List<ProjectQueueItem>();

        public void Refresh(IReadOnlyList<ProjectData> projects, string selectedId)
        {
            UiPool.Resize(items, itemTemplate, content, projects.Count);

            for (int i = 0; i < projects.Count; i++)
            {
                string id = projects[i].Id;
                items[i].Bind(projects[i], id == selectedId, () => Selected?.Invoke(id));
            }
        }
    }
}
