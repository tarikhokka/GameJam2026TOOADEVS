using System;
using System.Collections.Generic;
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

        public void Refresh(DeskSession session, string selectedId)
        {
            var projects = session.Config.Projects;
            UiPool.Resize(items, itemTemplate, content, projects.Count);

            for (int i = 0; i < projects.Count; i++)
            {
                string id = projects[i].Id;
                items[i].Bind(projects[i], session.GetDecision(id), id == selectedId, () => Selected?.Invoke(id));
            }
        }
    }
}
