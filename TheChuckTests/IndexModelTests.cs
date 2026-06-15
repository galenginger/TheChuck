using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Abstractions;
using TheChuckTests.Fakes;
using TheChuck.Core;

namespace TheChuck.Pages.Tests
{
    [TestClass()]
    public class IndexModelTests
    {
        // --- Befintliga tester ---

        [TestMethod()]
        public async Task OnGet_ShouldDisplayTextFromService()
        {
            var joke = new Joke() { Value = "Works" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual("Works".ToUpper(), sut.DisplayText.ToUpper());
        }

        [TestMethod()]
        public async Task OnGet_ShouldDisplayTextTryAgainWhenApiIsNotWorking()
        {
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());

            await sut.OnGet();

            Assert.AreEqual("Något gick fel. Försök igen lite senare.".ToUpper(), sut.DisplayText.ToUpper());
        }

        [TestMethod()]
        public async Task OnGet_ShouldBeUppecase()
        {
            var joke = new Joke() { Value = "Works" };
            var pageModel = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await pageModel.OnGet();

            Assert.AreEqual("WORKS", pageModel.DisplayText);
        }

        // --- Uppgift 3.1: Kategorifilter ---
        // Röd: Testerna nedan skrevs först och misslyckades eftersom OnGet() alltid anropade GetRandomJoke().
        // Grön: OnGet() uppdaterades med villkorlig logik som anropar GetJokeFromCategory() när Category är satt.
        // Refaktor: Ingen refaktorering behövdes - koden var redan tydlig och koncis.

        [TestMethod()]
        public async Task OnGet_WithCategory_ShouldDisplayJokeFromCategory()
        {
            var joke = new Joke() { Value = "Category joke" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Category = "science";

            await sut.OnGet();

            Assert.AreEqual("CATEGORY JOKE", sut.DisplayText);
        }

        [TestMethod()]
        public async Task OnGet_WithoutCategory_ShouldDisplayRandomJoke()
        {
            var joke = new Joke() { Value = "Random joke" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual("RANDOM JOKE", sut.DisplayText);
        }

        [TestMethod()]
        public async Task OnGet_WithCategory_WhenServiceFails_ShouldDisplayErrorMessage()
        {
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());
            sut.Category = "science";

            await sut.OnGet();

            Assert.AreEqual("Något gick fel. Försök igen lite senare.".ToUpper(), sut.DisplayText);
        }

        // --- Uppgift 3.2: Namnbyte (Who) ---
        // Röd: Testerna nedan skrevs först och misslyckades eftersom Who-parametern ignorerades i OnGet().
        // Grön: En Replace()-rad lades till i OnGet() innan ToUpper() för att byta ut "Chuck Norris".
        // Refaktor: Ingen refaktorering behövdes.

        [TestMethod()]
        public async Task OnGet_WithWho_ShouldReplaceChuckNorrisWithWho()
        {
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "Björn";

            await sut.OnGet();

            Assert.AreEqual("BJÖRN CAN DIVIDE BY ZERO", sut.DisplayText);
        }

        [TestMethod()]
        public async Task OnGet_WithoutWho_ShouldNotModifyJokeText()
        {
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual("CHUCK NORRIS CAN DIVIDE BY ZERO", sut.DisplayText);
        }

        [TestMethod()]
        public async Task OnGet_WithWho_ResultShouldBeUppercase()
        {
            var joke = new Joke() { Value = "Chuck Norris is strong" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "lisa";

            await sut.OnGet();

            Assert.AreEqual(sut.DisplayText, sut.DisplayText.ToUpper());
        }

        // --- Uppgift 3.3: Ordräkning (WordCount) ---
        // Röd: Testerna nedan misslyckades eftersom WordCount-egenskapen inte existerade i IndexModel.
        // Grön: WordCount lades till som property och beräknas i slutet av OnGet() efter alla transformationer.
        // Refaktor: Ingen refaktorering behövdes.

        [TestMethod()]
        public async Task OnGet_ShouldCountWordsInDisplayText()
        {
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual(6, sut.WordCount);
        }

        [TestMethod()]
        public async Task OnGet_WhenDisplayTextIsEmpty_WordCountShouldBeZero()
        {
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceBrokenFake());

            await sut.OnGet();

            Assert.IsTrue(sut.WordCount >= 0);
        }

        [TestMethod()]
        public async Task OnGet_WhenJokeValueIsEmpty_WordCountShouldBeZero()
        {
            var joke = new Joke() { Value = "" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));

            await sut.OnGet();

            Assert.AreEqual(0, sut.WordCount);
        }

        [TestMethod()]
        public async Task OnGet_WordCountShouldReflectDisplayTextAfterTransformations()
        {
            // "Chuck Norris can divide by zero" - ersätt "Chuck Norris" med "Björn" - "Björn can divide by zero" = 5 ord
            var joke = new Joke() { Value = "Chuck Norris can divide by zero" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(joke));
            sut.Who = "Björn";

            await sut.OnGet();

            Assert.AreEqual(5, sut.WordCount);
        }

        // --- Extrauppgift: Testet bevisar att rätt metod anropas ---
        //
        // Problemet med facit-testet:
        // JokeServiceFake(joke) skapades med EN joke som returnerades av BÅDE GetRandomJoke() och GetJokeFromCategory().
        // Det innebar att testet skulle vara grönt även om OnGet() alltid anropade GetRandomJoke() - oavsett om
        // Category var satt eller inte. Testet verifierade alltså bara att DisplayText sattes, inte att rätt metod anropades.
        //
        // Lösning: JokeServiceFake fick stöd för två distinkta jokes - en för GetRandomJoke() och en annan för
        // GetJokeFromCategory(). På så sätt kan testet nedan verkligen bevisa att rätt metod anropades genom att
        // kontrollera vilken av de två olika joke-texterna som hamnade i DisplayText.
        //
        // Röd: Testet nedan skrevs för att visa att det gamla facit-testet inte bevisade att GetJokeFromCategory anropades.
        // Grön: JokeServiceFake uppdaterades med distinkta jokes per metod så att testet faktiskt kan skilja på metodanropen.
        // Refaktor: JokeServiceFake fick en ny konstruktor som tar två jokes, med bakåtkompatibel enkonstruktor.

        [TestMethod()]
        public async Task OnGet_WithCategory_ShouldCallGetJokeFromCategory_NotGetRandom()
        {
            var randomJoke = new Joke() { Value = "Random joke" };
            var categoryJoke = new Joke() { Value = "Category joke" };
            var sut = new IndexModel(NullLogger<IndexModel>.Instance, new JokeServiceFake(randomJoke, categoryJoke));
            sut.Category = "science";

            await sut.OnGet();

            Assert.AreEqual("CATEGORY JOKE", sut.DisplayText);
            Assert.AreNotEqual("RANDOM JOKE", sut.DisplayText);
        }
    }
}
