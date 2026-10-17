**InspectResultView — Oğuzhan için bağlantı notu**

17 Ekim 2026 · Core API: `78a847c`. Bileşen: `Assets/_Project/Scripts/Desk/UI/InspectResultView.cs`, namespace `CivilJam.Desk`.

Üç sonuç görünümü ve animasyon/ses sunumu hazır. Bileşen kendisine verilen `InspectResult.Title/Detail` metnini gösterir; kuralları veya oyun durumunu değiştirmez. [Unity önizlemesi](InspectResultView_onizleme.png).

**Bağlantı**

1. Masa Canvas'ında, belge içeriğinin üzerinde çizilen tam ekran bir UI katmanının altına boş bir `RectTransform` nesnesi ekle; `InspectResultView` bileşenini koy. Parent, kartın kendisi kadar küçük değil, sonuçların konumlanacağı masa/Canvas alanı olmalı.
2. Bileşen kağıt/çerçeve/yazı nesnelerini ilk kullanımda kendi oluşturur. Ayrıca tam ekran Image veya perde ekleme. Tüm grafikler `raycastTarget = false`, CanvasGroup da etkileşimsizdir; kapatma tıklamasını inceleme yöneticisi ele alır.
3. Inspector alanlarını aşağıdaki gibi bağla. Inspector'dan bileşen eklenince `Reset` mevcut proje asset'lerini otomatik doldurur; builder veya çalışma anında `AddComponent` kullanılıyorsa bu alanları builder'da açıkça bağla.

| Serialize edilen alan | Referans |
|---|---|
| `titleFont` | `Assets/_Project/Fonts/PaytoneOne-Regular SDF.asset` |
| `detailFont` | `Assets/_Project/Fonts/FiraSans-Regular SDF.asset` |
| `click` | `Assets/_Project/Audio/sfx_click.ogg` |
| `stamp` | `Assets/_Project/Audio/sfx_stamp.ogg` |
| `audioSource` | İsteğe bağlı 2D AudioSource; atanmazsa ilk ses gerektiğinde kendi kaynağını kurar |
| `volume` | Varsayılan 0,65; Inspector'dan ayarlanabilir |

Builder'da diğer Desk bileşenleriyle aynı `SerializedObject`/`Wire` yaklaşımı kullanılabilir. Kullanılmayan ses alanı boşsa görünüm çalışmaya devam eder.

`a48d1c5` masa iskeleti için not: eski `Result` Image + `Text` geçici kartını bununla değiştir. `InspectController` içindeki geçici `resultText` yazımı ve `resultRoot.localScale` animasyonu kaldırılmalı; görünüm kendi animasyonunu yönetiyor. Çizginin çizilmesi ve bekleme/iptal akışı controller'da kalır. `DrawLine` içindeki doğrudan sonuç konumu yazımı da bileşenin kenar sıkıştırmasını ezmemeli; karşılaştırma anında doğru midpoint'i `Show`'a ver.

**Çağrı**

```csharp
InspectResult result = Inspection.Check(ctx, project, first, second, selectedFloorGroup);
resultView.Show(result, midpointAnchored);

// Çizgi/seçim temizlenirken, mod kapanırken veya yeni müşteride:
resultView.Hide();
```

`midpointAnchored`, mesajın parent RectTransform'unun **merkezine göre UI koordinatı** olmalı. Bileşen kendi anchor/pivot'unu 0,5/0,5 yapar. Screen pikselini doğrudan verme. Örneğin çizginin orta noktası screen koordinatındaysa:

```csharp
RectTransform parent = (RectTransform)resultView.transform.parent;
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    parent, midpointScreen, uiCamera, out Vector2 parentLocal);
Vector2 midpointAnchored = parentLocal - parent.rect.center;
// ScreenSpaceOverlay Canvas için uiCamera = null.
resultView.Show(result, midpointAnchored);
```

Ekran kenarında kaşenin kesilmemesi için bileşen konumu parent sınırlarına sıkıştırır. Normal konumda çizgi orta noktasını kullanır. Parent boyutunun güncel olması için runtime katmanı kurduktan sonra Canvas yerleşimi tamamlanmış olmalı.

**Davranış ve sorumluluk**

- `Match`: yeşil + click; `Mismatch`: kırmızı + stamp; `Unrelated`: gri-kahve + click. Başlık ve açıklama Core'dan aynen gelir.
- Gösterme `DeskMotion.Base`, gizleme `DeskMotion.Exit(DeskMotion.Base)` süresini ve mevcut eğrileri kullanır. Hareket ölçekli zamanla ilerler; Esc zamanı durdurunca yeni kaşe animasyonu da durur.
- **Otomatik kapanma sayacı yok.** Yaklaşık 1,6 saniye beklemeyi, tıklama/sağ tık iptalini, çizgiyi ve seçimi inceleme yöneticisi birlikte yönetmeli; temizlerken `Hide()` çağırmalı.
- `Hide()` kısa çıkış animasyonu yapar; bu sırada da girdi yakalamaz. Parent kapandığında anında görünmez hale gelir. Tekrar `Show` eski animasyonu iptal eder; önceki kapanış yeni sonucu gizlemez.
- `Show(null, ...)` görünümü gizler. Bileşen yeni müşteri veya karar olaylarını kendisi dinlemez; bunları çağıran masa yöneticisi temizler.
- Inspector bileşen menüsünde **Önizleme / SORUN YOK, UYUŞMAZLIK SAPTANDI, ALAKASIZ** seçenekleri var. Edit modunda görseli açar, ses çalmaz.

**Doğrulama**

Gerçek Core tipleriyle ayrı geçici proje kopyasında dört PlayMode testi geçti:

1. Üç sonucun Core metinlerini koruması, başlık/açıklama taşmaması ve masa raycast'lerini yakalamaması.
2. `Show → Hide → Show`, eski kapanışın yeni sonucu gizlememesi ve bileşen devre dışı bırakılıp açılınca yeniden çalışması.
3. Animasyon sırasında zaman durunca konum/ölçek/saydamlığın durması, devamda tamamlanması.
4. Uzun Türkçe açıklama, ekran kenarı konumu ve null sonuçla gizleme.

Üç durum Unity render'ıyla görsel olarak kontrol edildi. Ses kliplerinin referansları ve seçim kodu hazır; final ses seviyesi gerçek masa oynanışında dinlenerek ayarlanmalı. Tam İNCELE etkileşimi, çizgi, prefab ve Game/Windows kabulü bu bileşen testlerinin kapsamı değildir; Oğuzhan/Ahmet entegrasyonunda tamamlanır.

Aynı kopyada mevcut **39/39 EditMode testi** de geçti (Ahmet'in yeni Inspection testleri dahil). Asıl bileşenin dosyası, test ve render yapılan dosyayla bayt düzeyinde karşılaştırıldı.

Geçici test/render kanıtları: `/private/tmp/imza-inspect-view-c7v_uuvy/` (`view-tests.xml`, `view-tests.log`, `render.log`). Testler asıl oyuncu kaydından ayrı ürün adıyla çalıştırıldı.
