# DeskHeaderView — B2 bağlantısı (Onur / C1)

`Assets/_Project/Scripts/Desk/UI/DeskHeaderView.cs`, `CivilJam.Desk` assembly'si. Sadece sunum; Core durumunu değiştirmez, Flow'a bağımlılık eklemez. Prefab/builder'ı **Oğuzhan**, Faz 2 açıldıktan sonra bağlar.

## Yerleşim ve görünüm

- Yeni boş bir RectTransform'a eklenir; Awake'te dört kağıt etiketten oluşan üst barını kurar.
- Kök üst kenara yatay stretch, pivot `(0.5, 1)`, konum sıfır, **yükseklik 80 px**. Mevcut ofis/belgeler ve bar altındaki NASIL OYNANIR yerinden oynamaz.
- Solda Gün, esnek ortada müşteri/proje, sağda Kasa + varsa kırmızı Borç ve Onaylanan/Bekleyen.
- Borç sıfırken ikinci satır gizlenir, kasa ortalanır. Normal metinler sığacak kadar küçülür; çok uzun müşteri/proje adı iki satıra yayılabilir, sınırı aşarsa üç noktayla kısalır.
- Bütün Image/TMP öğeleri `raycastTarget = false`; Canvas/GraphicRaycaster veya `overrideSorting` eklemez. B1 karartmasında üst barın kararması beklenir.
- Sprite yokken kahve şerit, krem kart ve kahve kontur kullanılır. Eksik ikon saklanır ve yazıya ek alan açılır. Yeniden Refresh arayüzü çoğaltmaz.

## Builder'ın bağlayacağı alanlar

`Reset()` Editor'de mevcut dosyalardan boş alanları doldurur. Build/runtime için bağlantıları builder'da da serialize et; runtime AssetDatabase kullanmaz. Alanlar Awake'ten sonra bağlanırsa sonraki API çağrısı görünümü günceller.

| Serialize alanı | Asset |
|---|---|
| `titleFont` | `Fonts/PaytoneOne-Regular SDF.asset` |
| `bodyFont` | `Fonts/FiraSans-Regular SDF.asset` |
| `barSprite` | `Art/hud_bar.png` |
| `tagSprite` | `Art/hud_tag.png` (T1 border: 24/24/24/24; Image Sliced) |
| `dayIcon` | `Art/hud_icon_day.png` |
| `moneyIcon` | `Art/hud_icon_money.png` |
| `countIcon` | `Art/hud_icon_count.png` |

Asset yollarının başı `Assets/_Project/`. T1 kaynakları Tarık'ın `f8842b5` commitinde. Sprite'ların GUID'lerini değiştirmeye gerek yok.

## Üç API çağrısı

```csharp
// Eski DeskHud.Refresh'in state yenileme noktası:
header.Refresh(context);

// DeskPropsView.Show'daki müşteri/proje etiketi yerine:
header.SetClient(client != null ? client.DisplayName : "Başvuru sahibi", project.Name);

// DeskPropsView.ShowEmpty'deki etiket yerine:
header.SetDayDone(allDecided);
```

- `Refresh(GameContext ctx)`: Gün, `Money`, `Debt`, `ApprovedToday` ve `TodayProjects` içindeki `Pending` dosya sayısını okur. Merkez etiketini değiştirmez; sıra bağımlılığı yaratmaz. StateChanged aboneliğini bileşen kurmaz, bugünkü masa yenileme noktaları kullanılmalı.
- `SetClient(string clientName, string projectName)`: `müşteri · proje`; boş olan parça atlanır, ikisi boşsa etiket temizlenir. Önceki gün sonu mesajını değiştirir. İsim/proje rich text olarak yorumlanmaz.
- `SetDayDone(bool allDecided)`: true → `Bugünkü başvurular bitti.`; false → boş. Para/gün/sayı alanlarını değiştirmez.
- Null context/state için sayı alanları çizgi gösterir, istisna atmaz. Kök aktif/pasif yönetimi masadadır; API'ler host'u kendiliğinden açmaz/kapatmaz.

Eski koyu bandı yeni barla **değiştir**; ikisi üst üste bırakılmamalı. DeskHud/DeskPropsView içindeki eski TMP referansları kullanımdan çıkarılıyorsa onların yazma noktaları da bu üç çağrıya çevrilmeli; yeni bileşenin referansını yalnız eklemek yeterli olmaz. C1 bu kilitli dosyalara veya DeskScreen.prefab'a dokunmaz.

## Doğrulama

İzole Unity kopyasında API/lifecycle, borç aç/kapat, uzun isimler, 80 px yerleşim ve sprite'sız görünüm kontrol edilir. B2 sonrasında gerçek Sandbox_Desk/Game üzerinde müşteri değişimi, ek test/para değişimi ve mesai sonu ayrıca doğrulanmalı. C1-K karartma kontrolü ayrı Faz 2 işidir; C1'in tek başına hazır olması bu kontrolün yapıldığı anlamına gelmez.

### C1 teslim sonucu

- `8e8f828` tabanında izole Unity 6000.6.4f1: **55/55 EditMode**, **10/10 PlayMode** (4 mevcut Flow + 6 geçici C1 testi).
- Altı kontrol: gerçek state/sayı/para okuma ve state'e yazmama; borç aç/kapat; müşteri/gün sonu/boş etiket geçişleri ve literal metin; görselsiz/null context; uzun isim/80 px sınırı ve 1280/1920/2560 genişlik; inactive host'ta Awake öncesi API ve tekrar Refresh'te UI çoğalmaması.
- Üç görünüm Unity'nin URP render'ıyla görüntülendi: borçsuz, uzun isim + borç, görselsiz + gün sonu. `DeskHeaderView_onizleme.png`.
- Kanıt dosyaları `/private/tmp/imza-desk-header-ob92k7ac/`: `final-editmode.xml`, `final-playmode.xml`, `render-final.log`. Denetim kodu yalnız geçici kopyadadır; kaynak bileşenle byte eşitliği kontrol edildi.
- Gerçek masa bağlantısı ve C1-K henüz bu teslimin parçası olarak oynanmadı; Faz 2 işi.
