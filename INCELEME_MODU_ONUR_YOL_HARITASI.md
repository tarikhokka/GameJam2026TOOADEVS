**İnceleme modu — Ahmet'in Onur'dan istediği iş ve uygulama planı**

17 Ekim 2026 · İncelenen checkout: `abe8cd3` ve mevcut yerel değişiklikler. Kaynak: `memory.md:91–234`, özellikle onay notu ve ayrıntılı uygulama notu. Bu belge planlama çıktısıdır; mekanik henüz bu çalışma kapsamında uygulanmadı.

**Uygulama güncellemesi:** Ahmet'in `78a847c` Core API'si alındı. Onur'un `InspectResultView.cs` bileşeni gerçek API ile yazıldı; ayrı kopyada 4/4 PlayMode testi ve Unity render kontrolü geçti. Aşağıdaki ilk planın Core-bekleme bilgisi tarihsel kaldı. Güncel bağlantı rehberi: `Docs/Onur/InspectResultView_baglanti.md`. Oğuzhan'ın masa/prefab bağlantısı ve Ahmet'in son entegrasyon kabulü henüz tamamlanmadı.

**Ahmet ne istiyor?**

Oyuncu İNCELE modunu açacak, iki ayrı belgeden iki alan seçecek. Core bu ikisini karşılaştıracak. Seçilen alanların arasındaki çizginin ortasında Onur'un hazırladığı küçük, kaşe tarzında bir sonuç mesajı görünecek ve uygun kısa ses çalacak.

Örnek: Formdaki Ada/Parsel ile lab raporundaki Ada/Parsel farklıysa kırmızı “UYUŞMAZLIK SAPTANDI”; aynıysa yeşil “SORUN YOK”. Formdaki beton ile El Kitabı'ndaki demir seçilirse gri-kahve “ALAKASIZ”.

İnceleme ücretsiz ve sınırsız; kendi başına onay, ret, para veya risk değişikliği yapmıyor. Oyuncu son kararı kendisi veriyor. “SORUN YOK”, yalnızca seçilen çiftin kontrol sonucudur; örneğin form ile lab beyanının aynı olması binanın bütünüyle güvenli olduğunu kanıtlamaz.

**Onay ve uygulamanın mevcut durumu**

- `memory.md` bölüm başlığında eski “PLANLAMA AŞAMASI, KOD YAZILMAZ” yazısı duruyor. Hemen altındaki yeni not ve `abe8cd3` commit'i, planın ve Core düzeltmelerinin Ahmet tarafından **onaylandığını** söylüyor. Güncel talimat bu onay ve aşağıdaki uygulama notu.
- Bu checkout'ta `InspectResult`, `InspectVerdict`, `InspectTarget`, `Inspection` ve `DocumentData.StampLabName` uygulamaları henüz yok. Adlar belgede kesinleştirilmiş; kodun gelmesi ayrı bir adım.
- İş listesinde Ahmet'in Core maddesi henüz açık. Önce görsel iskelet hazırlanabilir; gerçek Core tipleri ve bağlantısı Ahmet'in kodu geldikten sonra eklenir. Yeni bir yerel `InspectResult`/enum kopyası oluşturmak ileride çakışma çıkarır.
- Bu turda test çalıştırılmadı; önceki genel analizdeki test sonuçları yeni mekaniğin doğrulandığı anlamına gelmez.

**İş bölümü**

| Kişi | Sorumluluk | Çıktı |
|---|---|---|
| Ahmet | Alan tanımları, karşılaştırma kuralları, erişilebilirlik kontrolü, sahte kaşe/parsel düzeltmeleri, Core testleri | `Inspection.Check(...)` ve `InspectResult` |
| Oğuzhan | İNCELE düğmesi, tıklanabilir alanlar, sarı vurgu, iki seçim, çizgi, iptal, zamanlama ve sıfırlama | Core'u çağıran masa etkileşimi; Onur'un bileşeninin prefab'a bağlanması |
| **Onur** | **Sonuç mesajının görünümü, açılış/kapanışı, konumu ve sesi** | **`InspectResultView.cs` ve `.meta`** |
| Tarık | Öğretici ve README açıklaması | Yeni mekaniği anlatan kısa metin |

Senin yeni dosyan: `Assets/_Project/Scripts/Desk/UI/InspectResultView.cs`, namespace `CivilJam.Desk`.

Normal alanın Flow olsa da bu dosya için Ahmet açıkça Desk'i istiyor: Desk ve Flow birbirine bağımlı değil. Bileşen Desk assembly'sinde derlenecek ve Core sonucunu okuyacak. Bu görev diğer Desk dosyalarını veya prefab'ı düzenleme kapsamı vermiyor; bunları Oğuzhan bağlıyor. `Game.unity` entegrasyonu Ahmet'te.

**Bileşenin kesin arayüzü**

```csharp
public void Show(InspectResult r, Vector2 anchoredPosition);
public void Hide();
```

