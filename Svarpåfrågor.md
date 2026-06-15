# Svar på frågor - Del 1

## Uppgift 1.3 - Reflektionsfrågor


### Vad händer om ett test misslyckas i pipelinen - driftsätts applikationen ändå?

Nej, applikationen driftsätts inte. GitHub Actions kör stegen i ordning och stannar vid det första steget som misslyckas. I `dotnet.yml` körs `dotnet test` innan deploy-steget, vilket innebär att om ett test är rött avbryts pipelinen och deploy-steget aldrig startar. Det skyddar mot att trasig kod hamnar i produktion.


### I vilken ordning körs stegen i dotnet.yml, och varför är den ordningen logisk?

Stegen körs i den här ordningen: restore -> build -> publish -> test -> deploy. Restore laddar hem alla beroenden som koden behöver. Build kompilerar koden och fångar syntaxfel tidigt. Publish skapar en produktionsklar version av applikationen. Test kör enhetstesterna och verifierar att logiken fungerar. Deploy skickar den färdiga versionen till MonsterASP.NET, men bara om alla tidigare steg lyckades. Ordningen är logisk eftersom varje steg bygger på att föregående steg lyckades - det vore meningslöst att testa kod som inte ens kompilerar.


### Vad är syftet med GitHub Secrets istället för att skriva lösenord direkt i YAML-filen?

GitHub Secrets lagrar känslig information krypterat och gör den aldrig synlig i koden eller git-historiken. Om man skriver lösenord direkt i YAML-filen hamnar de i repot och kan läsas av alla som har tillgang till koden - nuvarande eller framtida. Med secrets syns värdet bara som `***` i loggar och Actions-körningar, och man kan uppdatera ett lösenord utan att behöva ändra koden.

---

# Svar på frågor - Del 2

## Uppgift 2.1 - Förklara arkitekturen

### 1. Lagerarkitektur

Koden är uppdelad i tre lager för att varje del ska ha ett tydligt och avgränsat ansvar. `TheChuck.Core` innehåller domänmodellen `Joke` - det är "karnan" i applikationen och är helt oberoende av teknik som HTTP eller webbramverk. `TheChuck.Infrastructure` ansvarar för all kommunikation med omvärlden, i det här fallet att hämta data från API:et via `WebClient` och `JokeService`. `TheChuck` (presentationslagret) hanterar bara det användaren ser - det tar emot data från Infrastructure-lagret och visar den på sidan.

Om man blandar ihop lagren, till exempel lägger HTTP-anrop direkt i en Razor Page, blir koden svår att testa och svår att underhålla. Vill man byta ut API:et mot en annan datakälla måste man redigera vyfilen istället för en isolerad tjänst. Lageruppdelningen gör att varje del kan ändras, testas och bytas ut utan att resten påverkas.

---

### 2. Dependency Injection

Dependency Injection (DI) innebär att en klass inte skapar sina egna beroenden - istället får den dem "injectade" utifrån, vanligtvis via konstruktorn. I `Program.cs` registreras `IJokeService -> JokeService` och `IWebClient -> WebClient`, och när ASP.NET skapar en `IndexModel` skickar ramverket automatiskt in en `IJokeService` i konstruktorn.

Anledningen till att `IndexModel` tar emot ett interface (`IJokeService`) istället för den konkreta klassen `JokeService` är att det skapar ett löst beroende. `IndexModel` bryr sig bara om *vad* tjänsten kan göra (hämta skämt), inte *hur* den gör det. Det gör det möjligt att i tester byta ut `JokeService` mot `JokeServiceFake` utan att ändra ett enda tecken i `IndexModel`.

---

### 3. Interface och testbarhet

Ett interface definierar ett kontrakt - det beskriver vilka metoder som måste finnas, men inte hur de är implementerade. `IJokeService` kräver att det finns `GetRandomJoke()` och `GetJokeFromCategory()`, men säger inget om att de måste göra ett riktigt HTTP-anrop.

I testprojektet implementerar `JokeServiceFake` samma interface men returnerar ett förutbestämt skämt direkt från minnet, och `JokeServiceBrokenFake` kastar ett undantag för att simulera ett trasigt API. Eftersom `IndexModel` bara känner till interfacet kan testerna injicera vilken som helst av dessa fakes i konstruktorn - `IndexModel` märker ingen skillnad. Det gör att testerna är snabba, deterministiska och inte beroende av nätverket eller att `api.chucknorris.io` är uppe.

---

### 4. Single Responsibility

Single Responsibility-principen säger att en klass ska ha ett enda ansvarsområde. `JokeService` ansvarar för att *veta vilken URL som ska anropas* och *tolka resultatet* till en `Joke`. Om HTTP-logiken låg där inne också skulle `JokeService` ha två anledningar att ändras: om URL:erna ändras, och om sättet man gör HTTP-anrop på ändras.

Genom att lägga HTTP-logiken i `WebClient` kan man till exempel byta ut hur anrop görs (lägga till timeout, felhantering, autentisering) utan att röra `JokeService`. Det gör också att `WebClient` är återanvändbar för andra typer av anrop i framtiden, och att man kan testa `JokeService` isolerat genom att mocka `IWebClient`.

---

## Uppgift 2.2 - Sekvensdiagram

Flödet ser ut så här när en användare öppnar startsidan:

1. Webbläsaren skickar en GET-förfrågan till `/`
2. ASP.NET anropar `IndexModel.OnGet()`
3. `OnGet()` anropar `JokeService.GetRandomJoke()`
4. `JokeService` anropar `WebClient.Get<Joke>(url)`
5. `WebClient` gör ett HTTP GET-anrop till `api.chucknorris.io`
6. API:et svarar med JSON som deserialiseras till ett `Joke`-objekt
7. Objektet returneras upp genom lagren till `IndexModel`
8. `IndexModel` sätter `DisplayText` och sidan renderas för användaren

Konkreta klasser bakom interfacen: `IJokeService` -> `JokeService`, `IWebClient` -> `WebClient`. I tester byts dessa ut mot `JokeServiceFake` eller `JokeServiceBrokenFake` utan att `IndexModel` märker något.
