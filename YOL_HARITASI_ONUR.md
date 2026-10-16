# İmza — Onur ve ekip için güncel yol haritası

Tarih: 17 Ekim 2026. İncelenen kod: `5f4a49a`. Uzak dal kontrol edildi; inceleme sırasında yeni commit yoktu.

Bu belge mevcut durum analizi ve önerilen iş sırasıdır. Yeni oyun kuralı veya ekip adına verilmiş karar değildir. Tahmini süreler geliştirme ve ilk kontrol içindir; entegrasyon bekleme süresi hariçtir.

## 1. Okunan belgeler ve nasıl değerlendirildikleri

Repo içindeki 10 Markdown dosyasının tamamı okundu. Unity önbelleği ve üçüncü taraf paket belgeleri bu kapsama dahil değildir.

| Belge | Yol haritasındaki kullanımı |
|---|---|
| `CLAUDE.md` | Ortak kararların okunması ve çalışma notu kuralı |
| `memory.md` | Kararlar, sahiplik, Ç1–Ç5 işleri, teslim hedefleri |
| `AHMET.md` | Core sözleşmesi, uygulanan değişiklikler, bekleyen entegrasyonlar |
| `OGUZHAN.md` | Desk durumu, öğretici, olay kartı, görsel ve intro devri |
| `ONUR.md` | Flow, intro ve test durumu; kayıt ekranı bağımlılığı |
| `ICERIK_TASLAK.md` | Metin taslakları; asıl oyun içeriğinin asset'lerde olması |
| `CREDITS_AND_AI.md` | Oyun içi credits için hazır metin, ses kaynakları, intro kaydı |
| `README.md` | Oyuncuya vaat edilen kontroller ve teslim tanıtımı |
| `Assets/_Project/Art/README.md` | Flat cartoon kararı ve placeholder/final görsel listesi |
| `Assets/_Project/Audio/README.md` | Hazır ses dosyaları ve bağlanacak müzikler |

Durum belirlerken eski boş kutular yerine son uygulama commit'i ve mevcut kod esas alındı. Son doğrulanmış EditMode sonucu önceki çalışmada 15/15; bu belge incelemesi için testler tekrar çalıştırılmadı. Windows'ta son kaynak koduyla uçtan uca oynanış bu turda doğrulanmadı.

## 2. Bugün nerede duruyoruz?

| İş | Durum | Kalan kontrol |
|---|---|---|
| Yeni Oyun → hikâye intro'su → Game | Tamam, `5f4a49a`; Play geçişi doğrulandı | Tarık'ın yeni intro metnini incelemesi |
| Devam Et'in doğrudan Game'e gitmesi | Kod yolu hazır | Gerçek gün sonu kaydından yükleme testi |
| DayIntro ve Construction | Prefab/script hazır; Ahmet `efc003a` ile Game'e ekledi | Güncel Windows build'de kontrol |
| Gazete, gün özeti, ödeme, dört son | Mevcut ve Game'e bağlı | Borç önizlemesi, uzun içerik ve son kaynakla uçtan uca test |
| Core Ç2/Ç3/Ç5 | Uygulandı: proje türleri, esnek roller, tekrarsız bina adları | Yeni karakter/ad içerikleri ve oynanış turu |
| Desk öğreticisi, El Kitabı, akredite lab listesi | Mevcut | Ç1 değişince içerik ve arayüz uyarlaması |
| Kabadayı cam olayı ve Desk sesleri | Uygulanmış | Son build'de ses/ücret/olay kontrolü |
| Esc menüsü | Uygulandı; 15/15 EditMode + 3/3 Flow PlayMode geçti | Son Windows build kontrolü |
| Kayıt sonucu ekranı | Eksik | Önce Core kayıt sonucunu Flow'a vermeli |
| Menü ve kötü/orta son müzikleri | Dosyalar hazır, Flow'da oynatma bağlantısı yok | Onur bağlayacak |
| Oyun içi Emeği Geçenler | Ekran var, metin eksik | Ekip/AI/font ve yeni ses satırlarını eşitle |
| Final görseller ve portreler | Çoğu görsel belgede placeholder; 6 portre referansı boş | Tarık üretir/seçer; B/C kendi ekranlarına bağlar |
| Ç1 bölge/kat ve yeni El Kitabı | Onaylı iş; sözleşme alanları henüz uygulanmamış | A → B/EEE → C uyumluluk kontrolü |
| Ç4 tepki, hafıza ve hediye | Onaylı iş; API henüz yok | A + EEE + B ortak entegrasyonu |

