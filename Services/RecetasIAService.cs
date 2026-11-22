using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using reAprovecha.Models;

namespace reAprovecha.Services
{
    public class RecetasIAService
    {
        private readonly HttpClient _httpClient;

        // API KEY DE OPENAI PARA PRUEBAS EN VIVO
        private const string ApiKeyOpenAI = "";
        private const string EndpointOpenAI = "https://api.openai.com/v1/chat/completions";

        public RecetasIAService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(25)
            };
        }

        // MÉTODO PARA GENERAR UNA RECETA PRIORIZANDO ALIMENTOS PRÓXIMOS A VENCER
        public async Task<string> GenerarRecetaAsync(
            List<AlimentoModel> alimentosPorVencer,
            List<AlimentoModel> otrosAlimentos,
            string ingredientesExtra = "",
            string recetaAnterior = "")
        {
            // CONSTRUCCIÓN DE LISTAS DE INGREDIENTES DESDE EL INVENTARIO
            string listaUrgentes = alimentosPorVencer.Any()
                ? string.Join(", ", alimentosPorVencer.Select(a => $"{a.Nombre} ({a.Cantidad})"))
                : "Ninguno urgente";

            string listaDisponibles = otrosAlimentos.Any()
                ? string.Join(", ", otrosAlimentos.Select(a => $"{a.Nombre} ({a.Cantidad})"))
                : "Sin otros ingredientes registrados";

            // SI NO HAY API KEY CONFIGURADA O NO HAY INTERNET, DEVOLVEMOS UNA SUGERENCIA SEGURA SIN CERRAR LA APP
            if (string.IsNullOrWhiteSpace(ApiKeyOpenAI) || Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return GenerarSugerenciaLocalRespaldo(alimentosPorVencer, otrosAlimentos, ingredientesExtra, recetaAnterior);
            }

            try
            {
                string instruccionVariacion = string.IsNullOrWhiteSpace(recetaAnterior)
                    ? string.Empty
                    : $"- IMPORTANTE: Propón una receta DIFERENTE a esta anterior: {recetaAnterior}.\n";

                string promptUsuario =
                    $"Eres un asistente culinario enfocado en reducir el desperdicio de alimentos (ODS 12). " +
                    $"Genera una receta práctica, económica y saludable en español.\n" +
                    $"- Alimentos PRIORITARIOS (próximos a vencer, úsalos como base principal): {listaUrgentes}.\n" +
                    $"- Otros alimentos disponibles en casa: {listaDisponibles}.\n" +
                    $"- Preferencia o ingrediente adicional indicado por el usuario: {(string.IsNullOrWhiteSpace(ingredientesExtra) ? "Ninguno" : ingredientesExtra)}.\n" +
                    instruccionVariacion +
                    $"Estructura la respuesta exactamente con:\n" +
                    $"1. Nombre de la receta\n" +
                    $"2. Ingredientes del hogar utilizados\n" +
                    $"3. Ingredientes faltantes (si hiciera falta alguno básico para comprar)\n" +
                    $"4. Pasos de preparación breves.";

                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new { role = "system", content = "Eres un chef experto en aprovechamiento de alimentos del hogar." },
                        new { role = "user", content = promptUsuario }
                    },
                    temperature = 0.7
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, EndpointOpenAI);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKeyOpenAI);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return GenerarSugerenciaLocalRespaldo(alimentosPorVencer, otrosAlimentos, ingredientesExtra, recetaAnterior);
                }

                string jsonRespuesta = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonRespuesta);
                string contenido = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return string.IsNullOrWhiteSpace(contenido)
                    ? GenerarSugerenciaLocalRespaldo(alimentosPorVencer, otrosAlimentos, ingredientesExtra, recetaAnterior)
                    : contenido.Trim();
            }
            catch
            {
                // GESTIÓN SEGURA DE ERRORES DE CONEXIÓN SIN CERRAR LA APLICACIÓN
                return GenerarSugerenciaLocalRespaldo(alimentosPorVencer, otrosAlimentos, ingredientesExtra, recetaAnterior);
            }
        }

        // RESPUESTA DE RESPALDO CON ALTERNATIVAS VARIADAS CUANDO NO HAY CONEXIÓN O CLAVE API CONFIGURADA
        private string GenerarSugerenciaLocalRespaldo(
            List<AlimentoModel> alimentosPorVencer,
            List<AlimentoModel> otrosAlimentos,
            string ingredientesExtra,
            string recetaAnterior)
        {
            var baseReceta = alimentosPorVencer.Any() ? alimentosPorVencer : otrosAlimentos;
            if (!baseReceta.Any() && string.IsNullOrWhiteSpace(ingredientesExtra))
            {
                return "No tienes alimentos registrados en tu inventario todavía. Registra tus productos o escribe ingredientes arriba para sugerirte una receta.";
            }

            string principales = baseReceta.Any()
                ? string.Join(", ", baseReceta.Take(3).Select(a => a.Nombre))
                : ingredientesExtra;

            string[] alternativas =
            {
                $"Salteado Rápido de Aprovechamiento con {principales}\n\n" +
                $"• Ingredientes priorizados (próximos a vencer): {principales}\n" +
                $"• Ingredientes faltantes sugeridos: Aceite, ajo, sal y especias al gusto.\n\n" +
                $"Preparación:\n" +
                $"1. Lava y corta en trozos medianos {principales}.\n" +
                $"2. Saltea a fuego medio durante 8 a 10 minutos con ajo y condimentos.\n" +
                $"3. Sirve caliente para consumir primero lo próximo a vencer.",

                $"Tortilla o Revuelto Casero con {principales}\n\n" +
                $"• Ingredientes priorizados (próximos a vencer): {principales}\n" +
                $"• Ingredientes faltantes sugeridos: Huevos o harina base, aceite y sal.\n\n" +
                $"Preparación:\n" +
                $"1. Pica finamente {principales} y sofríe unos minutos.\n" +
                $"2. Incorpora la mezcla batida y cocina a fuego suave hasta cuajar.\n" +
                $"3. Acompaña al gusto y aprovecha el lote antes de su vencimiento.",

                $"Bowl o Ensalada Tibia de Aprovechamiento con {principales}\n\n" +
                $"• Ingredientes priorizados (próximos a vencer): {principales}\n" +
                $"• Ingredientes faltantes sugeridos: Limón o vinagre, aceite de oliva y sal.\n\n" +
                $"Preparación:\n" +
                $"1. Prepara {principales} según su textura (cocido o fresco).\n" +
                $"2. Mezcla en un bowl con un aderezo ligero de limón, aceite y sal.\n" +
                $"3. Consume al momento como opción rápida y nutritiva."
            };

            string elegida = alternativas[0];
            if (!string.IsNullOrWhiteSpace(recetaAnterior))
            {
                int indiceActual = Array.FindIndex(alternativas, item => recetaAnterior.StartsWith(item.Split('\n')[0]));
                int siguienteIndice = indiceActual >= 0 ? (indiceActual + 1) % alternativas.Length : 1;
                elegida = alternativas[siguienteIndice];
            }

            return $"{elegida}\n\n(Nota: Configurar API Key de OpenAI con conexión a Internet para obtener recetas personalizadas con IA).";
        }
    }
}
