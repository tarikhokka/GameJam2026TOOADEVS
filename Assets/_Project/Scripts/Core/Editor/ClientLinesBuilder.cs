using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilJam.Core.Editor
{
    /// <summary>
    /// Ç4: müteahhit tepki replikleri (A taslağı; EEE'den devralındı, EEE düzeltebilir).
    /// Sadece BOŞ listeleri doldurur: EEE'nin Inspector'da yazdıkları korunur.
    /// Metinlerin okunur kopyası: ICERIK_TASLAK.md → "Tepki replikleri".
    /// ReturnLines'ta {last} → "onaylamıştınız" / "reddetmiştiniz" (Core); cümleler ikisiyle de doğru okunur.
    /// Menü: CivilJam > Core > Fill Empty Client Lines
    /// </summary>
    public static class ClientLinesBuilder
    {
        class Lines
        {
            public string[] Approve, Reject, BribeRefused, Return;
        }

        static readonly Dictionary<string, Lines> Data = new Dictionary<string, Lines>
        {
            ["rifat"] = new Lines
            {
                Approve = new[]
                {
                    "Allah razı olsun hocam. Temeline kendim baktım, gönül rahatlığıyla yükselir.",
                    "Sağ olun hocam. Bitince çaya beklerim, bir de siz görün.",
                    "Eksiksiz çıkmasına sevindim hocam. Usta işi böyle olur."
                },
                Reject = new[]
                {
                    "Haklısınız hocam, gözümden kaçmış. Düzeltip yeniden getiririm.",
                    "Otuz yılda pek başıma gelmezdi... Peki hocam, eksiği tamamlarım.",
                    "Kızamam size hocam. Siz bakmasanız kim bakacak?"
                },
                BribeRefused = new string[0],
                Return = new[]
                {
                    "Yine ben geldim hocam. Geçen sefer projemi {last}; bunu daha da dikkatli hazırladım.",
                    "Hocam hatırlarsınız, geçen sefer {last}. Bu dosyayı da aynı titizlikle getirdim."
                }
            },
            ["sevim"] = new Lines
            {
                Approve = new[]
                {
                    "Teşekkürler hocam. Raporların hepsini sakladım, ne zaman isterseniz bakarız.",
                    "İçim rahat hocam. Çocuklar o sokaktan güvenle geçecek.",
                    "Sağ olun. Bu işi sizin gibi yapan pek kalmadı."
                },
                Reject = new[]
                {
                    "Anlıyorum hocam. Laboratuvarla tekrar konuşur, eksiği kapatırım.",
                    "Bir yerde hata yapmışım demek. İyi ki yakaladınız.",
                    "Peki hocam. Pahalıya patlayacak ama doğrusu bu."
                },
                BribeRefused = new string[0],
                Return = new[]
                {
                    "Merhaba yine hocam. Geçen sefer {last}; bu dosyada her satırı iki kez kontrol ettim.",
                    "Hocam, geçen sefer {last}. Bu sefer de numuneleri kendim teslim ettim."
                }
            },
            ["kemal"] = new Lines
            {
                Approve = new[]
                {
                    "İşte bu hocam! Kısa ve net, tam benim sevdiğim gibi.",
                    "Hayırlı olsun hocam. Temeli yarın sabah atıyoruz.",
                    "Görüyor musunuz, uzatmadık, herkes kazandı."
                },
                Reject = new[]
                {
                    "Hocam bu kadar uzatmaya ne gerek vardı?",
                    "Peki peki... Bu şehirde başka kapı da var.",
                    "Vaktimi yediniz hocam, vaktimi."
                },
                BribeRefused = new[]
                {
                    "Zarfı geri mi veriyorsunuz? Hocam, kimse bu kadar kitabına uygun çalışmıyor.",
                    "Ayıp oluyor ama hocam... Ben iyilik yapayım dedim.",
                    "Siz bilirsiniz. Ama bu zarf başka masada kalmaz."
                },
                Return = new[]
                {
                    "Hocam ben yine geldim, kestirmeden. Geçen sefer {last}, bu sefer de uzatmayalım.",
                    "Hatırladınız mı beni? Geçen sefer {last}. Bugün daha hızlı bitiririz."
                }
            },
            ["nuri"] = new Lines
            {
                Approve = new[]
                {
                    "Maşallah hocam, imzanız da yüzünüz kadar güzel!",
                    "Allah sizi başımızdan eksik etmesin hocam.",
                    "Yeğenime söyleyeceğim, hocamız yine yaptı diye."
                },
                Reject = new[]
                {
                    "Aaa hocam, yüreğim cız etti.",
                    "Olur mu böyle... Peki, hatırınız kalsın.",
                    "Kırıldım ama küsmem hocam, yine gelirim."
                },
                BribeRefused = new[]
                {
                    "Hocam zarfı beğenmediniz galiba. Bir dahakine daha dolgun getiririm.",
                    "Göz nurumu geri çevirdiniz... Olsun, ben ısrarcı adamım.",
                    "Az geldi demek. Anladım hocam, anladım."
                },
                Return = new[]
                {
                    "Hocaaam! Geçen sefer {last} ama ben size hiç küsmedim.",
                    "Yine ben, göz nurunuz! Geçen sefer {last}; bu sefer gönlünüzü hoş edeceğim."
                }
            },
            ["selim"] = new Lines
            {
                Approve = new[]
                {
                    "Teşekkürler hocam. Evrak dediğin böyle olur.",
                    "Bilirdim. Kaşesi, imzası, hepsi yerinde.",
                    "İyi çalışmalar hocam. Arşivinize düzgün bir dosya daha kattık."
                },
                Reject = new[]
                {
                    "Hangi belgede sorun gördünüz hocam? Ben göremedim.",
                    "İlginç... Laboratuvara bir daha soracağım.",
                    "Peki hocam. Kâğıtlar tamamdı ama siz bilirsiniz."
                },
                BribeRefused = new string[0],
                Return = new[]
                {
                    "İyi günler hocam. Geçen sefer {last}; bu dosya ondan da düzgün.",
                    "Yine ben hocam. Geçen sefer {last}. Evraklar her zamanki gibi eksiksiz."
                }
            },
            ["rustem"] = new Lines
            {
                Approve = new[]
                {
                    "Gördün mü hocam, ne güzel anlaştık.",
                    "Akıllı adamsın hocam. Camların da sağlam kalır.",
                    "Bu iyiliği unutmam hocam."
                },
                Reject = new[]
                {
                    "Yanlış yaptın hocam. Çok yanlış yaptın.",
                    "Bu mahallede böyle iş olmaz hocam... Görürsün.",
                    "Peki hocam. Akşama ofisin camlarına bir bak."
                },
                BribeRefused = new[]
                {
                    "Zarfımı geri mi itiyorsun? Bunu unutmam hocam.",
                    "Sen bilirsin. Ben kibarca teklif ettim."
                },
                Return = new[]
                {
                    "Yine ben hocam. Geçen sefer {last}; bunu ikimiz de hatırlıyoruz.",
                    "Hocam, geçen sefer {last}. Bugün nasıl biteceği sana bağlı."
                }
            },
        };

        [MenuItem("CivilJam/Core/Fill Empty Client Lines")]
        public static void Fill()
        {
            int filled = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:ClientSO", new[] { "Assets/_Project/Data/Clients" }))
            {
                var c = AssetDatabase.LoadAssetAtPath<ClientSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (c == null || !Data.TryGetValue(c.Id, out var lines)) continue;
                Undo.RecordObject(c, "Fill client lines");
                filled += FillIfEmpty(ref c.ApproveLines, lines.Approve);
                filled += FillIfEmpty(ref c.RejectLines, lines.Reject);
                filled += FillIfEmpty(ref c.BribeRefusedLines, lines.BribeRefused);
                filled += FillIfEmpty(ref c.ReturnLines, lines.Return);
                EditorUtility.SetDirty(c);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[ClientLines] {filled} liste dolduruldu");
        }

        static int FillIfEmpty(ref List<string> target, string[] source)
        {
            if (source == null || source.Length == 0) return 0;
            if (target != null && target.Count > 0) return 0;
            target = new List<string>(source);
            return 1;
        }
    }
}
