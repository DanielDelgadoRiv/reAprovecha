using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using reAprovecha.Models;

namespace reAprovecha.ViewModels
{
    // HEREDAMOS DE INOTIFYPROPERTYCHANGED PARA NOTIFICAR CAMBIOS A LA UI
    public class vMisAlimentosViewModel : INotifyPropertyChanged
    {
        private string _textoBusqueda;

        // COLECCIÓN OBSERVABLE PARA ACTUALIZAR EL COLLECTIONVIEW AUTOMÁTICAMENTE
        public ObservableCollection<AlimentoModel> ListaAlimentos { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set { if (_textoBusqueda != value) { _textoBusqueda = value; OnPropertyChanged(nameof(TextoBusqueda)); } }
        }

        // COMANDOS DE ACCIÓN SOBRE EL INVENTARIO
        public ICommand ConsumirAlimentoCommand { get; }
        public ICommand EliminarAlimentoCommand { get; }

        // CONSTRUCTOR
        public vMisAlimentosViewModel()
        {
            // DATOS DE PRUEBA INICIALES (SIMULANDO BASE DE DATOS LOCAL)
            ListaAlimentos = new ObservableCollection<AlimentoModel>
            {
                new AlimentoModel { Id = 1, Nombre = "Leche Entera", Cantidad = "1 Litro", Categoria = "Lácteos", FechaVencimiento = DateTime.Now.AddDays(2) },
                new AlimentoModel { Id = 2, Nombre = "Tomates Frescos", Cantidad = "500 g", Categoria = "Verduras", FechaVencimiento = DateTime.Now.AddDays(4) },
                new AlimentoModel { Id = 3, Nombre = "Pechuga de Pollo", Cantidad = "1 kg", Categoria = "Carnes", FechaVencimiento = DateTime.Now.AddDays(1) }
            };

            ConsumirAlimentoCommand = new Command<AlimentoModel>(OnConsumirAlimento);
            EliminarAlimentoCommand = new Command<AlimentoModel>(OnEliminarAlimento);
        }

        // LÓGICA PARA MARCAR UN ALIMENTO COMO CONSUMIDO
        private async void OnConsumirAlimento(AlimentoModel alimento)
        {
            if (alimento == null) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            bool confirmar = await page.DisplayAlert("Consumir", $"¿Deseas marcar '{alimento.Nombre}' como consumido?", "Sí", "No");
            if (confirmar)
            {
                ListaAlimentos.Remove(alimento);
                await page.DisplayAlert("Éxito", "El alimento ha sido registrado como consumido.", "OK");
            }
        }

        // LÓGICA PARA ELIMINAR UN ALIMENTO DEL INVENTARIO
        private async void OnEliminarAlimento(AlimentoModel alimento)
        {
            if (alimento == null) return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is null) return;

            bool confirmar = await page.DisplayAlert("Eliminar", $"¿Estás seguro de eliminar '{alimento.Nombre}' del inventario?", "Sí", "No");
            if (confirmar)
            {
                ListaAlimentos.Remove(alimento);
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