`Show` verilen sonucu, konumda gösterir; rengi ve sesi `r.Verdict` ile seçer. Başlık ve açıklama Core'un `r.Title` ve `r.Detail` alanlarından gelir. Sayı, kural eşiği, belge eşitliği veya proje güvenliği bu bileşende hesaplanmaz.

| Core sonucu | Başlık | Renk | Ses |
|---|---|---|---|
| `Match` | SORUN YOK | `#3f8a4f` | `sfx_click` |
| `Mismatch` | UYUŞMAZLIK SAPTANDI | `#c2412f` | `sfx_stamp` |
| `Unrelated` | ALAKASIZ | `#7a6650` | `sfx_click` |

Görünüm hafif eğik kaşe; başlık Paytone One, açıklama Fira Sans. İlgili font, ses klibi ve AudioSource referansları Inspector'dan bağlanabilir olmalı. Yeni ses üretimi veya yeni bir ses yönetim sistemi gerekmiyor.

**Uygulama sırası**

| Adım | Yapılacak iş | Bağımlılık | Kabul ölçütü | Tahmin |
|---|---|---|---|---|
| 1 | Oğuzhan'a bağlanabilir küçük görünümün düzenini ve Inspector alanlarını belirle | Mevcut onaylı API | Başlık, Detail, arka plan/çerçeve, konum ve ses alanları net | 10 dk |
| 2 | Üç durum için okunur kaşe görünümü ve ortak hareket stilini hazırla | `DeskMotion` mevcut | Üç renk ve iki font doğru; uzun başlık taşmıyor | 15–20 dk |
| 3 | Ahmet'in Core kodu geldiğinde gerçek `InspectResult` ile `Show/Hide` bağla | Core tiplerinin derlenmesi | Verdict → stil/ses; Title/Detail → yazılar; oyun state'i değişmiyor | 10–15 dk |
| 4 | Yeniden gösterme, gizleme, ses ve etkileşim ayrıntılarını tamamla | Adım 2–3 | Önceki animasyon yeni sonucu kapatmıyor; ses bir kez; masa girdisi korunuyor | 10–15 dk |
| 5 | Oğuzhan'ın bağladığı Sandbox_Desk'te birlikte doğrula | Masa etkileşimi + prefab bağlantısı | Aşağıdaki kabul listesi geçiyor | 15–20 dk |
| 6 | İlgili kod/meta ve AI kaydını temiz değişiklik olarak teslim et | Derleme ve kontroller | Yerel Unity gürültüsü dahil değil; Ahmet'in son Game/build kontrolüne hazır | 5–10 dk |

Hedef: yaklaşık **1 saatlik bileşen geliştirmesi**, ardından entegrasyon/kontrol payı; toplam yaklaşık 65–90 dakika. Core veya Oğuzhan'ın bağlantısını bekleme bu tahmine dahil değil.

**Oğuzhan'la ortak sınır: kim neyi yönetecek?**

Önerilen bağlantı akışı:

1. Oğuzhan iki alanı seçtirir ve `Inspection.Check(ctx, project, a, b, selectedFloorGroup)` çağırır.
2. Çizgiyi kendi çizdirir; orta noktayı mesajın parent RectTransform koordinatına çevirir.
3. `resultView.Show(result, anchoredPosition)` çağırır.
4. Yaklaşık 1,6 saniye sonra veya oyuncunun kapatma tıklamasında çizgi/seçimleri temizler ve `resultView.Hide()` çağırır. İnceleme modu açık kalır.
5. Mod kapanınca, yeni müşteri gelince, karar verilince ve gün bitince `Hide()` ile görünümü de temizler.

**Önemli uygulama tercihi:** mesajın ekranda kalma süresinin tek sahibi masa etkileşim yöneticisi olsun. Bileşen kendi kendine ikinci bir 1,6 saniyelik sayaç başlatmasın; aksi halde mesaj ve çizgi farklı zamanda kaybolabilir. Bu, mevcut API'ye uygun entegrasyon önerisidir; belgede ayrıca belirlenmiş bir yeni kural değildir.

Sağ tık veya belgeler dışındaki boş masaya tıklama seçimi iptal ediyor. **Esc oyun menüsüne ait.** Eski kısa yol haritasındaki “Esc iptal” ifadesi yerine, ayrıntılı ve daha yeni etkileşim notu esas alınmalı.

**Küçük ama önemli uygulama ayrıntıları**

