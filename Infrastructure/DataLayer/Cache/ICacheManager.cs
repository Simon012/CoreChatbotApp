using System.Threading.Tasks;
using System;
using CoreChatbotApp.Utilities;

namespace CoreChatbotApp.Infrastructure.DataLayer.Cache
{
    public enum StorageType
    {
        ConversationReference,
        ConversationLock,
        LastMessage,
        UserState,
        ConversationStep
    }

    public interface ICacheManager
    {
        Task<bool> FlushCache();
        Task<string> GetStringAsync(StorageType storageType, string UserIdAad);
        Task<bool> SetStringAsync(StorageType storageType, string UserIdAad, string value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(StorageType storageType, string UserIdAad);

        Task<bool> AcquireLockAsync(string UserIdAad, int timeoutInSeconds = Constants.MiddlewareConstants.ConversationLockMiddleware.DefaultLockTimeoutInSeconds);
        Task<bool> ReleaseLockAsync(string UserIdAad);

        string TimestampString(string str);
        (string str, DateTime? timestamp) UntimestampString(string str);
    }
}
