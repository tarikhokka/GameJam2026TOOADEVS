using System.IO;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Tek slot JSON kayıt (persistentDataPath). Sadece GameState kaydedilir, gün sonunda.</summary>
    public static class SaveSystem
    {
        const string FileName = "save.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(FilePath);

        public static bool Save(GameState state)
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(state, true));
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"[Save] Kayıt başarısız: {e.Message}");
                return false;
            }
        }

        public static GameState Load()
        {
            if (!HasSave) return null;
            try
            {
                return JsonUtility.FromJson<GameState>(File.ReadAllText(FilePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Save] Yükleme başarısız: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(FilePath);
        }
    }
}
