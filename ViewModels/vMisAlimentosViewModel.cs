using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using reAprovecha.Data;
using reAprovecha.Models;
using reAprovecha.Views;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vMisAlimentosViewModel : INotifyPropertyChanged
    {
        private readonly BaseDatosService _baseDatosService;
        private string _textoBusqueda;

        // LISTA PRIVADA PARA ALMACENAR TODOS LOS REGISTROS DESDE SQLITE
        private List<AlimentoModel> _listaOriginalAlimentos;

        // COLECCIÓN OBSERVABLE QUE ALIMENTA EL COLLECTIONVIEW EN TIEMPO REAL
        public ObservableCollection<AlimentoModel> ListaAlimentos { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (_textoBusqueda != value)
                {
                    _textoBusqueda = value;
                    OnPropertyChanged(nameof(TextoBusqueda));

                    // EJECUTAMOS EL FILTRADO AUTOMÁTICAMENTE CADA VEZ QUE CAMBIA EL TEXTO
                    FiltrarAlimentos();
                }
            }
        }

        // COMANDOS DE ACCIÓN SOBRE EL INVENTARIO
        public ICommand ConsumirAlimentoCommand { get; }
        public ICommand EliminarAlimentoCommand { get; }
        public ICommand CargarAlimentosCommand { get; }
        public ICommand VerFotoAlimentoCommand { get; }
        public ICommand EditarAlimentoCommand { get; }

        // CONSTRUCTOR
        public vMisAlimentosViewModel()
        {
            _baseDatosService = new BaseDatosService();
            _listaOriginalAlimentos = new List<AlimentoModel>();
            ListaAlimentos = new ObservableCollection<AlimentoModel>();

            // INICIALIZACIÓN DE COMANDOS
            ConsumirAlimentoCommand = new Command<object>(OnConsumirAlimento);
            EliminarAlimentoCommand = new Command<object>(OnEliminarAlimento);
            CargarAlimentosCommand = new Command(async () => await CargarAlimentosAsync());
            VerFotoAlimentoCommand = new Command<object>(OnVerFotoAlimento);
            EditarAlimentoCommand = new Command<object>(OnEditarAlimento);

            // CARGAMOS LOS DATOS REALES DE SQLITE AL INICIALIZAR LA VISTA
            _ = CargarAlimentosAsync();
        }

        // MÉTODO PARA CONSULTAR Y CARGAR LOS ALIMENTOS DESDE LA BASE DE DATOS LOCAL
        public async Task CargarAlimentosAsync()
        {
            try
            {
                var alimentosDesdeDb = await _baseDatosService.ObtenerAlimentosAsync();

                _listaOriginalAlimentos = alimentosDesdeDb ?? new List<AlimentoModel>();

                // REFRESCAMOS EL FILTRO VISUAL
                FiltrarAlimentos();
            }
            catch (Exception ex)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Error", $"No se pudo actualizar el inventario: {ex.Message}", "OK");
                }
            }
        }

        // LÓGICA DE FILTRADO REACTIVO POR NOMBRE O CATEGORÍA
        private void FiltrarAlimentos()
        {
            ListaAlimentos.Clear();

            var resultados = string.IsNullOrWhiteSpace(TextoBusqueda)
                ? _listaOriginalAlimentos
                : _listaOriginalAlimentos.Where(x =>
                    (!string.IsNullOrEmpty(x.Nombre) && x.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.Categoria) && x.Categoria.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))
                  ).ToList();

            foreach (var alimento in resultados)
            {
                ListaAlimentos.Add(alimento);
            }
        }

        // LÓGICA PARA MARCAR UN ALIMENTO COMO CONSUMIDO
        private async void OnConsumirAlimento(object parameter)
        {
            if (parameter is not AlimentoModel alimento) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            bool confirmar = await page.DisplayAlertAsync("Consumir", $"¿Deseas marcar '{alimento.Nombre}' como consumido?", "Sí", "No");
            if (confirmar)
            {
                try
                {
                    // ELIMINACIÓN FÍSICA EN LA BASE DE DATOS SQLITE
                    await _baseDatosService.EliminarAlimentoAsync(alimento);

                    // ACTUALIZACIÓN DE LAS COLECCIONES EN MEMORIA
                    _listaOriginalAlimentos.Remove(alimento);
                    ListaAlimentos.Remove(alimento);

                    await page.DisplayAlertAsync("Éxito", "El alimento ha sido registrado como consumido.", "OK");
                }
                catch (Exception ex)
                {
                    await page.DisplayAlertAsync("Error", $"No se pudo actualizar el registro: {ex.Message}", "OK");
                }
            }
        }

        // LÓGICA PARA ELIMINAR UN ALIMENTO DEL INVENTARIO
        private async void OnEliminarAlimento(object parameter)
        {
            if (parameter is not AlimentoModel alimento) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            bool confirmar = await page.DisplayAlertAsync("Eliminar", $"¿Estás seguro de eliminar '{alimento.Nombre}' del inventario?", "Sí", "No");
            if (confirmar)
            {
                try
                {
                    // ELIMINACIÓN FÍSICA EN LA BASE DE DATOS SQLITE
                    await _baseDatosService.EliminarAlimentoAsync(alimento);

                    // ACTUALIZACIÓN DE LAS COLECCIONES EN MEMORIA
                    _listaOriginalAlimentos.Remove(alimento);
                    ListaAlimentos.Remove(alimento);
                }
                catch (Exception ex)
                {
                    await page.DisplayAlertAsync("Error", $"No se pudo eliminar el registro: {ex.Message}", "OK");
                }
            }
        }

        // LÓGICA PARA VISUALIZAR LA FOTOGRAFÍA DEL ALIMENTO SELECCIONADO
        private async void OnVerFotoAlimento(object parameter)
        {
            // VALIDACIÓN DE PARÁMETRO Y VISTA
            if (parameter is not AlimentoModel alimento) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            // VERIFICACIÓN DE ARCHIVO DE IMAGEN
            if (string.IsNullOrWhiteSpace(alimento.RutaImagen) || !File.Exists(alimento.RutaImagen))
            {
                await page.DisplayAlertAsync("Sin fotografía", $"'{alimento.Nombre}' no tiene una fotografía disponible en el dispositivo.", "OK");
                return;
            }

            // VISTA MODAL DE FOTOGRAFÍA
            try
            {
                var resources = Application.Current?.Resources;
                var paginaFoto = new ContentPage
                {
                    Title = alimento.Nombre,
                    BackgroundColor = resources != null && resources.TryGetValue("VerdeFondo", out var bg) && bg is Color cBg ? cBg : Colors.Black,
                    Content = new Grid
                    {
                        RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                        Padding = 20,
                        Children =
                {
                    new Border
                    {
                        Style = resources != null && resources.TryGetValue("CardBorderStyle", out var st) && st is Style sCard ? sCard : null,
                        BackgroundColor = Colors.White,
                        Padding = 10,
                        Content = new Image
                        {
                            Source = ImageSource.FromFile(alimento.RutaImagen),
                            Aspect = Aspect.AspectFit
                        }
                    },
                    new Button
                    {
                        Text = "Cerrar",
                        Style = resources != null && resources.TryGetValue("BotonVerdeStyle", out var btnSt) && btnSt is Style sBtn ? sBtn : null,
                        Command = new Command(async () => await page.Navigation.PopModalAsync()),
                        Margin = new Thickness(0, 15, 0, 0)
                    }
                }
                    }
                };

                if (paginaFoto.Content is Grid grid && grid.Children.Count > 1 && grid.Children[1] is BindableObject botonCerrar)
                {
                    Grid.SetRow(botonCerrar, 1);
                }

                await page.Navigation.PushModalAsync(paginaFoto);
            }
            catch (Exception ex)
            {
                // MANEJO DE ERRORES
                await page.DisplayAlertAsync("Error", $"No se pudo abrir la fotografía: {ex.Message}", "OK");
            }
        }

        // MÉTODO PARA ABRIR LA PANTALLA DE EDICIÓN DEL ALIMENTO SELECCIONADO
        private async void OnEditarAlimento(object parameter)
        {
            if (parameter is not AlimentoModel alimento) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            try
            {
                // INSTANCIAMOS LA VISTA DE EDICIÓN Y CARGAMOS EL ALIMENTO EN SU VIEWMODEL
                var vistaEditar = new Views.vEditarAlimento();
                if (vistaEditar.BindingContext is vEditarAlimentoViewModel editarViewModel)
                {
                    editarViewModel.CargarAlimento(alimento);
                }

                await page.Navigation.PushModalAsync(vistaEditar);

                // RECARGAMOS EL INVENTARIO DESDE SQLITE AL REGRESAR DE LA EDICIÓN
                await CargarAlimentosAsync();
            }
            catch (Exception ex)
            {
                await page.DisplayAlertAsync("Error", $"No se pudo abrir la edición: {ex.Message}", "OK");
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
