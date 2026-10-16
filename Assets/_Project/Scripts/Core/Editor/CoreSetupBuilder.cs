using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Menü: CivilJam > Core > ...
    /// Varsayılan veri asset'lerini (Assets/_Project/Data) ve Game.unity sahnesini üretir.
    /// Var olan asset'lerin ÜZERİNE YAZMAZ (EEE'nin içerik düzenlemeleri korunur).
    /// </summary>
    public static class CoreSetupBuilder
    {
        const string DataDir = "Assets/_Project/Data";
        const string ScenePath = "Assets/_Project/Scenes/Game.unity";

        [MenuItem("CivilJam/Core/Create Missing Data Assets")]
        public static void CreateDataAssets()
        {
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory(DataDir + "/Days");
            Directory.CreateDirectory(DataDir + "/Expenses");

            GetOrCreate<BalanceSO>(DataDir + "/Balance.asset", _ => { });

            GetOrCreate<RuleSetSO>(DataDir + "/RuleSet.asset", r =>
            {
                r.Rules = new List<ParamRule>
                {
                    Rule(BuildingType.Residential, ParamType.ConcreteStrength, "Beton dayanımı", "MPa", 30f),
                    Rule(BuildingType.Residential, ParamType.RebarRatio, "Donatı oranı", "%", 1.0f),
                    Rule(BuildingType.Residential, ParamType.SoilQuality, "Zemin taşıma gücü", "kPa", 150f),
                    Rule(BuildingType.Residential, ParamType.SeismicDesign, "Deprem dayanım katsayısı", "", 1.0f),
                    Rule(BuildingType.Factory, ParamType.ConcreteStrength, "Beton dayanımı", "MPa", 35f),
                    Rule(BuildingType.Factory, ParamType.RebarRatio, "Donatı oranı", "%", 1.2f),
                    Rule(BuildingType.Factory, ParamType.SoilQuality, "Zemin taşıma gücü", "kPa", 200f),
                    Rule(BuildingType.Factory, ParamType.FireSafety, "Yangın kaçış genişliği", "m", 1.5f),
                };
            });

            GetOrCreate<DayConfigSO>(DataDir + "/Days/Day01.asset", d => { d.FromDay = 1; d.ProjectsPerDay = 3; d.NonCompliantRatio = 0.34f; });
            GetOrCreate<DayConfigSO>(DataDir + "/Days/Day03.asset", d => { d.FromDay = 3; d.ProjectsPerDay = 4; d.NonCompliantRatio = 0.5f; d.AllowBribes = true; });
            GetOrCreate<DayConfigSO>(DataDir + "/Days/Day05.asset", d => { d.FromDay = 5; d.ProjectsPerDay = 5; d.NonCompliantRatio = 0.5f; d.AllowBribes = true; d.AllowFakeDocuments = true; d.AllowThugs = true; });
            GetOrCreate<DayConfigSO>(DataDir + "/Days/Day08.asset", d => { d.FromDay = 8; d.ProjectsPerDay = 6; d.NonCompliantRatio = 0.5f; d.AllowBribes = true; d.AllowFakeDocuments = true; d.AllowThugs = true; d.AllowFactories = true; });

            GetOrCreate<ExpenseSO>(DataDir + "/Expenses/Rent.asset", e => { e.Id = "rent"; e.DisplayName = "Ofis kirası"; e.Amount = 400; e.Mandatory = true; });
            GetOrCreate<ExpenseSO>(DataDir + "/Expenses/Loan.asset", e => { e.Id = "loan"; e.DisplayName = "Kredi taksiti"; e.Amount = 300; e.Mandatory = true; });
            GetOrCreate<ExpenseSO>(DataDir + "/Expenses/Insurance.asset", e => { e.Id = "insurance"; e.DisplayName = "Mesleki sigorta"; e.Amount = 150; e.Mandatory = false; });

            GetOrCreate<NamePoolSO>(DataDir + "/NamePool.asset", n =>
            {
                n.FirstNames = new List<string> { "Elif", "Zeynep", "Defne", "Ece", "Nehir", "Ada", "Melis", "Sude", "Derya", "Gülsüm", "Hatice", "Nermin",
                    "Kerem", "Emir", "Baran", "Doruk", "Arda", "Tuna", "Yiğit", "Cem", "Selim", "Rıza", "Kadir", "Orhan" };
                n.LastNames = new List<string> { "Akbulut", "Sarıçam", "Tepeli", "Yazgan", "Erinç", "Koyuncu", "Ilgaz", "Bozdemir", "Kayalı", "Ormancı",
                    "Gökalp", "Sevindik", "Uçarlı", "Demirtaşlı", "Kumru", "Eryaman", "Taşkıran", "Alpaslı" };
                n.BuildingNames = new List<string> { "Papatya", "Lale", "Menekşe", "Yasemin", "Çınar", "Ihlamur", "Martı", "Güneş", "Huzur", "Umut", "Bahar", "Yıldız" };
                n.Neighborhoods = new List<string> { "Yeşiltepe", "Karaağaç", "Bağlarbaşı", "Esenyurtlu", "Kuşdili", "Sarıkaya" };
                n.Streets = new List<string> { "Akasya", "Söğüt", "Kavak", "Defne", "Erguvan", "Nar" };
                n.LabNames = new List<string> { "Merkez Yapı Lab.", "Anadolu Malzeme Test", "Kale Zemin Etüt" };
                n.EngineerNames = new List<string> { "Müh. S. Erdem", "Müh. T. Kaplan", "Müh. N. Aksoy", "Müh. B. Yalın" };
            });

            GetOrCreate<HeadlineSO>(DataDir + "/Headlines.asset", h =>
            {
                h.NoCollapse = new List<string> { "Deprem korkuttu, binalar ayakta kaldı" };
                h.SingleCollapse = new List<string> { "{building} yıkıldı: {count} can kaybı" };
                h.MultiCollapse = new List<string> { "Depremde {count} bina yıkıldı" };
                h.BribedCollapse = new List<string> { "Yıkılan binanın onayında usulsüzlük şüphesi" };
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CoreSetup] Data asset'leri hazır: " + DataDir);
        }

        [MenuItem("CivilJam/Core/Build Game Scene")]
        public static void BuildGameScene()
        {
            CreateDataAssets();

            if (File.Exists(ScenePath))
            {
                Debug.LogWarning("[CoreSetup] Game.unity zaten var, üzerine yazılmadı: " + ScenePath);
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();

            var so = new SerializedObject(gm);
            so.FindProperty("balance").objectReferenceValue = Load<BalanceSO>(DataDir + "/Balance.asset");
            so.FindProperty("rules").objectReferenceValue = Load<RuleSetSO>(DataDir + "/RuleSet.asset");
            so.FindProperty("names").objectReferenceValue = Load<NamePoolSO>(DataDir + "/NamePool.asset");
            so.FindProperty("headlines").objectReferenceValue = Load<HeadlineSO>(DataDir + "/Headlines.asset");
            FillList(so.FindProperty("dayConfigs"), FindAll<DayConfigSO>(DataDir + "/Days"));
            FillList(so.FindProperty("expenses"), FindAll<ExpenseSO>(DataDir + "/Expenses"));
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            Debug.Log("[CoreSetup] Game.unity oluşturuldu: " + ScenePath);
        }

        // ---------- yardımcılar ----------

        static ParamRule Rule(BuildingType b, ParamType p, string name, string unit, float required) =>
            new ParamRule { BuildingType = b, Param = p, DisplayName = name, Unit = unit, Required = required };

        static T GetOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        static List<T> FindAll<T>(string dir) where T : Object =>
            AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { dir })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

        static void FillList<T>(SerializedProperty list, List<T> items) where T : Object
        {
            list.ClearArray();
            for (int i = 0; i < items.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }

        static void AddSceneToBuild(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