- `anchoredPosition` ekran pikseli değil, mesajın bağlı olduğu RectTransform'un yerel konumudur. Oğuzhan'la aynı parent/anchor/pivot düzeni kullanılmalı; farklı çözünürlükte mesaj çizgiden kopmamalı.
- Sonuç, küçük bir bildirimdir. Tam ekran karartma veya belgeleri kapatan modal panel gerekmiyor. Mesajın grafiklerinin tıklamayı gereksiz yere yakalamadığı kontrol edilmeli; kapatma tıklamasını masa yöneticisi ele almalı.
- `PopupMotion` kendi `CanvasGroup.blocksRaycasts` değerini değiştiriyor. Modal kart desenini aynen kopyalamak yerine bu davranışa dikkat edilmeli. `DeskMotion.Base/Fast/Exit` ve mevcut eğrilerle yalnızca mesajı canlandırmak da onaylı hareket diline uyar; ortak `PopupMotion` dosyasını değiştirmek gerekmez.
- Art arda `Show`, `Show → Hide → Show` ve kapanırken tekrar gösterme güvenli olmalı. Önceki animasyon veya coroutine yeni sonucu sonradan gizlememeli.
- Her `Show` için uygun ses bir kez çalmalı; `Hide` tekrar sonuç sesi üretmemeli. Ses klibi bağlanmamışsa görsel sunum yine çalışmalı.
- Uzun uyuşmazlık açıklaması, Türkçe karakterler ve ekran kenarı konumları kontrol edilmeli. “UYUŞMAZLIK SAPTANDI” en geniş başlık olarak özellikle denenmeli.
- Kat grubu yanlışsa `Unrelated` kararını Core verir. Görünüm bunu kendi başına kırmızıya veya başka bir mesaja çevirmemeli.

**Kabul ve test listesi**

Onur'un bileşeni için:

- [ ] `Match`: yeşil kaşe, Core başlık/açıklaması, tek click sesi.
- [ ] `Mismatch`: kırmızı kaşe, Core başlık/açıklaması, tek stamp sesi.
- [ ] `Unrelated`: gri-kahve kaşe, Core başlık/açıklaması, tek click sesi.
- [ ] Açılış/kapanış mevcut Desk hareket stilinde; görsel hafif eğik.
- [ ] Uzun açıklama ve Türkçe karakterler okunur; konum değişince mesaj eski yerde kalmıyor.
- [ ] `Hide()` sonrasında görünüm ve geçici animasyon kalmıyor; tekrar gösterme çalışıyor.
- [ ] Para, gün, risk, proje kararı ve kayıt değişmiyor.

Oğuzhan'la entegrasyonda:

- [ ] Dürüst parametre + doğru kat grubu → SORUN YOK.
- [ ] Yanlış kat grubu / grup seçilmemesi → ALAKASIZ.
- [ ] Yetersiz parametre + doğru El Kitabı satırı → UYUŞMAZLIK SAPTANDI.
- [ ] Sahte belgede gerçekten farklı olan parsel/tarih/imza veya akredite olmayan lab/kaşe → uyuşmazlık.
- [ ] Ek test satın alınmadan gerçek değer hücresi seçilemiyor; satın alındıktan sonra uygun karşılaştırma çalışıyor.
- [ ] Beton ↔ demir gibi farklı bilgiler → ALAKASIZ.
- [ ] Mesaj çizginin ortasında; süre/tıklama ile mesaj, çizgi ve seçim birlikte temizleniyor.
- [ ] Aynı alanı yeniden seçme, sağ tık, boş masa, mod kapatma ve yeni müşteri temizliği doğru.
- [ ] İnceleme sırasında karar/ek test düğmeleri pasif; El Kitabı sekmeleri ve kat grupları çalışıyor.
- [ ] Esc menüsü açılıyor; alttaki kontrole tıklama sızmıyor; dönüşte arayüz takılmıyor.

Core'un karşılaştırma doğruluğunu test etmek Ahmet'in; bu listenin amacı onun sonucunun doğru sunulduğunu ve masa etkileşimine doğru bağlandığını görmek. Önce Sandbox_Desk, ardından Ahmet'in Game.unity ve Windows kontrolü gelir.

**Teslim edilecekler ve mevcut işlerin ilişkisi**

Yeni mekanik için senin teslimin: `InspectResultView.cs` + Unity'nin ürettiği `.meta`, Inspector bağlantı alanlarının kısa açıklaması, üç duruma ait doğrulama ve AI kullanım kaydı. Oğuzhan bu bileşeni kendi prefab/builder düzenine ekler. Doğrulama tamamlanınca ortak listedeki Onur maddesi kapatılır; tamamlanmamış iş şimdiden işaretlenmez.

Ödeme önizlemesi, kayıt hatası, müzikler ve son/gazete okunabilirliği mevcut görevlerin olarak devam ediyor. Bu yeni mekanik onları otomatik olarak tamamlamıyor veya iptal etmiyor. Yeni görev dar tutulursa Oğuzhan'ın entegrasyonunu beklerken bu Flow işlerine dönülebilir.

**İlk somut adım:** sonuç kaşesinin Inspector alanlarını ve yerel koordinat düzenini belirle; üç görsel durumu hazırla. Core tipleri bu checkout'ta henüz olmadığı için gerçek `Show(InspectResult, ...)` bağlantısı, Ahmet'in Core kodu geldikten sonra yapılmalı.
