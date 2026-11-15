using reAprovecha.ViewModels;

namespace reAprovecha.Views
{
    public partial class vMisAlimentos : ContentPage
    {
        private vMisAlimentosViewModel _viewModel;

        public vMisAlimentos()
        {
            InitializeComponent();
            _viewModel = BindingContext as vMisAlimentosViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // REFRESCAMOS LOS DATOS DIRECTAMENTE DESDE LA BASE DE DATOS SQLITE AL MOSTRAR LA VISTA
            if (_viewModel != null)
            {
                await _viewModel.CargarAlimentosAsync();
            }
        }
    }
}