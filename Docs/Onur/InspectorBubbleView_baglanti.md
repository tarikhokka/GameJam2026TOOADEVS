**InspectorBubbleView — Oğuzhan için bağlantı notu**

17 Ekim 2026 · Yeni diyalog Core'u: `8772f57`. Bileşen: `Assets/_Project/Scripts/Desk/UI/InspectorBubbleView.cs`, namespace `CivilJam.Desk`.

Denetçinin krem kağıt, koyu mürekkep, Fira Sans ve DENETÇİ etiketli konuşma balonu hazır. Metni harf harf yazar; yazı sesi mevcut `sfx_typing` klibidir. [Unity önizlemesi](InspectorBubbleView_onizleme.png), iki farklı uzunluktaki metni gösterir; oyunda aynı anda bir denetçi balonu kullanılır.

**Kurulum**

1. Masa Canvas'ında belgelerin üzerinde çizilen UI katmanının altına boş bir RectTransform nesnesi ekle; `InspectorBubbleView` koy. Bileşen kendi `Bubble`, kağıt, kuyruk, etiket ve metin nesnelerini kurar; ayrıca tam ekran perde ekleme.
2. Konumu root RectTransform'dan ayarla. Inspector'da eklenince Reset merkez anchor/pivot ve `(-360, 130)` başlangıç konumunu kurar; bu başlangıç noktası masa/belgelerin üst kenarı içindir, son konumu gerçek masada kontrol et. Runtime builder kullanıyorsan anchor/pivot/konumu açıkça ver. Bileşen Show sırasında bu konumu değiştirmez.
3. Font/ses alanlarını bağla. Inspector'dan eklendiğinde Reset mevcut asset'leri bulur; builder'da `Wire` ile açıkça bağla.

| Serialize edilen alan | Referans / varsayılan |
|---|---|
| `bodyFont` | `Assets/_Project/Fonts/FiraSans-Regular SDF.asset` |
| `labelFont` | `Assets/_Project/Fonts/FiraSans-Bold SDF.asset` |
| `typingClip` | `Assets/_Project/Audio/sfx_typing.ogg` |
| `typingSource` | İsteğe bağlı, **yalnızca bu balon için ayrılmış** 2D AudioSource; boşsa ilk yazı sesinde kendi kaynağını oluşturur |
| `typingVolume` | 0,45 |
| `charsPerSecond` | 40 |
| `soundEvery` | Her iki harf/rakamda bir ses akışını sürdürür; klibi her harfte başa sarmaz |
| `sentencePause` / `commaPause` | 0,18 / 0,08 sn; müteahhit balonuyla aynı süreler |
| `preferredWidth` | 720 UI birimi; parent genişliğine sığacak şekilde küçültülür |

Müteahhit balonunun veya efekt/müzik kaynağının AudioSource'unu burada paylaşma. Complete/Hide kendi yazı kaynağını durdurur. Klip atanmadığında görsel ve yazı çalışır; yeni bir ses dosyası gerekmiyor.

**Zorunlu API**

```csharp
inspectorBubble.Show(line.Text);
bool writing = inspectorBubble.IsTyping;
inspectorBubble.CompleteTyping();
inspectorBubble.Hide();
```

- `Show`: yalnızca verilen metni gösterir; önceki yazı/ses/animasyonu iptal eder. Balon tam metnin boyutuna baştan ölçülür; harfler yazılırken büyümez. Metin rich text olarak yorumlanmaz; `<` gibi karşılaştırma karakterleri görünür.
- `IsTyping`: harfler yazılırken true; doğal bitiş, Complete, Hide veya bileşen devre dışı kalınca false. Esc sırasında yazı beklerken true kalır.
- `CompleteTyping`: mevcut satırın tamamını hemen görünür yapar ve yazı sesini durdurur. Sonraki satıra geçmez, devam olayı üretmez.
- `Hide`: yazı ve sesi hemen iptal eder, tıklamayı hemen bırakır; kısa çıkış animasyonu yapar. Yeni müşteri, masa kapanması, karar veya konuşmacı değişiminde çağıran kod bunu temizlemeli. Parent devre dışı kalınca görünüm anında temizlenir.
- Null/boş/yalnızca boşluk içeren Show, Hide gibi davranır.

