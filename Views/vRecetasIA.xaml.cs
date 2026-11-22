using reAprovecha.ViewModels;

namespace reAprovecha.Views
{
    public partial class vRecetasIA : ContentPage
    {
        private readonly vRecetasIAViewModel _viewModel;

        public vRecetasIA()
        {
            InitializeComponent();
            _viewModel = BindingContext as vRecetasIAViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ACTUALIZAMOS LOS ALIMENTOS PRÓXIMOS A VENCER AL ABRIR LA PANTALLA
            if (_viewModel != null)
            {
                await _viewModel.CargarInventarioPrioritarioAsync();
            }
        }
    }
}
