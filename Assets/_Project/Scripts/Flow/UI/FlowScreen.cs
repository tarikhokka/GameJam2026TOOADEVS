using System;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Flow
{
    /// <summary>
    /// Flow ekranlarının ortak tabanı. UI ilk Show'da kodla kurulur; Completed bir kez tetiklenir
    /// (çift tıklamayla çift ödeme / çift gün ilerlemesi olmasın diye).
    /// Ekranlar state değiştirmez; sadece ctx'ten okur, tek yazma yolu ctx.Actions.
    /// </summary>
    public abstract class FlowScreen : MonoBehaviour, IGameScreen
    {
        public abstract GamePhase Phase { get; }
        public event Action Completed;

        protected GameContext Context { get; private set; }
        bool _built;
        bool _completed;

        public void Show(GameContext context)
        {
            Context = context;
            _completed = false;
            gameObject.SetActive(true);
            if (!_built)
            {
                Build();
                _built = true;
            }
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>UI nesnelerini bir kez kur.</summary>
        protected abstract void Build();

        /// <summary>Her Show'da (ve gerekirse sonra) context'ten yeniden doldur.</summary>
        protected abstract void Refresh();

        protected void Finish()
        {
            if (_completed) return;
            _completed = true;
            Completed?.Invoke();
        }
    }
}
