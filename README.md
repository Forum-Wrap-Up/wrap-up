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

Per EF, `ForumWrapUpDbContextFactory` legge prima `FORUM_WRAPUP_CONNECTION_STRING`, poi `ConnectionStrings__ForumWrapUp` e infine la chiave `ConnectionStrings:ForumWrapUp` negli User Secrets. I progetti Data e Web condividono lo stesso `UserSecretsId`, quindi la connection string salvata per Web è disponibile anche alle migrazioni con Data come startup project. In assenza di configurazione, la factory usa una stringa locale senza password. La generazione della migrazione non richiede che PostgreSQL sia avviato. L'app web carica gli User Secrets in ambiente `Development` e registra il contesto solo quando la connection string è configurata.

## Modello iniziale

Le tabelle `forum_profiles`, `forum_instances`, `forum_sections`, `forum_topics` e `forum_posts` appartengono allo schema PostgreSQL `raw`. Le altre tabelle rimangono nello schema predefinito (`public` con la configurazione PostgreSQL standard). Database e `DbContext` restano unici; le relazioni possono attraversare gli schemi.

- profili ForumFree, che possono essere sincronizzati anche prima della creazione dell'account locale;
- account e tentativi di registrazione con MID, con credenziali e stato di accesso separati dai dati ForumFree;
- istanze forum, sezioni, topic e post, conservando gli identificativi remoti e il contenuto HTML;
- punti originali e punti numerici nullable, perché l'API può restituire una stringa vuota;
- operazioni d'importazione persistite, con intervallo, stato e progressi;
- anni abilitati, riepiloghi annuali e statistiche giornaliere.

Le date dei post sono archiviate in UTC. La conversione al giorno locale `Europe/Rome` per le statistiche appartiene alla logica applicativa.

## Importazione dei dati raw

L'API amministrativa `POST /api/admin/imports` accoda l'importazione raw per MID e intervallo (`fromDate`/`toDate`, formato `YYYY-MM-DD`) e restituisce `202 Accepted` con l'ID e l'URL per consultare lo stato. `GET /api/admin/imports/{operationId}` restituisce stato e progresso. Le date sono inclusive nel fuso `Europe/Rome`; il worker è eseguito nella stessa applicazione e processa una sola operazione alla volta. Questa fase non genera le aggregazioni annuali. Il profilo ForumFree viene acquisito durante l'importazione dei post solo se il profilo locale non è ancora stato sincronizzato.

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
- `POST /api/admin/forum-profiles/{mid}/refresh` rilegge e salva il profilo ForumFree anche se non esiste ancora un account locale. Restituisce i dati del profilo, incluso `isBanned`; `502` indica che il profilo remoto non è raggiungibile o non è valido.

Gli account disattivati non sono ammessi a nuove importazioni. `ForumProfile.IsBanned` rappresenta lo stato di ban restituito da ForumFree e non modifica automaticamente `UserAccount.IsActive`, che rappresenta lo stato locale dell'account. Il modello EF include entrambi gli stati; la migrazione iniziale deve includere la tabella `forum_profiles` e la relazione uno-a-uno tra profilo e account locale.

## Accesso degli utenti

Il link **Accedi** apre un dialog nativo con MID e password locale del wrap-up. Senza JavaScript, o accedendo direttamente a una pagina protetta, viene usata `/Account/Login`. L'accesso usa gli account esistenti, creati tramite API amministrativa; l'auto-registrazione non è ancora implementata.

La sessione usa un cookie HttpOnly, SameSite=Lax e Secure in produzione, con scadenza assoluta di 15 giorni e senza rinnovo automatico. Gli account disattivati vengono rifiutati anche quando presentano una sessione esistente. Le credenziali errate, i MID sconosciuti e gli account disattivati producono lo stesso errore. Login e logout richiedono un token antiforgery; il logout è solo POST. Il limite è di 10 richieste di login al minuto per indirizzo IP, in memoria nella singola istanza. Dietro un reverse proxy, configurare gli indirizzi inoltrati soltanto per proxy fidati prima di usare l'IP client per il limite.

Dopo l'accesso, la testata mostra nickname (oppure MID), **Il mio wrap-up** ed **Esci**. `/WrapUp` è protetta da autenticazione utente e per ora mostra uno stato di riepilogo non disponibile: consultazione delle statistiche e download saranno implementati separatamente. Il cookie utente non autorizza le API amministrative.

### Test del login

```powershell
dotnet test tests\ForumWrapUp.Web.Tests\ForumWrapUp.Web.Tests.csproj
```

I test HTTP usano un database EF in memoria e account sintetici, senza accedere a PostgreSQL o avviare il worker di importazione.

## Notifiche temporanee

Le conferme non critiche, come l'uscita dall'account, usano il partial `_Notification` e `NotificationViewModel`. Con JavaScript appaiono in basso a destra, senza spostare il contenuto, e si chiudono dopo 6 secondi. Il timer si ferma su hover, focus e quando la scheda non è visibile; riprende dal tempo rimanente. La X usa l'icona SVG esterna e permette la chiusura manuale.

La durata è impostabile per ogni notifica, in millisecondi; `0` disattiva la chiusura automatica:

```cshtml
<partial name="_Notification" model='new NotificationViewModel("Operazione completata.", 8000)' />
<partial name="_Notification" model='new NotificationViewModel("Informazione da conservare a video.", 0)' />
```

Il modulo `notifications.js` inizializza tutti i partial e li dispone in una pila. Per contenuti aggiunti dinamicamente espone `initializeNotifications(root)`. Senza JavaScript, il messaggio resta leggibile nel flusso della pagina e la X viene nascosta. Gli errori dei form non usano questa notifica temporanea.

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
| `GetProfileByIdAsync` / `GetProfileByNicknameAsync` `banned` flag | `ForumProfile.IsBanned` (`UserProfile.Banned != 0`) |
| `GetProfileByIdAsync` / `GetProfileByNicknameAsync` | oggetto profilo indicizzato da una chiave JSON dinamica; nickname, avatar, data di registrazione, contatore messaggi e flag `IsBanned` sono salvati in `ForumProfile`, indipendentemente dalla presenza di un account locale |

`points` è serializzato come stringa e nei payload può essere vuoto: il valore originale viene mantenuto in `RawPoints`, mentre `Points` contiene il valore numerico quando è interpretabile, altrimenti `null`. Il modello non persiste attributi del profilo non usati dal prodotto (ad esempio permessi amministrativi o preferenze UI).
