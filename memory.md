# memory.md — Projenin ortak hafızası

## 1. Proje özeti
- CIVIL GAME JAM (ODTÜ KKK, KALTEV), 48 saat: 16 Ekim 2026 18:00 → 18 Ekim 2026 18:00 (UTC+3). Tema: "Afete Hazırlık ve Farkındalık".
- Unity 6000.6.4f1, 2D, uGUI + TextMeshPro, hedef platform Windows, 1920x1080. Kontrol sadece fare, Esc menü açar.
- Oyun (çalışma adı "İmza"): masa başı karar oyunu. Oyuncu bina onaylayan inşaat mühendisi.
  - Masaya proje ve laboratuvar belgeleri gelir; oyuncu onaylar veya reddeder; rüşvet teklif eden müteahhitler olur.
  - Onaylanan binalar yapılır → gün sonu afet kontrolü → afet varsa gazete/sonuç, yoksa gün özeti → haftalık ödeme günüyse hesaplaşma → gün sonu kayıt.
- Jüri ölçütleri: Oynanabilirlik 25, Temayla ilişki 25, Yaratıcılık 20, Görsel/işitsel bütünlük 15, Teknik bütünlük 15.
- Hassasiyet: Deprem Türkiye'de hassas bir konu.
  - Depremzedeler asla olumsuz gösterilmez; isimler tamamen kurgusal; gerçek olaylara doğrudan atıf yok.
  - Mizah karakterlerde; sonlar ve deprem sonrası ciddi ve duygusal.

## 2. Kesinleşen oyun kararları
- Level sistemi kaldırıldı; oyun gün sistemiyle ilerleyecek. Hâlâ plan aşamasındayız; aşağıdaki yeni akış kodda uygulanmış kabul edilmez.
- Planlanan akış: DayIntro → Desk → Construction → afet kontrolü → (afet varsa Quake/Result, yoksa DaySummary) → ödeme günüyse WeeklyExpenses → gün sonu Save → (NextDay | Ending).
- Her gün sonunda afet olasılığı vardır. Afet olmazsa bir sonraki gün olasılık artar. Başlangıç oranı, artış, üst sınır ve afet sonrası sıfırlama henüz kararlaştırılmadı.
- Eski level başına proje sayıları geçersizdir. Toplam gün sayısı, günlük proje sayıları ve içerik açılma günleri henüz belirlenmedi. Konut, sahte belgeler, fabrika/yangın içerik hedefleri; sel ve okul/hastane ek kapsamdır.
- Deprem şiddeti oyuncuya gösterilmez. Bina sadece 2 durumlu: ayakta / yıkık.
- Yıkılma kuralı (EarthquakeSimulator):
  - Strength = min(Actual/Required).
  - Strength >= 1 ise ASLA yıkılmaz (dürüst yol garantisi).
  - Aksi halde Strength + gürültü(±0.15) < Intensity ise yıkılır. Intensity yapılandırılmış afet aralığından çekilir, max <= 1.0.
- Rüşvet teklif eden her proje uygunsuzdur. Bazı uygunsuz projeler rüşvetsiz, sahte belgeyle temiz görünür; ek testle yakalanır. Dürüst usta = rüşvetsiz gelen gerçekten uygun proje.
- İki katmanlı sahte belge: (1) bedava ipuçları (parsel/tarih/lab/kaşe/imza tutarsızlığı), (2) ücretli ek test (Reported vs Actual farkını açar).
- Hapis: kalıcı JailRisk. Yıkım varsa zar atılır.
  - Önceki denge: yıkılan bina +0.15, rüşvetle onaylanan yıkılan bina +0.25. Eski temiz level -0.05 kuralının gün sistemindeki karşılığı açık; afetsiz günün riski azaltacağı varsayılmamalı.
  - İlk yıkımda bile hapis olasılığı var. Tüm sayılar BalanceSO'da.
