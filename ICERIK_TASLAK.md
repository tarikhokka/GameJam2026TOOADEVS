# ICERIK_TASLAK.md — SO metin taslakları (EEE)

Onaylanan metinler `Assets/_Project/Data/` altındaki SO asset'lerine taşınır. Bu dosya taslaktır; oyun buradan okumaz.

Kurallar: isimler tamamen kurgusal; mizah müteahhitlerde; oyuncuya "Hocam" denir (cinsiyetsiz); gerçek firma/olay çağrışımı yok.
Rüşvet tutarları öneri; denge A'da (BalanceSO ile birlikte).

## Müteahhitler (ClientSO)

> **17 Ekim akşam (EEE):** replikler genişletildi (selamlama 8, rüşvet 5, onay 6, ret 6, zarf geri 5, tekrar geliş 5; karakter sayfalarındaki cümleler de eklendi). **Güncel liste `Data/Clients/*.asset`'te**; aşağısı ilk taslaktır.

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

## Tepki replikleri (Ç4) — A taslağı, Tarık'tan devralındı

Ahmet (A) Tarık meşgulken yazdı; **Tarık istediği gibi düzeltebilir.** Oyun metni `Data/Clients/*.asset` içinden okur;
bu bölüm okunur kopyadır. Asset'e yazan araç (`CivilJam > Core > Fill Empty Client Lines`) sadece **boş** listeleri doldurur,
yani asset'te yapılan düzeltmelerin üzerine yazmaz. Düzeltmeyi doğrudan asset'te (Inspector) yap; bu bölümü de güncelle.
Dürüst ustalar (Rıfat, Sevim) ve Selim rüşvet vermediği için onlarda "zarf geri çevrildi" repliği yok.

### Rıfat Usta
- Onaylanınca (`ApproveLines`):
  - "Allah razı olsun hocam. Temeline kendim baktım, gönül rahatlığıyla yükselir."
  - "Sağ olun hocam. Bitince çaya beklerim, bir de siz görün."
  - "Eksiksiz çıkmasına sevindim hocam. Usta işi böyle olur."
- Reddedilince (`RejectLines`):
  - "Haklısınız hocam, gözümden kaçmış. Düzeltip yeniden getiririm."
  - "Otuz yılda pek başıma gelmezdi... Peki hocam, eksiği tamamlarım."
  - "Kızamam size hocam. Siz bakmasanız kim bakacak?"
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "Yine ben geldim hocam. Geçen sefer projemi {last}; bunu daha da dikkatli hazırladım."
  - "Hocam hatırlarsınız, geçen sefer {last}. Bu dosyayı da aynı titizlikle getirdim."

### Sevim Sağlamer
- Onaylanınca (`ApproveLines`):
  - "Teşekkürler hocam. Raporların hepsini sakladım, ne zaman isterseniz bakarız."
  - "İçim rahat hocam. Çocuklar o sokaktan güvenle geçecek."
  - "Sağ olun. Bu işi sizin gibi yapan pek kalmadı."
- Reddedilince (`RejectLines`):
  - "Anlıyorum hocam. Laboratuvarla tekrar konuşur, eksiği kapatırım."
  - "Bir yerde hata yapmışım demek. İyi ki yakaladınız."
  - "Peki hocam. Pahalıya patlayacak ama doğrusu bu."
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "Merhaba yine hocam. Geçen sefer {last}; bu dosyada her satırı iki kez kontrol ettim."
  - "Hocam, geçen sefer {last}. Bu sefer de numuneleri kendim teslim ettim."

### Kemal Kestirme
- Onaylanınca (`ApproveLines`):
  - "İşte bu hocam! Kısa ve net, tam benim sevdiğim gibi."
  - "Hayırlı olsun hocam. Temeli yarın sabah atıyoruz."
  - "Görüyor musunuz, uzatmadık, herkes kazandı."
