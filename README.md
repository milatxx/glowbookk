# GlowBook — Web + API + MAUI

GlowBook is mijn examenproject. Het bestaat uit drie delen:

- een website in ASP.NET Core MVC (Razor) met Identity Framework,
- een REST API die de mobiele app gebruikt (met JWT),
- een mobiele app in .NET MAUI (XAML + MVVM) die ook offline werkt en zelf synchroniseert.

## Hoe de solution is opgebouwd

- **GlowBook.Web**: de website, de API, mijn middleware en de logging
- **GlowBook.Model**: class library met mijn EF Core modellen, de DbContext en de seeding (`DbSeeder`)
- **GlowBook.Mobile**: de MAUI app met MVVM, een lokale SQLite databank en de sync

## Wat je nodig hebt

- Visual Studio 2022/2025 met deze workloads:
  - ASP.NET and web development
  - .NET MAUI development
- .NET SDK 9.x
- SQL Server (publiek bereikbaar, zoals de opdracht vraagt)

## Instellingen (secrets)

### 1) Connection string

De connection string zet je in **User Secrets**, niet in de code:

- `ConnectionStrings:DefaultConnection`

Bijvoorbeeld:

```
Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;
```

### 2) JWT (nodig om in te loggen in de app)

Ook in User Secrets:

- `Jwt:Key` (verplicht, minstens 32 tekens)
- `Jwt:Issuer` (standaard: `GlowBook`)
- `Jwt:Audience` (standaard: `GlowBookMobile`)
- `Jwt:DaysValid` (standaard: 30)

### 3) E-mail

- `Email:User`, `Email:Password`, `Email:FromEmail`, …

> **Let op:** het e-mailwachtwoord staat nergens in mijn code of op GitHub.
> In `appsettings.json` staan alleen lege waarden; de echte waarden zitten in User Secrets.

## Opstarten

1. Open de solution in Visual Studio.
2. Zet je secrets: connection string, `Jwt:Key` en de e-mailinstellingen.
3. Start **GlowBook.Web**.
   - De migrations worden automatisch uitgevoerd bij het opstarten (`MigrateAsync()`).
   - Daarna vult de seeding de databank (`GlowBook.Model/Data/DbSeeder.cs`).
4. Start daarna **GlowBook.Mobile** (op Windows of op de Android emulator).

> Ik gebruik Azure SQL. Je moet je IP-adres toevoegen aan de firewall van de server,
> anders start de website niet op en krijg je een foutmelding dat de server niet
> bereikbaar is. Je IP verandert als je op een ander netwerk zit (thuis of op school).

## Adressen tijdens het ontwikkelen

Deze staan in `GlowBook.Web/Properties/launchSettings.json`:

- HTTPS: `https://localhost:7129`
- HTTP: `http://localhost:5293`

De API zit onder `/api/*`, bijvoorbeeld:

- `https://localhost:7129/api/appointments` (zonder token krijg je 401, dat is normaal)

## Rollen en rechten (Identity Framework)

**Mijn rollen:**

- `Admin`
- `Owner`
- `Employee`

**Mijn policies** (staan in `Program.cs`):

- `CanManageAppointments`: Admin / Owner / Employee
- `CanViewReports`: Admin / Owner
- `RequireAdmin`: alleen Admin — die gebruik ik op het toevoegen, aanpassen en
  verwijderen van klanten en diensten (zowel op de website als in de API)

**In de frontend:**

- Het menu toont alleen de items die bij je rol horen.
- Dat menu verbergen is er vooral voor het gemak. De echte beveiliging zit in
  `[Authorize]` en mijn policies op de controllers. Wie de URL rechtstreeks intikt,
  komt er dus nog niet binnen.

## Testaccounts

**Admin (komt uit de seeding)**

- E-mail: `admin@glowbook.local`
- Wachtwoord: `Admin123!`
- Rol: Admin

**Zelf registreren**

- Wie zich registreert krijgt automatisch de rol `Employee`.
- Inloggen kan pas als het e-mailadres bevestigd is.

## E-mail bevestigen

Als iemand zich registreert, stuurt de API een mail met een link:

