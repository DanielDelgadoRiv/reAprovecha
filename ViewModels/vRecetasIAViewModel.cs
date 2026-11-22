using System.ComponentModel;
using System.Windows.Input;
using reAprovecha.Data;
using reAprovecha.Models;
using reAprovecha.Services;

namespace reAprovecha.ViewModels
{
    public class vRecetasIAViewModel : INotifyPropertyChanged
    {
        private readonly BaseDatosService _baseDatosService;
        private readonly RecetasIAService _recetasIAService;

        // VARIABLES PRIVADAS EN MEMORIA
        private List<AlimentoModel> _alimentosPorVencer;
        private List<AlimentoModel> _otrosAlimentos;
        private string _resumenPrioritarios;
        private string _ingredientesAdicionales;
        private string _recetaGenerada;
        private bool _estaCargando;
        private string _ultimaRecetaGenerada = string.Empty;
        private bool _tieneRecetaGenerada;

        // PROPIEDADES PÚBLICAS VINCULADAS A LA INTERFAZ
        public string ResumenPrioritarios
        {
            get => _resumenPrioritarios;
            set { if (_resumenPrioritarios != value) { _resumenPrioritarios = value; OnPropertyChanged(nameof(ResumenPrioritarios)); } }
        }

        public string IngredientesAdicionales
        {
            get => _ingredientesAdicionales;
            set { if (_ingredientesAdicionales != value) { _ingredientesAdicionales = value; OnPropertyChanged(nameof(IngredientesAdicionales)); } }
        }

        public string RecetaGenerada
        {
            get => _recetaGenerada;
            set { if (_recetaGenerada != value) { _recetaGenerada = value; OnPropertyChanged(nameof(RecetaGenerada)); } }
        }

        public bool EstaCargando
        {
            get => _estaCargando;
            set { if (_estaCargando != value) { _estaCargando = value; OnPropertyChanged(nameof(EstaCargando)); } }
        }

        public bool TieneRecetaGenerada
        {
            get => _tieneRecetaGenerada;
            set { if (_tieneRecetaGenerada != value) { _tieneRecetaGenerada = value; OnPropertyChanged(nameof(TieneRecetaGenerada)); } }
        }

        // COMANDOS DE LA VISTA
        public ICommand GenerarRecetaCommand { get; }
        public ICommand CargarPrioritariosCommand { get; }
        public ICommand OtraRecetaCommand { get; }

        // CONSTRUCTOR
        public vRecetasIAViewModel()
        {
            _baseDatosService = new BaseDatosService();
            _recetasIAService = new RecetasIAService();
            _alimentosPorVencer = new List<AlimentoModel>();
            _otrosAlimentos = new List<AlimentoModel>();

            ResumenPrioritarios = "Analizando inventario del hogar...";
            RecetaGenerada = "Presiona 'Generar Receta con IA' para obtener una preparación basada en tus alimentos próximos a vencer.";

            GenerarRecetaCommand = new Command(async () => await OnGenerarRecetaAsync());
            CargarPrioritariosCommand = new Command(async () => await CargarInventarioPrioritarioAsync());
            OtraRecetaCommand = new Command(async () => await OnGenerarRecetaAsync(solicitarAlternativa: true));

            _ = CargarInventarioPrioritarioAsync();
        }

        // MÉTODO PARA CONSULTAR SQLITE Y PRIORIZAR LOS ALIMENTOS PRÓXIMOS A VENCER
        public async Task CargarInventarioPrioritarioAsync()
        {
            try
            {
                var todos = await _baseDatosService.ObtenerAlimentosAsync() ?? new List<AlimentoModel>();
                DateTime limiteUrgente = DateTime.Today.AddDays(3);

                _alimentosPorVencer = todos
                    .Where(a => a.FechaVencimiento.Date <= limiteUrgente)
                    .OrderBy(a => a.FechaVencimiento)
                    .ToList();

                _otrosAlimentos = todos
                    .Where(a => a.FechaVencimiento.Date > limiteUrgente)
                    .OrderBy(a => a.FechaVencimiento)
                    .ToList();

                if (_alimentosPorVencer.Any())
                {
                    string nombres = string.Join(", ", _alimentosPorVencer.Select(a => $"{a.Nombre} ({a.FechaVencimiento:dd/MM})"));
                    ResumenPrioritarios = $"Prioritarios a vencer: {nombres}";
                }
                else if (_otrosAlimentos.Any())
                {
                    ResumenPrioritarios = $"No tienes productos urgentes. Disponibles en inventario: {_otrosAlimentos.Count} alimento(s).";
                }
                else
                {
                    ResumenPrioritarios = "Tu inventario está vacío. Agrega alimentos o escribe ingredientes abajo.";
                }
            }
            catch (Exception ex)
            {
                ResumenPrioritarios = $"No se pudo leer el inventario: {ex.Message}";
            }
        }

        // LÓGICA PARA SOLICITAR LA RECETA AL SERVICIO DE IA
        private async Task OnGenerarRecetaAsync(bool solicitarAlternativa = false)
        {
            if (EstaCargando) return;

            try
            {
                EstaCargando = true;
                RecetaGenerada = solicitarAlternativa
                    ? "Buscando otra receta diferente priorizando tus alimentos próximos a vencer..."
                    : "Generando receta inteligente priorizando tus alimentos próximos a vencer...";

                await CargarInventarioPrioritarioAsync();

                string anterior = solicitarAlternativa ? _ultimaRecetaGenerada : string.Empty;

                string resultado = await _recetasIAService.GenerarRecetaAsync(
                    _alimentosPorVencer,
                    _otrosAlimentos,
                    IngredientesAdicionales,
                    anterior);

                RecetaGenerada = resultado;
                _ultimaRecetaGenerada = resultado;
                TieneRecetaGenerada = true;
            }
            catch (Exception ex)
            {
                RecetaGenerada = $"No se pudo generar la receta en este momento: {ex.Message}";
            }
            finally
            {
                EstaCargando = false;
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
