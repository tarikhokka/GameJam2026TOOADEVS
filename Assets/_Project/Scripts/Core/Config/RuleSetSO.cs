using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Yönetmelik ve Denetçi El Kitabı'nın verisi.
    /// Gerekli değer projenin bina türü, deprem bölgesi (mahalleden) ve kat sayısına göre değişir (Ç1).
    /// Masa ekranı "min." değerini GÖSTERMEZ; oyuncu el kitabında arar.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Rule Set", fileName = "RuleSet")]
    public class RuleSetSO : ScriptableObject
    {
        [Tooltip("El kitabının giriş metni (EEE)")]
        [TextArea(3, 12)] public string HandbookIntro;

        [Tooltip("El kitabı s.1: mahalle → deprem bölgesi (1-4)")]
        public List<ZoneEntry> Zones = new List<ZoneEntry>();

        public List<ParamRule> Rules = new List<ParamRule>();

        /// <summary>Mahallenin deprem bölgesi; listede yoksa 0.</summary>
        public int ZoneOf(string neighborhood)
        {
            foreach (var z in Zones)
                if (string.Equals(z.Neighborhood, neighborhood, StringComparison.OrdinalIgnoreCase)) return z.Zone;
            return 0;
        }

        /// <summary>Bina türünün bütün kuralları (bölge/kat ayırmadan; el kitabı sayfaları için).</summary>
        public IEnumerable<ParamRule> RulesFor(BuildingType type)
        {
            foreach (var r in Rules)
                if (r.BuildingType == type) yield return r;
        }

        /// <summary>Bu projeye uygulanan kurallar: her parametre için bölge ve kata en özel uyan kural.</summary>
        public List<ParamRule> RulesFor(BuildingType type, int zone, int floors)
        {
            var best = new Dictionary<ParamType, ParamRule>();
            foreach (var r in Rules)
            {
                if (r.BuildingType != type || !r.Matches(zone, floors)) continue;
                if (!best.TryGetValue(r.Param, out var cur) || r.Specificity > cur.Specificity) best[r.Param] = r;
            }
            return new List<ParamRule>(best.Values);
        }

        /// <summary>Tek parametrenin bu projeye uygulanan kuralı; yoksa null.</summary>
        public ParamRule FindRule(BuildingType type, ParamType param, int zone, int floors)
        {
            ParamRule found = null;
            foreach (var r in Rules)
                if (r.BuildingType == type && r.Param == param && r.Matches(zone, floors)
                    && (found == null || r.Specificity > found.Specificity)) found = r;
            return found;
        }

        /// <summary>Değeri ekranda gösterilecek metne çevirir: harfli parametrede etiket (C25, ZB), değilse sayı + birim.</summary>
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

    [Serializable]
    public class ZoneEntry
    {
        public string Neighborhood;   // NamePool.Neighborhoods ile aynı yazım
        [Range(1, 4)] public int Zone = 1;
    }

    [Serializable]
    public class ParamRule
    {
        public BuildingType BuildingType;
        public ParamType Param;
        public string DisplayName;   // Ör. "Beton sınıfı"
        public string Unit;          // Ör. "MPa" (harfli parametrede boş)
        [Tooltip("En düşük kabul edilen değer. Harfli parametrede Labels içindeki sıra numarası.")]
        public float Required;

        [Header("Ne zaman geçerli (Ç1)")]
        [Tooltip("0 = bütün bölgeler, 1-4 = sadece o deprem bölgesi")]
        [Range(0, 4)] public int Zone;
        [Tooltip("0 = alt sınır yok")] public int MinFloors;
        [Tooltip("0 = üst sınır yok")] public int MaxFloors;

        [Header("El kitabı")]
        [Tooltip("Harfli parametre için kötüden iyiye sıra (ör. C20, C25, C30, C35). Boşsa sayısal.")]
        public List<string> Labels = new List<string>();
        [Tooltip("El kitabındaki açıklama (EEE)")]
        [TextArea] public string Description;

        [Tooltip("Uygunsuz projede Actual = Required * bu aralıktan bir oran (harfli parametrede 1-2 kademe aşağı)")]
        public Vector2 FailRatioRange = new Vector2(0.55f, 0.9f);

        public bool IsLabeled => Labels != null && Labels.Count > 0;

        public bool Matches(int zone, int floors) =>
            (Zone == 0 || Zone == zone) &&
            (MinFloors == 0 || floors >= MinFloors) &&
            (MaxFloors == 0 || floors <= MaxFloors);

        /// <summary>Bölge ve kat koşulu olan kural, genel kuraldan önce gelir.</summary>
        public int Specificity => (Zone != 0 ? 2 : 0) + (MinFloors != 0 || MaxFloors != 0 ? 1 : 0);
    }
}