- Reddedilince (`RejectLines`):
  - "Hocam bu kadar uzatmaya ne gerek vardı?"
  - "Peki peki... Bu şehirde başka kapı da var."
  - "Vaktimi yediniz hocam, vaktimi."
- Zarf geri çevrilip reddedilince (`BribeRefusedLines`):
  - "Zarfı geri mi veriyorsunuz? Hocam, kimse bu kadar kitabına uygun çalışmıyor."
  - "Ayıp oluyor ama hocam... Ben iyilik yapayım dedim."
  - "Siz bilirsiniz. Ama bu zarf başka masada kalmaz."
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "Hocam ben yine geldim, kestirmeden. Geçen sefer {last}, bu sefer de uzatmayalım."
  - "Hatırladınız mı beni? Geçen sefer {last}. Bugün daha hızlı bitiririz."

### Nuri Göznuru
- Onaylanınca (`ApproveLines`):
  - "Maşallah hocam, imzanız da yüzünüz kadar güzel!"
  - "Allah sizi başımızdan eksik etmesin hocam."
  - "Yeğenime söyleyeceğim, hocamız yine yaptı diye."
- Reddedilince (`RejectLines`):
  - "Aaa hocam, yüreğim cız etti."
  - "Olur mu böyle... Peki, hatırınız kalsın."
  - "Kırıldım ama küsmem hocam, yine gelirim."
- Zarf geri çevrilip reddedilince (`BribeRefusedLines`):
  - "Hocam zarfı beğenmediniz galiba. Bir dahakine daha dolgun getiririm."
  - "Göz nurumu geri çevirdiniz... Olsun, ben ısrarcı adamım."
  - "Az geldi demek. Anladım hocam, anladım."
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "Hocaaam! Geçen sefer {last} ama ben size hiç küsmedim."
  - "Yine ben, göz nurunuz! Geçen sefer {last}; bu sefer gönlünüzü hoş edeceğim."

### Selim Kâğıtçıoğlu
- Onaylanınca (`ApproveLines`):
  - "Teşekkürler hocam. Evrak dediğin böyle olur."
  - "Bilirdim. Kaşesi, imzası, hepsi yerinde."
  - "İyi çalışmalar hocam. Arşivinize düzgün bir dosya daha kattık."
- Reddedilince (`RejectLines`):
  - "Hangi belgede sorun gördünüz hocam? Ben göremedim."
  - "İlginç... Laboratuvara bir daha soracağım."
  - "Peki hocam. Kâğıtlar tamamdı ama siz bilirsiniz."
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "İyi günler hocam. Geçen sefer {last}; bu dosya ondan da düzgün."
  - "Yine ben hocam. Geçen sefer {last}. Evraklar her zamanki gibi eksiksiz."

### Balyoz Rüstem
- Onaylanınca (`ApproveLines`):
  - "Gördün mü hocam, ne güzel anlaştık."
  - "Akıllı adamsın hocam. Camların da sağlam kalır."
  - "Bu iyiliği unutmam hocam."
- Reddedilince (`RejectLines`):
  - "Yanlış yaptın hocam. Çok yanlış yaptın."
  - "Bu mahallede böyle iş olmaz hocam... Görürsün."
  - "Peki hocam. Akşama ofisin camlarına bir bak."
- Zarf geri çevrilip reddedilince (`BribeRefusedLines`):
  - "Zarfımı geri mi itiyorsun? Bunu unutmam hocam."
  - "Sen bilirsin. Ben kibarca teklif ettim."
