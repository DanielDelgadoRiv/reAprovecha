using SQLite;

namespace reAprovecha.Models
{
    // MODELO DE DATOS PARA CADA ALIMENTO REGISTRADO
    public class AlimentoModel
    {
        // IDENTIFICADOR PRINCIPAL Y DATOS PERSISTENTES
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string Cantidad { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string RutaImagen { get; set; }

        // PROPIEDADES CALCULADAS PARA MOSTRAR EN LA VISTA
        [Ignore]
        public int DiasRestantes => (FechaVencimiento.Date - DateTime.Today).Days;

        [Ignore]
        public bool EsProximoAVencer => DiasRestantes <= 2;

        [Ignore]
        public string DetalleCantidadCategoria => $"{Cantidad} | Categoría: {Categoria}";

        [Ignore]
        public string DetalleVencimiento => $"Vence el: {FechaVencimiento:dd/MM/yyyy}";

        [Ignore]
        public string EstadoVencimiento
        {
            get
            {
                if (DiasRestantes < 0) return $"Vencido hace {Math.Abs(DiasRestantes)} día(s)";
                if (DiasRestantes == 0) return "¡Vence HOY!";
                if (DiasRestantes == 1) return "Vence MAÑANA";
                return $"Vence en {DiasRestantes} días";
            }
        }
    }
}
