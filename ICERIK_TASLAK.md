# ICERIK_TASLAK.md — SO metin taslakları (EEE)

Onaylanan metinler `Assets/_Project/Data/` altındaki SO asset'lerine taşınır. Bu dosya taslaktır; oyun buradan okumaz.

Kurallar: isimler tamamen kurgusal; mizah müteahhitlerde; oyuncuya "Hocam" denir (cinsiyetsiz); gerçek firma/olay çağrışımı yok.
Rüşvet tutarları öneri; denge A'da (BalanceSO ile birlikte).

## Müteahhitler (ClientSO)

### 1. Rıfat Usta — dürüst usta
- Id: `rifat` · IsHonest: ✅ · IsThug: ❌ · BribeRange: 0-0
- Selamlama:
  - "Kolay gelsin hocam. Demirini de betonunu da kendim kontrol ettim, siz de bakın, içiniz rahat etsin."
  - "Hocam acele yok. Eksik görürseniz söyleyin, düzeltir gelirim."
  - "Otuz yıldır bu işi yaparım hocam. Yaptığım binada kendi torunum oturuyor."
- Rüşvet: —

### 2. Sevim Sağlamer — dürüst usta
- Id: `sevim` · IsHonest: ✅ · IsThug: ❌ · BribeRange: 0-0
- Selamlama:
  - "Merhaba hocam. Laboratuvar raporları ekte, numuneleri de kendim teslim ettim."
  - "Biraz pahalıya çıktı ama içim rahat. Siz yine de inceleyin."
  - "Bu sefer okul yolunun üstünde hocam, ona göre sağlam yaptık."
- Rüşvet: —

### 3. Kemal Kestirme — rüşvetçi
- Id: `kemal` · IsHonest: ❌ · IsThug: ❌ · BribeRange: 800-1500
- Selamlama:
  - "Hocam selamlar! Kısa keseceğim, sizin de vaktiniz kıymetli."
  - "Evraklar tamam hocam, ufak tefek şeylere takılmayalım."
  - "Benim işler hep hızlı hocam, adım üstünde."
- Rüşvet:
  - "Bu zarfı masada unuttum galiba... Sizde kalsın, bir dahakine alırım."
  - "Hocam bu işler uzatılmaz. Kestirmeden gidelim, ikimiz de kazanalım."
  - "Çay parası diyelim hocam. Biraz kalabalık bir çay."

### 4. Nuri Göznuru — rüşvetçi
- Id: `nuri` · IsHonest: ❌ · IsThug: ❌ · BribeRange: 1000-2000
- Selamlama:
  - "Hocaaam! Sizi görmek ne güzel, maşallah yüzünüz nur gibi."
  - "Getirdiğim proje de en az siz kadar güzel hocam."
  - "Yeğenim sizi çok övdü hocam, dedim mutlaka uğrayayım."
- Rüşvet:
  - "Göz nuru olsun hocam, lafı mı olur."
  - "Bayram değil seyran değil, ama ben size bir hediye getirdim."
  - "Şimdi imza atarsanız zarf da sizin, hatır da sizin."

### 5. Selim Kâğıtçıoğlu — sahte belgeci (rüşvet teklif etmez, belgeleri kusursuz görünür)
- Id: `selim` · IsHonest: ❌ · IsThug: ❌ · BribeRange: 0-0
- Selamlama:
  - "İyi günler hocam. Bütün evraklar eksiksiz, laboratuvar raporu da taze."
  - "Kaşesi de imzası da yerinde hocam, buyurun inceleyin."
  - "Ek teste gerek yok hocam, raporlar her şeyi söylüyor zaten."
- Rüşvet: —
- Not: A onayladı: `IsHonest=false`, BribeRange 0-0 = asla rüşvet teklif etmez.

### 6. Balyoz Rüstem — kabadayı (reddedilirse cam kırar)
- Id: `rustem` · IsHonest: ❌ · IsThug: ✅ · BribeRange: 1500-2500
- Selamlama:
  - "Hocam selamünaleyküm. Ben uzatmayı sevmem."
  - "Projeyi getirdim. Siz de onaylayacaksınız, değil mi hocam?"
  - "Bu mahallede herkes beni tanır hocam. Siz de tanıyın istedim."
- Rüşvet:
  - "Al hocam bunu. Ofisinizin camları çok güzel bu arada... Kırılsa yazık olur."
  - "İmza atarsın, zarfı alırsın. Atmazsan... neyse, sen atarsın."
  - "Ben iyilikten anlarım hocam. Kötülükten de anlarım."

## Gazete manşetleri (HeadlineSO)

Ton: ciddi, sade, saygılı. Depremzedeler asla olumsuz gösterilmez. Gerçek olay, tarih veya şehir adı yok.
Manşette ölü/yaralı sayısı yok; sayılar ve isimler gazetenin gövdesinde (C) gösterilir. Manşet olayı ve sorumluluğu anlatır.