- De link gaat naar `Account/ConfirmEmail`
- Pas als `EmailConfirmed` op true staat, kan je inloggen

## API (inloggen)

Route: `/api/auth`

- `POST /api/auth/register`
- `POST /api/auth/login`

Bij het inloggen controleer ik of:

- de gebruiker bestaat
- de gebruiker actief is (`IsActive`)
- het e-mailadres bevestigd is (`EmailConfirmed`)
- het wachtwoord klopt

Wat je terugkrijgt:

- `accessToken`
- `expiresUtc`
- `email`
- `displayName`
- `roles[]`
- `permissions[]`

Het token stuur je mee als header:

```
Authorization: Bearer <token>
```

## API (gegevens)

Mijn API-controllers staan in `GlowBook.Web/Controllers/Api/`:

- `GET /api/customers`
- `GET /api/services`
- `GET /api/staff`
- `GET /api/appointments`
- `POST/PUT/DELETE /api/...` (beveiligd met JWT en mijn policies)

De API gebruikt dezelfde rechten als de website, maar werkt met JWT in plaats van
cookies. Een mobiele app werkt namelijk niet met cookies. Daarom staan in
`Program.cs` beide manieren van aanmelden geregistreerd.

Alles in de API werkt asynchroon: `async/await` met `ToListAsync`,
`FirstOrDefaultAsync` en `SaveChangesAsync`.

## Meertaligheid

De website is volledig beschikbaar in drie talen:

- Nederlands (`nl`) — standaard
- Engels (`en`)
- Frans (`fr`)

**Wat vertaald is:** alle pagina's, ook inloggen en registreren, het
gebruikersbeheer, en het toevoegen, aanpassen en verwijderen van klanten,
diensten, medewerkers en afspraken. Ook mijn foutmeldingen zijn vertaald.

**Hoe ik het gedaan heb:**

- Eén bestand per taal: `Resources/SharedResources.{nl,en,fr}.resx`, met in elke
  taal dezelfde sleutels.
- In elke view voeg ik `IStringLocalizer<SharedResources>` toe en haal ik de tekst
  op met een sleutel.
- Voor de foutmeldingen zet ik in `Program.cs` de `DataAnnotationLocalizerProvider`
  naar `SharedResources`. Daardoor wordt bijvoorbeeld `ErrorMessage = "Val_Required"`
  gezien als een sleutel die in het resourcebestand wordt opgezocht. Zo wisselen
  ook mijn validatiemeldingen mee van taal.
- De taal bewaar ik in een cookie (`glowbook_culture`, zie `CultureCookieMiddleware`).
  Wisselen doe je met `?lang=nl|en|fr`.

## AJAX

Ik gebruik AJAX op twee pagina's:

- **Klanten** (`Views/Customers/Index.cshtml`): zoeken terwijl je typt.
  JavaScript stuurt met `fetch` een verzoek naar mijn actie `ListPartial`. Die geeft
  alleen de partial `_CustomersList.cshtml` terug, en die HTML vervangt de tabel.
  De pagina zelf herlaadt dus niet. Ik wacht 300 milliseconden voor ik het verzoek
  stuur, zodat er niet bij elke toets een verzoek vertrekt. De filterlogica staat in
  één methode (`BuildCustomerQuery`) die zowel `Index` als `ListPartial` gebruiken,
  zodat ik die code niet twee keer heb.
- **Kalender** (`Views/Appointments/Index.cshtml`): filteren met jQuery `$.ajax`.

## Schermen in de app

Inloggen, Agenda, Klanten, Diensten, Nieuwe afspraak, Rapporten en Instellingen.

- **Rapporten**: het totaal aantal afspraken, hoeveel er per status zijn, en de top 5
  meest geboekte diensten. Ik bereken dat uit mijn lokale SQLite databank, dus het
  werkt ook zonder internet.
- **Instellingen**: hier stel je de API-URL in, en je ziet of je online bent en
  wanneer er laatst gesynchroniseerd is. Er is ook een knop om zelf te synchroniseren.

## Offline werken (app)

De app werkt online en offline, en synchroniseert automatisch.

### Lokale opslag (SQLite)

