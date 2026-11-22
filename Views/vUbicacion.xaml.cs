using reAprovecha.ViewModels;

namespace reAprovecha.Views
{
    public partial class vUbicacion : ContentPage
    {
        private readonly vUbicacionViewModel _viewModel;

        public vUbicacion()
        {
            InitializeComponent();
            _viewModel = BindingContext as vUbicacionViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // CONSULTAMOS LA UBICACIÓN GPS AL ENTRAR EN LA PANTALLA
            if (_viewModel != null)
            {
                await _viewModel.ConsultarUbicacionAsync();
            }
        }
    }
}
