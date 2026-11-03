using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // Permite guardar datos localmente

namespace reAprovecha.ViewModels
{
    public class vInicioViewModel
    {
        public ICommand NavegarCommand { get; }

        public vInicioViewModel()
        {
            NavegarCommand = new Command(OnNavegar);
        }

        private async void OnNavegar()
        {
            // Verificamos si los datos del usuario ya están almacenados en el teléfono
            bool usuarioRegistrado = Preferences.Default.Get("UsuarioRegistrado", false);
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;

            if (page is null)
            {
                return;
            }

            if (usuarioRegistrado)
            {
                // Navegación jerárquica hacia el menú principal (Dashboard)
                await page.Navigation.PushAsync(new Views.vPrincipal());  
            }
            else
            {
                // Navegación jerárquica hacia la pantalla de registro
                await page.Navigation.PushAsync(new Views.vRegistro());
            }
        }
    }
}
