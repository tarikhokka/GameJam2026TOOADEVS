using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Denetçinin (oyuncu) uyuşmazlık cümleleri ve rüşvete tepkileri (memory.md "💬"). Metinler EEE'nin, şablon yer tutucuları Core'un.
    /// Yer tutucular: {param} {value} {required} {group} (ör. "4-6 kat") {reported} {actual} {form} {lab}.
    /// Boş şablon → Core'un varsayılanı kullanılır.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Dialogue", fileName = "Dialogue")]
    public class DialogueSO : ScriptableObject
    {
        [Header("Denetçi: uyuşmazlık cümleleri (şablon)")]
        [TextArea] public string ParamBelowRule = DefaultParamBelowRule;
        [TextArea] public string TestBelowRule = DefaultTestBelowRule;
        [TextArea] public string ReportedVsTest = DefaultReportedVsTest;
        [TextArea] public string ParcelMismatch = DefaultParcelMismatch;
        [TextArea] public string DateMismatch = DefaultDateMismatch;
        [TextArea] public string SignatureMismatch = DefaultSignatureMismatch;
        [TextArea] public string LabNotAccredited = DefaultLabNotAccredited;
        [TextArea] public string StampNotAccredited = DefaultStampNotAccredited;

        [Header("Denetçi: rüşvet teklifine kısa tepki (rastgele biri)")]
        [TextArea] public List<string> BribeReactions = new List<string>();

        public const string DefaultParamBelowRule = "{param} {value} yazıyor; {group}lı binada en az {required} olmalı.";
        public const string DefaultTestBelowRule = "Ek testte {param} {value} çıktı; {group}lı binada en az {required} olmalı.";
        public const string DefaultReportedVsTest = "Formda {param} {reported} yazıyor ama ek test {actual} gösteriyor.";
        public const string DefaultParcelMismatch = "Formdaki parsel {form}, raporda {lab} yazıyor.";
        public const string DefaultDateMismatch = "Formun tarihi {form}, raporun tarihi {lab}.";
        public const string DefaultSignatureMismatch = "Formu {form} imzalamış, raporu {lab}.";
        public const string DefaultLabNotAccredited = "{lab} akredite laboratuvarlar listesinde yok.";
        public const string DefaultStampNotAccredited = "Kaşedeki {lab} akredite listede yok.";
        public const string DefaultBribeReaction = "Bu zarf ne?";
    }
}
