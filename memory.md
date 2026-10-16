# memory.md — Projenin ortak hafızası

## 1. Proje özeti
- CIVIL GAME JAM (ODTÜ KKK, KALTEV), 48 saat: 16 Ekim 2026 18:00 → 18 Ekim 2026 18:00 (UTC+3). Tema: "Afete Hazırlık ve Farkındalık".
- Unity 6000.6.4f1, 2D, uGUI + TextMeshPro, hedef platform Windows, 1920x1080. Kontrol sadece fare, Esc menü açar.
- Oyun (çalışma adı "İmza"): masa başı karar oyunu. Oyuncu bina onaylayan inşaat mühendisi.
  - Masaya proje ve laboratuvar belgeleri gelir; oyuncu onaylar veya reddeder; rüşvet teklif eden müteahhitler olur.
  - Onaylanan binalar yapılır → deprem → sonuç ekranı (gazete manşeti) → ay sonu ekranı (gider seçimi).
- Jüri ölçütleri: Oynanabilirlik 25, Temayla ilişki 25, Yaratıcılık 20, Görsel/işitsel bütünlük 15, Teknik bütünlük 15.
- Hassasiyet: Deprem Türkiye'de hassas bir konu.
  - Depremzedeler asla olumsuz gösterilmez; isimler tamamen kurgusal; gerçek olaylara doğrudan atıf yok.
  - Mizah karakterlerde; sonlar ve deprem sonrası ciddi ve duygusal.

## 2. Kesinleşen oyun kararları
- Level akışı: LevelIntro → Desk → Construction → Quake → Result → MonthEnd → (NextLevel | Retry | Ending)
- Level 1: 3 proje, sadece konut. Level 2: 5 proje, sahte belgeler + kabadayılar. Level 3: 6-7 proje, fabrika + yangın. Level 4 (sel) ve 5 (okul/hastane) sadece vakit kalırsa.
- Deprem şiddeti oyuncuya gösterilmez. Bina sadece 2 durumlu: ayakta / yıkık.
- Yıkılma kuralı (EarthquakeSimulator):
  - Strength = min(Actual/Required).
  - Strength >= 1 ise ASLA yıkılmaz (dürüst yol garantisi).
  - Aksi halde Strength + gürültü(±0.15) < Intensity ise yıkılır. Intensity level aralığından çekilir, max <= 1.0.
- Rüşvet teklif eden her proje uygunsuzdur. Bazı uygunsuz projeler rüşvetsiz, sahte belgeyle temiz görünür; ek testle yakalanır. Dürüst usta = rüşvetsiz gelen gerçekten uygun proje.
- İki katmanlı sahte belge: (1) bedava ipuçları (parsel/tarih/lab/kaşe/imza tutarsızlığı), (2) ücretli ek test (Reported vs Actual farkını açar).
- Hapis: kalıcı JailRisk. Yıkım varsa zar atılır.
  - Yıkılan bina +0.15, rüşvetle onaylanan yıkılan bina +0.25, temiz level -0.05.
  - İlk yıkımda bile hapis olasılığı var. Tüm sayılar BalanceSO'da.
- Retry: her denemede yeni seed, yeni projeler.
- Giderler: kira, kredi taksiti, ek test, sigorta, kabadayı cam tamiri. Ödenmeyen kira/kredi borç biriktirir; borç limiti aşılırsa ofis kapanır (orta son).
- Kota: level'ı geçmek için en az X proje onaylanmalı.
- Save/Load: tek slot, JSON, persistentDataPath, sadece ay sonu ekranından sonra, sadece GameState kaydedilir.
- Sonlar:
  - İyi: son level yıkımsız.
  - Kötü: rüşvetli bina yıkıldı → mahkeme/hapis, kürsüde kurgusal isimler.
  - Orta: kota tutmadı veya borçtan ofis kapandı.
- Yıkım nedeni kartları elle yazılmaz; sonuç ekranı FailedParams / Actual / Required değerlerinden otomatik üretilir (şablon cümle).
- Sadece Türkçe. Jüri demosu için gizli level atlama kısayolu sonra eklenecek.

## 3. Mimari
- Tek `Game` sahnesi + ekranlar prefab. Diğer sahneler: MainMenu, Sandbox_Desk, Sandbox_Flow.
- Oyun mantığı saf C# (MonoBehaviour yok): üretim, deprem, ekonomi.
- Veri:
  - ScriptableObject: LevelConfigSO, RuleSetSO, ClientSO, ExpenseSO, BalanceSO, HeadlineSO, EndingSO.
  - Runtime plain C#: ProjectData, DocumentData, ParamEntry, GameState, GameContext, QuakeResult, MonthEndResult.
- Ekranlar IGameScreen uygular: Show(GameContext), Hide(), event Completed. GameManager sadece state değiştirir.
- Assembly'ler: CivilJam.Core, CivilJam.Desk, CivilJam.Flow. Desk ve Flow sadece Core'a bağlı, birbirini görmez.

### Dosya haritası
- `Assets/_Project/Scripts/Core/` — henüz repoda yok (16 Ekim 2026 itibarıyla ne yerelde ne origin/main'de). Dosyalar push'lanınca buraya tek satırlık açıklamalarla eklenecek.

## 4. Takım ve klasör sahipliği
- A (Core) — Ahmet İlman: Scripts/Core, Data.
- B (Masa): Scripts/Desk, Prefabs/Desk, Scenes/Sandbox_Desk.
- C (Akış): Scripts/Flow, Prefabs/Flow, Scenes/Sandbox_Flow, MainMenu.
- 4. kişi (EEE): Art, Audio, SO içerikleri, CREDITS_AND_AI.md.
- Scenes/Game.unity sadece A.
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
- Cts 08-14: ekonomi / rüşvet / ek test / Level 2
- Cts 14-20: Level 3 / 3 son / ses
- Cts 20-22: MVP kontrolü
- Cts gece - Paz sabah: denge + cila
- Paz 12:00: freeze. Paz 15-16:30: yükleme. 16:30-18:00: tampon.

### Durum
- **Tamamlananlar:** Core veri sözleşmesi, SO'lar, EarthquakeSimulator, GameManager iskeleti, asmdef'ler.
- **Sırada:** ProjectGenerator v0, EconomyCalculator, RuleSetSO/LevelConfigSO asset'leri.
- **Bilinen sorunlar:**
  - Tamamlanan Core dosyaları repoda görünmüyor (sadece "Initial check-in" commit'i var). A push'lamalı; sonra Dosya haritası doldurulacak.

## 7. Karar günlüğü
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

## Güncelleme kuralı
- Önemli bir karar, mimari değişiklik, yeni kural veya tamamlanan büyük iş olduğunda ilgili bölümü güncelle ve karar günlüğüne tarih-saatle satır ekle.
- Dosyayı kısa tut; kod kopyalama, sadece karar ve yönlendirme yaz.
