# UrlShortener

Domain-Driven Design (DDD) ve Clean Architecture prensipleriyle yazılmış, .NET 10 tabanlı bir URL kısaltma servisi.

## Mimari

```
src/
├── UrlShortener.Domain          → İş kuralları. Hiçbir framework'e ve NuGet paketine bağımlı değil.
├── UrlShortener.Application     → Use case'ler (CQRS komut/sorgu handler'ları).
├── UrlShortener.Infrastructure  → EF Core + SQLite, repository'ler, okuma sorguları, kısa kod üretici.
└── UrlShortener.Api             → Minimal API endpoint'leri (sunum katmanı).
tests/
├── UrlShortener.Domain.UnitTests
├── UrlShortener.Application.UnitTests
└── UrlShortener.ArchitectureTests  → Katman ve DDD kurallarını her build'de doğrular.
```

Bağımlılıklar her zaman **içe doğru** akar:

```
Api ──► Infrastructure ──► Application ──► Domain
 └────────────────────────────┘
```

## Ubiquitous language (ortak dil)

Kodda, testlerde ve API'de aynı kavramlar aynı isimlerle kullanılır:

| Terim | Anlamı | Koddaki karşılığı |
|---|---|---|
| **Short URL** | Bir kısa kodun bir hedef adrese yönlendirmesi | `ShortUrl` (aggregate) |
| **Short code** | Linki tanımlayan benzersiz, URL-güvenli anahtar (`aZ3k9Qx`) | `ShortCode` (value object) |
| **Original URL** | Yönlendirilecek mutlak http(s) adresi | `OriginalUrl` (value object) |
| **Allocate** | Bir short URL için kod ayırmak (istenen kodu almak ya da yenisini üretmek) | `ShortCodeAllocator.AllocateAsync` |
| **Visit** | Birinin kısa linke tıkladığı an. Değişmez bir olgu | `Visit` (aggregate) |
| **Resolve** | Kodu hedef adrese çözmek ve ziyareti kaydetmek | `ResolveShortUrlCommand` |
| **Expired** | Son kullanma tarihi geçmiş link | `ShortUrlStatus.Expired` |
| **Deactivate** | Linki kalıcı olarak kullanım dışı bırakmak (soft delete) | `ShortUrl.Deactivate` |

## Domain modeli

### Aggregate'ler

**`ShortUrl`** bir linkin kimliğini ve yaşam döngüsünü yönetir: oluşturma, son kullanma ve pasifleştirme.
- Public setter'ı ve public constructor'ı yok. Yalnızca `Create` factory metodu ile doğar.
- `RecordVisit(now)` iş kuralını uygular: yalnızca aktif bir link ziyaret edilebilir. Başarılıysa yeni bir `Visit` döner. Kendi durumunu **değiştirmez**.

**`Visit`** bir ziyareti temsil eden değişmez (immutable) bir kayıttır.
- `ShortUrl`'e yalnızca `ShortUrlId` üzerinden bağlıdır (reference by identity).
- Yalnızca `ShortUrl.RecordVisit` tarafından oluşturulabilir (`internal` factory). Böylece süresi dolmuş veya pasif bir link için ziyaret kaydedilemez.

### Neden iki ayrı aggregate?

Vernon'un aggregate tasarım kuralları:

1. **Aggregate'i gerçek tutarlılık sınırına göre çiz.** Ziyaret sayısı, linkin geçerliliğiyle aynı transaction'da tutarlı olmak zorunda değil. `ShortUrl`'ün koruması gereken kurallar yalnızca kod, adres, son kullanma ve pasiflik.
2. **Aggregate'leri küçük tut.** Sayaç `ShortUrl`'ün içinde olsaydı her yönlendirme aynı satırı güncellerdi. Eşzamanlı isteklerde güncellemeler birbirini ezer (*lost update*).
3. **Başka aggregate'e id ile referans ver.** `Visit` bir `ShortUrl` nesnesi tutmaz, yalnızca `ShortUrlId` tutar.

Ölçülen fark: 100 eşzamanlı yönlendirmeden sonra sayacı aggregate içinde tutan ilk sürüm `visitCount: 11` gösterdi. Mevcut sürüm `visitCount: 100` gösteriyor. Ziyaretler yalnızca ekleme (append-only) olduğu için hiçbir istek başka bir isteğin satırını güncellemez.

