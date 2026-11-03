using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    // Heredamos de INotifyPropertyChanged para que la UI se entere de los cambios
    public class vRegistroViewModel : INotifyPropertyChanged
    {
        private string _nombreUsuario;

        // 1. Propiedad conectada al Entry de la pantalla (Data Binding)
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

        // 2. Comando conectado al Botón "Registrarse"
        public ICommand RegistrarCommand { get; }

        public vRegistroViewModel()
        {
            // Inicializamos el comando indicando qué método ejecutar
            RegistrarCommand = new Command(OnRegistrar);
        }

        private async void OnRegistrar()
        {
            // Validación básica para evitar nombres vacíos
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Por favor, ingresa tu nombre.", "OK");
                return;
            }

            // 3. Persistencia de datos en el teléfono
            Preferences.Default.Set("UsuarioActual", NombreUsuario);

            // Confirmación visual
            await Application.Current.MainPage.DisplayAlert("¡Bienvenido!", $"Hola {NombreUsuario}, cuenta creada con éxito.", "Empezar");

            // 4. Navegación a la pantalla principal
            await Application.Current.MainPage.Navigation.PushAsync(new vPrincipal());
        }

        // Motor del Data Binding (Notificador de cambios)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
