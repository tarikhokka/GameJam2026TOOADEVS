# Belge turu — C2 masa kabulü

18 Ekim 2026 · Onur (C) · ana kabul tabanı `0b0a0b4`, Oğuzhan B2 `76db5f3` dahil. Son A2/onay/metin güncellemesi `759a870` ayrıca alındı.

## Kapsam ve sonuç

Gerçek `Assets/_Project/Scenes/Sandbox_Desk.unity` PlayMode'da yüklendi. Builder yeniden çalıştırılmadı; repodaki sahne ve prefab kullanıldı. C1 çekmecesi, formdaki hedef, karartma, iki belgeyi ayrı ayrı teslim etme, rüşvet diyaloğu ve Esc birlikte kontrol edildi.

1920×1080 hedefinde C1 bileşeninde veya B2 yerleşiminde düzeltme gerektiren hata bulunmadı. Runtime script, prefab, builder ve sahne değiştirilmedi. Bu teslim kabul notları, gerçek Unity görüntüleri ve ekip durum kaydıdır.

**Sonuç: 58/58 EditMode + 22/22 PlayMode geçti** (4 mevcut Flow + 9 C1 regresyonu + 9 C2 masa kabulü), taban `0b0a0b4`. Raporlar `editmode.xml` ve `final-playmode.xml`; logları aynı dizinde aynı adla `.log`. Son birleşik PlayMode sonucu 22 Passed, 0 Failed, 0 Skipped; Unity çıkış kodu 0. Sonraki `759a870` güncellemesinin tek runtime farkı zarf ipucu metni: güncel dosyayla **1/1 ek rüşvet kabulü** geçti (`latest-bribe-playmode.xml`), yeni metin ve taşmaması da doğrulandı. Diğer test edilen script'ler ana projeyle birebir aynı.

Çekmece sağda, düğme El Kitabı sekmelerinin altında; açılınca sola doğru kitabın üstüne gelir. Bu durum kaşe seçimi boyunca sürer. A2'nin kitabın üst satırlarının örtülmesi gözlemi değerlendirildi: B2 builder yorumunda da çekmecenin El Kitabı üstüne açılması açıkça tarifli. Kitap kapalı çekmecede okunuyor, kaşe seçerken kitapla etkileşim zaten kilitli ve kitabın başlığı açıkta. Bu nedenle başka satırları örtecek bir aşağı kaydırma yapılmadı; konum B'nin builder'ında korundu. Hedef alanı ve çekmece karartmanın üstünde, İNCELE düğmesi erişilebilir. ONAY/RED yazıları okunuyor; kaşe izi formdaki çerçevenin içinde kalıyor. Açılış 0,25 sn, geri dönüş 0,18 sn ve basma 0,12 sn mevcut değerleriyle kabul edildi.

## Masa kabul senaryoları

| Kontrol | Doğrulanan davranış |
|---|---|
| Yerleşim, gerçek raycast ve katmanlar | İlk gelişte belgeler kapalı, form açılana kadar kaşe pasif. Düğme, çekmece ve yazılar hedef ekranda. Gerçek raycast kaşe ve düğmelere ulaşıyor. Karartma alfa 0,2, tıklama almıyor; alan/çekmece/İNCELE üstünde. Kapanınca geçici Canvas'lar temizleniyor. |
| ONAY → form, sonra lab iadesi | Kaşe ve ilk belge tesliminde proje Pending; kasa ve onay sayısı aynı. İkinci belgeyle tek onay, ücret ve sayaç artışı. |
| RED → lab, sonra form iadesi | Belgeler ters sırayla verilebiliyor. İkinci teslimde ret, ücret yok; tepki tamamlanınca sıradaki müşteride eski kaşe veya odak kalmıyor. |
| Kaşeden önce iade / hedef dışı kaşe | Erken verilen belge geri dönüyor. Yanlış kaşe bırakışı karar üretmiyor; kaşe yerine dönüyor, çekmece açık kalıyor. |
| İnceleme ↔ çekmece | Gerçek düğme tıklamalarıyla mevcut B2 geçişi denendi. İki mod aynı anda açık kalmıyor; kapanışta karartma temizleniyor. Ahmet `759a870` ile geçişi onayladı. |
| Rüşvet | Form/El Kitabı beton alanları karşılaştırılarak gerçek `InspectionDialogue` teklifi üretildi; konuşurken kaşe pasif. Zarf tıklanıp kabul edilince otomatik ONAY izi var, proje hâlâ Pending ve kasa aynı. İki belge teslimiyle ücret + zarf bir kez ekleniyor. |
| Gerçek Esc menüsü | Kaşe eldeyken Esc menüsü açılınca sürükleme iptal; karar çıkmıyor, odak temizleniyor. Devam sonrası çekmece yeniden kullanılabiliyor. Desk → Flow bağımlılığı eklenmedi. |
| Ekranı gizle / yeniden göster | Elde kalan kaşe ve karartma temizleniyor; tekrar açılışta bağlantılar çalışıyor, eski karar gelmiyor. |
| Lab raporunu kaşeden sonra aç | Sadece form açılıp kaşe basıldıktan sonra lab ikonu çalışıyor. Form verilince ikonu geri gelmiyor; lab da verilince karar kesinleşiyor. |

