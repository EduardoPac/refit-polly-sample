using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PollyRefitSample.Models;

namespace PollyRefitSample.Services
{
    public interface IPokemonService
    {
        Task<List<Pokemon>> GetPokemonsAsync();
    }

    public class PokemonService : IPokemonService
    {
        private readonly IApiService _apiService; 

        public PokemonService()
        {
            _apiService = new ApiService();
        }

        public async Task<List<Pokemon>> GetPokemonsAsync()
        {
            var pokemons = new List<Pokemon>();

            for (var i = 1; i < 100; i++)
            {
                pokemons.Add(await _apiService.GetPokemon(i));
            }

            return pokemons.ToList();
        }
    }
}