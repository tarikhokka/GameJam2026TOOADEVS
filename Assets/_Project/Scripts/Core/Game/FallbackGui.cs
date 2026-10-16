using System.Text;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// GEÇİCİ debug arayüzü: sahnede IGameScreen'i olmayan fazları IMGUI ile gösterir.
    /// Desk/Flow ekranları Game sahnesine eklendikçe ilgili faz için otomatik devre dışı kalır. Final build'de GameManager.fallbackGui kapatılır.
    /// </summary>
    static class FallbackGui
    {
        static Vector2 _scroll;
        static GUIStyle _box;

        public static void Draw(GameManager gm)
        {
            var ctx = gm.Context;
            var s = ctx.State;

            if (_box == null)
                _box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, richText = true, wordWrap = true, fontSize = 15 };

            GUILayout.BeginArea(new Rect(20, 20, 900, Screen.height - 40), GUI.skin.window);
            GUILayout.Label($"<b>[YEDEK ARAYÜZ] {ctx.Phase}</b>   Gün {s.Day}/{ctx.Balance.TotalDays}   Para {s.Money} TL   Borç {s.Debt}   " +
                            $"Hapis riski {s.JailRisk:P0}   Afet olasılığı {s.DisasterChance:P0}", _box);

            _scroll = GUILayout.BeginScrollView(_scroll);
            switch (ctx.Phase)
            {
                case GamePhase.Desk: DrawDesk(ctx); break;
                case GamePhase.Result: DrawResult(ctx); break;
                case GamePhase.DailyExpenses: DrawExpenses(ctx); break;
                case GamePhase.Ending: GUILayout.Label($"SON: {s.Ending}", _box); break;
                default: GUILayout.Label(Describe(ctx), _box); break;
            }
            GUILayout.EndScrollView();

            if (ctx.Phase != GamePhase.Ending && GUILayout.Button("Devam →", GUILayout.Height(40)))
                gm.Complete();
            GUILayout.EndArea();
        }

        static string Describe(GameContext ctx)
        {
            switch (ctx.Phase)
            {
                case GamePhase.DayIntro: return $"{ctx.State.Day}. gün başlıyor. Masada {ctx.TodayProjects.Count} proje var.";
                case GamePhase.Construction: return $"Onaylanan binalar inşa ediliyor ({ctx.State.ApprovedToday} bugün).";
                case GamePhase.DaySummary: return $"Bugün afet olmadı. Yarın afet olasılığı {ctx.State.DisasterChance:P0}. (Gazete yok)";
                case GamePhase.Save: return "Gün sonu kaydedildi.";
                default: return ctx.Phase.ToString();
            }
        }

        static void DrawDesk(GameContext ctx)
        {
            foreach (var p in ctx.TodayProjects)
            {
                var sb = new StringBuilder();
                sb.Append($"<b>{p.Name}</b> — {p.Address} — {p.Floors} kat — ücret {p.Fee} TL — <i>{p.Verdict}</i>\n");
                foreach (var prm in p.Params)
                {
                    string shown = prm.Tested ? $"belge {prm.Reported} / <b>test {prm.Actual}</b>" : $"belge {prm.Reported}";
                    sb.Append($"  {prm.Type}: {shown} (gerekli {prm.Required})\n");
                }
                if (p.Documents.Count == 2)
                {
                    var a = p.Documents[0]; var l = p.Documents[1];
                    sb.Append($"  Başvuru: parsel {a.ParcelNo}, {a.Date}, imza {a.SignatureName}, kaşe {a.StampId}\n");
                    sb.Append($"  Lab ({l.LabName}): parsel {l.ParcelNo}, {l.Date}, imza {l.SignatureName}, kaşe {l.StampId}\n");
                }
                if (p.OffersBribe) sb.Append($"  <color=orange>Zarf: {p.BribeAmount} TL</color>\n");
                GUILayout.Label(sb.ToString(), _box);

                if (p.Verdict != ProjectVerdict.Pending) continue;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Onayla")) ctx.Actions.Approve(p.Id, false);
                if (p.OffersBribe && GUILayout.Button("Zarfı al + Onayla")) ctx.Actions.Approve(p.Id, true);
                if (GUILayout.Button("Reddet")) ctx.Actions.Reject(p.Id);
                if (!p.ExtraTestBought && GUILayout.Button($"Ek test ({ctx.Balance.ExtraTestCost} TL)")) ctx.Actions.BuyExtraTest(p.Id);
                GUILayout.EndHorizontal();
            }
        }

        static void DrawResult(GameContext ctx)
        {
            var q = ctx.LastQuake;
            var sb = new StringBuilder($"<b>GAZETE — {q.Day}. gün: DEPREM</b>\n");
            sb.Append($"Toplam: {q.TotalDeaths} ölü, {q.TotalInjured} yaralı. Hapis riski {q.JailRiskBefore:P0} → {q.JailRiskAfter:P0}" +
                      (q.JailRolled ? (q.Jailed ? "  <color=red>TUTUKLANDINIZ</color>" : "  (zar: kurtuldunuz)") : "") + "\n\n");
            if (q.Outcomes.Count == 0) sb.Append("Son afetten bu yana onayladığınız bina yok.\n");
            foreach (var o in q.Outcomes)
            {
                sb.Append($"• {o.ResponsibilityText}\n");
                if (o.Collapsed)
                    sb.Append($"   {o.Deaths} ölü, {o.Injured} yaralı. Kaybedilenler: {string.Join(", ", o.VictimNames)}\n");
            }
            GUILayout.Label(sb.ToString(), _box);
        }

        static void DrawExpenses(GameContext ctx)
        {
            var e = ctx.PendingExpenses;
            if (e == null) return;
            foreach (var line in e.Lines)
            {
                bool paid = GUILayout.Toggle(line.Paid, $" {line.DisplayName}: {line.Amount} TL" + (line.Mandatory ? " (zorunlu)" : ""));
                if (paid != line.Paid) ctx.Actions.SetExpensePaid(line.ExpenseId, paid);
            }
            GUILayout.Label($"Ödenecek: {e.TotalSelected} TL  —  Kalan: {ctx.State.Money - e.TotalSelected} TL", _box);
        }
    }
}
