# UrlShortener

Uzun linkleri kısaltan ve her kısa linkin kaç kez tıklandığını takip eden küçük bir servis. .NET 10 ile, Domain-Driven Design (DDD) yaklaşımıyla yazıldı.

```
https://github.com/dotnet/aspnetcore  →  http://localhost:5080/aspnet
```

## Hızlı başlangıç

Bilgisayarında [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) kurulu olmalı. macOS'ta `brew install --cask dotnet-sdk` ile kurabilirsin.

Projeyi çalıştır:

```bash
dotnet watch --project src/UrlShortener.Api
```

`Now listening on: http://localhost:5080` yazısını görünce hazırsın. `dotnet watch`, kodu değiştirdiğinde sunucuyu kendiliğinden yeniden başlatır. Veritabanı (`urlshortener.db`) ilk açılışta otomatik oluşturulur.

İlk kısa linkini oluştur:

```bash
curl -X POST http://localhost:5080/api/short-urls -H "Content-Type: application/json" -d '{"url":"https://github.com/dotnet/aspnetcore","customCode":"aspnet"}'
```

Sonra tarayıcıda `http://localhost:5080/aspnet` adresini aç. GitHub'a gideceksin.

## Neler yapabilirsin

| Ne yapmak istiyorsun? | İstek |
|---|---|
| Link kısalt | `POST /api/short-urls` |
| Tüm linklerini gör | `GET /api/short-urls` |
| Bir linkin detayına ve tıklanma sayısına bak | `GET /api/short-urls/{code}` |
| Bir linkin ne zaman tıklandığını gör | `GET /api/short-urls/{code}/visits` |
| Bir linki kapat | `DELETE /api/short-urls/{code}` |
| Kısa linke git | `GET /{code}` |

Bu isteklerin hepsini hazır olarak iki yerde bulabilirsin:

- **Postman:** [`postman/UrlShortener.postman_collection.json`](postman/UrlShortener.postman_collection.json) dosyasını Postman'de **Import** ile içe aktar. Bir link oluşturduğunda kodu otomatik hatırlar, diğer istekleri hiçbir şey değiştirmeden gönderebilirsin.
- **VS Code / Rider:** [`UrlShortener.Api.http`](src/UrlShortener.Api/UrlShortener.Api.http) dosyasını aç, isteğin üstündeki **Send Request**'e tıkla. VS Code'da REST Client eklentisi gerekir.

### Bilmende fayda var

- **Kendi kodunu seçebilirsin.** `customCode` vermezsen `aZ3k9Qx` gibi rastgele bir kod üretilir. Kod 4–32 karakter olmalı ve yalnızca harf, rakam, `-` ve `_` içermeli.
- **Aynı adresi tekrar kısaltırsan yeni bir link alırsın.** Her linkin tıklanma sayısı ayrı tutulur. Aynı sayfayı farklı yerlerde paylaşıp hangisinin daha çok tıklandığını görmek istersen işine yarar.
- **Linke son kullanma tarihi koyabilirsin** (`expiresAtUtc`). Süresi dolan ya da kapatılan link artık yönlendirme yapmaz, `410 Gone` döner.
- **Saatler UTC.** Türkiye saati için 3 saat ekle.
- **Listeler sayfa sayfa gelir.** Varsayılan olarak sayfa başına 20 kayıt döner. `?page=2&pageSize=50` gibi değiştirebilirsin, sayfa başına en fazla 100.
- **Hatalar anlaşılır döner.** Geçersiz bir istekte (`400`), bulunamayan bir linkte (`404`) ya da alınmış bir kodda (`409`) yanıtta ne olduğunu açıklayan bir mesaj bulunur.

## Proje nasıl düzenlendi

Proje, her birinin tek bir işi olan dört parçaya bölündü:

```
src/
├── UrlShortener.Domain          İş kuralları: "süresi dolmuş link yönlendirmez" gibi
├── UrlShortener.Application     Kullanıcının yapabildiği işler: link oluştur, listele, kapat
├── UrlShortener.Infrastructure  Veritabanı ve diğer teknik detaylar
└── UrlShortener.Api             Dışarıya açılan HTTP uç noktaları
```

Tek bir kural var: **içteki parça dıştakini bilmez.** Domain, veritabanının ya da HTTP'nin varlığından habersizdir. Bu sayede iş kuralları tek bir yerde toplanır ve veritabanı açmadan test edilebilir.

### Ortak dil

DDD'de kodda, testlerde ve konuşurken aynı kavramlar için aynı kelimeler kullanılır:

| Kavram | Anlamı |
|---|---|
| **Short URL** (`ShortUrl`) | Bir kısa kodun bir adrese yönlendirmesi |
| **Short code** (`ShortCode`) | Linki tanımlayan kısa anahtar, örneğin `aspnet` |
| **Visit** (`Visit`) | Birinin kısa linke tıkladığı an |
| **Resolve** | Kısa kodu gerçek adrese çevirip ziyareti kaydetmek |
| **Expired** | Son kullanma tarihi geçmiş link |
| **Deactivate** | Linki kalıcı olarak kapatmak |

## Neden böyle tasarlandı

**Kurallar, ilgili nesnenin içinde yaşar.** Geçersiz bir `ShortCode` oluşturulamaz, çünkü doğrulama nesnenin kendisinde yapılır. `ShortUrl`'ün durumu da dışarıdan değiştirilemez. Değişiklik yalnızca `Deactivate` gibi kurallarını kendisi kontrol eden metodlarla yapılabilir. Böylece "pasif bir linki yanlışlıkla canlandırmak" gibi bir hata yapılamaz.

**Ziyaretler linkten ayrı tutulur.** Her tıklama ayrı bir `Visit` kaydı olarak eklenir, linkin kendi kaydı hiç güncellenmez. Böylece aynı anda gelen tıklamalar birbirini ezmez. Sayaç linkin içinde tutulduğunda aynı anda gelen 100 tıklamanın yalnızca 11'i sayılıyordu. Bu yapıyla 100'ün 100'ü sayılıyor.

**Kod benzersizliği özel bir serviste kontrol edilir.** "Bir kod yalnızca bir linke ait olabilir" kuralını tek bir link kendi başına kontrol edemez, çünkü diğer linkleri bilmez. Bu iş `ShortCodeAllocator`'da yapılıyor. İki istek aynı anda aynı kodu isterse veritabanı da ikincisini reddediyor.

**Beklenen hatalar exception değil, sonuç olarak döner.** "Bu kod alınmış" ya da "link süresi dolmuş" durumları hata değil, olağan senaryolar. Bu yüzden exception fırlatmak yerine `Result` ile dönüyorlar. API bunları doğru HTTP koduna çeviriyor.

**Yönlendirme `302` ile yapılır, `301` ile değil.** `301` kullanılsaydı tarayıcı yönlendirmeyi hafızaya alır ve sonraki tıklamalar sunucuya hiç ulaşmazdı. O zaman da sayılamazlardı.

## Testler

```bash
dotnet test
```

Üç test projesi var:

- **Domain testleri:** İş kurallarını test eder, örneğin "süresi dolmuş link ziyaret edilemez".
- **Application testleri:** Akışları test eder, örneğin "hata varsa hiçbir şey kaydedilmez".
- **Mimari testleri:** Yukarıdaki tasarım kurallarının zamanla bozulmadığını kontrol eder. Birisi Domain'e veritabanı kodu eklerse test kırılır.

Ayrıca .NET'in kod analiz kuralları açık ve uyarılar hata sayılıyor. Kötü pratik içeren kod derlenmez.
