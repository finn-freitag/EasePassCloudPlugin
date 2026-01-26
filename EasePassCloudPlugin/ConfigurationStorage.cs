using EasePassExtensibility;

namespace EasePassCloudPlugin
{
    public class ConfigurationStorage : IStorageInjectable
    {
        public static ConfigurationStorage? Instance { get; set; }

        private SaveString? _saveString;
        private LoadString? _loadString;
        private SaveFile? _saveFile;
        private LoadFile? _loadFile;

        public SaveString? SaveString
        {
            get => _saveString;
            set => _saveString = value;
        }
        public LoadString? LoadString
        {
            get => _loadString;
            set => _loadString = value;
        }
        public SaveFile? SaveFile
        {
            get => _saveFile;
            set => _saveFile = value;
        }
        public LoadFile? LoadFile
        {
            get => _loadFile;
            set => _loadFile = value;
        }

        public ConfigurationStorage()
        {
            Instance = this;
        }
    }
}
