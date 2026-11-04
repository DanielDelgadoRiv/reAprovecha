using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vRegistroViewModel : INotifyPropertyChanged
    {
        private string _nombreUsuario;

        // PROPIEDAD CONECTADA AL ENTRY DE LA VISTA
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

        // COMANDO CONECTADO AL BOTÓN REGISTRARSE
        public ICommand RegistrarCommand { get; }

        // CONSTRUCTOR
        public vRegistroViewModel()
        {
            RegistrarCommand = new Command(OnRegistrar);
        }

        // LÓGICA DE NEGOCIO Y REGISTRO
        private async void OnRegistrar()
        {
            // VALIDACIÓN BÁSICA PARA EVITAR CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Por favor, ingresa tu nombre.", "OK");
                return;
            }

            // PERSISTENCIA DE DATOS LOCALES EN EL DISPOSITIVO
            Preferences.Default.Set("UsuarioActual", NombreUsuario);
            Preferences.Default.Set("UsuarioRegistrado", true);

            // MENSAJE DE CONFIRMACIÓN VISUAL
            await Application.Current.MainPage.DisplayAlert("¡Bienvenido!", $"Hola {NombreUsuario}, cuenta creada con éxito.", "Empezar");

            // VALIDACIÓN DEFENSIVA DE NULOS ANTES DE LA NAVEGACIÓN JERÁRQUICA
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
            {
                return;
            }

            // NAVEGACIÓN HACIA LA PANTALLA PRINCIPAL
            await page.Navigation.PushAsync(new vPrincipal());
        }

        // MOTOR DEL DATA BINDING (NOTIFICADOR DE CAMBIOS)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}