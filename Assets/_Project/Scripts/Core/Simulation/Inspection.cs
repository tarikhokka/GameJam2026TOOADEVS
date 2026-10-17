using System;
using System.Collections.Generic;

namespace CivilJam.Core
{
    public enum InspectDoc { Form, Lab, Handbook }

    public enum InspectField { Param, ExtraTest, Parcel, Date, Signature, LabName, LabStamp, AccreditedLabs }

    public enum InspectVerdict { Match, Mismatch, Unrelated }

    /// <summary>İnceleme modunda tıklanan bir alan. Param sadece Param / ExtraTest alanlarında anlamlıdır.</summary>
    [Serializable]
    public struct InspectTarget
    {
        public InspectDoc Doc;
        public InspectField Field;
        public ParamType Param;

        public static InspectTarget Of(InspectDoc doc, InspectField field) => new InspectTarget { Doc = doc, Field = field };
        public static InspectTarget OfParam(InspectDoc doc, ParamType param) => new InspectTarget { Doc = doc, Field = InspectField.Param, Param = param };
        public static InspectTarget OfTest(ParamType param) => new InspectTarget { Doc = InspectDoc.Lab, Field = InspectField.ExtraTest, Param = param };
    }

    public class InspectResult
    {
        public InspectVerdict Verdict;
        public string Title;    // "SORUN YOK" / "UYUŞMAZLIK SAPTANDI" / "ALAKASIZ"
        public string Detail;   // Kısa açıklama
    }

    /// <summary>
    /// İnceleme modu (Papers, Please tarzı; Ahmet kararı 17 Ekim akşam, kural tablosu memory.md "🔎").
    /// Oyuncu iki ayrı belgeden iki alan seçer; sonuç: uyuyor / uyuşmazlık / alakasız. Saf fonksiyon: oyunun durumunu değiştirmez.
    /// El kitabı eşiği oyuncunun seçtiği kat grubundan alınır; seçilmemiş ya da binaya ait olmayan grup → Alakasız.
    /// </summary>
    public static class Inspection
    {
        public const int NoFloorGroup = -1;
        public const string TitleMatch = "SORUN YOK";
        public const string TitleMismatch = "UYUŞMAZLIK SAPTANDI";
        public const string TitleUnrelated = "ALAKASIZ";

        /// <summary>Kat grubu indeksi → o grubun temsilci kat sayısı (0 = 1-3, 1 = 4-6, 2 = 7+).</summary>
        static readonly int[] GroupFloors = { 2, 5, 8 };

        /// <summary>Alan bu projede tıklanabilir mi? Ek test hücresi sadece test yapıldıysa.</summary>
        public static bool IsAvailable(ProjectData project, InspectTarget t)
        {
            if (project == null) return false;
            if (t.Field == InspectField.ExtraTest)
            {
                var e = project.GetParam(t.Param);
                return t.Doc == InspectDoc.Lab && e != null && e.Tested;
            }
            return true;
        }

        public static InspectResult Check(GameContext ctx, ProjectData project, InspectTarget a, InspectTarget b, int handbookFloorGroup)
        {
            if (ctx == null || project == null || a.Doc == b.Doc) return Unrelated();
            // Sırayı sabitle: Form < Lab < Handbook
            if (a.Doc > b.Doc) { var t = a; a = b; b = t; }

            bool aParam = a.Field == InspectField.Param || a.Field == InspectField.ExtraTest;
            bool bParam = b.Field == InspectField.Param || b.Field == InspectField.ExtraTest;
            if (aParam && bParam) return CheckParams(ctx, project, a, b, handbookFloorGroup);
            if (aParam || bParam) return Unrelated();

            var form = project.Documents.Count > 0 ? project.Documents[0] : null;
            var lab = project.Documents.Count > 1 ? project.Documents[1] : null;
            if (lab == null) return Unrelated();

            if (a.Doc == InspectDoc.Form && b.Doc == InspectDoc.Lab && a.Field == b.Field && form != null)
            {
                switch (a.Field)
                {
                    case InspectField.Parcel: return Same("Ada / Parsel", form.ParcelNo, lab.ParcelNo);
                    case InspectField.Date: return Same("Tarih", form.Date, lab.Date);
                    case InspectField.Signature: return Same("İmzalayan mühendis", form.SignatureName, lab.SignatureName);
                }
                return Unrelated();
            }

            if (a.Doc == InspectDoc.Lab && b.Doc == InspectDoc.Handbook && b.Field == InspectField.AccreditedLabs)
            {
                if (a.Field == InspectField.LabName)
                    return Accredited(ctx, lab.LabName, "Raporu yazan laboratuvar");
                if (a.Field == InspectField.LabStamp)
                    return Accredited(ctx, StampLabOf(lab), "Kaşedeki laboratuvar");
            }
            return Unrelated();
        }

