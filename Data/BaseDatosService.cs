using SQLite;
using reAprovecha.Models;

namespace reAprovecha.Data
{
    // SERVICIO DE GESTIÓN DE BASE DE DATOS LOCAL CON SQLITE
    public class BaseDatosService
    {
        private SQLiteAsyncConnection _database;

        // INICIALIZACIÓN SEGURA DE LA CONEXIÓN Y CREACIÓN OBLIGATORIA DE TABLAS
        private async Task Init()
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "reaprovecha.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // CREACIÓN EXPLÍCITA DE LA TABLA ASEGURANDO LA PERSISTENCIA LOCAL
            await _database.CreateTableAsync<AlimentoModel>();
        }

        // OBTENER TODOS LOS ALIMENTOS REGISTRADOS
        public async Task<List<AlimentoModel>> ObtenerAlimentosAsync()
        {
            await Init();
            return await _database.Table<AlimentoModel>().ToListAsync();
        }

        // GUARDAR O ACTUALIZAR UN ALIMENTO
        public async Task<int> GuardarAlimentoAsync(AlimentoModel alimento)
        {
            await Init();
            if (alimento.Id != 0)
            {
                return await _database.UpdateAsync(alimento);
            }
            else
            {
                return await _database.InsertAsync(alimento);
            }
        }

        // ELIMINAR UN ALIMENTO
        public async Task<int> EliminarAlimentoAsync(AlimentoModel alimento)
        {
            await Init();
            return await _database.DeleteAsync(alimento);
        }
    }
}
