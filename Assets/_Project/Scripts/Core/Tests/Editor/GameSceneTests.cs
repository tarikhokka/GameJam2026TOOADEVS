using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CivilJam.Core.Tests
{
    /// <summary>Game.unity kurulumunu doğrular: GameManager, EventSystem ve faz başına en fazla bir ekran.</summary>
    public class GameSceneTests
    {
        const string ScenePath = "Assets/_Project/Scenes/Game.unity";

        [Test]
        public void GameScene_HasManagerEventSystemAndUniqueScreens()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Assert.IsNotNull(Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include), "GameManager yok");
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include), "EventSystem yok");

            var phases = new HashSet<GamePhase>();
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
                if (mb is IGameScreen screen)
                    Assert.IsTrue(phases.Add(screen.Phase), $"{screen.Phase} için birden fazla ekran var");

            Assert.IsTrue(phases.Contains(GamePhase.Desk), "Desk ekranı Game.unity'de yok");
        }
    }
}
