using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Yönetmelik ve Denetçi El Kitabı'nın verisi (v2, 17 Ekim öğle).
    /// 5 parametre (2 sayısal + 3 seçenekli); gerekli değer sadece binanın KAT SAYISINA göre değişir (1-3 / 4-6 / 7+ kat).
    /// Mahalle / deprem bölgesi kaldırıldı. Masa "min." değerini GÖSTERMEZ; oyuncu el kitabındaki tablodan bulur.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Rule Set", fileName = "RuleSet")]
    public class RuleSetSO : ScriptableObject
    {
        [Tooltip("El kitabının giriş metni (EEE)")]
        [TextArea(3, 12)] public string HandbookIntro;

        public List<ParamRule> Rules = new List<ParamRule>();

        /// <summary>Bina türünün bütün kuralları (kat grubu ayırmadan; el kitabı tablosu için).</summary>
        public IEnumerable<ParamRule> RulesFor(BuildingType type)
        {
            foreach (var r in Rules)
                if (r.BuildingType == type) yield return r;
        }

        /// <summary>Bu binaya uygulanan kurallar: her parametre için kat sayısına en özel uyan kural.</summary>
        public List<ParamRule> RulesFor(BuildingType type, int floors)
        {
            var best = new Dictionary<ParamType, ParamRule>();
            foreach (var r in Rules)
            {
                if (r.BuildingType != type || !r.Matches(floors)) continue;
                if (!best.TryGetValue(r.Param, out var cur) || r.Specificity > cur.Specificity) best[r.Param] = r;
            }
            return new List<ParamRule>(best.Values);
        }

        /// <summary>Tek parametrenin bu binaya uygulanan kuralı; yoksa null.</summary>
        public ParamRule FindRule(BuildingType type, ParamType param, int floors)
        {
            ParamRule found = null;
            foreach (var r in Rules)
                if (r.BuildingType == type && r.Param == param && r.Matches(floors)
                    && (found == null || r.Specificity > found.Specificity)) found = r;
            return found;
        }

        // ---- Uyumluluk (Desk RulebookView, a9932ac): mahalle/bölge kaldırıldı, her zaman BOŞ. ----
        // RulebookView Zones boşken tek "Yönetmelik" tablosu gösteriyor; v2 için doğru davranış bu.
        // Oğuzhan bölge kodunu kendi tarafından kaldırınca bu iki üye silinecek (memory.md'de not).

        /// <summary>Kaldırıldı: her zaman boş liste.</summary>
        public IReadOnlyList<ZoneEntry> Zones => Array.Empty<ZoneEntry>();

        /// <summary>Kaldırıldı: bölge yok sayılır, FindRule(tür, parametre, kat) ile aynı.</summary>
        public ParamRule FindRule(BuildingType type, ParamType param, int zone, int floors) => FindRule(type, param, floors);

        /// <summary>Değeri ekranda gösterilecek metne çevirir: seçenekli parametrede etiket (C25, Nervürlü), değilse sayı + birim.</summary>
        public string FormatValue(ParamType param, float value)
        {
            foreach (var r in Rules)
            {
                if (r.Param != param) continue;
                if (r.Labels != null && r.Labels.Count > 0)
                {
                    int i = Mathf.Clamp(Mathf.RoundToInt(value), 0, r.Labels.Count - 1);
                    return r.Labels[i];
                }
                string num = value.ToString(Mathf.Approximately(value, Mathf.Round(value)) ? "0" : "0.0#", CultureInfo.GetCultureInfo("tr-TR"));
                return string.IsNullOrEmpty(r.Unit) ? num : $"{num} {r.Unit}";
            }
            return value.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR"));
        }

        /// <summary>Parametrenin görünen adı (ilk kuraldan); yoksa enum adı.</summary>
        public string DisplayNameOf(ParamType param)
        {
            foreach (var r in Rules)
                if (r.Param == param && !string.IsNullOrEmpty(r.DisplayName)) return r.DisplayName;
            return param.ToString();
        }
    }

    /// <summary>Kaldırıldı (uyumluluk için duruyor; RuleSetSO.Zones hep boş).</summary>
    [Serializable]
    public class ZoneEntry
    {
        public string Neighborhood;
        public int Zone;
    }

    [Serializable]
    public class ParamRule
    {
        public BuildingType BuildingType;
        public ParamType Param;
        public string DisplayName;   // Ör. "Kolon kalınlığı"
        public string Unit;          // Ör. "cm" (seçenekli parametrede boş)
        [Tooltip("En düşük kabul edilen değer. Seçenekli parametrede Labels içindeki sıra numarası.")]
        public float Required;

        [Header("Ne zaman geçerli")]
        [Tooltip("0 = alt sınır yok")] public int MinFloors;
        [Tooltip("0 = üst sınır yok")] public int MaxFloors;

        [Header("El kitabı")]
        [Tooltip("Seçenekli parametre için kötüden iyiye sıra (ör. Düz, Nervürlü). Boşsa sayısal.")]
        public List<string> Labels = new List<string>();
        [Tooltip("El kitabındaki açıklama (EEE)")]
        [TextArea] public string Description;

        [Tooltip("Uygunsuz projede Actual = Required * bu aralıktan bir oran (seçenekli parametrede 1-2 kademe aşağı)")]
        public Vector2 FailRatioRange = new Vector2(0.55f, 0.9f);

        public bool IsLabeled => Labels != null && Labels.Count > 0;

        public bool Matches(int floors) =>
            (MinFloors == 0 || floors >= MinFloors) &&
            (MaxFloors == 0 || floors <= MaxFloors);

        /// <summary>Kat koşulu olan kural, genel kuraldan önce gelir.</summary>
        public int Specificity => MinFloors != 0 || MaxFloors != 0 ? 1 : 0;

        /// <summary>El kitabı sütun başlığı, ör. "1-3 kat", "7+ kat", "Tüm binalar".</summary>
        public string FloorsLabel =>
            MinFloors == 0 && MaxFloors == 0 ? "Tüm binalar"
            : MaxFloors == 0 ? $"{MinFloors}+ kat"
            : MinFloors == 0 ? $"en çok {MaxFloors} kat"
            : $"{MinFloors}-{MaxFloors} kat";
    }
}