### Value object'ler

| Value object | Kurallar |
|---|---|
| `ShortCode` | 4–32 karakter, yalnızca `a-z A-Z 0-9 - _`. Rezerve kelimeler (`admin`, `health`, `openapi`, `swagger`) kullanılamaz. |
| `OriginalUrl` | Mutlak `http`/`https` adresi, en fazla 2048 karakter. `javascript:`, `ftp:`, göreli yollar reddedilir. |
| `ShortUrlId`, `VisitId` | Strongly-typed id. `Guid.CreateVersion7()` ile zamana göre sıralanabilir. |

Geçersiz bir value object oluşturulamaz. `Create` metodu `Result<T>` döner.

### Domain service: `ShortCodeAllocator`

"Bir kısa kod yalnızca bir linke ait olabilir" kuralını tek bir `ShortUrl` kendi başına koruyamaz, çünkü diğer bütün linkleri bilmesi gerekir. Birden fazla aggregate'i ilgilendiren bu tür kurallar DDD'de bir **domain service** içinde yaşar. Application katmanı yalnızca bu servisi çağırır, kuralın kendisini bilmez.

Kontrol ile kayıt arasında bir yarış (race condition) mümkün olduğu için veritabanındaki unique index son güvence olarak kalır. Çakışma olursa Infrastructure bunu `UniqueConstraintViolationException`'a çevirir ve API `409 Conflict` döner.

### Domain event'ler

`ShortUrlCreatedDomainEvent` ve `ShortUrlDeactivatedDomainEvent` aggregate tarafından üretilir. `SaveChangesAsync` sırasında toplanır ve **commit'ten sonra** `IDomainEventHandler<T>`'lere dağıtılır.

### Diğer kararlar

- **Exception yerine Result pattern:** Beklenen iş hataları `Result<T>` + `Error` ile döner. Exception'lar gerçekten beklenmedik durumlar için kalır.
- **Zaman soyutlaması:** `DateTime.UtcNow` yerine `TimeProvider` kullanılır. Domain metodları zamanı parametre olarak alır, böylece testler deterministik çalışır.
- **Kod kalitesi derlemede denetlenir:** .NET analyzer'ları önerilen modda açık ve uyarılar hata sayılıyor (`Directory.Build.props`, `.editorconfig`). Best practice'e aykırı kod derlenmez.
- **Kurallar testle korunur:** `ArchitectureTests` Domain'in hiçbir altyapıya bağımlı olmadığını doğrular. Ayrıca entity'lerde public setter ya da constructor olmadığını ve aggregate'lerin birbirine yalnızca id ile referans verdiğini kontrol eder.

## Application katmanı (CQRS)

