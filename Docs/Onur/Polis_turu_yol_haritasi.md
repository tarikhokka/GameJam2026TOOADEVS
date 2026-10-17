# 18 Ekim — Onur poliş turu planı

Kabul tabanı: `d579a84` (B2 ve son ekranlar dahil). Kaynak: `memory.md` §7, Ahmet'in K1–K4 kararları ve faz kapısı. Onur'un son düzeltmesi: Tarık'ın işleri devralınmıyor; **bizim kapsamımız C1 ve C2**. T1/T2 Tarık'ta ve tamamlandı (`f8842b5`, `1dd7870`). C1 `1b47ce6`, C2/C1-K `d2dddb9` hazır. Faz 2 `fac2bae` ile açıldı; birleşmiş sürümde C2/C1-K kabulü 55/55 EditMode + 10/10 PlayMode ile geçti. Teslim ve görüntüler: `Faz2_kabul_notlari.md`. Tarık'a arka plan dolgu izi rötuş notu bırakıldı. Son entegrasyon/build A3 Ahmet'te.

## Ahmet'in istediği sonuç

1. El Kitabı kat grubunu binanın kat adedinden otomatik gösterir; grup düğmeleri gösterge olur. Yanlış grup nedeniyle ALAKASIZ çıkmaz.
2. İnceleme modunda masa hafif kararır; incelenebilir alanlar, ONAY/RED, İNCELE, çizgi/sonuç ve diyaloglar aydınlık kalır. ONAY/RED modu kapatıp karar verir; EK TEST ve zarf pasif kalır.
3. Masa ve ana menü düğmeleri ONAY/RED/EK TEST ile aynı görsel aileye geçer.
4. Üst bar temaya uyarlanır; yüksekliği **80 px** kalır, masa/ofis kaymaz.
5. Hızlı Bakış'ın alttaki RED kutusu kalkar; beş parametre büyür.
6. BETON/DONATI/YAPI/ZEMİN sekmeleri ve sayfaları kalkar. HIZLI BAKIŞ ve LAB kalır; silinen bölüm notları başka yere taşınmaz.
7. Ana müşteriler üçer, yeni müşteriler birer kez gelir; aynı gün tekrar yok. Bu kural mevcut, A gerçek kadro testini güçlendirecek.

## Bizim teslimlerimiz

### C1 — yeni üst bar bileşeni (ilk adım; bağımsız)

- `Assets/_Project/Scripts/Desk/UI/DeskHeaderView.cs` + `.meta`, `CivilJam.Desk`.
- Awake'te kendini kurar; Reset fontları ve varsa görselleri bağlar. Görseller yokken kahve şerit, krem etiket ve konturla çalışır.
- API sabit tutulur: `Refresh(GameContext ctx)`, `SetClient(string clientName, string projectName)`, `SetDayDone(bool allDecided)`.
- Solda Gün; ortada müşteri/proje; sağda Kasa, varsa kırmızı Borç ve Onaylanan/Bekleyen. Bekleyen sayısı bugünkü Pending projelerden, diğer değerler Core durumundan okunur.
- Üste yaslı 80 px; NASIL OYNANIR barın altında kalır. Yeni ekonomik kural veya Flow bağımlılığı eklenmez.
- Oğuzhan'ın builder'ı hangi font/sprite alanlarını bağlayacağını anlatan kısa bağlantı notu teslim edilir. Desk prefab'ını biz değiştirmeyiz.
- Kontrol: borçlu/borçsuz durum, uzun müşteri/proje adları (iki ikili dahil), sıfır/birden çok bekleyen, gün sonu, sprite'lı/sprite'sız görünüm, metin kesilmesi ve 80 px sınırı.
- B1 geldikten sonra sonuç kaşesi ve denetçi balonunun karartmanın üstünde kaldığı gerçek Sandbox_Desk'te ayrıca doğrulanır; B1 yokken bu kısım tamamlandı sayılmaz.

### Tarık'ın teslimleri (bizim kapsamımız dışında)

T1 görselleri `f8842b5` ve T2 metinleri `1dd7870` ile alındı. C1 mevcut hud_bar/hud_tag/ikonları kullanır. Art, TutorialView, README ve ICERIK_TASLAK üzerinde bu işleri yeniden yapmayız.

### C2 — ana menü (Faz 2 kapısı açıldıktan sonra)

- Devam Et / Yeni Oyun / Emeği Geçenler / Çıkış ve credits Geri: yeni `btn_menu`, kodla Paytone One yazı, mevcut ButtonHover davranışı.
- Resimdeki eski üç yuvaya bağlı yerleşim kaldırılır. Kayıt varsa dört, yoksa üç düğme düzgün ve ortalanmış sütunda görünür.
- MainMenu sahnesindeki sprite/font bağlantıları kaydedilir; ambiyans, logo, intro ve kayıt akışı korunur.
- Kontrol: kayıtlı/kayıtsız menü, credits/Geri, Yeni Oyun intro, Devam Et ve mevcut SettingsMenuTests/PauseMenuTests. Esc görünümü yalnız vakit kalırsa; yeni tam ekran ve ses ayarları korunur.

## Takım sınırları ve sıra

**A:** Core, otomatik grup, El Kitabı sadeleşmesi, gerçek kadro testi, Game.unity, son entegrasyon/build. **B:** karartma ve karar butonları; İNCELE/NASIL OYNANIR + üst barın builder/prefab bağlantısı. Biz DeskHud, DeskPropsView, DeskScreen, HandbookUi, RulebookView veya Desk prefab'ını düzenlemeyiz.

1. C1: yeni görsellerle bileşen ve bağlantı notu → Unity doğrulaması → teslim.
2. Faz 1'de A1/A2/B1/C1/T1/T2 tamamlanıp A, Faz 2'yi açana kadar C2 ve C1-K yapılmaz. MainMenuScreen ve Desk prefab/builder dosyalarına bu aşamada dokunulmaz.
3. Faz 2 açılınca C2 ana menü ve C1-K kaşe/balon karartma kontrolü yapılır.
4. Tamamlanan her paket için `memory.md` §7'de görev kodu ve commit hash'i; faz kontrolü ve B2 bağlantısı ayrıca takip edilir.
5. Son kod teslimi **18 Ekim 11:00**, freeze **12:00**; final build/ZIP A'da.

Önceki ödeme/kayıt hataları, müzik, gazete düğmesi ve `hedef_arayuz.png` araç kaydı ayrı açık işlerdir. Bu çalışma onları tamamlanmış saymaz.
