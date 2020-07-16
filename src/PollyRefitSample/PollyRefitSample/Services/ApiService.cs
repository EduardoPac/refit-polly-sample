using System;
using System.Collections.Generic;
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
        
        public ApiService()
        {
            _pokemonFunctions = RestService.For<IPokemonFunctions>("https://pokeapi.co/api/v2");
        }


        public async Task<Pokemon> GetPokemon(int id)
        {
            try
            {
                return await _pokemonFunctions.GetPokemon(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}