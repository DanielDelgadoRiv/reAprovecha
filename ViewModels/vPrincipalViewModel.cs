using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vPrincipalViewModel : INotifyPropertyChanged
    {
        private string _saludoUsuario;
        private string _alertaVencimientoTexto;

        // PROPIEDADES PÚBLICAS (DATA BINDING)
        public string SaludoUsuario
        {
            get => _saludoUsuario;
            set { if (_saludoUsuario != value) { _saludoUsuario = value; OnPropertyChanged(nameof(SaludoUsuario)); } }
        }

        public string AlertaVencimientoTexto
        {
            get => _alertaVencimientoTexto;
            set { if (_alertaVencimientoTexto != value) { _alertaVencimientoTexto = value; OnPropertyChanged(nameof(AlertaVencimientoTexto)); } }
        }

        // COMANDOS DE NAVEGACIÓN
        public ICommand IrRegistrarAlimentoCommand { get; }
        public ICommand IrMisAlimentosCommand { get; }
        public ICommand IrRecetasCommand { get; }
        public ICommand IrUbicacionCommand { get; }

        // CONSTRUCTOR
        public vPrincipalViewModel()
        {
            // CARGAMOS EL NOMBRE DEL USUARIO DESDE LAS PREFERENCIAS LOCALES
            string nombre = Preferences.Default.Get("UsuarioActual", "Usuario");
            SaludoUsuario = $"¡Hola, {nombre}!";
            AlertaVencimientoTexto = "No tienes alimentos próximos a vencer por ahora.";

            // INICIALIZAMOS LOS COMANDOS DE NAVEGACIÓN
            IrRegistrarAlimentoCommand = new Command(OnIrRegistrarAlimento);
            IrMisAlimentosCommand = new Command(OnIrMisAlimentos);
            IrRecetasCommand = new Command(OnIrRecetas);
            IrUbicacionCommand = new Command(OnIrUbicacion);
        }

        // LÓGICA DE NAVEGACIÓN HACIA REGISTRAR ALIMENTO
        private async void OnIrRegistrarAlimento()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.Navigation.PushAsync(new vRegistrarAlimento());
        }

        // LÓGICA DE NAVEGACIÓN HACIA MIS ALIMENTOS
        private async void OnIrMisAlimentos()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.DisplayAlert("Inventario", "Módulo de listado de alimentos en desarrollo.", "OK");
        }

        // LÓGICA DE NAVEGACIÓN HACIA RECETAS IA
        private async void OnIrRecetas()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.DisplayAlert("Recetas IA", "Módulo de OpenAI en desarrollo.", "OK");
        }

        // LÓGICA DE NAVEGACIÓN HACIA UBICACIÓN
        private async void OnIrUbicacion()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.DisplayAlert("GPS", "Módulo de mapas y ubicaciones en desarrollo.", "OK");
        }

        // MOTOR DEL DATA BINDING (NOTIFICADOR DE CAMBIOS)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
