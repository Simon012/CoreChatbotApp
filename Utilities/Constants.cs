namespace CoreChatbotApp.Utilities
{
    public class Constants
    {
        public static class SystemConstants
        {
            /// <summary>
            /// Necessary to filter out irrelevant log messages
            /// </summary>
            public const string LoggingPrefix = "log_prefix: ";
            /// <summary>
            /// Necessary when syncing with the Graph API and checking for new / deleted users. Value represents the max. accepted delta.
            /// </summary>
            public const double MaxDeletionRate = 0.07;
        }
        public static class MiddlewareConstants
        {
            public static class ConversationLockMiddleware
            {
                public const string ConversationLockKeyPrefix = "convo_lock_";
                public const int DefaultLockTimeoutInSeconds = 60;
                public const int DefualtTimeForInLockOperationsInSeconds = 30;
                public const int TimeoutForTotalLockingOperationInSeconds = DefaultLockTimeoutInSeconds - DefualtTimeForInLockOperationsInSeconds > 0 ? DefaultLockTimeoutInSeconds - DefualtTimeForInLockOperationsInSeconds : 10;
            }
        }
    }
}
