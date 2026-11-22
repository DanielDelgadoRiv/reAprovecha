using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using reAprovecha.Data;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vPrincipalViewModel : INotifyPropertyChanged
    {
        private readonly BaseDatosService _baseDatosService;
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
        public ICommand IrAvisosCommand { get; }
        public ICommand IrRecetasCommand { get; }
        public ICommand IrUbicacionCommand { get; }
        public ICommand EditarUsuarioCommand { get; }

        // CONSTRUCTOR
        public vPrincipalViewModel()
        {
            _baseDatosService = new BaseDatosService();

            // CARGAMOS EL NOMBRE DEL USUARIO DESDE LAS PREFERENCIAS LOCALES
            string nombre = Preferences.Default.Get("UsuarioActual", "Usuario");
            SaludoUsuario = $"¡Hola, {nombre}!";
            AlertaVencimientoTexto = "Verificando alimentos próximos a vencer...";

            // INICIALIZAMOS LOS COMANDOS DE NAVEGACIÓN
            IrRegistrarAlimentoCommand = new Command(OnIrRegistrarAlimento);
            IrMisAlimentosCommand = new Command(OnIrMisAlimentos);
            IrAvisosCommand = new Command(OnIrAvisos);
            IrRecetasCommand = new Command(OnIrRecetas);
            IrUbicacionCommand = new Command(OnIrUbicacion);
            EditarUsuarioCommand = new Command(OnEditarUsuario);

            // CARGA INICIAL DE ALERTAS DE VENCIMIENTO
            _ = ActualizarAlertaVencimientoAsync();
        }

        // MÉTODO PARA CONSULTAR Y ACTUALIZAR EL BANNER DE ALIMENTOS PRÓXIMOS A VENCER
        public async Task ActualizarAlertaVencimientoAsync()
        {
            try
            {
                var alimentos = await _baseDatosService.ObtenerAlimentosAsync();
                int cantidadUrgentes = alimentos.Count(a => a.EsProximoAVencer);

                AlertaVencimientoTexto = cantidadUrgentes == 0
                    ? "No tienes alimentos próximos a vencer por ahora."
                    : $"Tienes {cantidadUrgentes} alimento(s) próximos a vencer.";
            }
            catch (Exception)
            {
                AlertaVencimientoTexto = "No se pudo verificar el estado de vencimientos.";
            }
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

            await page.Navigation.PushAsync(new vMisAlimentos());
        }

        // LÓGICA DE NAVEGACIÓN HACIA AVISOS DE VENCIMIENTO
        private async void OnIrAvisos()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.Navigation.PushAsync(new vAvisos());
        }

        // LÓGICA DE NAVEGACIÓN HACIA RECETAS IA
        private async void OnIrRecetas()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.Navigation.PushAsync(new vRecetasIA());
        }

        // LÓGICA DE NAVEGACIÓN HACIA UBICACIÓN
        private async void OnIrUbicacion()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            await page.Navigation.PushAsync(new vUbicacion());
        }

        // LÓGICA PARA EDITAR EL NOMBRE DE USUARIO
        private async void OnEditarUsuario()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            // MOSTRAMOS UN DIÁLOGO NATIVO PARA CAPTURAR EL NUEVO NOMBRE
            string nuevoNombre = await page.DisplayPromptAsync(
                "Editar Perfil",
                "Actualiza tu nombre de usuario:",
                initialValue: Preferences.Default.Get("UsuarioActual", "Usuario"),
                maxLength: 30,
                keyboard: Keyboard.Text);

            if (!string.IsNullOrWhiteSpace(nuevoNombre))
            {
                // GUARDAMOS EL NUEVO VALOR EN LAS PREFERENCIAS LOCALES
                Preferences.Default.Set("UsuarioActual", nuevoNombre.Trim());

                // ACTUALIZAMOS LA PROPIEDAD REACTIVA PARA QUE LA VISTA REFLEJE EL CAMBIO AL INSTANTE
                SaludoUsuario = $"¡Hola, {nuevoNombre.Trim()}!";

                //await page.DisplayAlertAsync("Éxito", "Nombre de usuario actualizado correctamente.", "OK");
            }
        }

        // MOTOR DEL DATA BINDING (NOTIFICADOR DE CAMBIOS)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
