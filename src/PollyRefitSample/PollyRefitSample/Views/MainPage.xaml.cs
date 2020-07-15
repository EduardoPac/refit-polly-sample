using PollyRefitSample.ViewModels;
using Xamarin.Forms;

namespace PollyRefitSample.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            BindingContext = new MainViewModel();
        }
    }
}
