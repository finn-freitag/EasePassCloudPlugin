using System;
using System.Collections.Generic;
using System.Text;

namespace EasePassCloudPlugin
{
    // VERY IMPORTANT: This is NOT secure encryption.
    // It is merely obfuscation to prevent casual snooping and automated filesystem scanners.
    public static class Obfuscator
    {
        private const char Key = 'E';
        private const string prefix = "plgobf"; // plugin obfuscation // Identifier for obfuscated strings

        public static string Encrypt(string text)
        {
            char[] buffer = text.Select(c => (char)(c ^ Key)).ToArray();
            string xored = new string(buffer);

            return prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(xored));
        }

        public static string Decrypt(string base64Text)
        {
            if (!base64Text.StartsWith(prefix))
                return base64Text;
            base64Text = base64Text.Substring(prefix.Length);

            byte[] data = Convert.FromBase64String(base64Text);
            string xored = Encoding.UTF8.GetString(data);

            char[] buffer = xored.Select(c => (char)(c ^ Key)).ToArray();
            return new string(buffer);
        }
    }
}