Güncel temel sayılar kod/asset'ten doğrulandı: 10 gün, başlangıç 2.500 TL, onay ücreti 300–500 TL, kira 600 TL, kredi 300 TL, borç sınırı 2.000 TL, deprem olasılığı %20 başlangıç ve afetsiz gün başına +%15. Sahte belge 3. günden açık. Kota, fabrika, yangın ve sigorta mevcut kapsamda yok.

## 3. Belgelerde düzeltilmesi gereken eski bilgiler

| Eski veya çelişkili not | Güncel karşılığı |
|---|---|
| ONUR.md eski kontrol listesinde gazete/ödeme/menü hâlâ boş | Bunlar hazır; yeniden yapılmayacak |
| ONUR.md eski bölümünde Weekly/PendingWeekly ve kayıp verisi bekleniyor | Günlük sözleşme ve kayıp verisi zaten mevcut |
| memory.md özetinde DayIntro/Construction hâlâ yedek ekran | Ahmet Game'e ekledi; eksik kalan Save sunumu |
| OGUZHAN.md intro kararını ve Office sahnesini bekliyor | Intro Flow'a taşındı ve Game'e bağlandı |
| CREDITS intro satırı eski dosya yolunu gösteriyor | Yeni yol `Assets/_Project/Scripts/Flow/UI/IntroSequence.cs`; güncellemeyi Oğuzhan üstlendi |
| Eski metinlerde 3 son, haftalık ödeme, %50 olasılık, 400–700 ücret | 4 son, günlük ödeme, %20/+%15, 300–500 ücret |
| memory.md üst kısmında sahte belge 5. günden deniyor | Day03.asset artık sahte belgeyi 3. günden açıyor |
| “Dürüst usta her zaman uygun proje getirir” | Dürüst usta da dikkatsiz proje getirebilir; rüşvet/sahte belge getirmez |
| ICERIK_TASLAK.md eski açıklamasında İyi son yıkımla da gelebilir | İyi son için hiç yıkım olmamalı; asıl kaynak güncel kod ve EndingSO asset'leri |
| Sesler “hazır”, fakat Audio README hâlâ P diyor | Dosyanın bulunması, final kabulü ve oyunda çalması ayrı ayrı işaretlenmeli |
| Stil notunda “~12 saat kaldı” | Takvim için bu eski süre ifadesi kullanılmayacak; belgelerdeki gerçek tarih/saat esas alınacak |

Öneri: Kişisel dosyalarda en üstte tek güncel durum listesi tutulsun; eski oturum kayıtları “tarihsel” olarak kalsın. Ortak kararlar ile yapılmış kod aynı şey sayılmasın.

## 4. Onur'un uygulama sırası

### 1 — Esc menüsü: ilk başlanacak iş

17 Ekim güncellemesi: Kod tamamlandı; EditMode 15/15 ve Flow PlayMode 3/3 geçti. Menü sahnelere otomatik kurulur. Windows build'de son kontrol bekliyor.

**Sahip:** Onur. **Bağımlılık:** Temel arayüz için ekip beklemek gerekmiyor. **Tahmin:** 45–90 dakika.

- MainMenu, intro ve Game'deki ekranlarda Esc davranışını tanımla.
- Game'de Devam / Ana menü / Çıkış; ana menüde gereksiz bir “ana menüye dön” seçeneği gösterme.
- Menü açıkken arkadaki onay, ret, ödeme ve intro ilerleme girdilerini engelle. Sadece `Time.timeScale = 0` kullanmak yeterli değildir; UI ve intro'nun gerçek zamanlı coroutine'leri devam edebilir.
- Ana menüye dönüşte “Gün içindeki ilerleme kaydedilmez; son gün sonu kaydı korunur” bilgisini ver. Yeni manuel kayıt kuralı ekleme.
- Sahne değişiminde zaman ölçeği, girdi kilidi ve varsa ses durumu doğru geri alınmalı.
- Global menü kurulumu Game.unity kaydetmeden yapılabiliyorsa Flow içinde çöz; sahneye bileşen eklemek gerekiyorsa Ahmet kursun.

