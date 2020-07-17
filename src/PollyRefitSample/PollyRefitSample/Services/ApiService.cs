using System;
using System.Threading.Tasks;
using PollyRefitSample.Models;
using Refit;

namespace PollyRefitSample.Services
{
    public interface IApiService
    {
        Task<Pokemon> GetPokemon(int id);
    }
    
    public interface IPokemonFunctions
    {
        [Get("/pokemon/{id}")]
        Task<Pokemon> GetPokemon(int id);
    }
    
    public class ApiService : IApiService
    {
        private readonly IPokemonFunctions _pokemonFunctions;
        private readonly IPollyService _pollyService;
        
        public ApiService()
        {
            _pokemonFunctions = RestService.For<IPokemonFunctions>("https://pokeapi.co/api/v3");
            _pollyService = new PollyService();
        }

        public async Task<Pokemon> GetPokemon(int id)
        {
            try
            {
                var func = new Func<Task<Pokemon>>(() => _pokemonFunctions.GetPokemon(id));
                
                return await _pollyService.WaitAndRetry<Pokemon>(func, 10, 3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}