using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace CivilJam.Core.Editor {
public static class AuditRunner {
 static List<T> Load<T>(string folder) where T:Object => AssetDatabase.FindAssets("t:"+typeof(T).Name,new[]{folder}).Select(g=>AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(a=>a.name).ToList();
 public static void Run(){
  const string o="/tmp/civil-audit-20261018/latest/"; BalanceReport.Run();BalanceReport.ContentStats();
  var b=AssetDatabase.LoadAssetAtPath<BalanceSO>("Assets/_Project/Data/Balance.asset");var rules=AssetDatabase.LoadAssetAtPath<RuleSetSO>("Assets/_Project/Data/RuleSet.asset");var names=AssetDatabase.LoadAssetAtPath<NamePoolSO>("Assets/_Project/Data/NamePool.asset");var clients=Load<ClientSO>("Assets/_Project/Data/Clients");var days=Load<DayConfigSO>("Assets/_Project/Data/Days");var expenses=Load<ExpenseSO>("Assets/_Project/Data/Expenses");
  var reveal=typeof(BalanceReport).GetMethod("RevealBribes",BindingFlags.Static|BindingFlags.NonPublic);
  int maxOut=0,maxCollapsed=0;var sb=new StringBuilder();
  for(int seed=1000;seed<1300;seed++){
   var c=new GameContext{Balance=b,Rules=rules,Names=names,Clients=clients};var s=new GameSession(c,days,expenses);s.NewGame(seed);
   while(c.State.Day<=7){s.BeginDay();reveal.Invoke(null,new object[]{c});foreach(var p in c.TodayProjects)s.Approve(p.Id,true);s.FinishDesk();var q=s.RunDisasterCheck();
    if(q.Occurred){if(q.Outcomes.Count>maxOut){maxOut=q.Outcomes.Count;File.WriteAllText(o+"real-max-outcomes-quake.json",JsonUtility.ToJson(q,true));}
     if(q.CollapsedCount>maxCollapsed){maxCollapsed=q.CollapsedCount;File.WriteAllText(o+"real-max-collapse-quake.json",JsonUtility.ToJson(q,true));}}
    s.PrepareExpenses();s.FinishExpenses();if(s.EvaluateEnding()!=EndingType.None)break;c.State.Day++;
   }
  }
  sb.AppendLine($"300 actual-data bribery runs: max outcomes={maxOut}, max collapsed={maxCollapsed}");
  var ctx=new GameContext{Balance=b,State=new GameState{Day=7,TotalBribesTaken=5,TotalApproved=1,Money=5000}};ctx.State.Buildings.Add(new ProjectData{Id="unsafe",DayDecided=7,Params=new List<ParamEntry>{new ParamEntry{Required=40,Actual=20,Reported=40}}});var session=new GameSession(ctx,days,expenses);sb.AppendLine($"Ending with five bribes, one unsafe standing building, no collapses: {session.EvaluateEnding()}");
  sb.AppendLine($"Windows build target supported on this Editor: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64)}");
  var g=ctx.State.Buildings[0];sb.AppendLine($"Unsafe building scoped after first earthquake: {EarthquakeSimulator.IsInScope(g,7)}");
  File.WriteAllText(o+"editor-audit.txt",sb.ToString());Debug.Log(sb.ToString());
 }
}
}
