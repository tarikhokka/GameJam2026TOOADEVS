# 7 günlük yapı — Onur içerik teslim planı

## Güncel durum

İncelenen Core sürümü: `e79203b` (Ahmet'in Core 2. adımı); teslim öncesinde `8981203` ayarlar menüsü commit'i de alındı. Yeni iş paketinde Onur, Tarık'a yazılmış içerik ve portre görevlerini de üstleniyor; Onur'un bildirimine göre ekip bu iş bölümünü biliyor. Önceki katkıların sahipliği değişmiyor.

Ahmet'in kararı: 7 gün, günlük 3 / 3 / 4 / 4 / 4 / 5 / 5 proje; toplam 28 ziyaret, 14 uygun ve 14 uygunsuz proje. Altı ana müteahhit üçer kez, on yeni karakter birer kez gelir. Zarf 2., sahte belge 3., kabadayı 4. günde açılır. Yüz, projenin uygunluğunu ele vermemeli.

Core sözleşmeleri ve ziyaret programı mevcut. Onur'un ChatGPT ile ürettiğini belirttiği son 10 JPEG, İndirilenler klasöründen alındı; orijinaller değiştirilmedi. On şeffaf 1000×1000 portre ve on Generic ClientSO hazırlandı. `Dialogue.asset` yedi havuzda 42 replik içeriyor; README 7 güne uyarlandı. Unity import ve gerçek kadro kontrolleri geçti.

Onur'un açık tercihi: 1. ve 4. görsellerdeki ikililer **birlikte gelen tek müşteri** olarak kalır. Bir görsel = bir ClientSO = tek ziyaret ve tek proje; kişiler ayrılmaz, ek oyun mantığı gerekmez. Hayvanlar da portrede korunur.

## Portre ve müşteri eşlemesi

| Id / dosya numarası | Görsel | DisplayName |
|---|---|---|
| g01 / Generic01 | İş kıyafetli iki ortak | Mahir ve Orhan Yaprak |
| g02 / Generic02 | Başörtülü, köpekli kadın | Nermin Güney |
| g03 / Generic03 | Reflektörlü yelekli, kedili kadın | Derya Aksu |
| g04 / Generic04 | Takım elbiseli iki ortak | Vedat ve İdil Saran |
| g05 / Generic05 | Bordo kapüşonlu erkek | Okan Yel |
| g06 / Generic06 | Kırmızı hırkalı kadın | Bahar İnce |
| g07 / Generic07 | Bordo montlu, bıyıklı erkek | Cemil Koral |
| g08 / Generic08 | Gözlüklü, hardal gömlekli erkek | Emre Duru |
| g09 / Generic09 | Kot ceketli kadın | Ece Soyer |
| g10 / Generic10 | Lacivert montlu, bıyıklı erkek | Turgut Erden |

İsimler kurgu amaçlı seçildi; karakterlere sabit iyi/kötü rolü atanmadı. İkililerde ortak repliklerden birini grubun temsilcisi söyler.

- Görsel kontrol: `Generic_portraits_onizleme.png` (arka planı koyu seçilerek şeffaflık ve saç araları kontrol edildi).
- Kaynak/output SHA256 ve dosya eşlemeleri: `Generic_portrait_manifest.json`.
- Yerel hazırlama aracı: `prepare_generic_portraits.py`; yeni çizim veya harici görsel API çağrısı yapmaz. JPEG'in beyaz arka planını, gözden geçirilmiş saç boşluklarını temizler; oranı koruyarak alta ortalar. Mevcut altı portreyle aynı 66 px üst boşluk ve 1000 px tuval kullanılır.

## Doğrulama ve kalan bağlantı

- İzole Unity 6000.6.4f1 kopyasında **54/54 EditMode geçti**: 52 mevcut Core testi ve yalnızca geçici kopyaya eklenen iki gerçek içerik kontrolü.
- Teslim tabanı `8981203` alındıktan sonra **54/54 EditMode ve 4/4 mevcut Flow PlayMode** tekrar geçti (Esc ve yeni ses ayarları dahil).
- Gerçek asset'lerden 16 benzersiz müşteri yükleniyor. On yeni sprite import oluyor; her biri 1000×1000, Sprite/Single, PPU 100, alfa açık ve doğru ClientSO'ya bağlı. Yedi replik havuzunun her birinde altı cümle var; bütün yeni müşteriler ortak havuza düşüyor.
- Gerçek kadroyla 300 yeni oyunda 28 proje, tam 14 uygunsuz, ana müşteriler üçer/yeni müşteriler birer kez; selamlamalar dolu; JSON roundtrip sonrası ziyaret programı korunuyor.
- Kaynak/output SHA256, alfa kanalı ve GUID benzersizliği ayrıca kontrol edildi. Önizleme koyu zeminde görsel olarak incelendi. Geçici denetim kodu proje kaynaklarına eklenmedi.
- Bu sonuçlar Game.unity'de yeni müşterilerle elle oynanış yapıldığı anlamına gelmez. **Ahmet Link Data ile yeni ClientSO'ları bağlamalı; Oğuzhan g01/g04 dahil masada kadrajı kontrol etmeli.** Son denge/build Ahmet'te.
- Denetim çıktısı: `/private/tmp/imza-generic-audit-nfzikfma/`; `editmode.xml`, `prepush-editmode.xml`, `prepush-playmode.xml` ve aynı adlı log dosyaları.

## Teslim sırası

1. **Ortak replikler:** `Assets/_Project/Data/Dialogue.asset` içindeki yedi havuza beş-altışar kısa cümle yaz. Alanlar: `GenericGreetingLines`, `GenericApproveLines`, `GenericRejectLines`, `GenericMismatchLines`, `GenericBribeLines`, `GenericBribeCloseLines`, `GenericBribeRefusedLines`. Toplam 35–42 cümle. Dil nötr esnaf dili; belirli teknik parametre, cinsiyet, özel geçmiş veya sabit para tutarı geçmesin. Selamlama projenin iyi/kötü olduğunu açıklamasın. Mevcut denetçi şablonları korunur.
2. **Portreler:** `Assets/_Project/Art/portrait_g01.png` … `portrait_g10.png`. Her biri ayrı 1000×1000 şeffaf PNG; mevcut altı portrenin çizgi/palet dilinde, belden yukarı ve alta ortalı. Kaynaklar Onur tarafından sağlandı, yeniden görsel üretimi gerekmedi. İki ikili aynı görselde tutuldu.
3. **Karakter dosyaları:** `Assets/_Project/Data/Clients/Generic01.asset` … `Generic10.asset`, her biri kendi `.meta` dosyasıyla. `Id`: `g01` … `g10`; benzersiz kurgusal ad soyad; `IsGeneric: true`, `IsHonest: false`, `IsThug: false`, `BribeRange: 0–0`. Zarf tutarı Core'un `GenericBribeRange` alanından gelir. `Portrait` ilgili sprite'a bağlı olur. Kendi replik listeleri boş bırakılır, ortak havuza düşer. Hediye ve ret sonrası zarf artışı eklenmez.
4. **Belgeler:** README'deki iki gün ifadesini 7'ye çevir; yeni karakter görsellerini Art README'ye kaydet; kullanılan gerçek araç ve dosyaları CREDITS'e ekle. Üretilmemiş görseller için tamamlandı/üretildi kaydı yazma.
5. **Unity kontrolü:** 6000.6.4f1 ile import; on PNG'nin boyutu, alfa kanalı, Sprite import'u ve GUID bağlantıları doğrulanır. On karakterin Id/ad/portre eşleşmesi ve yedi havuzun doluluğu kontrol edilir. İlgili Core EditMode testleri çalıştırılır. Görsellerin masadaki kadrajını Oğuzhan doğrular.
6. **Ahmet'e teslim:** kapsamı belirli commit; proje kaynakları, `.meta` ve CREDITS birlikte. Unity'nin yerel font/Packages/settings değişiklikleri ve mevcut genel analiz dosyası bu pakete alınmaz. Ahmet `Link Data` ile 16 müteahhidi Game.unity'ye bağlar; Game sahnesini biz kaydetmeyiz. Yeni oyunda 28 ziyaretin dağılımı ve save/load ile programın korunması doğrulanır. Program kayıtla saklandığı için eski kayıt yeni kadronun kabul testi olarak kullanılmaz. Son denge ve build Ahmet'te.

## Sorumluluklar

| Kişi | Bu paketteki sorumluluk |
|---|---|
| Onur | On portre, on ClientSO, yedi ortak replik havuzu, README/Art README/CREDITS ve içerik doğrulaması |
| Oğuzhan | Masa/öğreticide 10 → 7 metinleri ve yeni portrelerin yerleşimi |
| Ahmet | Core, ekonomi kararı, Game.unity Link Data, uçtan uca test ve build |

Eski Flow hata listesi ayrı takip edilir; bu yeni içerik paketinin tamamlanmış sayılması için ölçüt değildir. Portreler yetişmezse Core'un ana müteahhitlerle doldurma yedeği mevcut; yedeğin çalışması on yeni karakterin teslim edildiği anlamına gelmez.