- Eski level tekrar kuralı kaldırıldı; başarısız gün sonrası devam/tekrar davranışı henüz kesinleşmedi.
- Giderler haftada bir ödenecek. Kalemler: kira, kredi taksiti, ek test, sigorta, kabadayı cam tamiri. Ek testin anında tahsil edilmesi ve sigortanın kapsam başlangıcı henüz netleştirilmeli. Ödenmeyen kira/kredi borç biriktirir; borç limiti aşılırsa ofis kapanır (orta son).
- Kota: eski level kotasının günlük/haftalık karşılığı ve eksik kalmasının sonucu henüz belirlenmedi.
- Save/Load: kayıt gün sonunda yapılacak; ay sonu kaydı kaldırıldı. Tek slot JSON / persistentDataPath yaklaşımı korunuyor. GameState gün, para, borç ve afet olasılığı gibi devam için gereken durumu kapsamalı; kesin kayıt şeması A ile netleştirilecek. Gün içi manuel kayıt kararı yok.
- Sonlar:
  - İyi: gün sistemindeki kesin bitiş koşulu henüz belirlenecek (eski koşul son level yıkımsızdı).
  - Kötü: mahkeme/hapis, kürsüde kurgusal isimler. Rüşvetli yıkımın doğrudan son mu yoksa JailRisk üzerinden mi işleyeceği netleştirilmeli.
  - Orta: borçtan ofis kapanışı; kota kaynaklı sonun koşulu açık.
- Yıkım nedeni kartları elle yazılmaz; sonuç ekranı FailedParams / Actual / Required değerlerinden otomatik üretilir (şablon cümle).
- Sadece Türkçe. Jüri demosu için gizli gün atlama kısayolu sonra eklenecek.

## 3. Mimari
- Tek `Game` sahnesi + ekranlar prefab. Diğer sahneler: MainMenu, Sandbox_Desk, Sandbox_Flow.
- Oyun mantığı saf C# (MonoBehaviour yok): üretim, deprem, ekonomi.
- Veri:
  - ScriptableObject: DayConfigSO (eski plan adı LevelConfigSO), RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO.
  - Runtime plain C#: ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult, WeeklyExpenseResult (eski plan adı MonthEndResult). Bu adlar hedef mimaridir; kod yeniden adlandırılmadı.
- Ekranlar IGameScreen uygular: Show(GameContext), Hide(), event Completed. GameManager sadece state değiştirir.
- Assembly'ler: CivilJam.Core, CivilJam.Desk, CivilJam.Flow. Desk ve Flow sadece Core'a bağlı, birbirini görmez.

### Dosya haritası
- `Assets/_Project/Scripts/Core/` — önceki notta repoda olmadığı belirtilmişti; bu güncellemede yerel/remote durum yeniden doğrulanmadı. Dosyalar push'lanınca buraya tek satırlık açıklamalarla eklenecek.

## 4. Takım ve klasör sahipliği
- A (Core) — Ahmet İlman: Scripts/Core, Data.
- B (Masa): Scripts/Desk, Prefabs/Desk, Scenes/Sandbox_Desk.
- C (Akış) — Onur Kabadayılar: Scripts/Flow, Prefabs/Flow, Scenes/Sandbox_Flow, MainMenu.
- 4. kişi (EEE): Art, Audio, SO içerikleri, CREDITS_AND_AI.md.
- Scenes/Game.unity sadece A.
- Onur’un kişisel C rolü planı: `ONUR.md`. Ortak kararların kaynağı bu dosyadır; kişisel plan ortak kararları değiştirmez.
- Kural: kimse başkasının sahne/prefab dosyasını açıp kaydetmez. Saatte bir main'i çek.

## 5. Jam kuralları (ihlal = diskalifiye)
- Repo PUBLIC olmalı (private kalırsa puandan bağımsız diskalifiye). Build ile repo kodu uyumlu olmalı.
- Her hazır asset ve AI kullanımı eklendiği anda CREDITS_AND_AI.md'ye yazılır (görsel/ses/kod/metin/diğer, küçük kullanım dahil). Redistribute edilemeyen asset'ler repoya konmaz.
- Jam öncesi hazırlanmış kod veya asset commit'lenmez.
- Teslim: Windows build ZIP + en fazla 1 dk video (MP4/AVI) + public repo + kurulum/kontrol talimatı.
- Sınırlar: yeni teslim başlatma 18 Ekim 18:00; Git desteği 18 Ekim 12:00; planlanan feature freeze 18 Ekim 12:00.

## 6. Yol haritası ve durum
- Cum 20-22: kurulum + veri sözleşmesi
- Cum 22-02: dikey dilim v0
- Cts 08-14: ekonomi / rüşvet / ek test / sahte belge içerikleri
- Cts 14-20: fabrika-yangın / 3 son / ses
- Cts 20-22: MVP kontrolü
- Cts gece - Paz sabah: denge + cila
- Paz 12:00: freeze. Paz 15-16:30: yükleme. 16:30-18:00: tampon.