        static InspectResult CheckParams(GameContext ctx, ProjectData project, InspectTarget a, InspectTarget b, int group)
        {
            if (a.Param != b.Param) return Unrelated();
            var e = project.GetParam(a.Param);
            if (e == null || !IsAvailable(project, a) || !IsAvailable(project, b)) return Unrelated();
            // El kitabında parametre satırı dışında bir alan (ör. ek test) olamaz
            if (b.Doc == InspectDoc.Handbook && b.Field != InspectField.Param) return Unrelated();
            if (a.Doc == InspectDoc.Form && a.Field != InspectField.Param) return Unrelated();

            string name = ctx.Rules != null ? ctx.Rules.DisplayNameOf(a.Param) : a.Param.ToString();
            string Fmt(float v) => ctx.Rules != null ? ctx.Rules.FormatValue(a.Param, v) : v.ToString("0.##");

            if (b.Doc == InspectDoc.Handbook)
            {
                // Eşik: oyuncunun seçtiği kat grubu; binanın grubu değilse alakasız
                if (group < 0 || group >= GroupFloors.Length || ctx.Rules == null) return Unrelated();
                var selected = ctx.Rules.FindRule(project.BuildingType, a.Param, GroupFloors[group]);
                var actual = ctx.Rules.FindRule(project.BuildingType, a.Param, project.Floors);
                if (selected == null || selected != actual) return Unrelated();

                float value = a.Field == InspectField.ExtraTest ? e.Actual : a.Doc == InspectDoc.Lab ? e.LabValue : e.Reported;
                string what = a.Field == InspectField.ExtraTest ? $"{name} (ek test)" : name;
                return value >= selected.Required
                    ? Result(InspectVerdict.Match, $"{what}: {Fmt(value)}, {selected.FloorsLabel} için uygun")
                    : Result(InspectVerdict.Mismatch, $"{what}: {Fmt(value)}, {selected.FloorsLabel} için en az {Fmt(selected.Required)}");
            }

            // Form ↔ Lab (beyan ya da ek test)
            float left = e.Reported;                                             // form her zaman beyanı gösterir
            float right = b.Field == InspectField.ExtraTest ? e.Actual : e.LabValue; // lab beyanı formdan farklı olabilir
            string rightName = b.Field == InspectField.ExtraTest ? "ek test" : "rapordaki beyan";
            return Math.Abs(left - right) < 1e-4f
                ? Result(InspectVerdict.Match, $"{name}: beyan ile {rightName} aynı ({Fmt(left)})")
                : Result(InspectVerdict.Mismatch, $"{name}: beyan {Fmt(left)}, {rightName} {Fmt(right)}");
        }

        static InspectResult Same(string what, string x, string y) =>
            string.Equals(x ?? "", y ?? "", StringComparison.Ordinal)
                ? Result(InspectVerdict.Match, $"{what} iki belgede aynı")
                : Result(InspectVerdict.Mismatch, $"{what} farklı: {x} ↔ {y}");

        static InspectResult Accredited(GameContext ctx, string labName, string what) =>
            IsAccredited(ctx, labName)
                ? Result(InspectVerdict.Match, $"{what} ({labName}) akredite listede")
                : Result(InspectVerdict.Mismatch, $"{what} ({labName}) akredite listede yok");

        static bool IsAccredited(GameContext ctx, string labName)
        {
            if (string.IsNullOrEmpty(labName) || ctx.Names == null || ctx.Names.LabNames == null) return false;
            string key = ProjectGenerator.UpperTr(labName.Trim());
            foreach (var l in ctx.Names.LabNames)
                if (!string.IsNullOrEmpty(l) && ProjectGenerator.UpperTr(l.Trim()) == key) return true;
            return false;
        }

        /// <summary>Kaşedeki lab adı; eski veride (StampLabName boş) "ADI · AKREDİTE" metninden çıkarılır.</summary>
        static string StampLabOf(DocumentData lab)
        {
            if (!string.IsNullOrEmpty(lab.StampLabName)) return lab.StampLabName;
            if (string.IsNullOrEmpty(lab.StampId)) return "";
            int i = lab.StampId.IndexOf(" · ", StringComparison.Ordinal);
            return i > 0 ? lab.StampId.Substring(0, i) : lab.StampId;
        }

        static InspectResult Result(InspectVerdict v, string detail) => new InspectResult
        {
            Verdict = v,
            Title = v == InspectVerdict.Match ? TitleMatch : TitleMismatch,
            Detail = detail
        };

        static InspectResult Unrelated() => new InspectResult
        {
            Verdict = InspectVerdict.Unrelated,
            Title = TitleUnrelated,
            Detail = "Bu iki bilgi birbiriyle karşılaştırılamaz."
        };
    }
}
