# UrlShortener

Domain-Driven Design (DDD) ve Clean Architecture prensipleriyle yazılmış, .NET 10 tabanlı bir URL kısaltma servisi.

## Mimari

```
src/
├── UrlShortener.Domain          → İş kuralları. Hiçbir framework'e bağımlı değil.
├── UrlShortener.Application     → Use case'ler (CQRS komut/sorgu handler'ları).
├── UrlShortener.Infrastructure  → EF Core + SQLite, repository, kısa kod üretici.
└── UrlShortener.Api             → Minimal API endpoint'leri (sunum katmanı).
tests/
├── UrlShortener.Domain.UnitTests
├── UrlShortener.Application.UnitTests
└── UrlShortener.ArchitectureTests  → Katman bağımlılık kurallarını test eder.
```

Bağımlılıklar her zaman **içe doğru** akar:

```
Api ──► Infrastructure ──► Application ──► Domain
 └────────────────────────────┘
```

`Domain` hiçbir projeye ve hiçbir NuGet paketine referans vermez. `ArchitectureTests` bu kuralı her build'de doğrular.

## Domain modeli

| Kavram | Tür | Açıklama |
|---|---|---|
| `ShortUrl` | **Aggregate Root** | Kısa kod → orijinal URL eşlemesi. Yaşam döngüsünü (son kullanma, pasifleştirme) ve ziyaret istatistiğini yönetir. |
| `ShortUrlId` | Value Object (strongly-typed id) | `Guid.CreateVersion7()` ile üretilir, sıralanabilir. |
| `ShortCode` | Value Object | 4–32 karakter, yalnızca `a-z A-Z 0-9 - _`. Rezerve kelimeler (`admin`, `health`, `openapi`, `swagger`) kullanılamaz. |
| `OriginalUrl` | Value Object | Mutlak `http`/`https` adresi, en fazla 2048 karakter. `javascript:`, `ftp:` vb. reddedilir. |
| `ShortUrlStatus` | Enum | `Active`, `Expired`, `Deactivated` |
| `ShortUrlCreated/Visited/DeactivatedDomainEvent` | Domain Event | Aggregate'teki durum değişikliklerini dışarıya duyurur. |
| `IShortUrlRepository` | Repository arayüzü | Domain'de tanımlı, Infrastructure'da implemente edilir. |
| `IShortCodeGenerator` | Domain servisi arayüzü | Kısa kod üretimi (implementasyonu: kriptografik Base62). |

**Uygulanan DDD prensipleri**

- **Zengin domain modeli:** `ShortUrl`'ün public setter'ı yok. Durum yalnızca `Create`, `Visit`, `Deactivate` gibi davranış metodlarıyla değişir ve bu metodlar iş kurallarını korur (ör. süresi dolmuş link ziyaret sayısını artırmaz).
- **Kendini doğrulayan value object'ler:** Geçersiz bir `ShortCode` veya `OriginalUrl` nesnesi oluşturulamaz.
- **Exception yerine Result pattern:** Beklenen iş hataları `Result<T>` + `Error` ile döner. Exception'lar gerçekten beklenmedik durumlar için kalır.
- **Domain event'ler:** `SaveChangesAsync` sırasında toplanır ve **commit'ten sonra** `IDomainEventHandler<T>`'lere dağıtılır.
- **Zaman soyutlaması:** `DateTime.UtcNow` yerine `TimeProvider` kullanılır, böylece testler deterministik çalışır.

## Application katmanı (CQRS)

MediatR kullanılmadı (v13'ten itibaren ticari lisanslı). Onun yerine sade `ICommandHandler` / `IQueryHandler` arayüzleri var ve assembly taramasıyla DI'a otomatik kaydediliyorlar.

| Use case | Tür | Açıklama |
|---|---|---|
| `CreateShortUrlCommand` | Command | Özel kod verilmezse benzersiz kod üretir (çakışmada 5 denemeye kadar tekrar eder). |
| `VisitShortUrlCommand` | Command | Kodu çözümler, ziyareti kaydeder, yönlendirilecek URL'i döner. |
| `GetShortUrlByCodeQuery` | Query | Detay ve istatistik. |
| `DeactivateShortUrlCommand` | Command | Linki pasifleştirir (soft delete). |

Aynı özel kodu aynı anda talep eden iki istek olursa veritabanındaki unique index devreye girer. Infrastructure bunu `UniqueConstraintViolationException`'a çevirir ve handler `409 Conflict` döner.

## API

| Metod | Yol | Açıklama | Başarılı yanıt |
|---|---|---|---|
| `POST` | `/api/short-urls` | Kısa URL oluştur | `201 Created` |
| `GET` | `/api/short-urls/{code}` | Detay + istatistik | `200 OK` |
| `DELETE` | `/api/short-urls/{code}` | Pasifleştir | `204 No Content` |
| `GET` | `/{code}` | Orijinal URL'e yönlendir | `302 Found` |
| `GET` | `/health` | Sağlık kontrolü | `200 OK` |
| `GET` | `/openapi/v1.json` | OpenAPI dokümanı (Development) | `200 OK` |

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

## Sonraki adımlar

- **EF Core migrations:** Şema değişmeye başladığında `EnsureCreated` yerine `dotnet ef migrations add Initial` + `MigrateAsync` kullan.
- **PostgreSQL / SQL Server:** Yalnızca `Infrastructure/DependencyInjection.cs` ve unique-constraint hata kodu değişir. Domain ve Application etkilenmez.
- **Outbox pattern:** Domain event'ler şu an commit'ten sonra aynı process içinde dağıtılıyor. Güvenilir entegrasyon event'leri (ör. mesaj kuyruğu) için outbox tablosu ekle.
- **Cache:** Yönlendirme okuma-ağırlıklı bir iş. `GetByCodeAsync` önüne Redis / `HybridCache` konabilir.
- **Ziyaret sayacı:** Çok yüksek trafikte aggregate üzerinde sayaç artırmak satır kilidi çekişmesi yaratır. Ziyaretleri `ShortUrlVisitedDomainEvent` üzerinden ayrı bir Analytics bounded context'ine akıtmak daha ölçeklenebilir olur.
- **Rate limiting & kimlik doğrulama:** `POST` endpoint'ini `AddRateLimiter` ile sınırla, linkleri kullanıcıya bağla.