**Tıklama ve satır ilerletme**

Balonun kendi kağıt alanı sol tıklamayı alır; dışarıdaki masayı kaplayan bir raycast alanı yoktur. Sağ tık burada işlem yapmaz.

- Yazı sürerken ilk sol tıklama yalnızca metni tamamlar.
- Yazı bitmişken sonraki sol tıklama `ContinueRequested` olayını tetikler. Kontrolör buradan sıradaki satıra geçebilir.

```csharp
// Kontrolör kurulurken bir kez bağla; kontrolör kaldırılırken abonelikten çık.
inspectorBubble.ContinueRequested += AdvanceDialogue;

// Denetçi satırı başladığında:
inspectorBubble.Show(line.Text);

// Müteahhit satırına geçerken / diyalog iptalinde:
inspectorBubble.Hide();
```

Aynı tıklamayı hem bu olayla hem kontrolörün ayrı global fare kontrolüyle ilerletme: ilk tıklama yazıyı tamamlarken sonraki satırı da atlamamalı. Kontrolör tıklamayı tamamen kendisi yönetecekse dört zorunlu API'yi kullanabilir; ContinueRequested aboneliğini ayrıca kurması gerekmez.

**İş bölümü**

`ctx.Actions.Inspect` sonucundaki `Lines` sırası, `DialogueSpeaker` seçimi, karşı konuşmacının balonu, karar/inceleme kilitleri, zarf animasyonu ve `BribeRevealed` görünürlüğü masa kontrolöründe kalır. Bu bileşen Core'u çağırmaz, replik seçmez, para/risk/karar değiştirmez ve kendiliğinden sonraki konuşmaya geçmez.

Yazı, noktalama beklemesi ve balon animasyonu ölçekli zaman kullanır. Esc açıldığında harfler/animasyon durur; ses PauseMenu'nün AudioListener.pause davranışına uyar. Menü açıkken balona tıklama etkisizdir. Hareket süre/eğrileri `DeskMotion.Fast`, `DeskMotion.Exit` ve mevcut eğrilerden gelir.

**Doğrulama ve kalan entegrasyon**

Gerçek diyalog Core'u ile ayrı Unity 6000.6.4f1 kopyasında altı balon testi geçti: kısmi yazı ve tamamlama, doğal bitiş/ses durması, Hide ve yeni metnin eski coroutine'i iptal etmesi, ilk/ikinci tıklamanın ayrılması, Esc sırasında yazı ve tıklama durması, boş/null/Türkçe ve `<` içeren metin/yerleşim. Mevcut üç Esc testiyle toplam **9/9 PlayMode** geçti. İki örnek Unity render'ıyla görsel olarak incelendi.

Bu doğrulama balon bileşenine aittir. Masa prefab'ına bağlantı, iki konuşmacının sırayla oynatılması, diyalog sırasında düğme kilitleri, zarfın doğru anda çıkması ve ses seviyesinin masa içinde dinlenmesi Oğuzhan/Ahmet entegrasyonunda kontrol edilmeli.

Aynı kopyada mevcut **46/46 EditMode testi** de geçti. Bu sayı yeni diyalog Core'u ve rüşvet kurallarının mevcut testlerini içerir; masa diyaloğunun tamamının oynanış kabulü anlamına gelmez.

Geçici kanıtlar: `/private/tmp/imza-inspector-bubble-huomfwoj/` (`playmode.xml`, `render.log`, `Assets/InspectorBubbleAudit/`). Testler ana oyuncu kaydından ayrı ürün adıyla çalıştırıldı.