**Bitti ölçütü:** Esc ile aç/kapat, Devam, ana menü ve çıkış çalışır; menü açıkken arka ekranda tek bir karar/ödeme bile yapılmaz. Intro ilerlemez. Geri dönünce oyun takılı kalmaz.

### 2 — Ödeme ekranını Core sonucuyla tutarlı yap

**Sahip:** Onur + Ahmet. **Bağımlılık:** Core'dan güvenilir ödeme önizlemesi. **Tahmin:** 60–120 dakika ortak çalışma.

Kodda doğrulanan sorun: `DailyExpensesScreen.UpdateTotals`, ödeme sonrası kasayı `Money - TotalSelected` olarak gösteriyor. Core ise zorunlu kalemleri önce değerlendiriyor, kasa yetmeyen gideri ödemiyor ve borç ödemesini kısmen yapabiliyor.

Örnek: kasa 1.000 TL, eski borç 2.000 TL; kira 600 + kredi 300 + borç ödemesi seçili. Ekrandaki basit çıkarma -1.900 TL gösterebilir. Core'un sonucu kasa 0 TL ve borç 1.900 TL olur. Ayrıca isteğe bağlı borç ödemesini seçmemek yeni borç oluşturmaz; mevcut genel uyarı bunu net ayırmıyor.

- Ahmet'ten gerçek state'i değiştirmeyen bir önizleme sonucu iste; API adı henüz kararlaştırılmış değil.
- Önizleme Core'un aynı ödeme sırasını ve kısmi borç ödeme kuralını kullanmalı; Flow'a ikinci ekonomi hesabı yazılmamalı.
- Kasa sonrası, borç sonrası, gerçekten ödenecek tutar ve borca eklenecek zorunlu gider ayrı gösterilmeli.

**Bitti ölçütü:** Yeterli kasa, yetersiz kasa, cam tamiri, borcun kısmen ödenmesi ve isteğe bağlı ödemenin seçilmemesi durumlarında önizleme ile gerçek sonuç eşleşir. Çift tıklama ikinci ödeme oluşturmaz.

### 3 — Kayıt geri bildirimi ve yükleme akışı

**Sahip:** Ahmet kayıt sonucu; Onur ekran/prefab. **Bağımlılık:** Core sözleşmesi. **Tahmin:** 60–120 dakika ortak çalışma.

- Şu an `AdvanceDayAndSave()` bool döndürüyor; `GameManager` bu sonucu kullanmıyor. Flow'a kayıt sonucu verilmeden başarı yazılmayacak.
- Kayıt şu an ekran Show edilmeden önce senkron tamamlanıyor. Önce sonuç gösteren sade bir ekran yeterli; gerçek iş sürmüyorsa yapay “Kaydediliyor” animasyonu gerekli değil.
- Başarıda “Kaydedildi / Sonraki gün”; başarısızlıkta doğru hata durumu ve Core tarafından sağlanan güvenli tekrar deneme yolu.
- **Tekrar deneme günü bir kez daha artırmamalı.** Mevcut metot her çağrıda `Day++` yapıyor; aynı metodu yeniden çağıran buton hatalı olur.
- Yazma izni/yazma hatası, bozuk veya okunamayan kayıt için Core davranışını Ahmet netleştirsin. Kayıt bozukken sessizce Yeni Oyun açılmasının oyuncuyu yanıltıp yanıltmadığı incelensin.
- Onur Save prefab'ını üretir; Ahmet Game'e kurar.

**Bitti ölçütü:** Başarılı kayıttan Devam Et doğru günün başını açar; intro tekrar oynamaz; kayıt hatasında başarı yazmaz; tekrar deneme gün atlatmaz; son ekranına girilince Devam Et kaydı kalmaz.

### 4 — Oyun içi credits'i belgelerle eşitle

**Sahip:** Onur; içerik kaynağı Tarık'ın hazır bloğu. **Bağımlılık:** Yok. **Tahmin:** 20–40 dakika.

