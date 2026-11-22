using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using reAprovecha.Models;
using reAprovecha.Services;

namespace reAprovecha.ViewModels
{
    public class vUbicacionViewModel : INotifyPropertyChanged
    {
        private readonly UbicacionService _ubicacionService;
        private Location _ubicacionActual;

        // VARIABLES PRIVADAS EN MEMORIA
        private string _estadoUbicacion;
        private string _ingredienteBuscado;
        private bool _estaCargando;

        // COLECCIÓN OBSERVABLE DE COMERCIOS CERCANOS
        public ObservableCollection<ComercioCercanoModel> ListaComercios { get; set; }

        // PROPIEDADES PÚBLICAS VINCULADAS A LA INTERFAZ
        public string EstadoUbicacion
        {
            get => _estadoUbicacion;
            set { if (_estadoUbicacion != value) { _estadoUbicacion = value; OnPropertyChanged(nameof(EstadoUbicacion)); } }
        }

        public string IngredienteBuscado
        {
            get => _ingredienteBuscado;
            set { if (_ingredienteBuscado != value) { _ingredienteBuscado = value; OnPropertyChanged(nameof(IngredienteBuscado)); } }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set { if (_estaCargando != value) { _estaCargando = value; OnPropertyChanged(nameof(EstaCargando)); } }
        }

        // COMANDOS DE LA VISTA
        public ICommand ActualizarUbicacionCommand { get; }
        public ICommand BuscarEnMapaCommand { get; }
        public ICommand AbrirComercioMapaCommand { get; }

        // CONSTRUCTOR
        public vUbicacionViewModel()
        {
            _ubicacionService = new UbicacionService();
            ListaComercios = new ObservableCollection<ComercioCercanoModel>();
            EstadoUbicacion = "Presiona 'Actualizar Ubicación GPS' para detectar comercios cercanos.";

            ActualizarUbicacionCommand = new Command(async () => await ConsultarUbicacionAsync());
            BuscarEnMapaCommand = new Command(async () => await AbrirBusquedaEnMapaAsync());
            AbrirComercioMapaCommand = new Command<ComercioCercanoModel>(async (comercio) => await AbrirComercioEnMapaAsync(comercio));
        }

        // MÉTODO PARA OBTENER COORDENADAS Y LISTAR COMERCIOS CERCANOS
        public async Task ConsultarUbicacionAsync()
        {
            if (EstaCargando) return;

            try
            {
                EstaCargando = true;
                EstadoUbicacion = "Consultando sensor GPS...";

                var resultado = await _ubicacionService.ObtenerUbicacionActualAsync();
                _ubicacionActual = resultado.Ubicacion;
                EstadoUbicacion = resultado.MensajeEstado;

                ListaComercios.Clear();
                if (_ubicacionActual != null)
                {
                    var cercanos = _ubicacionService.ObtenerComerciosCercanos(_ubicacionActual);
                    foreach (var comercio in cercanos)
                    {
                        ListaComercios.Add(comercio);
                    }
                }
            }
            finally
            {
                EstaCargando = false;
            }
        }

        // LÓGICA PARA BUSCAR INGREDIENTES FALTANTES EN LA APP DE MAPAS DEL TELÉFONO
        private async Task AbrirBusquedaEnMapaAsync()
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            try
            {
                string termino = string.IsNullOrWhiteSpace(IngredienteBuscado)
                    ? "supermercado o mercado"
                    : $"comprar {IngredienteBuscado}";

                string uriMapa = _ubicacionActual != null
                    ? $"geo:{_ubicacionActual.Latitude.ToString(CultureInfo.InvariantCulture)},{_ubicacionActual.Longitude.ToString(CultureInfo.InvariantCulture)}?q={Uri.EscapeDataString(termino)}"
                    : $"geo:0,0?q={Uri.EscapeDataString(termino)}";

                await Launcher.Default.OpenAsync(new Uri(uriMapa));
            }
            catch (Exception ex)
            {
                if (page != null)
                {
                    await page.DisplayAlertAsync("Aviso", $"No se pudo abrir la aplicación de mapas: {ex.Message}", "OK");
                }
            }
        }

        // LÓGICA PARA ABRIR UN COMERCIO SUGERIDO DIRECTAMENTE EN EL MAPA
        private async Task AbrirComercioEnMapaAsync(ComercioCercanoModel comercio)
        {
            if (comercio == null) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            try
            {
                string lat = comercio.Latitud.ToString(CultureInfo.InvariantCulture);
                string lon = comercio.Longitud.ToString(CultureInfo.InvariantCulture);
                string uri = $"geo:{lat},{lon}?q={lat},{lon}({Uri.EscapeDataString(comercio.Nombre)})";

                await Launcher.Default.OpenAsync(new Uri(uri));
            }
            catch (Exception ex)
            {
                if (page != null)
                {
                    await page.DisplayAlertAsync("Aviso", $"No se pudo abrir el mapa: {ex.Message}", "OK");
                }
            }
        }

        // MOTOR DEL DATA BINDING
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
