using System.Collections.Generic;
using System.Text;
using CivilJam.Core;
using UnityEngine;

namespace CivilJam.Desk
{
    // Sandbox_Desk sahnesinde masayı sahte bir günle başlatır; mesai bitince sonucu loglar ve sonraki günü açar.
    public class DeskSandboxBootstrap : MonoBehaviour
    {
        [SerializeField] private DeskScreen screen;
        [SerializeField] private Sprite[] portraits; // sahte müşterilere verilir (EEE'nin portrait_*.png'leri)
        [SerializeField] private RuleSetSO rules;    // Data/RuleSet.asset (oyundaki yönetmelik); boşsa sahte 3 kural
        [SerializeField] private DialogueSO dialogue; // Data/Dialogue.asset (EEE): denetçinin uyuşmazlık cümleleri; boşsa Core varsayılanı
        [SerializeField] private GiftSO[] gifts;     // Data/Gifts (EEE); dürüst usta onaylanınca birini verir

        private readonly List<string> officeGifts = new List<string>(); // kabul edilen hediyeler sandbox günleri boyunca kalır

        private GameContext context;
        private int day;

        private void Start()
        {
            screen.Completed += OnCompleted;            StartDay();
        }

        // Aynı sahte gün tekrar açılır ama gün sayısı artar (öğretici sadece 1. gün çıkar).
        private void StartDay()
        {
            context = SandboxDeskContext.Create(portraits, rules, gifts, officeGifts);
            context.Dialogue = dialogue;
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
