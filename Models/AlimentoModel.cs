namespace reAprovecha.Models
{
    // MODELO DE DATOS PARA CADA ALIMENTO REGISTRADO
    public class AlimentoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaVencimiento { get; set; }

        // PROPIEDADES CALCULADAS PARA MOSTRAR EN LA VISTA
        public string DetalleCantidadCategoria => $"{Cantidad} | Categoría: {Categoria}";
        public string DetalleVencimiento => $"Vence el: {FechaVencimiento:dd/MM/yyyy}";
    }
}
