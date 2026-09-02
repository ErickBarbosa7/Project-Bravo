using Npgsql;

namespace BravoBack.Data
{
    // Utilidad para resolver la connection string de forma robusta.
    // Render inyecta automaticamente la variable DATABASE_URL con formato
    // postgres://usuario:password@host:puerto/base. Este helper la convierte
    // a un formato valido para Npgsql y da soporte a variables alternativas.
    public static class ConnectionStringResolver
    {
        // Orden de prioridad para las variables de entorno / configuracion.
        private static readonly string[] EnvKeys =
        {
            "ConnectionStrings__DefaultConnection",
            "DATABASE_URL",
            "POSTGRES_URL",
            "PGDATABASE",
            "ConnectionStrings:DefaultConnection"
        };

        public static string Resolve(IConfiguration configuration)
        {
            foreach (var key in EnvKeys)
            {
                var value = configuration[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var parsed = Normalize(value);
                if (!string.IsNullOrEmpty(parsed))
                {
                    return parsed;
                }
            }

            // Fallback final: el valor de appsettings.json
            return Normalize(configuration.GetConnectionString("DefaultConnection") ?? "");
        }

        // Convierte una URL postgres:// a connection string Npgsql.
        // Tambien acepta una connection string Npgsql ya valida (la deja igual).
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var trimmed = value.Trim();

            // Solo convertimos URLs del estilo postgres:// o postgresql://
            if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(trimmed);

                    var builder = new NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port > 0 ? uri.Port : 5432,
                        Database = uri.LocalPath.TrimStart('/'),
                        Username = uri.UserInfo.Split(':')[0],
                        Password = uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':', 2)[1] : "",
                        SslMode = SslMode.Prefer
                    };

                    return builder.ConnectionString;
                }
                catch (Exception)
                {
                    // Si no se puede parsear, devolvemos vacio para que falle con
                    // un mensaje claro en el arranque en lugar de una cadena corrupta.
                    return "";
                }
            }

            return trimmed;
        }
    }
}
