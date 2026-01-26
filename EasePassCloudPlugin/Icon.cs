using System;
using System.Collections.Generic;
using System.Text;

namespace EasePassCloudPlugin
{
    internal static class Icon
    {
        public static Uri GetIconUri()
        {
            string path = Path.GetTempFileName();
            File.WriteAllBytes(path, Properties.Resources.Icon);
            return new Uri(path);
        }
    }
}
