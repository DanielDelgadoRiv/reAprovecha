using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using reAprovecha.Data;
using reAprovecha.Models;

namespace reAprovecha.ViewModels
{
    // VIEWMODEL PARA LA PANTALLA DE AVISOS DE ALIMENTOS PRÓXIMOS A VENCER
    public class vAvisosViewModel : INotifyPropertyChanged
    {
        // SERVICIOS Y COLECCIONES
        private readonly BaseDatosService _baseDatosService;
        private string _mensajeEstado;

        public ObservableCollection<AlimentoModel> AlimentosProximos { get; } = new();

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set
            {
                if (_mensajeEstado != value)
                {
                    _mensajeEstado = value;
                    OnPropertyChanged();
                }
            }
        }

        // COMANDOS DE ACCIÓN
        public ICommand ConsumirAlimentoCommand { get; }

        // CONSTRUCTOR
        public vAvisosViewModel()
        {
            _baseDatosService = new BaseDatosService();
            ConsumirAlimentoCommand = new Command<AlimentoModel>(async (alimento) => await EjecutarConsumirAlimentoAsync(alimento));
        }

        // CARGA Y FILTRADO DE ALIMENTOS QUE VENCEN HOY O EN LOS PRÓXIMOS 2 DÍAS
        public async Task CargarAvisosAsync()
        {
            try
            {
                var todos = await _baseDatosService.ObtenerAlimentosAsync();
                var filtrados = todos
                    .Where(a => a.EsProximoAVencer)
                    .OrderBy(a => a.FechaVencimiento)
                    .ToList();

                AlimentosProximos.Clear();
                foreach (var item in filtrados)
                {
                    AlimentosProximos.Add(item);
                }

                MensajeEstado = AlimentosProximos.Count == 0
                    ? "No tienes alimentos próximos a vencer en los próximos 2 días."
                    : $"Tienes {AlimentosProximos.Count} alimento(s) para consumir pronto.";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar avisos: {ex.Message}";
            }
        }

        // LÓGICA PARA MARCAR UN ALIMENTO COMO CONSUMIDO DESDE AVISOS
        private async Task EjecutarConsumirAlimentoAsync(AlimentoModel alimento)
        {
            if (alimento is null)
                return;

            var paginaActual = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (paginaActual is null)
                return;

            bool confirmar = await paginaActual.DisplayAlertAsync("Consumir alimento",
                $"¿Deseas marcar '{alimento.Nombre}' como consumido?",
                "Sí", "No");

            if (!confirmar)
                return;

            await _baseDatosService.EliminarAlimentoAsync(alimento);
            AlimentosProximos.Remove(alimento);

            MensajeEstado = AlimentosProximos.Count == 0
                ? "No tienes alimentos próximos a vencer en los próximos 2 días."
                : $"Tienes {AlimentosProximos.Count} alimento(s) para consumir pronto.";
        }

        // NOTIFICACIÓN DE CAMBIOS MVVM
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
