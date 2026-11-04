using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    public class vRegistroViewModel : INotifyPropertyChanged
    {
        private string _nombreUsuario;

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                if (_nombreUsuario != value)
                {
                    _nombreUsuario = value;
                    OnPropertyChanged(nameof(NombreUsuario));
                }
            }
        }

        public ICommand RegistrarCommand { get; }

        // CONSTRUCTOR
        public vRegistroViewModel()
        {
            // Inicializamos el comando indicando qué método ejecutar
            RegistrarCommand = new Command(OnRegistrar);
        }

        // LÓGICA DE NEGOCIO Y REGISTRO
        private async void OnRegistrar()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Por favor, ingresa tu nombre.", "OK");
                return;
            }

            Preferences.Default.Set("UsuarioActual", NombreUsuario);
            Preferences.Default.Set("UsuarioRegistrado", true);

            await Application.Current.MainPage.DisplayAlert("¡Bienvenido!", $"Hola {NombreUsuario}, cuenta creada con éxito.", "Empezar");

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}