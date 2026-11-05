# Forum Wrap-Up

Applicazione web ASP.NET Core per la generazione del wrap-up annuale dei post ForumCommunity.

## Progetti

- `src/ForumWrapUp.Web`: applicazione MVC.
- `src/ForumWrapUp.Data`: entità EF Core, `DbContext` e configurazione PostgreSQL.
- `src/ForumWrapUp.Importing`: integrazione ForumFree.NET e logiche di importazione dei dati raw.

Il modello EF è la fonte del modello relazionale. Durante la validazione si modificano solo le entità e la configurazione: non sono presenti migrazioni o snapshot. La prima migrazione sarà generata dopo l'approvazione del modello.

## Database

Il provider scelto è PostgreSQL con EF Core 10. Per avviare l'app con il database configurare la variabile d'ambiente `ConnectionStrings__ForumWrapUp` con la connection string PostgreSQL. Non inserire credenziali reali nei file versionati.

Ripristinare il tool EF locale al primo utilizzo:

```powershell
dotnet tool restore
```

Solo dopo la validazione del modello, creare la migrazione iniziale dalla root del repository:

```powershell
dotnet ef migrations add InitialCreate --project src\ForumWrapUp.Data --startup-project src\ForumWrapUp.Data --output-dir Migrations
```

Applicare le migrazioni al database configurato:

```powershell
dotnet ef database update --project src\ForumWrapUp.Data --startup-project src\ForumWrapUp.Data
```

Per EF, `ForumWrapUpDbContextFactory` legge prima `FORUM_WRAPUP_CONNECTION_STRING` e poi `ConnectionStrings__ForumWrapUp`; in assenza di entrambe usa una stringa locale senza password. La generazione della migrazione non richiede che PostgreSQL sia avviato. L'app web legge invece `ConnectionStrings__ForumWrapUp` e registra il contesto solo quando la variabile è configurata.

## Modello iniziale

Le tabelle `forum_instances`, `forum_sections`, `forum_topics` e `forum_posts` appartengono allo schema PostgreSQL `raw`. Le altre tabelle rimangono nello schema predefinito (`public` con la configurazione PostgreSQL standard). Database e `DbContext` restano unici; le relazioni possono attraversare gli schemi.

- account e tentativi di registrazione con MID;
- istanze forum, sezioni, topic e post, conservando gli identificativi remoti e il contenuto HTML;
- punti originali e punti numerici nullable, perché l'API può restituire una stringa vuota;
- operazioni d'importazione persistite, con intervallo, stato e progressi;
- anni abilitati, riepiloghi annuali e statistiche giornaliere.

Le date dei post sono archiviate in UTC. La conversione al giorno locale `Europe/Rome` per le statistiche appartiene alla logica applicativa.

## Importazione dei dati raw

L'API amministrativa `POST /api/admin/imports` accoda l'importazione raw per MID e intervallo (`fromDate`/`toDate`, formato `YYYY-MM-DD`) e restituisce `202 Accepted` con l'ID e l'URL per consultare lo stato. `GET /api/admin/imports/{operationId}` restituisce stato e progresso. Le date sono inclusive nel fuso `Europe/Rome`; il worker è eseguito nella stessa applicazione e processa una sola operazione alla volta. Questa fase non genera le aggregazioni annuali.

Configurare `ForumApi:ForumUri` con l'URI assoluto della root del forum e, facoltativamente, `ForumApi:MaximumPageDelay` con un intervallo massimo (formato `TimeSpan`, ad esempio `00:05:00`). Il cookie è letto dal provider `IForumApiCookieProvider` tramite la chiave di configurazione `ForumApi:Cookie`. In sviluppo, WebApplication carica .NET User Secrets quando l'ambiente è `Development`; impostare il valore senza inserirlo nei file versionati:

```powershell
dotnet user-secrets set "ForumApi:Cookie" "<cookie value>" --project src\ForumWrapUp.Web\ForumWrapUp.Web.csproj
```

In produzione configurare la stessa chiave tramite il secret storage scelto per l'hosting, ad esempio la variabile d'ambiente `ForumApi__Cookie`.

Le API amministrative richiedono un token nell'header HTTP `Authorization`, usando lo schema Bearer esclusivamente su HTTPS. Configurare il token fuori dal repository, ad esempio tramite la variabile d'ambiente `AdminApi__Token`; il token è distinto dal login degli utenti e non va inserito in query string o log.

## Gestione amministrativa degli utenti

Tutti gli endpoint seguenti richiedono l'autenticazione amministrativa:

- `GET /api/admin/users` elenca gli account; `GET /api/admin/users/{mid}` legge un account. Le risposte non includono mai l'hash della password.
- `POST /api/admin/users` crea un account per MID e attesta la verifica del profilo da parte dell'amministratore; il body contiene `mid` e `password`. La password (almeno 12 caratteri) viene hashata lato server.
- `PUT /api/admin/users/{mid}/password` reimposta la password, sempre hashata lato server.
- `DELETE /api/admin/users/{mid}` disattiva l'account senza eliminare dati associati; `POST /api/admin/users/{mid}/restore` lo riattiva.

Gli account disattivati non sono ammessi a nuove importazioni. Il modello EF include lo stato `IsActive`; la migrazione iniziale deve includere questa proprietà.

## Mappatura delle risposte ForumFree.NET

I payload di riferimento sono in `C:\Repos\Other\Personal\ForumFree.NET\payload`; i DTO consumati dal package sono nel repository ForumFree.NET, non duplicati nel progetto Data.

| API / campo JSON | Destinazione nel modello |
|---|---|
| `GetPostsByUserIdAsync.idForum` | `ForumInstance.ApiForumId` |
| `pages`, `total`, `currentPage` | progressi dell'`ImportOperation` (pagine/record attesi e acquisiti) |
| `member.id`, `member.name`, `member.joined` | MID e dati profilo dell'account; il MID è la chiave dell'account |
| `posts[].id`, `date`, `points`, `content` | `ForumPost.ExternalId`, `PublishedAtUtc`, `Points`/`RawPoints`, `ContentHtml` |
| `posts[].topic.id/title/posts` | `ForumTopic.ExternalId`, `TitleHtml`, `PostCount` |
| `posts[].forum.id/name` | `ForumSection.ExternalId`, `Name` |
| `GetProfileByIdAsync` / `GetProfileByNicknameAsync` | oggetto profilo indicizzato da una chiave JSON dinamica; si conservano nel profilo account nickname, avatar, data di registrazione e contatore messaggi utili all'applicazione |

`points` è serializzato come stringa e nei payload può essere vuoto: il valore originale viene mantenuto in `RawPoints`, mentre `Points` contiene il valore numerico quando è interpretabile, altrimenti `null`. Il modello non persiste attributi del profilo non usati dal prodotto (ad esempio permessi amministrativi o preferenze UI).
