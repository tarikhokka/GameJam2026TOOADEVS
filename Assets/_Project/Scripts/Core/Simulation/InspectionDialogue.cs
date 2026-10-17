using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    /// <summary>
    /// Uyuşmazlık diyaloğu + rüşvetin uyuşmazlıktan sonra teklif edilmesi (Ahmet kararı 17 Ekim ~19:20, memory.md "💬").
    ///  - Kaşe: Inspection.Check (aynı kurallar).
    ///  - UYUŞMAZLIK ve bu projede bu alan çifti için ilk kez: denetçi cümlesi (DialogueSO şablonu, gerçek veriyle) +
    ///    müteahhit cevabı (ClientSO.MismatchLines).
    ///  - Projede zarf varsa ve henüz teklif edilmediyse (ilk uyuşmazlık): teklif (BribeLines) + denetçi tepkisi
    ///    (DialogueSO.BribeReactions) + kapanış (ClientSO.BribeCloseLines); ProjectData.BribeRevealed = true.
    ///  - Boş replik listesi → o satır atlanır (diyalog yine çalışır).
    /// Oyun (GameSession.Inspect) ve sandbox'lar aynı fonksiyonu çağırır.
    /// </summary>
    public static class InspectionDialogue
    {
        public static InspectOutcome Run(GameContext ctx, ProjectData project, InspectTarget a, InspectTarget b, int floorGroup, Random rng)
        {
            var outcome = new InspectOutcome { Result = Inspection.Check(ctx, project, a, b, floorGroup) };
            if (project == null || outcome.Result.Verdict != InspectVerdict.Mismatch || project.Verdict != ProjectVerdict.Pending)
                return outcome;

            if (a.Doc > b.Doc) { var t = a; a = b; b = t; }
            string key = Key(a) + "|" + Key(b);
            if (project.InspectedMismatches.Contains(key)) return outcome; // aynı uyuşmazlık: sadece kaşe
            project.InspectedMismatches.Add(key);

            var client = ctx.GetClient(project.ClientId);
            Add(outcome, DialogueSpeaker.Inspector, InspectorLine(ctx, project, a, b, floorGroup));
            Add(outcome, DialogueSpeaker.Client, rng.Pick(client != null ? client.MismatchLines : null, ""));

            if (project.OffersBribe && !project.BribeRevealed)
            {
                project.BribeRevealed = true;
                outcome.BribeOffered = true;
                Add(outcome, DialogueSpeaker.Client, rng.Pick(client != null ? client.BribeLines : null, ""));
                var reactions = ctx.Dialogue != null ? ctx.Dialogue.BribeReactions : null;
                Add(outcome, DialogueSpeaker.Inspector, rng.Pick(reactions, DialogueSO.DefaultBribeReaction));
                Add(outcome, DialogueSpeaker.Client, rng.Pick(client != null ? client.BribeCloseLines : null, ""));
            }
            return outcome;
        }

        static string Key(InspectTarget t) => $"{t.Doc}.{t.Field}.{(int)t.Param}";

        static void Add(InspectOutcome o, DialogueSpeaker who, string text)
        {
            if (!string.IsNullOrWhiteSpace(text)) o.Lines.Add(new DialogueLine { Speaker = who, Text = text });
        }

        /// <summary>Denetçinin cümlesi; a ve b sıralı (Form &lt; Lab &lt; Handbook) ve sonuç UYUŞMAZLIK.</summary>
        static string InspectorLine(GameContext ctx, ProjectData p, InspectTarget a, InspectTarget b, int group)
        {
            var d = ctx.Dialogue;
            var rules = ctx.Rules;
            var form = p.Documents.Count > 0 ? p.Documents[0] : null;
            var lab = p.Documents.Count > 1 ? p.Documents[1] : null;
            var e = p.GetParam(a.Param);
            string Fmt(float v) => rules != null ? rules.FormatValue(a.Param, v) : v.ToString("0.##");
            string name = rules != null ? rules.DisplayNameOf(a.Param) : a.Param.ToString();

            if ((a.Field == InspectField.Param || a.Field == InspectField.ExtraTest) && e != null)
            {
                if (b.Doc == InspectDoc.Handbook)
                {
                    var rule = rules != null ? rules.FindRule(p.BuildingType, a.Param, p.Floors) : null;
                    bool test = a.Field == InspectField.ExtraTest;
                    return Fill(Pick(test ? d?.TestBelowRule : d?.ParamBelowRule, test ? DialogueSO.DefaultTestBelowRule : DialogueSO.DefaultParamBelowRule),
                        ("param", name), ("value", Fmt(test ? e.Actual : a.Doc == InspectDoc.Lab ? e.LabValue : e.Reported)),
                        ("required", rule != null ? Fmt(rule.Required) : ""), ("group", rule != null ? rule.FloorsLabel : ""));
                }
                if (b.Field != InspectField.ExtraTest) // form ↔ lab beyanı (değer farkı)
                    return Fill(Pick(d?.FormVsLab, DialogueSO.DefaultFormVsLab),
                        ("param", name), ("form", Fmt(e.Reported)), ("lab", Fmt(e.LabValue)));
                return Fill(Pick(d?.ReportedVsTest, DialogueSO.DefaultReportedVsTest),
                    ("param", name), ("reported", Fmt(e.Reported)), ("actual", Fmt(e.Actual)));
            }

            switch (a.Field)
            {
                case InspectField.Parcel:
                    return Fill(Pick(d?.ParcelMismatch, DialogueSO.DefaultParcelMismatch), ("form", form?.ParcelNo), ("lab", lab?.ParcelNo));
                case InspectField.Date:
                    return Fill(Pick(d?.DateMismatch, DialogueSO.DefaultDateMismatch), ("form", form?.Date), ("lab", lab?.Date));
                case InspectField.Signature:
                    return Fill(Pick(d?.SignatureMismatch, DialogueSO.DefaultSignatureMismatch), ("form", form?.SignatureName), ("lab", lab?.SignatureName));
                case InspectField.LabName:
                    return Fill(Pick(d?.LabNotAccredited, DialogueSO.DefaultLabNotAccredited), ("lab", lab?.LabName));
                case InspectField.LabStamp:
                    return Fill(Pick(d?.StampNotAccredited, DialogueSO.DefaultStampNotAccredited), ("lab", StampLab(lab)));
            }
            return "";
        }

        static string StampLab(DocumentData lab)
        {
            if (lab == null) return "";
            if (!string.IsNullOrEmpty(lab.StampLabName)) return lab.StampLabName;
            int i = lab.StampId != null ? lab.StampId.IndexOf(" · ", StringComparison.Ordinal) : -1;
            return i > 0 ? lab.StampId.Substring(0, i) : lab.StampId ?? "";
        }

        static string Pick(string custom, string fallback) => string.IsNullOrWhiteSpace(custom) ? fallback : custom;

        static string Fill(string template, params (string key, string value)[] values)
        {
            foreach (var (k, v) in values) template = template.Replace("{" + k + "}", v ?? "");
            return template;
        }
    }
}
