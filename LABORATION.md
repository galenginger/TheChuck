# Laboration – The Chuck

## Introduktion

Det här projektet är en ASP.NET Core-webbapplikation som hämtar Chuck Norris-skämt från ett öppet API. Projektet är uppbyggt i lager och demonstrerar principer som **Clean Code**, **Dependency Injection** och **Test-Driven Development (TDD)**.

Din uppgift är att förstå projektet, driftsätta det och sedan bygga ut det med hjälp av TDD.

---

## Del 1 – Driftsättning med GitHub Actions

Projektet innehåller redan en CI/CD-pipeline i `.github/workflows/dotnet.yml` som automatiskt bygger, testar och driftsätter applikationen till **MonsterASP.NET** vid varje push.

### Uppgift 1.1 – Skapa ett MonsterASP.NET-konto

1. Gå till [MonsterASP.NET](https://www.monsteraspl.net/) och registrera ett gratis konto.
2. Skapa en ny webbplats i kontrollpanelen. Notera:
   - **Website name** (namnet på din webbplats)
   - **Server computer name** (WebDeploy-server, t.ex. `waws-prod-xxx.publish.azurewebsites.windows.net`)
   - **Username** och **Password** för WebDeploy

### Uppgift 1.2 – Forka och konfigurera repot

1. Forka det här repot till ditt eget GitHub-konto.
2. Gå till **Settings → Secrets and variables → Actions** i ditt fork och lägg till följande secrets:

   | Secret-namn            | Värde                          |
   |------------------------|--------------------------------|
   | `WEBSITE_NAME`         | Namnet på din webbplats        |
   | `SERVER_COMPUTER_NAME` | WebDeploy-serverns adress      |
   | `SERVER_USERNAME`      | Ditt WebDeploy-användarnamn    |
   | `SERVER_PASSWORD`      | Ditt WebDeploy-lösenord        |

### Uppgift 1.3 – Trigga pipelinen

1. Gör en liten förändring i koden (t.ex. ändra en text) och pusha till `main`.
2. Gå till fliken **Actions** i GitHub och följ körningen steg för steg.
3. Verifiera att alla steg (build, publish, test, deploy) är gröna.
4. Öppna din webbplats på MonsterASP.NET och bekräfta att applikationen är igång.

### Frågor att besvara (1–2 meningar vardera)

- Vad händer om ett test misslyckas i pipelinen – driftsätts applikationen ändå?
- I vilken ordning körs stegen i `dotnet.yml`, och varför är den ordningen logisk?
- Vad är syftet med GitHub Secrets istället för att skriva lösenord direkt i YAML-filen?

---

## Del 2 – Kodstruktur och Clean Code

Projektet är organiserat i fyra projekt. Öppna filerna och studera koden innan du besvarar frågorna.

### Projektstruktur

```
TheChuck/              – ASP.NET Core Razor Pages (presentationslager)
TheChuck.Core/         – Domänmodeller (t.ex. Joke)
TheChuck.Infrastructure/  – Tjänster och HTTP-klient
TheChuckTests/         – Enhetstester med MSTest
```

### Uppgift 2.1 – Förklara arkitekturen

Besvara följande frågor skriftligt (3–5 meningar vardera):

1. **Lagerarkitektur** – Varför är koden uppdelad i `Core`,, `Infrastructure` och `TheChuck`? Vad är ansvaret för varje lager? Vad händer om du blandar ihop dem (t.ex. lägger HTTP-anrop direkt i en Razor Page)?

2. **Dependency Injection** – Titta på `Program.cs` och konstruktorn i `IndexModel`. Förklara vad Dependency Injection är och varför `IndexModel` tar emot ett `IJokeService`-interface istället för en konkret `JokeService`.

3. **Interface och testbarhet** – Titta på `IJokeService`, `JokeServiceFake` och `JokeServiceBrokenFake` i testprojektet. Förklara hur interfaces möjliggör att testa `IndexModel` utan att göra riktiga anrop till `api.chucknorris.io`.

4. **Single Responsibility** – Titta på `JokeService` och `WebClient`. Varför är HTTP-anropen separerade till en egen klass (`WebClient`) istället för att ligga direkt i `JokeService`?

### Uppgift 2.2 – Spåra ett anrop genom lagren

Följ flödet från att en användare öppnar startsidan till att ett skämt visas:

```
Webbläsare → IndexModel.OnGet() → JokeService.GetRandomJoke() → WebClient.Get<Joke>() → api.chucknorris.io
```

Rita ett enkelt sekvensdiagram (för hand eller digitalt) som visar anropskedjan och vilka klasser/interface som är inblandade.

---

## Del 3 – Test-Driven Development (TDD)

Använd **Red–Green–Refactor**-cykeln för varje uppgift:

1. **Red** – Skriv ett test som misslyckas (rött).
2. **Green** – Skriv minimal kod för att testet ska bli grönt.
3. **Refactor** – Förbättra koden utan att testerna slutar fungera.

Alla tester ska följa **Arrange–Act–Assert**-mönstret och använda fake-objekt (som de befintliga i `TheChuckTests/Fakes/`).

---

### Uppgift 3.1 – Skämt från kategori

> **User story:**
> *Som användare vill jag kunna surfa till `/?Category=science` för att få ett skämt från en specifik kategori, så att jag kan välja vilken typ av skämt jag ser.*

**Försök själv först.** Utforska koden och se vad som redan finns på plats – du kommer att hitta ledtrådar. Kom ihåg: börja alltid med ett rött test.

<details>
<summary>Ledtrådar och guide (öppna om du kört fast)</summary>

**Vad som redan finns:**
- `Category`-egenskapen i `IndexModel` är redan bunden till querystring via `[BindProperty(SupportsGet = true)]`
- `IJokeService` har redan metoden `GetJokeFromCategory(string category)`
- `JokeServiceFake` implementerar inte `GetJokeFromCategory` ännu – den kastar `NotImplementedException`

**Acceptanskriterier:**
- Om `Category` är satt (inte null eller tom) anropas `GetJokeFromCategory(category)` istället för `GetRandomJoke()`
- Om `Category` är tom anropas `GetRandomJoke()` som vanligt
- Felhanteringen ska fungera på samma sätt som idag

**Vägledning:**

1. Börja med att uppdatera `JokeServiceFake` så att `GetJokeFromCategory` returnerar ett förutsägbart skämt istället för att kasta undantag – annars kan du inte skriva isolerade tester.
2. Skriv minst tre tester i `IndexModelTests` (skriv dem röda först!):
   - `OnGet_WithCategory_ShouldDisplayJokeFromCategory`
   - `OnGet_WithoutCategory_ShouldDisplayRandomJoke`
   - `OnGet_WithCategory_WhenServiceFails_ShouldDisplayErrorMessage`
3. Implementera logiken i `IndexModel.OnGet()` tills alla tester är gröna.

</details>

---

### Uppgift 3.2 – Byt ut namn i skämtet

> **User story:**
> *Som användare vill jag kunna surfa till `/?Who=Björn` för att se skämtet med mitt eget namn istället för "Chuck Norris", så att jag kan dela ett personligt skämt med någon.*

**Försök själv först.** Tänk på i vilken ordning transformationerna bör ske – ordningen spelar roll.

<details>
<summary>Ledtrådar och guide (öppna om du kört fast)</summary>

**Vad som redan finns:**
- `Who`-egenskapen i `IndexModel` är redan bunden till querystring men används inte i `OnGet()`

**Acceptanskriterier:**
- Om `Who` är satt ersätts alla förekomster av `"Chuck Norris"` i skämttexten med värdet i `Who`
- Ersättningen sker *innan* texten görs om till versaler
- Om `Who` är tom visas skämtet oförändrat

**Vägledning:**

1. Skriv tester *innan* du implementerar (rött test → grön kod):
   - `OnGet_WithWho_ShouldReplaceChuckNorrisWithWho`
   - `OnGet_WithoutWho_ShouldNotModifyJokeText`
   - `OnGet_WithWho_ResultShouldBeUppercase`
2. Tips för Arrange i testerna: använd ett skämt med texten `"Chuck Norris can divide by zero"` och testa med `Who = "Björn"`.
3. Implementera logiken i `IndexModel.OnGet()`.

</details>

---

### Uppgift 3.3 – Räkna ord i ett skämt

> **User story:**
> *Som användare vill jag se hur många ord skämtet innehåller direkt på sidan, så att jag snabbt kan avgöra om det är ett kort eller långt skämt.*

**Försök själv först.** Fundera på var räkningen ska ske i flödet och vilka edge cases som finns.

<details>
<summary>Ledtrådar och guide (öppna om du kört fast)</summary>

**Acceptanskriterier:**
- `IndexModel` har en ny egenskap `WordCount` av typen `int`
- `WordCount` innehåller antalet ord i `DisplayText` *efter* att all textbearbetning är klar
- Ord separeras av ett eller flera mellanslag
- En tom sträng ger `WordCount = 0`

**Vägledning:**

1. Skriv tester:
   - `OnGet_ShouldCountWordsInDisplayText`
   - `OnGet_WhenDisplayTextIsEmpty_WordCountShouldBeZero`
   - `OnGet_WordCountShouldReflectDisplayTextAfterTransformations` – verifiera att det är den färdiga `DisplayText` (versaliserad, namn utbytt) som räknas
2. Lägg till egenskapen `WordCount` i `IndexModel` och beräkna värdet i `OnGet()`.
3. Visa `WordCount` i vyn `Index.cshtml` (frivilligt men rekommenderas för att bekräfta att det fungerar i webbläsaren).

</details>

---

### Extrauppgift – testar testerna verkligen rätt sak?

Titta noga på det här testet från facit för uppgift 3.1:

```csharp
[TestMethod]
public async Task OnGet_WithCategory_ShouldDisplayJokeFromCategory()
{
    var joke = new Joke() { Value = "Category joke" };
    var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
    sut.Category = "science";

    await sut.OnGet();

    Assert.AreEqual("CATEGORY JOKE", sut.DisplayText);
}
```

**Fråga:** Bevisar det här testet att `GetJokeFromCategory` faktiskt anropas? Eller skulle testet vara grönt även om implementationen *alltid* anropar `GetRandomJoke`? Varför?

Resonera kring frågan och fundera på hur testet skulle behöva se ut för att verkligen verifiera att rätt metod anropas. Kolla sedan svaret i `FACIT.md`.

---

## Är jag klar?

Gå igenom listan innan du öppnar `FACIT.md`. Bocka av varje punkt du faktiskt genomfört.

**Del 1 – Driftsättning**
- [ ] Jag har skapat ett konto och en webbplats på MonsterASP.NET
- [ ] Actions-pipelinen har körts och alla steg är gröna
- [ ] Applikationen är tillgänglig och fungerar på min MonsterASP.NET-adress
- [ ] Jag har besvarat de tre reflektionsfrågorna skriftligt

**Del 2 – Kodstruktur**
- [ ] Jag har förklarat lagerarkitekturen och vad varje projekt ansvarar för
- [ ] Jag har förklarat vad Dependency Injection är och varför ett interface används
- [ ] Jag har förklarat hur fake-objekt möjliggör testning utan riktiga API-anrop
- [ ] Jag har förklarat varför `WebClient` är en separat klass
- [ ] Jag har ritat ett sekvensdiagram som visar anropskedjan

**Del 3 – TDD**
- [ ] Jag körde `dotnet test` och alla *befintliga* tester var gröna innan jag började
- [ ] Jag skrev ett rött test *innan* jag implementerade varje uppgift
- [ ] Uppgift 3.1: Minst tre nya tester är gröna, `Category`-querystring fungerar
- [ ] Uppgift 3.2: Minst tre nya tester är gröna, `Who`-querystring fungerar
- [ ] Uppgift 3.3: Minst tre nya tester är gröna, `WordCount` beräknas korrekt
- [ ] `dotnet test` – alla tester (gamla och nya) är gröna

---

## Självutvärdering

Öppna `FACIT.md` och jämför dina lösningar och svar. Reflektera sedan kring följande – det finns inga rätt eller fel här, det handlar om din egen förståelse.

**Driftsättning**
- Förstår du vad som händer i varje steg i pipelinen, eller körde du bara igenom den?
- Kan du förklara för någon annan varför testerna måste passera innan deploy sker?

**Kodstruktur**
- Kunde du förklara Dependency Injection med egna ord, utan att titta i koden?
- Förstår du skillnaden mellan ett interface och en konkret klass – och *varför* den skillnaden spelar roll?

**TDD**
- Följde du Red–Green–Refactor, eller skrev du implementationen först och testerna efteråt?
- Testar dina tester *beteende* (vad koden ska göra) eller *implementation* (hur den gör det)?
- Kan du förklara varför testerna inte anropar det riktiga API:et?

> Är det något du fortfarande är osäker på efter att ha läst facit – skriv ned det och ta upp det på nästa genomgång.
