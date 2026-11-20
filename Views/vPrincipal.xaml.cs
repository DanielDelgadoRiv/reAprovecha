using reAprovecha.ViewModels;

namespace reAprovecha.Views
{
    // CÓDIGO DE RESPALDO PARA LA PANTALLA PRINCIPAL
    public partial class vPrincipal : ContentPage
    {
        // CONSTRUCTOR
        public vPrincipal()
        {
            InitializeComponent();
        }

        // ACTUALIZACIÓN DE ALERTAS AL REGRESAR A LA VISTA
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is vPrincipalViewModel viewModel)
            {
                await viewModel.ActualizarAlertaVencimientoAsync();
            }
        }
    }
}
