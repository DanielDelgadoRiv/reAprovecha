using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using reAprovecha.Models;

namespace reAprovecha.ViewModels;

// VIEWMODEL PARA EDITAR UN ALIMENTO REGISTRADO
public partial class vEditarAlimentoViewModel : ObservableObject
{
    // CAMPOS Y PROPIEDADES OBSERVABLES DEL FORMULARIO
    [ObservableProperty]
    private AlimentoModel alimentoOriginal;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string cantidadTexto = string.Empty;

    [ObservableProperty]
    private string categoriaSeleccionada = string.Empty;

    [ObservableProperty]
    private DateTime fechaVencimiento = DateTime.Today;

    [ObservableProperty]
    private string rutaImagen = string.Empty;

    // COLECCIÓN DE CATEGORÍAS DISPONIBLES
    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Frutas",
        "Verduras",
        "Lácteos",
        "Carnes",
        "Cereales",
        "Otros"
    };

    // INICIALIZACIÓN DE DATOS DEL ALIMENTO A EDITAR
    public void CargarAlimento(AlimentoModel alimento)
    {
        AlimentoOriginal = alimento;
        Nombre = alimento.Nombre;
        CantidadTexto = alimento.Cantidad.ToString();
        CategoriaSeleccionada = string.IsNullOrWhiteSpace(alimento.Categoria) ? "Otros" : alimento.Categoria;
        FechaVencimiento = alimento.FechaVencimiento == default ? DateTime.Today : alimento.FechaVencimiento;
        RutaImagen = alimento.RutaImagen ?? string.Empty;
    }

    // COMANDO PARA GUARDAR CAMBIOS EN SQLITE
    [RelayCommand]
    private async Task GuardarCambiosAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null || alimentoOriginal is null) return;

        // VALIDACIÓN DE CAMPOS OBLIGATORIOS
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            await page.DisplayAlertAsync("Validación", "Ingresa el nombre del alimento.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CantidadTexto))
        {
            await page.DisplayAlertAsync("Validación", "Ingresa una cantidad válida.", "OK");
            return;
        }

        // ACTUALIZACIÓN DE PROPIEDADES DEL MODELO
        alimentoOriginal.Nombre = Nombre.Trim();
        alimentoOriginal.Cantidad = CantidadTexto.Trim();
        alimentoOriginal.Categoria = CategoriaSeleccionada;
        alimentoOriginal.FechaVencimiento = FechaVencimiento.Date;
        alimentoOriginal.RutaImagen = RutaImagen;

        // SE INVOCA MÉTODO DE ACTUALIZACIÓN EN SQLITE
        // PROXIMA NAVEGACION
        await page.Navigation.PopModalAsync();
    }

    // COMANDO PARA CANCELAR Y CERRAR SIN GUARDAR
    [RelayCommand]
    private async Task CancelarAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null) return;

        await page.Navigation.PopModalAsync();
    }
}
