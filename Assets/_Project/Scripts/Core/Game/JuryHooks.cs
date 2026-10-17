using System;

namespace CivilJam.Core
{
    /// <summary>(EEE, 18 Ekim) Jüri menüsünde (J) seçilebilen sahneler. GameManager.JuryJump.</summary>
    public enum JuryScene { EndingGood, EndingBad, EndingCollapsed, EndingClosed, Bribe, Gift, Thug }

    /// <summary>
    /// Jüri menüsü için Desk ile Flow arasındaki tek bağlantı (iki assembly birbirini görmez, ikisi de Core'u görür).
    /// Desk: öğretici ilk kez kapanınca RaiseTutorialFinished. Flow (JuryMenu): dinler, "J'ye bas" ipucunu gösterir.
    /// </summary>
    public static class JuryHooks
    {
        public static event Action TutorialFinished;
        public static void RaiseTutorialFinished() => TutorialFinished?.Invoke();
    }
}
