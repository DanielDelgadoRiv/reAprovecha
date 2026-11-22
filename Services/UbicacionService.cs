using reAprovecha.Models;

namespace reAprovecha.Services
{
    public class UbicacionService
    {
        // MÉTODO PARA SOLICITAR PERMISO Y OBTENER LA UBICACIÓN ACTUAL DEL GPS
        public async Task<(Location Ubicacion, string MensajeEstado)> ObtenerUbicacionActualAsync()
        {
            try
            {
                var estadoPermiso = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (estadoPermiso != PermissionStatus.Granted)
                {
                    estadoPermiso = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (estadoPermiso != PermissionStatus.Granted)
                {
                    return (null, "Permiso de ubicación denegado. Actívalo para detectar comercios cercanos.");
                }

                var solicitud = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(12));
                var ubicacion = await Geolocation.Default.GetLocationAsync(solicitud);

                if (ubicacion == null)
                {
                    ubicacion = await Geolocation.Default.GetLastKnownLocationAsync();
                }

                if (ubicacion == null)
                {
                    return (null, "No se pudo obtener la señal GPS. Verifica que la ubicación del dispositivo esté encendida.");
                }

                return (ubicacion, $"Ubicación detectada: {ubicacion.Latitude:F5}, {ubicacion.Longitude:F5}");
            }
            catch (FeatureNotSupportedException)
            {
                return (null, "Este dispositivo no soporta el sensor GPS.");
            }
            catch (FeatureNotEnabledException)
            {
                return (null, "El GPS está desactivado en el dispositivo. Enciéndelo e intenta nuevamente.");
            }
            catch (PermissionException)
            {
                return (null, "No se concedieron permisos para acceder al GPS.");
            }
            catch (Exception ex)
            {
                return (null, $"Error al consultar el GPS: {ex.Message}");
            }
        }

        // MÉTODO PARA GENERAR COMERCIOS Y MERCADOS REFERENCIALES CERCANOS A LAS COORDENADAS OBTENIDAS
        public List<ComercioCercanoModel> ObtenerComerciosCercanos(Location ubicacion)
        {
            if (ubicacion == null)
            {
                return new List<ComercioCercanoModel>();
            }

            return new List<ComercioCercanoModel>
            {
                new ComercioCercanoModel
                {
                    Nombre = "Mercado Municipal / Feria Fresca",
                    Tipo = "Frutas, verduras y lácteos frescos",
                    DistanciaReferencial = "A ~450 m de tu ubicación",
                    Descripcion = "Ideal para conseguir vegetales y productos frescos faltantes para tus recetas.",
                    Latitud = ubicacion.Latitude + 0.0032,
                    Longitud = ubicacion.Longitude + 0.0021
                },
                new ComercioCercanoModel
                {
                    Nombre = "Supermercado y Despensa del Barrio",
                    Tipo = "Abarrotes, granos y condimentos",
                    DistanciaReferencial = "A ~700 m de tu ubicación",
                    Descripcion = "Surte ingredientes básicos como aceites, harinas, pastas y especias.",
                    Latitud = ubicacion.Latitude - 0.0045,
                    Longitud = ubicacion.Longitude + 0.0030
                },
                new ComercioCercanoModel
                {
                    Nombre = "Tienda Orgánica y Productos Locales",
                    Tipo = "Productos naturales y consumo responsable",
                    DistanciaReferencial = "A ~1.1 km de tu ubicación",
                    Descripcion = "Opciones a granel para comprar solo la cantidad exacta que necesitas y evitar desperdicios.",
                    Latitud = ubicacion.Latitude + 0.0065,
                    Longitud = ubicacion.Longitude - 0.0042
                }
            };
        }
    }
}
