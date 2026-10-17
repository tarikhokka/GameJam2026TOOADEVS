using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace CivilJam.EditorTools
{
    /// <summary>
    /// (EEE, 17 Ekim) Assets/_Project/Fonts içindeki TTF'lerden TMP font asset'i üretir ("PaytoneOne-Regular SDF", "FiraSans-Regular SDF", "FiraSans-Bold SDF").
    /// Unity açılınca eksik olan varsa kendiliğinden bir kez çalışır; elle: CivilJam > EEE > Create Font Assets.
    /// TMP'nin kendi "Create > TextMeshPro > Font Asset > SDF" komutunu kullanır, sonra Türkçe dahil karakter setini
    /// önceden ekleyip asset'i Static yapar: oyun sırasında atlas değişmez, dosya kendiliğinden kirlenmez.
    /// </summary>
    [InitializeOnLoad]
    static class FontAssetCreator
    {
        const string FontDir = "Assets/_Project/Fonts";
        const string Charset =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
            "ÇĞİÖŞÜçğıöşüÂâÎîÛû·•—–…’‘“”«»°%×";

        static FontAssetCreator()
        {
            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool("CivilJam.FontAssetCreator.Ran", false)) return;
                SessionState.SetBool("CivilJam.FontAssetCreator.Ran", true);
                CreateMissing();
            };
        }

        [MenuItem("CivilJam/EEE/Create Font Assets")]
        public static void CreateMissing()
        {
            if (!AssetDatabase.IsValidFolder(FontDir)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Font", new[] { FontDir }))
            {
                string fontPath = AssetDatabase.GUIDToAssetPath(guid);
                string assetPath = Path.Combine(Path.GetDirectoryName(fontPath), Path.GetFileNameWithoutExtension(fontPath) + " SDF.asset").Replace('\\', '/');
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null) continue;

                var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (font == null) continue;
                Selection.activeObject = font;
                if (!EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Font Asset/SDF"))
                {
                    Debug.LogWarning($"[FontAssetCreator] TMP menüsü bulunamadı: {fontPath}");
                    continue;
                }
                AssetDatabase.Refresh();

                var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                if (asset == null) { Debug.LogWarning($"[FontAssetCreator] Oluşturulamadı: {assetPath}"); continue; }
                asset.TryAddCharacters(Charset, out string missing);
                asset.atlasPopulationMode = AtlasPopulationMode.Static;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                Debug.Log($"[FontAssetCreator] {assetPath} oluşturuldu" + (string.IsNullOrEmpty(missing) ? "." : $" (fontta olmayan: {missing})"));
            }
        }
    }
}
