namespace Sillycore
{
    public struct Constants
    {
        public struct MessageTypes
        {
            public const string Error = "error";
            public const string Info = "info";
            public const string Warning = "warning";
            public const string Success = "success";
        }

        public const string DateTimeProvider = "DateTimeProvider";
        public const string UseShutDownDelay = "UseShutDownDelay";
        public const string ConfigManager = "ConfigManager";
        public const string ServiceCollection = "Services";
        public const string ServiceProvider = "ServiceProvider";
        public const string LoggerFactory = "LoggerFactory";
        public const string Configuration = "Configuration";
        public const string BackgroundJobManager = "BackgroundJobManager";
        public const string ConfigServerAddress = "ConfigServerAddress";
        public const string ConfigServerAppName = "ConfigServerAppName";
        public const string ConfigServerReloadTimer = "ConfigServerReloadTimer";
        public const string ConfigServerReloadTimeInMiliseconds = "ConfigServerReloadTimeInMiliseconds";
        public const string ConfigServerFirstLoadSucceeded = "ConfigServerFirstLoadSucceeded";
        public const string OnStartActions = "OnStartActions";
        public const string OnStopActions = "OnStopActions";
        public const string OnStoppedActions = "OnStoppedActions";
    }
}