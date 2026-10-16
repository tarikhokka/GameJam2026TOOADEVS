using System.Text;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sandbox_Desk sahnesinde masayı sahte bir günle başlatır; mesai bitince sonucu loglar ve sonraki günü açar.
    public class DeskSandboxBootstrap : MonoBehaviour
    {
        [SerializeField] private DeskScreen screen;

        private GameContext context;
        private int day;

        private void Start()
        {
            screen.Completed += OnCompleted;
            StartDay();
        }

        // Aynı sahte gün tekrar açılır ama gün sayısı artar (öğretici sadece 1. gün çıkar).
        private void StartDay()
        {
            context = SandboxDeskContext.Create();
            context.State.Day = ++day;
            screen.Show(context);
        }

        private void OnCompleted()
        {
            var state = context.State;
            var sb = new StringBuilder();
            sb.AppendLine($"[Desk] Mesai bitti. Kasa {state.Money}, bugün onaylanan {state.ApprovedToday}, " +
                          $"alınan zarf {state.TotalBribesTaken}");
            foreach (var p in context.TodayProjects)
                sb.AppendLine($"  {p.Name}: {p.Verdict}{(p.BribeAccepted ? " +zarf" : "")}{(p.ExtraTestBought ? " +test" : "")}" +
                              $" (gerçekte {(p.IsCompliant ? "uygun" : "UYGUNSUZ")})");
            Debug.Log(sb.ToString());

            StartDay();
        }
    }
}