- MainMenuScreen içindeki metin şu an üç ses satırı içeriyor. CREDITS_AND_AI.md sonundaki ekip, sesler, font ve AI metni daha kapsamlı.
- Hazır bloğu oyun içi ekrana taşı; metin uzadığı için kaydırma veya sayfalama kullan, okunmaz biçimde küçültme.
- Kaynak atfının tamamını ve lisans adını görünür tut; Geri butonu metin altında kaybolmasın.
- Intro'nun kaynak/AI kayıt satırını Oğuzhan güncelleyecek; Onur'un Flow satırına intro ekleme yapılmayacak.

**Bitti ölçütü:** Oyun içindeki içerik hazır blokla eşleşir; 1920×1080 ve küçük pencerede bütün satırlara erişilir.

### 5 — Hazır müzikleri Flow'a bağla

**Sahip:** Onur. **Bağımlılık:** Dosyalar zaten hazır. **Tahmin:** 30–60 dakika.

- `mus_menu.wav`: ana menüde döngü; intro sırasında sürüp sürmeyeceği Tarık'la netleştirilsin.
- `mus_ending_sad.mp3`: Bad, MiddleClosed ve MiddleCollapsed sonlarında.
- İyi son için tanımlanmış ayrı müzik yok; yeni parça varsayma.
- Aynı sahneye dönüldüğünde üst üste AudioSource oluşmasın; Desk ortam sesiyle istemsiz çakışmasın.
- Ses dosyasına uzantıdan yol üretmek yerine AudioClip referansıyla bağlan.

**Bitti ölçütü:** Menü → intro → Game → son → menü turunda müzikler doğru yerde başlar ve durur; üst üste binmez.

### 6 — Flow görselleri ve okunabilirlik

**Sahip:** Tarık görsel; Onur Flow yerleşimi. **Bağımlılık:** Final asset'ler. **Tahmin:** Asset'ler geldikten sonra 60–120 dakika.

- Önce ana menü/intro, ödeme ve gün başı; ardından Construction, gün özeti ve son ekranları.
- Flat cartoon tarzıyla renk, font, buton ve boşlukları ortaklaştır. Ayrı bir görsel tarz başlatma.
- Final görsel aynı ad ve GUID ile gelirse mevcut referansları koru; yeni görsel varsa yalnızca gereken bağlantıyı kur.
- “Enkazın Gölgesi” için ayrı final görsel gerekip gerekmediğini Tarık'la netleştir; mevcut dört son / üç görsel eşleşmesini kontrol et.
- Gazeteyi 0 bina, tek bina ve çok bina senaryolarında kontrol et; bütün sonuçlara ulaşmak için sayfalama/kaydırma gerekip gerekmediğini gör.
- Uzun Türkçe isimler, uzun son metinleri, çok büyük para/borç ve dinamik Türkçe karakterleri kontrol et.

**Bitti ölçütü:** Placeholder yazıları kaldırılmış; gazete, son metni ve butonlar üst üste binmiyor; her içeriğe erişilebiliyor. Bina yıkımı animasyonu veya yeni deprem sahnesi bu işin parçası değil.

### 7 — Ahmet'in Ç1 ve Ç4 güncellemeleri geldiğinde Flow uyumu

**Sahip:** A Core, B Desk, EEE içerik; Onur Flow kontrolü. **Bağımlılık:** Uygulanmış ve belgelenmiş API. **Tahmin:** Her sözleşme değişikliğinden sonra 30–60 dakika ilk kontrol.

- Ç1'in planlanan alanlarını şimdiden var sayarak kod yazma. Şu an `Zones`, `Labels`, `HandbookIntro` gibi sözleşme taslakları uygulanmış değil.
- Ç1 gelince gazetede yeni parametre adları ve yıkım nedeni metinleri doğru mu kontrol et. Mevcut `ParamName` eski parametreler için yedek eşleme kullanıyor.
- Harfli değerler, kat/bölge kuralı ve değişen veri düzeninin sandbox örneklerini etkileyip etkilemediğini kontrol et.
- Ç4 gelince hediye/tepki UI'ını Oğuzhan yapar; Onur para/son ekranlarının yeni Core state'ini doğru okuduğunu doğrular.
- `ProjectData.Kind` oyuncuya gösterilmez. Flow doğrudan Desk assembly'sine bağlanmaz.

**Bitti ölçütü:** Güncel Core ile derleme temiz; gazete ve son istatistikleri doğru; gerçek oyun ile sandbox ayrımı korunmuş.

