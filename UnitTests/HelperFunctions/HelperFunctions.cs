using System.IO;

namespace UnitTests.HelperFunctions
{
    internal static class HelperFunctions
    {
        public static string GetEmbeddedHtml(string resourceName)
        {
            if (!resourceName.EndsWith(".htm"))
            {
                resourceName += ".htm";
            }

            using (Stream fileStream = typeof(HelperFunctions).Assembly.GetManifestResourceStream($"UnitTests.ExampleHtmls.{resourceName}"))
            {
                if (fileStream == null)
                {
                    return null;
                }

                using (StreamReader sr = new(fileStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static string GetEmbeddedText(string resourceName)
        {
            using (Stream fileStream = typeof(HelperFunctions).Assembly.GetManifestResourceStream($"UnitTests.Testfiles.{resourceName}"))
            {
                if (fileStream == null)
                {
                    return null;
                }

                using (StreamReader sr = new(fileStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