Belgelerin verilmesi doğrudan `HandBack()` çağrısıyla atlanmadı: `DraggableSheet`'in begin/drag/end olayları gerçek müşteri `DropArea` hedefine gönderildi. Kaşe bırakışı da gerçek `EventTrigger` ve hedef alanıyla yürütüldü.

## Kanıt ve doğrulamanın sınırı

Test ortamı Unity **6000.6.4f1**, ayrı proje `/private/tmp/imza-document-c2-xdftcfng`, ürün adı `ImzaDocumentC2Audit`. Ana proje dosyaları ve oyuncunun Imza kaydı korunarak çalışıldı. Kabul testleri yalnız geçici kopyanın `Assets/StampTrayAudit/` klasöründe.

Hedef ekran **1920×1080**, Game View sabit çözünürlük olarak ayarlandı. Batch'in ilk 640×480 / 4:3 görünümünde sağdaki sabit konum ekran dışına düştüğü için o tanı koşusu hedef çözünürlük kabulü sayılmadı. Ret sonrası geçiş testi keyfi iki saniye yerine gerçek müşteri değişimini bekler; ürünün tepki süreleri korunur.

Bu sahne `SandboxDeskContext` / `FakeGameActions` kullanır; uyuşmazlık ve rüşvet diyaloğu gerçek `InspectionDialogue` üzerinden geçer. Üretim `GameSession` ile yedi günlük akış, jüri menüsü, Game sahnesi ve teslim build'i **A2/A3 kapsamındadır**. Bu C2 notu onların tamamlandığı anlamına gelmez.

## A2 güncellemesi ve Ahmet'e teslim

C2 kabulü sırasında `759a870` geldi ve pullandı. Önceki iki not bu commit'te çözüldü:

1. **İnceleme açıkken kaşe düğmesi:** Ahmet'in onayı ortak hafızaya A tarafından yazıldı; mevcut geçiş kodda kalıyor. C2'de doğrulanan davranışla aynı: mod kapanıyor, çekmece açılıyor; ikisi aynı anda açık kalmıyor.
2. **Zarf ipucu:** A metni “kararı yine kaşe çekmecesinden verirsin” olarak düzeltti. C2 ürün koduna dokunmadı.

A'nın kendi A2 kaydına göre B2 dahil yedi günlük oynanış, 27 kaşeyi çekmeceden sürükleme, 27 kararı iki belgeyle iade, Devam Et, kabadayı, hediye, jüri menüsü ve dört son kontrolü geçti. Bunlar A'nın sonuçlarıdır; C2'nin Sandbox sonuçlarıyla karıştırılmamalı. C2 tesliminden sonra Faz 3 kapısını ve A3 build/ZIP işini A yönetir.

## Görüntüler

Görüntüler gerçek sahnenin 1920×1080 URP render'ıdır; çizim/mockup değildir.

- `Belge_turu_C2_cekmece.png`: açık çekmece, odak alanı ve hafif karartma.
- `Belge_turu_C2_surukleme.png`: kaşe elde, form hedefinin üzerinde.
- `Belge_turu_C2_onay.png`: ONAY izi; üst barda kasa ve sayaç hâlâ aynı, belgeler teslim bekliyor.
