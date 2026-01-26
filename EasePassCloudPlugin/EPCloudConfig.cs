using System;
using System.Collections.Generic;
using System.Text;

namespace EasePassCloudPlugin
{
    internal class EPCloudConfig
    {
        public string Host { get; set; }
        public bool SaveReadonlyOfflineCopies { get; set; }
        public List<string> AccessTokens { get; set; } = new List<string>();
    }
}
