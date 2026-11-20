using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using reAprovecha.Data;
using reAprovecha.Models;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vRegistrarAlimentoViewModel : INotifyPropertyChanged
    {
        // INSTANCIA DEL SERVICIO DE BASE DE DATOS LOCAL
        private readonly BaseDatosService _baseDatosService;

        // VARIABLES PRIVADAS EN MEMORIA
        private string _nombre;
        private string _cantidad;
        private string _categoriaSeleccionada;
        private DateTime _fechaVencimiento = DateTime.Now;
        private string _rutaImagenLocal;

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

        // LISTA ESTÁTICA PARA EL COMBOBOX (PICKER)
        public List<string> Categorias { get; }

        // COMANDOS PARA LOS BOTONES
        public ICommand TomarFotoCommand { get; }
        public ICommand GuardarCommand { get; }

        // CONSTRUCTOR
        public vRegistrarAlimentoViewModel()
        {
            // INICIALIZAMOS EL SERVICIO SQLITE
            _baseDatosService = new BaseDatosService();

            // CATEGORÍAS DE ALIMENTOS PREDEFINIDAS PARA EL PICKER
            Categorias = new List<string>
            {
                "Frutas", "Verduras", "Lácteos", "Carnes", "Cereales", "Otros"
            };

            TomarFotoCommand = new Command(OnTomarFoto);
            GuardarCommand = new Command(OnGuardar);
        }

        // LÓGICA DE FOTOGRAFÍA (CÁMARA NATIVA)
        private async void OnTomarFoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult foto = await MediaPicker.Default.CapturePhotoAsync();
                    if (foto != null)
                    {
                        // GUARDAR UNA COPIA EN EL ALMACENAMIENTO INTERNO DE LA APP
                        var nuevoArchivo = Path.Combine(FileSystem.AppDataDirectory, foto.FileName);
                        using (var stream = await foto.OpenReadAsync())
                        using (var nuevoStream = File.OpenWrite(nuevoArchivo))
                        {
                            await stream.CopyToAsync(nuevoStream);
                        }

                        _rutaImagenLocal = nuevoArchivo;

                        // ACTUALIZAR EL ESTADO DEL CHECK VISUAL
                        EstadoFotoTexto = "✔ Fotografía adjuntada con éxito";
                        ColorEstadoFoto = Colors.Green;

                        await Application.Current.MainPage.DisplayAlertAsync("Cámara", "Fotografía adjuntada con éxito.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlertAsync("Error", $"No se pudo capturar la foto: {ex.Message}", "OK");
            }
        }

        // PROPIEDADES PARA EL ESTADO VISUAL DE LA FOTOGRAFÍA
        private string _estadoFotoTexto = "📎 No se ha adjuntado ninguna fotografía";
        public string EstadoFotoTexto
        {
            get => _estadoFotoTexto;
            set { if (_estadoFotoTexto != value) { _estadoFotoTexto = value; OnPropertyChanged(nameof(EstadoFotoTexto)); } }
        }

        private Color _colorEstadoFoto = Colors.Gray;
        public Color ColorEstadoFoto
        {
            get => _colorEstadoFoto;
            set { if (_colorEstadoFoto != value) { _colorEstadoFoto = value; OnPropertyChanged(nameof(ColorEstadoFoto)); } }
        }

        // LÓGICA DE GUARDADO Y VALIDACIÓN CON PERSISTENCIA LOCAL SQLITE
        private async void OnGuardar()
        {
            // VALIDACIÓN BÁSICA DE CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Cantidad) || string.IsNullOrWhiteSpace(CategoriaSeleccionada))
            {
                await Application.Current.MainPage.DisplayAlertAsync("Aviso", "Por favor completa todos los campos del alimento.", "Entendido");
                return;
            }

            // VALIDACIÓN DEFENSIVA DE NULOS ANTES DE OPERAR CON LA VISTA
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
            {
                return;
            }

            // CREACIÓN DEL MODELO DE DATOS PARA SQLITE
            var nuevoAlimento = new AlimentoModel
            {
                Nombre = Nombre.Trim(),
                Cantidad = Cantidad.Trim(),
                Categoria = CategoriaSeleccionada,
                FechaVencimiento = FechaVencimiento,
                RutaImagen = _rutaImagenLocal
            };

            // ALMACENAMIENTO FÍSICO EN LA BASE DE DATOS LOCAL
            await _baseDatosService.GuardarAlimentoAsync(nuevoAlimento);

            // MENSAJE DE ÉXITO Y RETORNO
            await page.DisplayAlertAsync("Éxito", $"Alimento '{Nombre}' ({Cantidad}) guardado.", "OK");
            await page.Navigation.PopAsync();
        }

        // MOTOR DEL DATA BINDING (NOTIFICADOR DE CAMBIOS)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}