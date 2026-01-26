using EasePassExtensibility;

namespace EasePassCloudPlugin
{
    public class AboutPage : IAboutPlugin
    {
        public string PluginName => "Ease Pass Cloud";

        public string PluginDescription => "This plugin enables you to host your password databases on your server to use them on multiple devices.";

        public string PluginAuthor => "Finn Freitag";

        public string PluginAuthorURL => "https://finnfreitag.com?ref=ep_plgn_cloud";

        public Uri PluginIcon => Icon.GetIconUri();
    }
}
