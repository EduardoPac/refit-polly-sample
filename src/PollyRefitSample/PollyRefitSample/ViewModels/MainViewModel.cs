using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using PollyRefitSample.Models;
using PollyRefitSample.Services;
using Xamarin.Forms;

namespace PollyRefitSample.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Pokemon> Pokemons { get; set; }
        private IPokemonService _pokemonService;
        public ICommand RefreshCommand;
        
        public MainViewModel()
        {
            Pokemons = new ObservableCollection<Pokemon>();
            _pokemonService = new PokemonService();
            RefreshCommand = new Command(RefreshExecute);
            
            LoadAsync();
        }

        private async void RefreshExecute() => Busy = false;

        public override async Task LoadAsync()
        {
            Busy = true;
            try
            {
                var pokemonsAPI = await _pokemonService.GetPokemonsAsync();

                Pokemons.Clear();

                foreach (var pokemon in pokemonsAPI)
                {
                    Pokemons.Add(pokemon);
                }
                
                Busy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro", ex.Message);
            }
        }

        public static byte[] GetImageStreamFromUrl(string url)
        {
            try
            {
                using (var webClient = new HttpClient())
                {
                    var imageBytes = webClient.GetByteArrayAsync(url).Result;

                    return imageBytes;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return null;
            }
        }
    }
}