Yer tutucular:
- `{building}` = bina adı ("Papatya Apartmanı"). **Arkasına ek gelmez** (Türkçe ek uyumu bozulur: "Apartmanı'nın"); cümleler buna göre yazıldı.
- `{count}` = yıkılan **bina** sayısı (sadece MultiCollapse'ta). Ahmet'in örneğindeki "{count} can kaybı" kullanımı kaldırıldı.
- Not A/C'ye: can kaybı manşette istenirse ayrı bir `{deaths}` yer tutucusu eklenmeli, `{count}` ile karıştırılmamalı.

### NoCollapse — afet oldu, hiçbir bina yıkılmadı
- "Deprem korkuttu, binalar ayakta kaldı"
- "Sarsıntı geçti, şehir ayakta"
- "Mahalleli sokakta sabahladı: Yıkım yok"
- "Uzmanlar: Denetimli binalar sınavı geçti"
- "Derin bir nefes: Depremde can kaybı yaşanmadı"

### SingleCollapse — tek bina yıkıldı
- "Şehir bu sabah bir binasını kaybetti: {building}"
- "Mahalle yasta: {building} depreme dayanamadı"
- "Yıkılan bina: {building}. Sorumlular aranıyor"
- "Komşular anlatıyor: {building} bir anda çöktü"
- "Soruşturma başladı: {building} neden yıkıldı?"

### MultiCollapse — birden fazla bina yıkıldı
- "Depremde {count} bina yıkıldı"
- "Şehirde acı tablo: {count} bina çöktü"
- "{count} bina ayakta kalamadı"
- "Kara gün: {count} yapı yerle bir oldu"
- "Denetim sorusu: {count} binanın onayı incelenecek"

### BribedCollapse — rüşvetle onaylanan bina yıkıldı
- "Yıkılan binanın onayında usulsüzlük şüphesi"
- "Bu imza kimin?"
- "Belgeler sahte, imza gerçek"
- "Savcılık harekete geçti: Onay sürecinde rüşvet iddiası"
- "Müteahhit gözaltında, gözler onaylayan mühendiste"

## Sonlar (EndingSO)

Ton: ciddi ve duygusal; mizah yok. Oyuncuya "sen" denir. Gerçek olay/yer adı yok.
Görseller (EEE): `ending_good.png`, `ending_bad.png`, `ending_closed.png`.

### Good — İyi son
- Title: "Ayakta Kalanlar"
- Body:
  > Ofisinin kapısını son kez kilitledin.
  >
  > Masandan geçen her projeyi okudun. Eksik demiri, sahte raporu, masaya bırakılan zarfı geri çevirdin. Kimi gün kirayı zor ödedin, kimi gün kapında bir kabadayı bekledi.
  >
  > Ama yer sarsıldığında, imzanı taşıyan binaların hepsi ayaktaydı. İçlerinde uyuyan çocuklar ertesi sabah okula gitti.
  >
  > Gazeteler ayakta kalan binalar için manşet atmaz. Kimse adını bilmeyecek. Ama o binalarda yaşayan herkes, farkında olmadan, sana bir sabah borçlu.
  >
  > Deprem doğanın işidir. Binanın ayakta kalması ise bizim.

### Bad — Kötü son (mahkeme / hapis)
- Title: "Hüküm"
- Body:
  > Mahkeme salonu kalabalık. Ön sıralarda, yıkılan binada yakınlarını kaybedenler oturuyor. Ellerinde fotoğraflar var.
  >
  > Hâkim isimleri tek tek okuyor. Her isimde salon biraz daha sessizleşiyor.
  >
  > Savcı bir dosya kaldırıyor: ruhsat, laboratuvar raporu ve en altta senin imzan.
  >
  > "Bu belgeleri incelediniz mi?"
  >
  > Cevap veremiyorsun. Zarftaki para çoktan harcandı. O binada yaşayanlar ise geri gelmeyecek.
  >
  > Bir imza birkaç saniye sürer. Sonuçları bir ömür.
- Not A/C'ye: "Hâkim isimleri okuyor" satırının altında yıkılan binaların kurgusal kayıp isimleri (`BuildingOutcome.VictimNames`) gösterilebilir. Body sabit metin; isimler ekranda ayrı listelenir.

### MiddleClosed — Orta son (borçtan ofis kapandı)
- Title: "Kapanan Kapı"
- Body:
  > Kira yine ödenemedi. Bankadan son uyarı geldi. Bu sabah ofisinin kapısına bir kilit asıldı.
  >
  > Masadaki dosyaları kutuya koydun. Kaşeni, kalemini, yarım kalmış bir laboratuvar raporunu.
  >
  > Bu şehirde dürüst kalmanın bir bedeli var. Bazen o bedel ayın sonunu getirememek.
  >
  > İmzanı taşıyan binalar şehirde duruyor. Hangisinin bir sonraki depremde ayakta kalacağını artık sen değil, zaman gösterecek.
  >
  > Işığı kapattın.
