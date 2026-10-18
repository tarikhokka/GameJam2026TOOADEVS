# Belge turu C1 — kaşe çekmecesi

18 Ekim 2026 · Onur (C) · API: `memory.md` §7 "Belge turu".

`Assets/_Project/Scripts/Desk/UI/StampTrayView.cs`, `CivilJam.Desk` içinde, kendini kuran bir görünüm bileşenidir. Tarık'ın T1 `4a88ff2` görsellerini kullanır. Görseller boşken de çizilir. Core'a karar göndermez; `Stamped` olayını masaya bildirir.

## Oğuzhan — B2 bağlantısı

Prefab'ı sadece kendi builder'ınla kur. Onur C1'de `DeskScreen`, builder, prefab veya sahne değiştirmedi.

1. Desk Canvas'ı altında bir `RectTransform` oluştur; önerilen boyut **640 × 260 tuval pikseli**, ölçek 1. Konumu El Kitabı'nın sağına göre builder belirler. Bileşen kökün anchor/pivot/konum/boyutunu değiştirmez. Düğme kökün sağ kenarında, çekmece onun solunda kalır.
2. `StampTrayView` ekle. Editor `Reset()` şu alanları doldurur; builder bunları da açıkça serialize edebilir:
   - `buttonSprite` → `Art/btn_stamps.png`
   - `traySprite` → `Art/stamp_tray.png`
   - `approveSprite` / `rejectSprite` → `Art/stamp_approve.png` / `stamp_reject.png`
   - `titleFont` → `Fonts/PaytoneOne-Regular SDF.asset`
3. Bir kez bağla:

```csharp
tray.Bind(() => desk.StampArea, () => desk.CanStamp);
tray.Stamped += desk.StampDecision;
tray.OpenChanged += OnStampTrayOpenChanged;
```

`OpenChanged(true)` çekmece açılmaya başlarken, `false` kapanmaya başlarken gelir. B2 burada `InspectDim` / `InspectLift` ile **kaşe alanını ve bileşenin bütün kökünü** aydınlık bırakır. Sadece çekmece görselini yükseltme: sürüklenen kaşe kökteki ayrı `DragLayer`'a taşınır. Bileşenin kendi kalıcı Canvas'ı veya karartma görseli yoktur. Açılışta kendi kökünü aynı ebeveyn içinde son kardeş yapar.

4. Yeni müşteri, gizlenen masa ve otomatik zarf kaşesi gibi dış değişimlerde gerektiğinde `Close(true)` kullan. `SetVisible(false)` sürükleme ve bekleyen basmayı da iptal eder; `SetVisible(true)` yalnızca düğmeyi gösterir. Event aboneliklerini ekran yok edilirken kaldır.
5. Kaşe izi ve sesi `DeskScreen.StampDecision` / `DecisionStampView` / `DeskAudio` üzerinden bir kez çalışır. `HandBack()` **bu bileşenden çağrılmaz**; B1 iki belge de teslim edilince çağırır. A1'de dönüş tipi `bool` oldu; C1 bu dönüşe bağlı değildir.

## Davranış ve sınırlar

- Çekmece sağdaki düğmeden sola doğru, maskelenerek açılır; tekrar düğme veya çekmece dışındaki boş alan kapatır.
- İçerideki ahşap aralıklarına tıklamak dışarı tıklama sayılmaz.
- ONAY ve RED kaşeleri sol fare sürüklemesiyle alınır; tutulunca hafif büyür, gölgesi uzar. Hedef testi bırakılan fare konumuyla yapılır.
- Geçerli bırakma: form üzerinde kısa basma hareketi → **temas anında tek `Stamped(bool)`** → yerine dönüş → çekmece kapanışı. Kaşe izi ve ses masa tarafındadır.
- Geçersiz bırakma: kaşe yerine döner, çekmece açık kalır.
- Hedef etkin değilse, form kaşeli/diyalogda/incelemedeyse (`canOpen == false`) düğme pasiftir. İzin sürükleme veya temas öncesinde kalkarsa karar olayı çıkmaz. Temas olayı izin durumunu değiştirirse geri dönüş hareketi tamamlanır.
- Ana görünümün `GraphicRaycaster`'ı kilitlenirse yeni giriş ve bekleyen temas iptal edilir. `PauseMenu` referansı veya Desk → Flow bağımlılığı yoktur.
- Kapatma, gizleme, yeniden bağlama ve `Stamped` dinleyicisinden aynı anda gizleme güvenlidir; ertelenmiş eski karar olayı kalmaz.
- Hareket süreleri Inspector'da `slideDuration` 0,25, `returnDuration` 0,18, `pressDuration` 0,12 sn; `DeskMotion` ve unscaled zaman kullanılır.
- Oyun kuralı, rüşvet miktarı, para ve proje sonucu bu sınıfta tutulmaz. Kaşeden sonra yeniden açılmayı masanın `CanStamp` izni engeller.

## Doğrulama

Testler ve ekran görüntüleri ana projeyi/savelerini değiştirmeyen Unity **6000.6.4f1** kopyasında üretildi; ürün adı `ImzaStampTrayAudit`. Kanıt dizini: `/private/tmp/imza-stamp-tray-a18rk7hz`.

Taban `2e8643d` (A1 `a9e80a7` dahil): **58/58 EditMode + 13/13 PlayMode** (4 mevcut Flow + 9 geçici C1 kabulü) geçti. Son raporlar `final-editmode.xml` ve `verified-playmode.xml`; logları aynı adlarla `.log`. Son testteki script ana projedekiyle birebir aynı. Açık çekmece ve görselsiz durum gerçek URP render'ında gözle kontrol edildi; ONAY/RED yazıları kaşelerin boş etiketinde, KAŞE yazısı açık renkte okunuyor.

![Bağımsız Unity testinden kaşe çekmecesi](StampTrayView_onizleme.png)

Önizleme, `stamp-tray-open.png` içindeki bileşen bölgesinin kırpımıdır; oyun sahnesi entegrasyonu değildir.

Geçici kabul testleri aynı dizindeki `Assets/StampTrayAudit/` içindedir; ürüne test assembly'si veya test sahnesi eklenmedi. C1 kabulü şunları kapsar: gerçek sprite/font ve raycast, iki kararın temas anında tek olayı, yanlış bırakma, izin/hedef kaybı, kapat/gizle/yeniden bağla, olay içinde eşzamanlı gizleme, ölçekli kamera Canvas'ında koordinatlar, unscaled hareket, ana girdi kilidi ve görselsiz çalışma.

**Bu önizleme bağımsız C1 bileşenidir.** Gerçek Sandbox_Desk yerleşimi, B2 karartma/inceleme/zarf bağlantıları ve belge iadesi C2/A2'de doğrulanacaktır; Faz 2'yi Ahmet açar.
