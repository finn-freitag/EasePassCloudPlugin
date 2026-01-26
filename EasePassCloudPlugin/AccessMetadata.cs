using System;
using System.Collections.Generic;
using System.Text;

namespace EasePassCloudPlugin
{
    internal class AccessMetadata
    {
        public string DatabaseName { get; set; }
        public string LastModified { get; set; }
        public bool Locked { get; set; }
        public bool Readonly { get; set; }
    }
}