### 8 — Windows'ta teslim turu

**Sahip:** Ahmet build; bütün ekip oynanış. **Bağımlılık:** Yukarıdaki temel akış işleri. **Tahmin:** 90–150 dakika ve bulunan hataların düzeltmesi.

- Baştan sona normal bir 10 günlük oyun oynanır; yalnızca F9/F10 turu yeterli sayılmaz.
- Dört son koşulu ayrı kontrol edilir. F9/F10 gösterim ve hızlı test için kullanılır; her sonu tek başına garanti etmez.
- Kayıt, kapat/aç, Devam Et; Esc menüsü; müzik; ekran oranı; son ekranından menüye dönüş birlikte denenir.
- Yeni müteahhit ve veri asset'lerini Ahmet `Link Data` ile Game'e bağlar; sahnede kayıp referans kalmadığı kontrol edilir.
- Final ZIP temiz bir klasöre çıkarılıp exe çalıştırılır. Test edilen kaynak commit'i build ile eşleştirilir.

**Bitti ölçütü:** Akışta takılma, kayıp sahne, yanlış kayıt mesajı, çift ödeme/gün ilerlemesi, okunamayan temel ekran veya eksik teslim dosyası yok.

## 5. Ekipte paralel ilerleyecek işler

| Kişi | Önce | Sonra | Onur'u etkileyen çıktı |
|---|---|---|---|
| Ahmet | Kayıt sonucu/tekrar deneme ve ödeme önizlemesi sözleşmesi | Ç1, Ç4, denge ve Windows build | Core API + Game'e Save/menu kurulumu gerekirse entegrasyon |
| Oğuzhan | Ç1 sayfalı El Kitabı; rapordaki min./renk ve yan yana karşılaştırma ipuçlarını plana göre düzenleme | Ç4 tepkiler/hediye; final Desk görselleri | Güncel Desk prefab'ı ve intro credits kaydının düzeltilmesi |
| Tarık | Final görsel ailesi; intro/öğretici metni incelemesi | Müteahhitleri 10–12'ye çıkarma, ad havuzunu yaklaşık 30'a genişletme, Ç1/Ç4 metinleri | Portreler, Flow görselleri, son metinler, credits içeriği |
| Onur | Esc menüsü; Core bağımlılıklarını net iletme | Ödeme/Save, credits/müzik, Flow cila ve test | Tam ve tutarlı oyuncu akışı |

Müteahhidin kayarak gelmesi, hafif sallanması ve konuşma balonu Tarık'ın görsel önerisi olarak belgelenmiş; B'nin uygulama onayı/planı henüz kesin değil. B'ye otomatik olarak tamamlanmış görev yazılmamalı. Sprite sheet/yürüme animasyonu talep edilmemeli.

## 6. Önerilen teslim takvimi

Takvim, repo belgelerindeki hedeflere dayanır; dışarıdan jam duyurusu doğrulaması yapılmadı. Saatler UTC+3.

| Zaman | Hedef | Çıkış koşulu |
|---|---|---|
| Şimdi, ilk çalışma bloğu | Onur Esc menüsü; Ahmet'e Save/önizleme ihtiyaçları; Tarık/Oğuzhan içerik notları | Tek bir açık bağımlılık listesi; Esc çalışıyor |
| Sonraki blok | Ödeme ve Save sözleşmesi geldikçe tamamla; beklerken credits/müzik | Yanlış başarı/para mesajı yok; hazır sesler oyunda |
| 17 Ekim gündüz | A/B/EEE Ç1; Onur görsel ve gazete uyumu | Ana oynanış güncellemesi birlikte çalışıyor |
| 17 Ekim 20:00 build hedefi | Entegre Windows sürümü | Açılış + bir günlük döngü + kayıt/Devam Et |
| 17 Ekim akşamı–18 Ekim sabahı | Ç4 hazırsa entegrasyon, dört son, normal 10 gün, denge/okunabilirlik | Teslim adayı sürüm; büyük API değişikliği kapanmış |
| 18 Ekim 12:00 | Planlanan feature freeze; belgelerde Git desteği için de bu saat geçiyor | Yeni özellik eklenmez; teslimi engelleyen hatalar ele alınır |
| 18 Ekim 12:00–15:00 | Final build, ZIP, ekran görüntüsü, en fazla 1 dakikalık video, README | Birlikte teslim edilebilir paket |
| 18 Ekim 15:00–16:30 | Yükleme ve indirilmiş paketi kontrol | Linkler ve açılış çalışıyor |
| 18 Ekim 16:30–18:00 | Yükleme/son kontrol tamponu | Belgelerdeki 18:00 son teslim saatine iş bırakılmaz |

