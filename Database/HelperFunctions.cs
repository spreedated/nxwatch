using System.IO;
using System.Linq;
using System.Reflection;

namespace Database
{
    internal static class HelperFunctions
    {
        public static string LoadEmbeddedSql(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (!name.EndsWith(".sql"))
            {
                name += ".sql";
            }

            Assembly a = typeof(HelperFunctions).Assembly;
            string resourceName = $"Database.Sql.{name}";

            if (!a.GetManifestResourceNames().Any(x => x == resourceName))
            {
                return null;
            }

            using (Stream s = a.GetManifestResourceStream(resourceName))
            {
                using (StreamReader r = new(s))
                {
                    return r.ReadToEnd();
                }
            }
        }
    }
}
