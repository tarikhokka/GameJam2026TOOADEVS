using System;

namespace CivilJam.Core
{
    /// <summary>
    /// Her ekran (Desk, gazete, haftalık hesap, son...) bunu uygular.
    /// Ekran state değiştirmez; işi bitince Completed'ı bir kez tetikler, GameManager sonraki faza geçer.
    /// </summary>
    public interface IGameScreen
    {
        GamePhase Phase { get; }
        void Show(GameContext context);
        void Hide();
        event Action Completed;
    }
}