Süre sıkışırsa önerilen öncelik: kayıt/ödeme doğruluğu ve tamamlanabilir oyun → Esc/README tutarlılığı → credits → okunabilirlik ve ses → görsel cila. Ç1/Ç4 zaten onaylı ekip işleri olduğundan bunları iptal kararı Onur tek başına vermez; 20:00 build'i riske girerse kapsamı ekip birlikte daraltır. Yangın/sel, fabrika, sigorta ve yeni deprem sahnesi eklenmez.

## 7. Son test listesi

| Senaryo | Beklenen sonuç |
|---|---|
| Kayıtsız açılış | Devam Et yok; Yeni Oyun intro açar |
| Altı intro sayfası | Fare/Space/Enter ilerler; Office aranmadan Game açılır |
| Gerçek kayıttan Devam Et | Intro atlanır; doğru gün ve para/borç yüklenir |
| Kayıt yazma hatası + tekrar deneme | Başarı yazılmaz; gün ikinci kez artmaz |
| Bozuk kayıt | Core'un belirlenmiş hata davranışı; sessiz veri kaybı hissi yaratılmaz |
| Esc: intro, Desk, gazete, ödeme, son | Arkadaki girdiler engellenir; Devam düzgün döner |
| İnşaat: 0 ve 2 onay | Doğru metin; sonraki faz bir kez açılır |
| Afetsiz gün | Gazete açılmaz; gün özeti → ödeme |
| Afet: boş kapsam, ayakta, yıkık, çok bina | Uygun gazete ve erişilebilir sonuç listesi |
| Para yetersiz / eski borç / cam tamiri | Önizleme ve Core sonucu aynı; kasa eksiye düşmez |
| Dört son | Doğru metin/görsel/müzik; son kaydı silinir |
| Uzun adlar/metinler; küçük pencere ve 1920×1080 | Yazılar ve butonlar erişilebilir |
| Menü → intro → Desk → son → menü | Müzik üst üste binmez; ses kaynakları çoğalmaz |
| Temiz klasörde Windows ZIP | Exe açılır, kayıt ve kontroller çalışır; README sürümle eşleşir |

## 8. Ekibe iletilecek kısa notlar

**Ahmet:** “Intro main'de (`5f4a49a`). Sırada Esc, ödeme doğruluğu ve Save var. Kayıt sonucunu Flow'a ileten ve tekrar denemede günü artırmayan bir yol ile state'i değiştirmeyen ödeme önizlemesi gerekiyor. Save prefab'ı hazır olunca Game'e sen ekleyeceksin.”

**Oğuzhan:** “Intro Flow/UI/IntroSequence.cs'ye taşındı; Yeni Oyun → intro → Game çalışıyor. Üstlendiğin credits satırında yeni yolu ve entegrasyon bilgisini güncelleyebilirsin. Ç1/Ç4 geldiğinde sen Desk'i, ben Flow uyumunu kontrol edeceğim.”

**Tarık:** “Intro'nun sayısal istatistiklerini ve sel/yangın ifadesini çıkardık; 2–3. sayfa metnini incelemeni bekliyoruz. Menü/logo, ödeme, gün özeti ve son görsellerini aynı flat cartoon ailesinde netleştirelim. Menü/son sesleri hazır; Flow'a bağlayacağız.”

Bu notlar hazırlanmıştır; kimseye araçla mesaj gönderilmedi.

## 9. İlk somut adım

Esc kodu ve otomatik testleri tamamlandı. Onur için sıradaki uygulama: **Ahmet'le ödeme önizlemesini Core sonucuyla eşleştirmek ve Save sonucu sözleşmesini netleştirmek.** Ahmet'in cevabı beklenirken credits ve müzik bağımsız ilerleyebilir. Windows turunda Esc de yeniden kontrol edilir.
