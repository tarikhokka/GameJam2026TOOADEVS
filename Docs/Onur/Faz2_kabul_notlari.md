# 18 Ekim — Onur C2 ve C1-K kabulü

Onur kapsamı: C2 ana menü ve C1-K karartma kabulü. Tarık'ın T1/T2 işleri devralınmadı. Birleşik taban: `d579a84` (B2 `1c652eb`, son ekranlar `8ba3454` dahil); üzerine bu C2 değişikliği.

## C2 sonucu

- MainMenu, eski arka plan yuvalarından bağımsız 440×102 px düğmeleri 14 px arayla aynı merkez etrafında dizer: kayıt varsa dört, yoksa üç düğme.
- Yeni `btn_menu` oranı korunur; MainMenu sahnesinde Paytone One bağlıdır. Tek satır, iç boşluk ve uzun yazı için otomatik boyutlandırma vardır.
- Emeği Geçenler ekranının Geri düğmesi aynı görsel/yazı stilindedir.
- ButtonHover, menü ambiyansı, logo, Devam Et kayıt yükleme ve tek intro başlatma davranışı korunur. İsteğe bağlı Esc görünümü bu pakete alınmadı; mevcut ayarlar ve duraklatma testleri geçti.
- Runtime değişikliği yalnız `MainMenuScreen.cs`; sahnede yalnız `buttonFont` GUID'i değişti. Desk prefab/builder ve Game.unity bu teslimde değiştirilmedi.

## C1-K sonucu

İzole kopyada **gerçek `Sandbox_Desk.unity`** yüklendi. İlk başvuru sonuçlandırılıp ikinci başvuruya geçildi; form ve El Kitabı beton alanlarına tıklanarak gerçek Core uyuşmazlığı üretildi (C25 / en az C30).

- İnceleme modunda masa karartması açık, alfa 0,20; karartma görseli tıklama yakalamıyor.
- UYUŞMAZLIK SAPTANDI kaşesi karartmanın üstünde ve tam görünür.
- Denetçinin gerçek C25/C30 konuşması yazısı tamamlandığında balon da karartmanın üstünde ve tam görünür; konuşma sırasında inceleme modu açık kalıyor.
- Mod kapanınca karartma ve geçici sıralama Canvas'ları kaldırılıyor.
- **Oğuzhan'a karartma düzeltmesi gerektiren hata bulunmadı.** B2'nin yeni üst barı ve düğmeleriyle aynı kabul yeniden çalıştırıldı.

## Doğrulama

Unity **6000.6.4f1**; asıl proje/sahneler açılıp kaydedilmeden izole kopyada derleme ve test:

| Kontrol | Sonuç |
|---|---|
| Mevcut EditMode paketi | 55/55 |
| Mevcut Flow PlayMode: PauseMenu + SettingsMenu | 4/4 |
| Geçici C2/C1-K PlayMode kabulü | 6/6 |
| Birleşik PlayMode toplamı | 10/10 |
| 1920×1080 Unity render görsel kontrolü | Kayıtlı/kayıtsız menü, Geri, kaşe ve denetçi balonu |

Geçici altı kontrol: iki kayıt durumunda merkez/taşma/çakışma/font/sprite; credits-Geri ve fare vurgusu; kayıtlı günün yüklenmesi; çift tıklamada tek intro; gerçek Sandbox_Desk uyuşmazlık kaşesi ve konuşma sırası; mod kapanınca katman temizliği. Denetim kodu yalnız geçici kopyada tutuldu. Kayıt denemeleri ayrı ürün adına ait dosyada yapıldı; kullanıcının oyun kaydı değiştirilmedi. Bu kabul final Windows build/ZIP veya tam yedi günlük manuel oyun turu yerine geçmez; bunlar A3 kapsamındadır.

## Görsel kanıt

- [Kayıtlı menü](Faz2_menu_kayitli.png)
- [Kayıtsız menü](Faz2_menu_kayitsiz.png)
- [Emeği Geçenler / Geri](Faz2_credits.png)
- [İnceleme kaşesi](Faz2_karartma_kase.png)
- [Denetçi konuşması](Faz2_karartma_diyalog.png)

Tarık'a görsel notu: sütun eski yuva bölgesinin üzerine geliyor. Kayıtsız menüde ilk düğmenin hemen üstünde `menu_bg` dolgu/parlama izi açıkta kalıyor; kaynak görsele dokunulmadı. Tarık aynı ad/GUID ile bölgeyi rötuşlayabilir. §7'ye not bırakıldı; A3 son görsel kontrolünde değerlendirilecek.

## Ahmet'e teslim

C2 ve C1-K tamam. B2 `1c652eb` de alındı; Faz 2'nin üç teslimi hazır. §7 faz tablosunda C2/C1-K commit'i kayıtlıdır. Ahmet, A3'te Game.unity entegrasyonunu kendi alanında yapıp son oynanış/ekran görüntüsü/build/ZIP kontrolüne geçebilir. Faz 3'ü açma kararı A'dadır.
