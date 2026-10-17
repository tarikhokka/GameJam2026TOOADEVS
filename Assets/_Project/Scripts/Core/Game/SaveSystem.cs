using System.IO;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>Tek slot JSON kayıt (persistentDataPath). Sadece GameState kaydedilir, gün sonunda.
    /// Yazma önce geçici dosyaya yapılır, sonra yerine konur: yarıda kalan yazma eski kaydı bozmaz.</summary>
    public static class SaveSystem
    {
        const string FileName = "save.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(FilePath);

        /// <summary>Son Load() kayıt dosyası olduğu hâlde okuyamadıysa hata metni; yoksa null.</summary>
        public static string LastLoadError { get; private set; }

        public static bool Save(GameState state) => Save(state, out _);

        public static bool Save(GameState state, out string error)
        {
            error = null;
            string tmp = FilePath + ".tmp";
            try
            {
                File.WriteAllText(tmp, JsonUtility.ToJson(state, true));
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tmp, FilePath);
                return true;
            }
            catch (System.Exception e) // IO, yetki (UnauthorizedAccess), disk dolu...
            {
                error = e.Message;
                Debug.LogError($"[Save] Kayıt başarısız: {e.Message}");
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* yoksay */ }
                return false;
            }
        }

        /// <summary>Kayıt yoksa ya da okunamıyorsa null. Okunamadıysa LastLoadError dolar.</summary>
        public static GameState Load()
        {
            LastLoadError = null;
            if (!HasSave) return null;
            try
            {
                var state = JsonUtility.FromJson<GameState>(File.ReadAllText(FilePath));
                if (state == null || state.Day < 1) throw new IOException("Kayıt dosyası boş ya da bozuk.");
                return state;
            }
            catch (System.Exception e)
            {
                LastLoadError = e.Message;
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
