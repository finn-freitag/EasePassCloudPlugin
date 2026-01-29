using EasePassExtensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasePassCloudPlugin
{
    public class Logger : ILoggerInjectable
    {
        private static ILogger LoggerF = null;
        ILogger ILoggerInjectable.Logger { get => LoggerF; set => LoggerF = value; }

        const string prefix = "[epcloud] ";

        public static void Log(string message)
        {
            LoggerF.Log(prefix + message);
        }

        public static void LogError(string message)
        {
            LoggerF.LogError(prefix + message);
        }

        public static void LogException(Exception exception)
        {
            LoggerF.LogException(exception);
        }

        public static void LogWarning(string message)
        {
            LoggerF.LogWarning(prefix + message);
        }
    }
}
