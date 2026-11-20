using reAprovecha.ViewModels;

namespace reAprovecha.Views
{
    public partial class vAvisos : ContentPage
    {
        private readonly vAvisosViewModel? _viewModel;

        // CONSTRUCTOR
        public vAvisos()
        {
            InitializeComponent();
            _viewModel = BindingContext as vAvisosViewModel;
        }

        // RECARGA REACTIVA DE DATOS AL MOSTRAR LA PANTALLA
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel is not null)
            {
                await _viewModel.CargarAvisosAsync();
            }
        }
    }
}