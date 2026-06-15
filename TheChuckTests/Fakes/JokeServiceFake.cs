using TheChuck.Core;

namespace TheChuckTests.Fakes
{
    internal class JokeServiceFake : IJokeService
    {
        private readonly Joke _randomJoke;
        private readonly Joke _categoryJoke;

        public JokeServiceFake(Joke joke) : this(joke, joke) { }

        public JokeServiceFake(Joke randomJoke, Joke categoryJoke)
        {
            _randomJoke = randomJoke;
            _categoryJoke = categoryJoke;
        }

        public Task<Joke?> GetRandomJoke() => Task.FromResult<Joke?>(_randomJoke);

        public Task<Joke?> GetJokeFromCategory(string category) => Task.FromResult<Joke?>(_categoryJoke);
    }
}
