using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using CivilJam.Core;
using CivilJam.Desk;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CivilJam.Flow.Tests {
public class AuditProbeTests {
 const string Out = "/tmp/civil-audit-20261018/latest/";
 static T Field<T>(object o,string n) => (T)o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(o);
 static object Call(object o,string n,params object[] args) => o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(o,args);
 static void Put(object o,string n,object v) => o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);
 GameManager gm;
 [UnitySetUp] public IEnumerator Setup() {
  Time.timeScale=1;AudioListener.pause=false;GameLaunch.ContinueFromSave=false;
  yield return SceneManager.LoadSceneAsync("Game"); yield return null;
  gm=Object.FindAnyObjectByType<GameManager>();gm.Session.NewGame(1000);gm.Session.BeginDay();
 }
 [UnityTearDown] public IEnumerator Cleanup(){var p=Object.FindAnyObjectByType<PauseMenu>();if(p!=null)p.Resume();Time.timeScale=1;AudioListener.pause=false;yield return null;}
 void HideScreens(){foreach(var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))if(mb is IGameScreen s)s.Hide();}
 void StartDesk(){gm.Context.State.Day=2;gm.Complete();Object.FindAnyObjectByType<TutorialView>(FindObjectsInactive.Include).Close();}
 static string Texts(Component x)=>string.Join("\n",x.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text));
 static void Render(string name){
  var cameraGo=new GameObject("AuditRenderCamera",typeof(Camera));var cam=cameraGo.GetComponent<Camera>();
  cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.orthographic=true;cam.orthographicSize=540;cam.nearClipPlane=.1f;cam.farClipPlane=2000;cam.transform.position=new Vector3(0,0,-100);
  var rt=new RenderTexture(1920,1080,24);rt.Create();cam.targetTexture=rt;
  var roots=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude).Where(c=>c.isRootCanvas).ToArray();
  var modes=roots.Select(c=>c.renderMode).ToArray();var cameras=roots.Select(c=>c.worldCamera).ToArray();var distances=roots.Select(c=>c.planeDistance).ToArray();
  for(int i=0;i<roots.Length;i++){roots[i].renderMode=RenderMode.ScreenSpaceCamera;roots[i].worldCamera=cam;roots[i].planeDistance=100;}
  Canvas.ForceUpdateCanvases(); foreach(var t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude))t.ForceMeshUpdate(); cam.Render();
  var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Out+name+".png",tex.EncodeToPNG());RenderTexture.active=prev;
  var info=Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude).Select(t=>$"{t.name} | font={t.fontSize:0.0} | overflow={t.isTextOverflowing} | {(t.text ?? "").Replace('\n',' ')}");File.WriteAllLines(Out+name+"-texts.txt",info);
  for(int i=0;i<roots.Length;i++){roots[i].renderMode=modes[i];roots[i].worldCamera=cameras[i];roots[i].planeDistance=distances[i];}
  cam.targetTexture=null;rt.Release();Object.Destroy(tex);Object.Destroy(rt);Object.Destroy(cameraGo);
 }
 [UnityTest] public IEnumerator PaymentPreview_ShouldMatchCore(){
  HideScreens();var ctx=gm.Context;ctx.State.Money=500;ctx.State.Debt=0;gm.Session.PrepareExpenses();foreach(var l in ctx.PendingExpenses.Lines)l.Paid=true;
  var screen=Object.FindAnyObjectByType<DailyExpensesScreen>(FindObjectsInactive.Include);screen.Show(ctx);yield return null;
  var preview=ExpenseCalculator.Preview(ctx.State,ctx.PendingExpenses);var totals=Field<TextMeshProUGUI>(screen,"_totals").text;
  File.WriteAllText(Out+"payment-evidence.txt",totals+$"\nCORE: MoneyAfter={preview.MoneyAfter}, DebtAfter={preview.DebtAfter}, PaidTotal={preview.PaidTotal}, AddedToDebt={preview.AddedToDebt}");Render("payment-mismatch");
  Assert.IsTrue(totals.Contains("ödeme sonrası: "+preview.MoneyAfter+" TL"),"UI and actual Core result differ: "+totals+"; Core MoneyAfter="+preview.MoneyAfter+" DebtAfter="+preview.DebtAfter);
 }
 [UnityTest] public IEnumerator SaveFailure_ShouldNotClaimSuccess(){
  HideScreens();gm.Context.State.Day=2;gm.Context.LastSave=new SaveResult{Ok=false,Day=2,Error="Audit simulated write failure"};
  var screen=Object.FindAnyObjectByType<SaveScreen>(FindObjectsInactive.Include);screen.Show(gm.Context);yield return null;Render("save-failure");var text=Texts(screen);File.WriteAllText(Out+"save-evidence.txt",text);
  Assert.IsFalse(text.Contains("kaydedildi")||text.Contains("KAYDEDİLDİ"),"Save failed but success is shown: "+text);
 }
 [UnityTest] public IEnumerator LoadFailure_ShouldBeVisible(){
  HideScreens();gm.Context.LoadFailed=true;gm.Context.LoadError="Audit corrupt save";var screen=Object.FindAnyObjectByType<DayIntroScreen>(FindObjectsInactive.Include);screen.Show(gm.Context);yield return null;Render("load-failure");var text=Texts(screen);
  Assert.IsTrue(text.Contains("kayıt")||text.Contains("Kayıt")||text.Contains("bozuk"),"LoadFailed has no visible notice: "+text);
 }
 [UnityTest] public IEnumerator DeskTyping_ShouldFreezeWhenPaused(){
  StartDesk();yield return new WaitForSecondsRealtime(.7f);var client=Object.FindAnyObjectByType<ClientSceneView>();client.Say(new string('A',300));yield return new WaitForSecondsRealtime(.12f);
  var menu=Object.FindAnyObjectByType<PauseMenu>();menu.Open();var text=Field<TMP_Text>(client,"balloonText");int before=text.maxVisibleCharacters;yield return new WaitForSecondsRealtime(.5f);int after=text.maxVisibleCharacters;
  File.WriteAllText(Out+"pause-evidence.txt",$"Before={before}; After={after}; IsOpen={PauseMenu.IsOpen}; timeScale={Time.timeScale}");Assert.AreEqual(before,after,"Client dialogue continued behind pause menu");
 }
 [UnityTest] public IEnumerator AfterStamp_ExtraTestShouldBeDisabled(){
  StartDesk();yield return new WaitForSecondsRealtime(.7f);var desk=Object.FindAnyObjectByType<DeskScreen>();var form=Field<PaperWindow>(desk,"formWindow");form.Open();yield return null;desk.StampDecision(true);Assert.IsTrue(desk.IsStamped);
  var lab=Field<LabReportView>(desk,"labView");var button=Field<Button>(lab,"extraTestButton");bool enabledBefore=button.interactable;int before=gm.Context.State.Money;if(enabledBefore)button.onClick.Invoke();int after=gm.Context.State.Money;File.WriteAllText(Out+"stamp-extra-evidence.txt",$"IsStamped={desk.IsStamped}; ButtonBefore={enabledBefore}; ButtonAfter={button.interactable}; Money {before}->{after}");
  Assert.AreEqual(before,after,"Extra test charged after irreversible stamp, while verdict is still Pending");
 }
 [UnityTest] public IEnumerator VisualAudit_AllPrimaryScreens(){
  HideScreens();var day=Object.FindAnyObjectByType<DayIntroScreen>(FindObjectsInactive.Include);day.Show(gm.Context);yield return null;Render("day-intro");day.Hide();
  gm.Complete();var desk=Object.FindAnyObjectByType<DeskScreen>();var tutorial=Object.FindAnyObjectByType<TutorialView>();yield return new WaitForSecondsRealtime(.8f);Render("tutorial-first");var coach=Object.FindAnyObjectByType<FirstRunCoach>();if(coach!=null)Call(coach,"Finish");yield return null;tutorial.Close();
  var props=Field<DeskPropsView>(desk,"props");props.FormIcon.GetComponentInChildren<Button>(true).onClick.Invoke();props.LabIcon.GetComponentInChildren<Button>(true).onClick.Invoke();yield return new WaitForSecondsRealtime(1f);Render("desk-all-documents");
  HideScreens();var ctx=gm.Context;ctx.LastQuake=new QuakeResult{Day=6,Occurred=true};for(int i=0;i<9;i++)ctx.LastQuake.Outcomes.Add(new BuildingOutcome{BuildingName="Uzun İsimli Şehir Konutları "+i,Address="Papatya Sokak No: 42",Collapsed=true,Deaths=8,Injured=15,VictimNames=new List<string>{"Ayşe Kaya","Mehmet Yıldız","Elif Arslan","Deniz Demir","Ece Yılmaz","Can Aydın"},ResponsibilityText="Rüşvet alarak imzaladığınız bina (3. gün).",WasBribed=true,FailedParams=ctx.TodayProjects[0].Params});
  var result=Object.FindAnyObjectByType<ResultScreen>(FindObjectsInactive.Include);result.Show(ctx);yield return new WaitForSecondsRealtime(3f);Render("newspaper-nine-collapses");result.Hide();
  var ending=Object.FindAnyObjectByType<EndingScreen>(FindObjectsInactive.Include);foreach(var type in new[]{EndingType.Good,EndingType.Bad,EndingType.MiddleClosed,EndingType.MiddleCollapsed}){ctx.State.Ending=type;ending.Show(ctx);yield return new WaitForSecondsRealtime(.1f);Render("ending-"+type);ending.Hide();}
  yield return SceneManager.LoadSceneAsync("MainMenu");yield return new WaitForSecondsRealtime(1f);Render("main-menu");var menu=Object.FindAnyObjectByType<MainMenuScreen>();Call(menu,"SetCredits",true);yield return null;Render("credits");
  Assert.Pass("Rendered primary screens at 1920x1080 with actual Game scene references; synthetic long newspaper case");
 }
 [UnityTest] public IEnumerator CoachFrame_MatchesActualIconCoordinates(){
  gm.Complete();yield return new WaitForSecondsRealtime(1.5f);var coach=Object.FindAnyObjectByType<FirstRunCoach>();var desk=Object.FindAnyObjectByType<DeskScreen>();var props=Field<DeskPropsView>(desk,"props");var frame=Field<RectTransform>(coach,"_frame");
  Func<RectTransform,Vector2> center=r=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));var target=center(props.FormIcon);var highlighted=center(frame);float delta=Vector2.Distance(target,highlighted);
  File.WriteAllText(Out+"coach-geometry.txt",$"Screen={Screen.width}x{Screen.height}; Target={target}; Frame={highlighted}; Difference={delta}");
  Assert.Less(delta,10f,"Coach highlight must track actual form icon in the unmodified Game view");
 }
 [UnityTest] public IEnumerator FirstRunCoach_ActualPointerStampAndDocumentReturn(){
  gm.Complete();yield return new WaitForSecondsRealtime(.8f);var desk=Object.FindAnyObjectByType<DeskScreen>();var coach=Object.FindAnyObjectByType<FirstRunCoach>();Assert.IsNotNull(coach);Assert.AreEqual(0,Field<int>(coach,"_step"));Render("coach-open-form");
  var props=Field<DeskPropsView>(desk,"props");props.FormIcon.GetComponentInChildren<Button>(true).onClick.Invoke();yield return null;Assert.AreEqual(1,Field<int>(coach,"_step"));
  props.LabIcon.GetComponentInChildren<Button>(true).onClick.Invoke();yield return null;Assert.AreEqual(2,Field<int>(coach,"_step"));Call(coach,"Next");yield return null;
  var inspect=Field<InspectController>(desk,"inspect");inspect.SetMode(true);yield return null;Assert.AreEqual(4,Field<int>(coach,"_step"));
  var spots=desk.GetComponentsInChildren<InspectSpot>(true);var a=spots.First(x=>x.Target.Doc==InspectDoc.Form && x.Target.Field==InspectField.Parcel);var b=spots.First(x=>x.Target.Doc==InspectDoc.Lab && x.Target.Field==InspectField.Parcel);
  Call(inspect,"SpotClicked",a,PointerEventData.InputButton.Left);Call(inspect,"SpotClicked",b,PointerEventData.InputButton.Left);yield return null;Assert.AreEqual(5,Field<int>(coach,"_step"));Call(coach,"Next");yield return null;
  var tray=Field<StampTrayView>(desk,"stampTray");tray.transform.Find("Toggle").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(7,Field<int>(coach,"_step"));Render("coach-stamp");
  var stamp=Field<object>(tray,"approve");var stampRect=Field<RectTransform>(stamp,"Rect");var e=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,stampRect.TransformPoint(stampRect.rect.center))};
  Call(tray,"BeginDrag",stamp,e);e.position=RectTransformUtility.WorldToScreenPoint(null,desk.StampArea.TransformPoint(desk.StampArea.rect.center));Call(tray,"Drag",stamp,e);Call(tray,"EndDrag",stamp,e);yield return new WaitForSecondsRealtime(.5f);Assert.IsTrue(desk.IsStamped);Assert.AreEqual(8,Field<int>(coach,"_step"));
  var p=gm.Context.TodayProjects[0];var form=Field<PaperWindow>(desk,"formWindow");var lab=Field<PaperWindow>(desk,"labWindow");
  foreach(var window in new[]{form,lab}){
   var sheet=window.Sheet;var r=(RectTransform)sheet.transform;var target=window.HandBackTarget;var ev=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};sheet.OnBeginDrag(ev);ev.position=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(target.rect.center));sheet.OnDrag(ev);sheet.OnEndDrag(ev);yield return null;
  }
  yield return null;Assert.AreEqual(ProjectVerdict.Approved,p.Verdict);Assert.IsNull(Object.FindAnyObjectByType<FirstRunCoach>());File.WriteAllText(Out+"coach-evidence.txt","9 tutorial steps completed with comparison event, tray pointer drop and both document pointer returns.");
 }
 [UnityTest] public IEnumerator RealWorstCaseNews_ShouldRemainWithinBoxes(){
  HideScreens();var ctx=gm.Context;ctx.LastQuake=JsonUtility.FromJson<QuakeResult>(File.ReadAllText(Out+"real-max-collapse-quake.json"));ctx.State.Day=ctx.LastQuake.Day;
  var screen=Object.FindAnyObjectByType<ResultScreen>(FindObjectsInactive.Include);screen.Show(ctx);yield return new WaitForSecondsRealtime(3f);Render("newspaper-real-worst-case");
  var story=Field<TextMeshProUGUI>(screen,"_story");File.WriteAllText(Out+"real-news-evidence.txt",$"Day={ctx.LastQuake.Day}; Outcomes={ctx.LastQuake.Outcomes.Count}; Collapsed={ctx.LastQuake.CollapsedCount}; StoryOverflow={story.isTextOverflowing}; StoryFont={story.fontSize}");Assert.IsFalse(story.isTextOverflowing,"Real simulated newspaper text extends beyond its allocated box");
 }
 [UnityTest] public IEnumerator SevenDays_StampAndReturnBothDocuments(){
  gm.Session.NewGame(1000);gm.Session.BeginDay();gm.Context.State.Money=100000; // isolate phase integration from economy, separately tested by balance simulation
  var summaries=new List<string>();int decisions=0;int guard=0;
  while(gm.Context.Phase!=GamePhase.Ending && guard++<2000){
   if(gm.Context.Phase!=GamePhase.Desk){gm.Complete();yield return null;continue;}
   var desk=Object.FindAnyObjectByType<DeskScreen>();var coach=Object.FindAnyObjectByType<FirstRunCoach>();if(coach!=null)Call(coach,"Finish");var tutorial=Object.FindAnyObjectByType<TutorialView>();if(tutorial!=null)tutorial.Close();
   foreach(string field in new[]{"stampDelay","reactionHold","shiftEndDelay","shiftEndFade","shiftEndHold"})Put(desk,field,.01f);
   var client=Field<ClientSceneView>(desk,"scene");Put(client,"charsPerSecond",100000f);
   var p=gm.Context.TodayProjects.Find(x=>x.Verdict==ProjectVerdict.Pending);
   if(p==null){yield return null;continue;}
   var form=Field<PaperWindow>(desk,"formWindow");var lab=Field<PaperWindow>(desk,"labWindow");form.Open();lab.Open();yield return null;
   desk.StampDecision(p.IsCompliant);Assert.IsTrue(desk.IsStamped);Assert.AreEqual(ProjectVerdict.Pending,p.Verdict);
   Call(form,"GiveBack");Assert.AreEqual(ProjectVerdict.Pending,p.Verdict,"First document must not finalize decision");Call(lab,"GiveBack");Assert.AreNotEqual(ProjectVerdict.Pending,p.Verdict,"Both documents should finalize decision");decisions++;
   summaries.Add($"Day={gm.Context.State.Day}; ID={p.Id}; Verdict={p.Verdict}");
   for(int wait=0;wait<300 && gm.Context.Phase==GamePhase.Desk && Field<bool>(desk,"stamping");wait++){
    client.CompleteTyping();var gift=Field<GiftOfferView>(desk,"giftOffer");if(gift!=null && gift.IsOpen)Call(gift,"Accept");var card=Field<EventCardView>(desk,"eventCard");if(card!=null && Field<GameObject>(card,"panelRoot").activeSelf)Call(card,"Dismiss");yield return null;
   }
  }
  File.WriteAllLines(Out+"seven-days-evidence.txt",summaries.Concat(new[]{$"Phase={gm.Context.Phase}; Day={gm.Context.State.Day}; Decisions={decisions}; Guard={guard}; Ending={gm.Context.State.Ending}"}));Assert.AreEqual(GamePhase.Ending,gm.Context.Phase);Assert.AreEqual(7,gm.Context.State.Day);Assert.AreEqual(28,decisions);Assert.AreEqual(14,gm.Context.State.TotalApproved);
 }
}
}
