# GlowBook — Web + API + MAUI

GlowBook is een examenproject bestaande uit:
- een **ASP.NET Core MVC webapplicatie** (Razor) met Identity Framework,
- een **REST API** voor de MAUI app (JWT),
- een **.NET MAUI mobile app** (XAML + MVVM) met **offline SQLite** en **automatische synchronisatie**.

## Solution structuur
- **GlowBook.Web**: ASP.NET Core MVC + Razor + REST API + middleware + logging
- **GlowBook.Model**: Class Library met EF Core models + DbContext + seeding (DbSeeder)
- **GlowBook.Mobile**: .NET MAUI app (XAML) met MVVM + offline SQLite + sync

## Vereisten
- Visual Studio 2022/2025 met workloads:
  - ASP.NET and web development
  - .NET MAUI development
- .NET SDK 9.x
- SQL Server (publiek bereikbaar volgens opdracht)

## Configuratie (Secrets / Environment)
### 1) Connection string (SQL Server)
Zet de connection string in **User Secrets** of environment variables:

- `ConnectionStrings:DefaultConnection`

Voorbeeld (SQL Server):
- `Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;`

### 2) JWT (voor MAUI login)
Zet via User Secrets of environment variables:
- `Jwt:Key` **(verplicht)**
- `Jwt:Issuer` (default: `GlowBook`)
- `Jwt:Audience` (default: `GlowBookMobile`)
- `Jwt:DaysValid` (default: `30`)

### 3) E-mail (SMTP)
Zet via config (User Secrets / env):
- `Email:*` (host, port, username, password, sender, …)

> Belangrijk: SMTP wachtwoorden / toegangscodes staan NIET in code of GitHub.

## Installatie & Run
1. Open de solution in Visual Studio.
2. Zet secrets: **DefaultConnection**, **Jwt:Key**, **Email settings**.
3. Start **GlowBook.Web**.
   - Bij opstarten worden migrations automatisch toegepast via `MigrateAsync()`.
   - Seeding gebeurt via `GlowBook.Model/Data/DbSeeder.cs`.
4. Start daarna **GlowBook.Mobile** (Windows of Android emulator/telefoon).

## Lokale URLs (Development)
In `GlowBook.Web/Properties/launchSettings.json`:

- HTTPS: `https://localhost:7129`
- HTTP: `http://localhost:5293`

De API zit onder `/api/*`:
- `https://localhost:7129/api/appointments` (401 zonder token = OK, endpoint bestaat)

## Rollen & autorisatie (Identity Framework)
Rollen:
- **Admin**
- **Owner**
- **Employee**

Policies (in `Program.cs`):
- `CanManageAppointments`: Admin/Owner/Employee
- `CanViewReports`: Admin/Owner

Frontend:
- Menu-items in de webapp worden getoond/verborgen op basis van rollen.

## Test accounts
### Admin (seed)
- Email: `admin@glowbook.local`
- Wachtwoord: `Admin123!`
- Rol: `Admin`

### Registratie
- Nieuwe registratie krijgt automatisch rol **Employee** (zie `POST /api/auth/register`).
- De gebruiker kan pas inloggen nadat het e-mailadres bevestigd is (`EmailConfirmed = true`).

## E-mail verificatie
Bij registratie verstuurt de API een bevestigingsmail met een link naar de webapp:
- De link wijst naar `Account/ConfirmEmail`
- Inloggen kan pas wanneer `EmailConfirmed = true`

## API (Auth)
Base route:
- `/api/auth`

Endpoints:
- `POST /api/auth/register`
- `POST /api/auth/login`

Login controleert:
- gebruiker bestaat
- gebruiker is actief (`IsActive`)
- e-mail is bevestigd (`EmailConfirmed`)
- wachtwoord klopt

Login response bevat:
- `accessToken`
- `expiresUtc`
- `email`
- `displayName`
- `roles[]`
- `permissions[]` (claims met type `"permission"`)

JWT header voorbeeld:
- `Authorization: Bearer <token>`

## API (Data)
Voorbeeld endpoints (afhankelijk van controllers):
- `GET /api/customers`
- `GET /api/services`
- `GET /api/staff`
- `GET /api/appointments`
- `POST/PUT/DELETE /api/...` (protected met JWT + policies)

Autorisatieregels:
- Protected endpoints volgen dezelfde rechtenlogica als de webapp (rollen/policies).

## Offline werking (MAUI)
De MAUI app ondersteunt online/offline gebruik met automatische synchronisatie:

### Lokale opslag (SQLite)
- SQLite database: `glowbook_mobile.db3` (in `FileSystem.AppDataDirectory`)
- Selecte data wordt lokaal opgeslagen voor offline gebruik:
  - Customers
  - Services
  - Staff
  - Appointments (LocalAppointmentV2)
- Offline acties worden bewaard als **PendingChange**.

### Synchronisatie
Wanneer er terug internet is:
1. **Push**: PendingChange items worden naar de API verstuurd (asynchroon).
2. **Pull**: lookups (customers/services/staff) worden opnieuw opgehaald.
3. **Pull/merge**: appointments worden opgehaald en gemerged met lokale data.

### MAUI Base URL
De MAUI app gebruikt een instelbare `ApiBaseUrl`:
- Windows: `https://localhost:7129/api/`
- Android emulator: `http://10.0.2.2:5293/api/`

> Android emulator gebruikt `10.0.2.2` om de host (jouw pc) te bereiken.

## GDPR
- Lokaal opgeslagen: enkel “selecte” klant/service/afspraak-data nodig voor offline gebruik.
- JWT token wordt opgeslagen in **SecureStorage** (MAUI).
- Uitloggen verwijdert lokaal:
  - token (`gb_token`)
  - expiry (`gb_expires_utc`)
- Er worden geen wachtwoorden lokaal opgeslagen.

## Logging & foutafhandeling
### Web
- **Serilog** logt naar: `logs/glowbook-.log` (rolling per dag).
- Middleware:
  - ErrorHandlingMiddleware (foutafhandeling + boodschap)
  - CultureCookieMiddleware (taal/cultuur cookie)
  - ActiveUserMiddleware (controle op actieve gebruiker)

### Mobile
- Fouten worden gelogd naar Debug output (en opgevangen in ViewModels waar nodig).

## Meertaligheid
- Webapp is meertalig met minstens 3 talen:
  - Nederlands (`nl`) — default
  - Engels (`en`)
  - Frans (`fr`)
- Culture wordt ingesteld via cookie: `glowbook_culture`
- Resources staan onder `Resources` (localization).

## AI / derden (plagiaat & bronnen)
### NuGet packages
- `CommunityToolkit.Mvvm`
- `sqlite-net-pcl`
- `Serilog`

### AI gebruik (vermelding)
AI werd gebruikt als hulpmiddel voor:
- MVVM structuur en ViewModel bindings
- offline sync (PendingChange patroon)
- API client methodes (GET/POST/PUT/DELETE)
- seeding structuur (DbSeeder) en fixes rond EF modellen

Alle AI-gegenereerde code werd:
- aangepast aan de GlowBook modellen/structuur,
- opgeschoond (overbodige code verwijderd),
- getest in de oplossing.

