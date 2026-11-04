using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vRegistrarAlimentoViewModel : INotifyPropertyChanged
    {
        // VARIABLES PRIVADAS EN MEMORIA
        private string _nombre;
        private string _cantidad;
        private string _categoriaSeleccionada;
        private DateTime _fechaVencimiento = DateTime.Now;

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
                        await Application.Current.MainPage.DisplayAlert("Cámara", $"Foto capturada: {foto.FileName}", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "Este dispositivo no soporta captura fotográfica.", "OK");
                }
            }
            catch (Exception ex)
            {
                // GESTIÓN DE EXCEPCIONES PARA EVITAR CIERRES INESPERADOS
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo acceder a la cámara: {ex.Message}", "OK");
            }
        }

        // LÓGICA DE GUARDADO Y VALIDACIÓN
        private async void OnGuardar()
        {
            // VALIDACIÓN BÁSICA DE CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Cantidad) || string.IsNullOrWhiteSpace(CategoriaSeleccionada))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Por favor completa todos los campos del alimento.", "Entendido");
                return;
            }

            // VALIDACIÓN DEFENSIVA DE NULOS ANTES DE OPERAR CON LA VISTA
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null)
            {
                return;
            }

            // MENSAJE DE ÉXITO Y RETORNO
            await page.DisplayAlert("Éxito", $"Alimento '{Nombre}' ({Cantidad}) guardado en {CategoriaSeleccionada}.", "Genial");
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