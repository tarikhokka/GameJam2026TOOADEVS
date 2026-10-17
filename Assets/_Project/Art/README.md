# Art — sahibi: 4. kişi (EEE)

Bu klasöre sadece EEE yazar. Başkası görsel isterse EEE'ye söyler.

7 günlük yeni yapı paketinde Onur, Tarık'ın portre ve içerik görevlerini devraldı (Onur'un bildirimine göre ekip biliyor). Yeni `portrait_g01.png` … `portrait_g10.png`: Onur'un sağladığı ChatGPT çizimleri; yerel script ile arka plan temizlendi ve 1000×1000 şeffaf PNG'ye yerleştirildi. g01 ve g04'teki ikililer birlikte gelen tek müşteridir. Eşleme ve önizleme: `Docs/Onur/YediGun_icerik_yol_haritasi.md`. Eski portrelerin GUID'leri değiştirilmedi.

## Kurallar
- **Tarz: el çizimi 2D cartoon** — kalın koyu kahve kontur, yumuşak cel gölge, sıcak palet; Gemini/ChatGPT ile ortak tarz prompt'uyla üretilir. Müteahhit önden, masanın karşısında oturur; yürüme animasyonu yok. Beyaz arka planlı üretilen görsellerin arka planı script ile silinir.
- **Stil (karar 02:20): hazır asset, piksel art yok.** Yeniden dağıtılabilir lisanslı (tercihen CC0) asset'ler; mümkünse tek kaynak/tek stil ailesinden. Her asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır; CC BY ise oyun içi Emeği Geçenler metnine de.
- Referans çözünürlük 1920x1080. Sprite import: Texture Type = Sprite (2D and UI), Sprite Mode = Single, Pixels Per Unit = 100.
- Önce placeholder konur; gerçek görsel **aynı dosya adıyla üzerine yazılır** (.meta/GUID korunur, prefab referansları bozulmaz).
- İsimler küçük harf + alt çizgi, Türkçe karakter yok: `alan_ad_durum.png`.
- Hazır asset eklendiği anda `CREDITS_AND_AI.md`'ye yazılır.
- Hassasiyet: gerçek deprem görüntüsü yok, enkaz altında insan yok, depremzedeler olumsuz gösterilmez.