MediatR kullanılmadı (v13'ten itibaren ticari lisanslı). Onun yerine sade `ICommandHandler` / `IQueryHandler` arayüzleri var ve assembly taramasıyla DI'a otomatik kaydediliyorlar.

| Use case | Tür | Açıklama |
|---|---|---|
| `CreateShortUrlCommand` | Command | `ShortCodeAllocator` ile kod ayırır, `ShortUrl` oluşturur. |
| `ResolveShortUrlCommand` | Command | Kodu çözer, `ShortUrl.RecordVisit` ile bir `Visit` kaydeder, hedef adresi döner. |
| `GetShortUrlsQuery` | Query | Tüm linkler, durum ve ziyaret sayılarıyla birlikte. Tek SQL sorgusuyla okunur (`IShortUrlReader`). |
| `GetShortUrlVisitsQuery` | Query | Bir linkin ziyaret geçmişi (`IVisitReader`). |
| `GetShortUrlByCodeQuery` | Query | Link detayları ve istatistik. İstatistikler `IVisitReader` ile ziyaret kayıtlarından okunur (read side). |
| `DeactivateShortUrlCommand` | Command | Linki pasifleştirir. |

## API

| Metod | Yol | Açıklama | Başarılı yanıt |
|---|---|---|---|
| `POST` | `/api/short-urls` | Kısa URL oluştur | `201 Created` |
| `GET` | `/api/short-urls?page=1&pageSize=20` | Tüm linkler, en yeniden eskiye | `200 OK` |
| `GET` | `/api/short-urls/{code}` | Detay + istatistik | `200 OK` |
| `GET` | `/api/short-urls/{code}/visits?page=1&pageSize=20` | Ziyaret geçmişi, en yeniden eskiye | `200 OK` |
| `DELETE` | `/api/short-urls/{code}` | Pasifleştir | `204 No Content` |
| `GET` | `/{code}` | Orijinal URL'e yönlendir | `302 Found` |
| `GET` | `/health` | Sağlık kontrolü | `200 OK` |
| `GET` | `/openapi/v1.json` | OpenAPI dokümanı (Development) | `200 OK` |

Liste endpoint'leri sayfalıdır. `pageSize` en fazla 100, varsayılanı 20'dir. Yanıtta `items`, `totalCount`, `totalPages` ve `hasNextPage` alanları bulunur.

Hatalar [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) formatında döner. `Error.Type` → HTTP durum kodu eşlemesi şöyle: `Validation → 400`, `NotFound → 404`, `Conflict → 409`, `Gone → 410` (süresi dolmuş veya pasif link).

Yönlendirme bilinçli olarak `301` yerine `302` ile yapılır. `301` tarayıcıda cache'lendiği için sonraki ziyaretler sunucuya hiç ulaşmaz ve sayılamaz.

**Örnek istek**

```http
POST /api/short-urls
Content-Type: application/json

{
  "url": "https://github.com/dotnet/aspnetcore",
  "customCode": "aspnet",
  "expiresAtUtc": "2030-01-01T00:00:00Z"
}
```

```json
{
  "id": "0199...",
  "code": "aspnet",
  "shortLink": "http://localhost:5080/aspnet",
  "originalUrl": "https://github.com/dotnet/aspnetcore",
  "status": "Active",
  "createdAtUtc": "2026-09-26T10:00:00+00:00",
  "expiresAtUtc": "2030-01-01T00:00:00+00:00",
  "deactivatedAtUtc": null,
  "visitCount": 0,
  "lastVisitedAtUtc": null
}
```

Tüm uç noktalar hazır bir Postman koleksiyonu olarak [`postman/UrlShortener.postman_collection.json`](postman/UrlShortener.postman_collection.json) dosyasında. Postman'de **Import** ile içe aktarabilirsin.

Diğer örnekler için [`UrlShortener.Api.http`](src/UrlShortener.Api/UrlShortener.Api.http) dosyasına bakabilirsin (VS Code REST Client, Rider ve Visual Studio ile doğrudan çalışır).

## Çalıştırma

Gereksinim: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
dotnet run --project src/UrlShortener.Api
```

Uygulama `http://localhost:5080` adresinde açılır. SQLite veritabanı (`urlshortener.db`) ilk açılışta otomatik oluşturulur.

```bash
dotnet test
```

## Bilinçli sınırlar ve sonraki adımlar

- **Tek bounded context:** Şu an link yönetimi ve ziyaretler aynı context'te. Analitik büyürse (referrer, ülke, cihaz vb.) `Visit` ayrı bir *Analytics* bounded context'ine taşınabilir ve bir integration event ile beslenebilir.
- **Outbox pattern:** Domain event'ler şu an commit'ten sonra aynı process içinde dağıtılıyor. Güvenilir entegrasyon event'leri (ör. mesaj kuyruğu) için bir outbox tablosu gerekir.
- **EF Core migrations:** Şema değişmeye başladığında `EnsureCreated` yerine `dotnet ef migrations add Initial` + `MigrateAsync` kullanılmalı.
- **PostgreSQL / SQL Server:** Yalnızca `Infrastructure` değişir. Domain ve Application etkilenmez.
- **Cache:** Yönlendirme okuma-ağırlıklı bir iş. `GetByCodeAsync` önüne Redis / `HybridCache` konabilir.
- **Rate limiting ve kimlik doğrulama:** `POST` endpoint'i `AddRateLimiter` ile sınırlanabilir, linkler kullanıcılara bağlanabilir.
- **Domain'in karmaşıklığı:** URL kısaltma görece basit bir domain. DDD'nin asıl getirisi karmaşık iş kurallarında ortaya çıkar. Bu proje kalıpların doğru uygulanışını göstermek için bu kapsamda tutuldu.