### Durum
- **Plan durumu:** Gün sistemi tasarımı sürüyor. Bu güncellemede kod yazılmadı.
- **Önceki durum notu (bu oturumda doğrulanmadı):** Core veri sözleşmesi, SO'lar, EarthquakeSimulator, GameManager iskeleti, asmdef'ler tamamlandı olarak bildirilmişti.
- **Sırada:** ProjectGenerator v0, EconomyCalculator, RuleSetSO/DayConfigSO asset'leri; gün/hafta/save veri sözleşmesinin kesinleştirilmesi.
- **Bilinen sorunlar:**
  - Önceki notta Core dosyalarının push beklediği belirtilmişti; güncel durum A ile doğrulanmalı.

### Açık kararlar (öneriler kesinleşmiş değildir)
- Toplam gün sayısı / günlük proje sayısı; 14 gün önerisi onaylanmadı.
- Afet olasılığının başlangıcı, artışı, tavanı ve afet sonrası davranışı.
- Önceki günlerin ayakta kalan binalarının sonraki afetlere dahil edilmesi.
- Başarısız gün, kota, sonlar ve JailRisk kurallarının gün sistemine uyarlanması.
- Haftalık ilk ödeme günü, ek test tahsilatı ve sigortanın kapsam dönemi.
- Save sonrası yüklemenin sonraki sabaha dönmesi ve kayıt şemasının kesinleştirilmesi.

## 7. Karar günlüğü
Eski satırlar tarihseldir; aşağıdaki 21:09 güncellemesi level/ay sonu kararlarının yerini alır.

| Tarih-saat | Karar | Gerekçe |
|---|---|---|
| 2026-10-16 | Oyun: "İmza", masa başı mühendis onay oyunu | Tema (afete hazırlık) ile doğrudan bağ, fareyle oynanabilir kapsam |
| 2026-10-16 | Level akışı LevelIntro→Desk→Construction→Quake→Result→MonthEnd | Tek döngü, ekran ekran ayrılabilir iş |
| 2026-10-16 | Strength >= 1 asla yıkılmaz; aksi halde gürültülü Intensity kıyası | Dürüst oyuncu cezalandırılmaz, belirsizlik korunur |
| 2026-10-16 | Deprem şiddeti gizli, bina 2 durumlu | Basit görsel, net sonuç |
| 2026-10-16 | İki katmanlı sahte belge (bedava ipucu + ücretli ek test) | Dikkat ödüllendirilir, ekonomiye bağlanır |
| 2026-10-16 | Kalıcı JailRisk, sayılar BalanceSO'da | Birikimli sonuç, kolay denge ayarı |
| 2026-10-16 | Retry'da yeni seed | Ezber yerine okuma |
| 2026-10-16 | Save: tek slot JSON, sadece ay sonundan sonra, sadece GameState | Minimum kapsam, tutarlı kayıt noktası |
| 2026-10-16 | 3 son: İyi / Kötü / Orta | Oyuncu seçimlerinin net karşılığı |
| 2026-10-16 | Yıkım nedeni metinleri otomatik şablondan | Elle içerik yükü yok |
| 2026-10-16 | Tek Game sahnesi + prefab ekranlar, IGameScreen | Merge çakışmasını azaltır |
| 2026-10-16 | Saf C# mantık; Core/Desk/Flow asmdef, Desk-Flow birbirini görmez | Paralel çalışma, test edilebilirlik |
| 2026-10-16 | Klasör sahipliği; Game.unity sadece A | Sahne/prefab çakışmalarını önler |
| 2026-10-16 | Sadece Türkçe; hassasiyet kuralları | Kapsam + konunun ağırlığı |
| 2026-10-16 21:09 (UTC+3) | Level kaldırıldı; gün sistemi. Afetsiz gün sonrası afet ihtimali artar | Onur tarafından kesinleştirildi; oranlar henüz açık |
| 2026-10-16 21:09 (UTC+3) | Giderler haftalık; kayıt gün sonunda | Ay sonu ödeme/kayıt akışının yerini alır |
| 2026-10-16 21:09 (UTC+3) | C rolü Onur Kabadayılar; kişisel çalışma planı ONUR.md | Plan aşamasında sorumluluk ve açık karar takibi |

## Güncelleme kuralı
- Önemli bir karar, mimari değişiklik, yeni kural veya tamamlanan büyük iş olduğunda ilgili bölümü güncelle ve karar günlüğüne tarih-saatle satır ekle.
- Dosyayı kısa tut; kod kopyalama, sadece karar ve yönlendirme yaz.
