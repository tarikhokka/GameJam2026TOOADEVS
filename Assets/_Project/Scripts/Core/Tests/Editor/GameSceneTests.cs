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

            var gm = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(gm, "GameManager yok");
            var so = new UnityEditor.SerializedObject(gm);
            foreach (var list in new[] { "dayConfigs", "expenses", "clients" })
            {
                var prop = so.FindProperty(list);
                Assert.Greater(prop.arraySize, 0, $"GameManager.{list} boş");
                for (int i = 0; i < prop.arraySize; i++)
                    Assert.IsNotNull(prop.GetArrayElementAtIndex(i).objectReferenceValue, $"GameManager.{list}[{i}] kayıp referans");
            }
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include), "EventSystem yok");

            var phases = new HashSet<GamePhase>();
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
                if (mb is IGameScreen screen)
                    Assert.IsTrue(phases.Add(screen.Phase), $"{screen.Phase} için birden fazla ekran var");

            foreach (var required in new[] { GamePhase.DayIntro, GamePhase.Construction, GamePhase.Desk, GamePhase.Result, GamePhase.DaySummary, GamePhase.DailyExpenses, GamePhase.Save, GamePhase.Ending })
                Assert.IsTrue(phases.Contains(required), $"{required} ekranı Game.unity'de yok");
        }
    }
}
