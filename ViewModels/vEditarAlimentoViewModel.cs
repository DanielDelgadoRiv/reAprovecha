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

        // LISTA ESTÁTICA PARA EL COMBOBOX (PICKER)
        public List<string> Categorias { get; }

        // COMANDOS PARA LOS BOTONES
        public ICommand GuardarCambiosCommand { get; }
        public ICommand CancelarCommand { get; }

        // CONSTRUCTOR CON INYECCIÓN DE DEPENDENCIAS
        public vEditarAlimentoViewModel(BaseDatosService baseDatosService = null)
        {
            // INICIALIZAMOS O REUTILIZAMOS EL SERVICIO SQLITE
            _baseDatosService = baseDatosService ?? new BaseDatosService();

            // CATEGORÍAS DE ALIMENTOS PREDEFINIDAS PARA EL PICKER
            Categorias = new List<string>
            {
                "Frutas", "Verduras", "Lácteos", "Carnes", "Cereales", "Otros"
            };

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
