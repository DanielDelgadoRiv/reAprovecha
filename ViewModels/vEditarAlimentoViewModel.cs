using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using reAprovecha.Data;
using reAprovecha.Models;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vEditarAlimentoViewModel : INotifyPropertyChanged
    {
        // INSTANCIA DEL SERVICIO DE BASE DE DATOS LOCAL
        private readonly BaseDatosService _baseDatosService;

        // REFERENCIA AL REGISTRO ORIGINAL PARA CONSERVAR SU ID EN SQLITE
        private AlimentoModel _alimentoOriginal;

        // VARIABLES PRIVADAS EN MEMORIA
        private string _nombre;
        private string _cantidad;
        private string _categoriaSeleccionada;
        private DateTime _fechaVencimiento = DateTime.Now;
        private string _rutaImagenLocal;
        private string _estadoFotoTexto;
        private Color _colorEstadoFoto = Colors.Gray;

        // PROPIEDADES PÚBLICAS VINCULADAS A LA INTERFAZ (DATA BINDING)
        public string Nombre
        {
            get => _nombre;
            set { if (_nombre != value) { _nombre = value; OnPropertyChanged(nameof(Nombre)); } }
        }

        public string Cantidad
        {
            get => _cantidad;
            set { if (_cantidad != value) { _cantidad = value; OnPropertyChanged(nameof(Cantidad)); } }
        }

        public string CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set { if (_categoriaSeleccionada != value) { _categoriaSeleccionada = value; OnPropertyChanged(nameof(CategoriaSeleccionada)); } }
        }

        public DateTime FechaVencimiento
        {
            get => _fechaVencimiento;
            set { if (_fechaVencimiento != value) { _fechaVencimiento = value; OnPropertyChanged(nameof(FechaVencimiento)); } }
        }

        public string RutaImagenLocal
        {
            get => _rutaImagenLocal;
            set { if (_rutaImagenLocal != value) { _rutaImagenLocal = value; OnPropertyChanged(nameof(RutaImagenLocal)); } }
        }

        public string EstadoFotoTexto
        {
            get => _estadoFotoTexto;
            set { if (_estadoFotoTexto != value) { _estadoFotoTexto = value; OnPropertyChanged(nameof(EstadoFotoTexto)); } }
        }

        public Color ColorEstadoFoto
        {
            get => _colorEstadoFoto;
            set { if (_colorEstadoFoto != value) { _colorEstadoFoto = value; OnPropertyChanged(nameof(ColorEstadoFoto)); } }
        }

        // LISTA ESTÁTICA PARA EL COMBOBOX (PICKER)
        public List<string> Categorias { get; }

        // COMANDOS PARA LOS BOTONES
        public ICommand TomarFotoCommand { get; }
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CancelarCommand { get; }

        // CONSTRUCTOR PÚBLICO SIN PARÁMETROS PARA ENLACE DIRECTO EN XAML
        public vEditarAlimentoViewModel()
        {
            // INICIALIZAMOS EL SERVICIO SQLITE
            _baseDatosService = new BaseDatosService();

            // CATEGORÍAS DE ALIMENTOS PREDEFINIDAS PARA EL PICKER
            Categorias = new List<string>
            {
                "Frutas", "Verduras", "Lácteos", "Carnes", "Cereales", "Otros"
            };

            TomarFotoCommand = new Command(OnTomarFoto);
            ActualizarEstadoFoto();
            GuardarCambiosCommand = new Command(OnGuardarCambios);
            CancelarCommand = new Command(OnCancelar);
        }

        // CARGA INICIAL DE LOS DATOS DEL ALIMENTO SELECCIONADO
        public void CargarAlimento(AlimentoModel alimento)
        {
            if (alimento is null)
            {
                return;
            }

            _alimentoOriginal = alimento;
            Nombre = alimento.Nombre;
            Cantidad = alimento.Cantidad;
            CategoriaSeleccionada = string.IsNullOrWhiteSpace(alimento.Categoria) ? "Otros" : alimento.Categoria;
            FechaVencimiento = alimento.FechaVencimiento == default ? DateTime.Now : alimento.FechaVencimiento;
            RutaImagenLocal = alimento.RutaImagen;
            ActualizarEstadoFoto();
        }

        // MÉTODO PARA ACTUALIZAR EL INDICADOR CON CHECK DE FOTOGRAFÍA ADJUNTA
        private void ActualizarEstadoFoto()
        {
            bool tieneFoto = !string.IsNullOrWhiteSpace(RutaImagenLocal) && File.Exists(RutaImagenLocal);

            if (tieneFoto)
            {
                EstadoFotoTexto = "✔ Fotografía adjunta";
                ColorEstadoFoto = Colors.SeaGreen;
            }
            else
            {
                EstadoFotoTexto = "📎 No se ha adjuntado ninguna fotografía";
                ColorEstadoFoto = Colors.Gray;
            }
        }

        // LÓGICA PARA TOMAR O SELECCIONAR FOTOGRAFÍA DEL ALIMENTO
        private async void OnTomarFoto()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            try
            {
                string opcion = await page.DisplayActionSheetAsync(
                    "Fotografía del alimento",
                    "Cancelar",
                    null,
                    "Tomar foto con cámara",
                    "Elegir de galería");

                if (string.IsNullOrWhiteSpace(opcion) || opcion == "Cancelar")
                {
                    return;
                }

                FileResult archivo = null;

                if (opcion == "Tomar foto con cámara")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        archivo = await MediaPicker.Default.CapturePhotoAsync();
                    }
                }
                else if (opcion == "Elegir de galería")
                {
                    var resultados = await MediaPicker.Default.PickPhotosAsync();
                    archivo = resultados?.FirstOrDefault();
                }

                if (archivo != null)
                {
                    string nombreArchivo = $"alimento_{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
                    string rutaDestino = Path.Combine(FileSystem.AppDataDirectory, nombreArchivo);

                    await using var streamOrigen = await archivo.OpenReadAsync();
                    await using var streamDestino = File.OpenWrite(rutaDestino);
                    await streamOrigen.CopyToAsync(streamDestino);

                    RutaImagenLocal = rutaDestino;
                    ActualizarEstadoFoto();
                }
            }
            catch (PermissionException)
            {
                await page.DisplayAlertAsync("Permiso requerido", "Debes conceder permiso para acceder a la cámara o galería.", "OK");
            }
            catch (Exception ex)
            {
                await page.DisplayAlertAsync("Error", $"No se pudo actualizar la fotografía: {ex.Message}", "OK");
            }
        }

        // LÓGICA DE ACTUALIZACIÓN Y VALIDACIÓN CON PERSISTENCIA LOCAL SQLITE
        private async void OnGuardarCambios()
        {
            // VALIDACIÓN BÁSICA DE CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Cantidad) || string.IsNullOrWhiteSpace(CategoriaSeleccionada))
            {
                var paginaAlerta = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (paginaAlerta is not null)
                {
                    await paginaAlerta.DisplayAlertAsync("Aviso", "Por favor completa todos los campos del alimento.", "Entendido");
                }
                return;
            }

            // VALIDACIÓN DEFENSIVA DE NULOS ANTES DE OPERAR CON LA VISTA Y EL MODELO
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null || _alimentoOriginal is null)
            {
                return;
            }

            // ACTUALIZACIÓN DE CAMPOS DEL MODELO
            _alimentoOriginal.Nombre = Nombre.Trim();
            _alimentoOriginal.Cantidad = Cantidad.Trim();
            _alimentoOriginal.Categoria = CategoriaSeleccionada;
            _alimentoOriginal.FechaVencimiento = FechaVencimiento;
            _alimentoOriginal.RutaImagen = RutaImagenLocal;

            // ALMACENAMIENTO FÍSICO EN LA BASE DE DATOS LOCAL (UPDATE POR ID != 0)
            await _baseDatosService.GuardarAlimentoAsync(_alimentoOriginal);

            // MENSAJE DE ÉXITO Y RETORNO
            await page.DisplayAlertAsync("Éxito", $"Alimento '{Nombre}' ({Cantidad}) actualizado.", "OK");

            if (page.Navigation.ModalStack.Count > 0)
            {
                await page.Navigation.PopModalAsync();
            }
            else if (page.Navigation.NavigationStack.Count > 1)
            {
                await page.Navigation.PopAsync();
            }
        }

        // LÓGICA PARA CANCELAR LA EDICIÓN SIN GUARDAR CAMBIOS
        private async void OnCancelar()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
            {
                return;
            }

            if (page.Navigation.ModalStack.Count > 0)
            {
                await page.Navigation.PopModalAsync();
            }
            else if (page.Navigation.NavigationStack.Count > 1)
            {
                await page.Navigation.PopAsync();
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
