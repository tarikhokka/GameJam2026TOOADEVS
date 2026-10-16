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
- Not: Ahmet'e sor: rüşvetsiz uygunsuz müteahhit için `IsHonest=false`, BribeRange 0-0 doğru mu?

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