- Mijn databank heet `glowbook_mobile.db3` en staat in `FileSystem.AppDataDirectory`
- Deze gegevens bewaar ik lokaal om offline te kunnen werken:
  - `LocalCustomer`
  - `LocalService`
  - `LocalStaff`
  - `LocalAppointmentV2`
- Wat je offline aanpast, bewaar ik als `PendingChange`.

Mijn lokale modellen zijn platter dan die op de server. Ik bewaar bijvoorbeeld
`CustomerName` als gewone tekst in plaats van een verwijzing naar een andere tabel.
Zo moet ik voor een lijst geen joins doen op de telefoon, en dat is sneller.

### Hoe de sync werkt

`SyncService.TrySyncEverythingAsync()` doet drie dingen als er internet is:

1. **Push** — eerst stuur ik de wachtende wijzigingen (`PendingChange`) naar de API.
   Lukt dat, dan haal ik ze uit de wachtrij.
2. **Pull lookups** — klanten, diensten en medewerkers opnieuw ophalen.
3. **Pull afspraken** — de afspraken ophalen en samenvoegen met wat er lokaal staat.

Push doe ik met opzet eerst. Anders zou het ophalen mijn eigen offline wijzigingen
overschrijven. Elke stap controleert eerst `IsOnline()`. Is er geen internet, dan
blijft alles lokaal staan tot de volgende keer.

### Welk adres de app gebruikt

De app gebruikt een `ApiBaseUrl` die je kan aanpassen in het scherm Instellingen:

- Windows: `https://localhost:7129/api/`
- Android emulator: `http://10.0.2.2:5293/api/`

De emulator gebruikt `10.0.2.2` om bij mijn pc te komen. Zou ik daar `localhost`
gebruiken, dan verwijst dat naar de emulator zelf.

> Vanaf Android 9 laat Android geen gewone http-verbindingen meer toe. Omdat ik
> lokaal met http werk, staat er `android:usesCleartextTraffic="true"` in
> `Platforms/Android/AndroidManifest.xml`. In een echte productieomgeving zou ik
> https gebruiken en is dat niet nodig.

## GDPR

- Ik bewaar lokaal alleen de gegevens die echt nodig zijn om offline te werken.
- Het JWT token staat in `SecureStorage`, dus niet bij de gewone instellingen.
- Bij uitloggen verwijder ik lokaal:
  - het token (`gb_token`)
  - de vervaldatum (`gb_expires_utc`)
- Wachtwoorden bewaar ik nooit op het toestel.

## Logging en fouten opvangen

### Website

- Serilog schrijft naar `logs/glowbook-.log`, met een nieuw bestand per dag.
- Mijn eigen middleware:
  - `ErrorHandlingMiddleware` — vangt fouten op en toont de gebruiker een
    nette boodschap in plaats van een crash
  - `CultureCookieMiddleware` — bewaart de taalkeuze in een cookie
  - `ActiveUserMiddleware` — kijkt of de ingelogde gebruiker nog actief is en
    logt hem anders uit

### App

- Fouten schrijf ik naar de Debug output en ik vang ze op in mijn ViewModels.

## Verantwoording & AI-gebruik

Tijdens dit project heb ik gebruikgemaakt van AI als hulpmiddel. Ik gebruikte AI
vooral om mij te helpen wanneer ik vastzat of iets niet goed begreep.

* **Fouten oplossen:** AI hielp mij bij het begrijpen van foutmeldingen en bij het
  zoeken naar mogelijke oplossingen.
* **Code begrijpen:** Ik gebruikte AI om bepaalde stukken code en .NET/MAUI-concepten
  beter te begrijpen.
* **Code-assistentie:** AI gaf soms voorbeelden of hielp met syntax wanneer ik niet
  wist hoe ik iets moest schrijven.
* **Projectstructuur:** AI hielp mij bij het organiseren van onderdelen zoals Models,
  Controllers, Services, Views en ViewModels.
* **Code-assistentie:** Hulp bij complexere code en specifieke syntax, bijvoorbeeld voor de `ApiClient`/`HttpClient`-configuratie, SQLite en de offline synchronisatie van gegevens.


Ik heb de voorgestelde oplossingen zelf getest en aangepast aan mijn project.