- Tekrar gelince (`ReturnLines`; {last} = onaylamıştınız / reddetmiştiniz):
  - "Yine ben hocam. Geçen sefer {last}; bunu ikimiz de hatırlıyoruz."
  - "Hocam, geçen sefer {last}. Bugün nasıl biteceği sana bağlı."

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
Görseller (EEE): `ending_good.png`, `ending_bad.png`, `ending_closed.png`, `ending_collapsed.png`.
Kural (güncel, 17 Ekim): metinler sonun kod koşuluyla çelişmez. İyi son = 10. gün bitti ve **hiç bina yıkılmadı** ("binaların hepsi ayaktaydı" denebilir). Kötü son rüşvet varsaymaz (rüşvetsiz yıkımla da hapis çıkabilir). Enkazın Gölgesi = yıkım oldu, hapis yok. Kapanan Kapı = borç 2.000 TL'yi aştı.
Esas kaynak: `Data/Endings/*.asset`. Bu dosya taslak/arşivdir; düzeltme önce asset'te yapılır, sonra buraya işlenir.

### Good — İyi son
- Title: "Ayakta Kalanlar"
- Body:
  > Ofisinin kapısını son kez kilitledin.
  >
  > Günler boyunca masandan geçen her dosyaya bir karar verdin. Masaya bırakılan zarflar oldu, kapında bekleyen kabadayılar oldu. Kimi gün kirayı zor ödedin.
  >
  > İmzanı taşıyan ve hâlâ ayakta duran her binada şimdi insanlar uyuyor, çocuklar büyüyor.
  >
  > Gazeteler ayakta kalan binalar için manşet atmaz. Kimse adını bilmeyecek. Ama doğru atılmış her imza, birinin ertesi sabah uyanması demek.
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
  > Cevap veremiyorsun. O dosyaya bir kez daha bakabilirdin. O binada yaşayanlar ise geri gelmeyecek.
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
  > Bu şehirde bir mühendis olarak ayakta kalmak kolay değil. Kira, kredi, borç; hepsi aynı masada, imzanın yanında bekliyor.
  >
  > İmzanı taşıyan binalar şehirde duruyor. Hangisinin bir sonraki depremde ayakta kalacağını artık sen değil, zaman gösterecek.
  >
  > Işığı kapattın.

### MiddleCollapsed — Orta son (yıkım oldu, hapis yok)
- Title: "Enkazın Gölgesi" (taslak: A, son hali: EEE)
- Koşul: son gün bitti, en az bir imzalı bina yıkıldı, hapis çıkmadı. 1 ya da çok bina yıkılmış olabilir → metin sayı varsaymaz.
- Body:
  > Kimse kapını çalmadı. Soruşturma dosyası kapandı, adın gazetelerde geçmedi.
  >
  > Ama imzanı taşıyan binaların hepsi ayakta değil. Bir zamanlar insanların yaşadığı yerde şimdi boş bir arsa ve her sabah önünden geçtiğin bir çit var. Orada yaşayanların adlarını gazetede okudun.
  >
  > O gün masandan geçen dosyayı hâlâ hatırlıyorsun. Bir satırı daha dikkatli okusaydın ne değişirdi, bilmiyorsun.
  >
  > Mahkeme seni yargılamadı. Bu soruyu artık her gün kendin soruyorsun.

## El Kitabı sekme notları (RulebookView.sectionNotes) — EEE taslağı, 17 Ekim akşam
Her bölüm sekmesinin altında görünen kısa afet farkındalık cümlesi. Doğru, sade, korkutmayan; oyuncuya "neden bu kural var" dedirtsin. Oğuzhan Inspector'dan `sectionNotes`'a aynen koyar.

- **BETON:** Betonun sınıfı ne kadar yük taşıyabileceğini gösterir. Şantiyede betona su katmak işi kolaylaştırır ama betonu zayıflatır.
- **DONATI:** Depremde binanın kırılmadan esnemesini betonun içindeki demir sağlar. Nervürlü demir betona sıkı tutunur, düz demir kayabilir.
- **YAPI:** Kolonlar binanın bütün yükünü zemine taşır. Kat arttıkça kolon kalınlaşmalı; kolon kesmek ya da taşıyıcı duvarı yıkmak binayı zayıflatır.
- **ZEMİN:** Aynı bina yumuşak zeminde sert zemine göre daha çok sallanır. Temel zemine göre tasarlanır; zemin etüdü yapılmadan inşaata başlanmaz.