## İhtiyaç listesi (B/C ile netleştir)
| Dosya | Ekran | Kim kullanıyor | Durum |
|---|---|---|---|
| `desk_bg.png` | Desk ARKA katman: oda + koltuk + masa (ChatGPT, 1920x1080; masa sadeleştirildi: kaşe, kağıtlar, notlar, kalem kaldırıldı, kupa/kalemlik/isimlik kaldı). Çizim sırası: desk_bg → portre → desk_front → kitaplar | B | F |
| `desk_front.png` | Desk ÖN katman: sadece masa (şeffaf, 1920x1080, tam ekran). Portrenin ÖNÜNE çizilir, karakterin belden aşağısını kapatır | B | F |
| `doc_paper.png` | Desk | B | P |
| `stamp_approve.png` | Desk | B | P |
| `stamp_reject.png` | Desk | B | P |
| `signature.png` | Desk | B | P |
| `bld_house_standing.png` | Construction/Result | C | P |
| `bld_house_collapsed.png` | Result (gazete) | C | P |
| `city_bg.png` | Construction | C | P |
| `newspaper_bg.png` | Result | C | F |
| `dayintro_bg.png` | DayIntro (gün başı): yeni ofis + masa (office_bg, desk_surface, desk_edge) birleşimi, hafif bulanık ve loş, ortadaki beyaz yazı okunsun diye. DayIntroScreen.prefab kök Image sprite'ına bağlı | C | F |
| `construction_bg.png` | Construction (inşaat ekranı): ofis penceresinden görünen vinçli şantiye manzarası (desk_bg kaynağından kırpılıp büyütüldü, bulanık, gün batımı tonu). ConstructionScreen.prefab kök Image sprite'ına bağlı | C | F |
| `daysummary_bg.png` | DaySummary: yeni ofis + masa, akşam mavisi, bulanık (17 Ekim akşam yenilendi) | C | F |
| `save_bg.png` | Save: yeni ofis + masa, gece, ışıklar sönük (17 Ekim akşam yenilendi). SaveScreen.prefab background | C | F |
| `payment_bg.png` | Günlük ödeme: yeni ofis + masa, loş sıcak ton, bulanık (17 Ekim akşam yenilendi) | C | F |
| `menu_bg.png`, `logo.png` | MainMenu (poliş T1: çizili 3 tahta düğme yuvası silindi, yeri şehirle dolduruldu; akşam şehir + vinçler; "İMZA" logosu, Lilita One + imza kıvrımı, 900x330 şeffaf) | C | F |
| `ending_good.png` | Ending İyi "Ayakta Kalanlar": gün batımında düzenli ofis, masada onaylı (✓) dosyalar, pencerede ayakta binalar (ChatGPT, 17 Ekim akşam) | C | F |
| `ending_bad.png` | Ending Kötü "Hüküm": parmaklıklar arkasından hücre, yatakta proje rulolarıyla (ChatGPT, 17 Ekim akşam) | C | F |
| `ending_closed.png` | Ending Orta "Kapanan Kapı" (iflas): mühürlü, şeritli ofis, koliler, devrik çay (ChatGPT, 17 Ekim akşam) | C | F |
| `ending_collapsed.png` | Ending Orta "Enkazın Gölgesi" (yeni ofis + masa, neredeyse renksiz soğuk gri, pencereden solgun ışık — 18 Ekim yenilendi; MiddleCollapsed.asset'e bağlı) | C | F |
| `gift_rifat/sevim/kemal/nuri/selim/rustem.png` | Desk: ofis hediyeleri (GiftSO.Sprite), 300x300 şeffaf, konturlu | B | F |
| `portrait_rifat/sevim/kemal/nuri/selim/rustem.png` | Desk (müteahhit, ClientSO.Portrait) | B | F (ChatGPT ile üretildi, arka planı silindi; 600x600 şeffaf, karakter alta ortalı, belden yukarısı) |
| `chair_back.png` | KULLANILMIYOR (yeni desk_bg'de koltuk var). Silinmedi, referans kırılmasın diye duruyor | B | — |
| `book_rules.png` | Desk: "EL KİTABI — Olması gereken değerler" (script ile sıfırdan çizildi, masada yatan 3B kitap, 640x320 şeffaf; gölge kodda), masada sağ altta HEP durur (tıklanınca El Kitabı penceresi) | B | F |
| `book_lab.png` | Desk: "LAB RAPORU — Beyan edilen değerler" (script ile sıfırdan çizildi, masada yatan halkalı klasör, 640x320 şeffaf; gölge kodda), müteahhitle GELİR, sol altta durur, kararla birlikte GİDER | B | F |

### Yeni masa arayüzü (17 Ekim ~15:30, Ahmet kararı: üst yarı ofis, alt yarı 3 sabit belge)
| Dosya | Ne | Kim kullanıyor | Durum |
|---|---|---|---|
| `office_bg.png` | Üst yarı ofis (1920x540, desk_bg'den kırpıldı; koltuk ortada). Portre bunun önüne | B | F |
| `desk_edge.png` | Düz ahşap masa kenarı (1920x110, şeffaf). Portrenin ÖNÜNE, ofisin alt kenarına bindirilir | B | F |
| `desk_surface.png` | Alt yarı masa yüzeyi, ahşap (1920x540) | B | F |
| `paper.png` | Eskimiş krem kağıt (600x800). **9-slice**, border 32 px: Image Type = Sliced | B | F |
| `icon_building/tube/book.png` | Belge başlık ikonları: form / lab raporu / El Kitabı (160x160 şeffaf) | B | F |
| `stamp_lab.png`, `stamp_municipal.png` | Kırmızı lab kaşesi ("AKREDİTE LABORATUVAR"), mavi belediye kaşesi ("BETONYA BELEDİYESİ · YAPI DENETİM"); 400x400 şeffaf | B | F |
| `bld_low/mid/high.png` | Formdaki bina resmi: 3 / 5 / 9 kat (öneri 1-3 / 4-6 / 7+ kat), 300x360 şeffaf | B | F |

**17 Ekim akşam görsel yenileme (ChatGPT, düzen aynı):** `office_bg`, `desk_surface`, `desk_edge`, `portrait_*` (artık 1000x1000, aynı yerleşim oranı), `menu_bg` aynı adlarla yenilendi. Yeniler:
| Dosya | Ne | Kim kullanıyor | Durum |
|---|---|---|---|
| `doc_form.png` | Yapı Denetim Formu: klasörlü, ataçlı boş kağıt (1065x1307 şeffaf); yazılar kodla | B | F |
| `doc_lab.png` | Laboratuvar Raporu: boş kağıt destesi (1057x1295) | B | F |
| `doc_handbook.png` | Denetçi El Kitabı: solda spiralli boş defter (1093x1279) | B | F |
| `tab_beton/donati/yapi/zemin.png` | **Artık kullanılmıyor** (poliş K3: BETON/DONATI/YAPI/ZEMİN sekmeleri kalktı). Silinmedi, prefab'da bağlı. Eskiden: El Kitabı sekmeleri (120x300) | B | — |
| `btn_approve.png`, `btn_reject.png` | Formdaki ONAY (yeşil ✓) / RED (kırmızı ✕) kaşe düğmeleri (440x150) | B | F |
| `btn_test.png` | Lab raporundaki mavi EK TEST düğmesi (420x120) | B | F |
| `btn_menu.png` | Ana menü ve akış ekranı düğmesi (poliş T1, 520x120: koyu ahşap kahve, ince damar, ikonsuz; ONAY/RED/EK TEST ailesi; yazı kodla). Eskisi menu_bg'deki tahtadan kesilmişti | C | F |
| `btn_inspect.png` | Poliş T1: İNCELE düğmesi (420x120, koyu kömür, solda büyüteç; yazı kodla). ONAY/RED/EK TEST ailesi | B | F |
| `btn_help.png` | Poliş T1: NASIL OYNANIR düğmesi (420x100, hardal, solda "?"; yazı kodla) | B | F |
| `hud_bar.png` | Poliş T1: üst bar şeridi (1920x88, koyu ahşap, alt kontur + gölge; 76 px pano + 12 px gölge) | C | F |
| `hud_tag.png` | Poliş T1: üst bar kağıt etiketi (256x72, krem, mürekkep kontur). **9-slice, Sprite Border 24** | C | F |
| `hud_icon_day/money/count.png` | Poliş T1: üst bar ikonları (64x64): takvim yaprağı, banknot + madeni para, klasör | C | F |
| `bribe_envelope_open.png` | Açık rüşvet zarfı + kurgusal banknotlar (1226x1193 şeffaf; ChatGPT, Tarık; arka planı A sildi). Zarfa tıklanınca BribeEnvelopeView'da; tutar kodla ön banknotun ovaline yazılır | B | F |
| `btn_wood.png` | Masa ahşabından (desk_surface) açık renkli düğme, 420x100; masadaki İNCELE düğmesinin ilk hali (şimdi btn_menu kullanıyor); şu an kullanılmıyor | B | F |
| `ui_checkbox.png`, `ui_check.png` | Ödeme ekranı onay kutusu (krem kutu, koyu kontur) ve yeşil tik (işaretli = ödenecek), 96x96 | C | F |
| `menu_lights_0..3.png` | Ana menü pencere ışıkları: menu_bg'deki sarı pencerelerden çıkarılmış 4 şeffaf katman (1920x1080; 0-1 "söner", 2-3 "parlar"). MenuAmbience yanıp söndürür | C | F |
| `menu_glow.png`, `menu_dust.png` | Ana menü güneş halesi (256 radyal) ve havadaki toz zerresi (32 radyal) | C | F |
| `menu_redlight.png`, `menu_load.png` | Ana menü vinç ikaz lambası (kırmızı radyal) ve sol vinçten sarkan çelik kiriş (halat + kanca + I-kiriş siluet, pivot üstte) | C | F |

Yazı tipleri `Assets/_Project/Fonts/`: başlık **Paytone One**, metin **Fira Sans** (Regular/Bold), TMP asset'leri `* SDF.asset` (Türkçe set önceden yüklü, Static).

Eski masa (desk_bg/desk_front/book_*) yeni arayüzde kullanılmıyor; referans kırılmasın diye silinmedi.

Durum: `-` yok, `P` placeholder, `F` final.
