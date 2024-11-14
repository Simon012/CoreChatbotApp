using CoreChatbotApp.Infrastructure.DataLayer.Cache;
using CoreChatbotApp.Logic.UserStateManagement;
using CoreChatbotApp.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CoreChatbotApp.Infrastructure.DataLayer
{
    public class DataManager : IDataManager
    {
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;

        public DataManager(ICacheManager cacheManager, ILogger<DataManager> logger)
        {
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public Task<bool> IsConversationUnlockedAsync(string userIdAad, int? timeoutInSeconds)
        {
            if (timeoutInSeconds.HasValue && timeoutInSeconds.Value > 0)
            {
                return _cacheManager.AcquireLockAsync(userIdAad, timeoutInSeconds.Value);
            }
            else
            {
                return _cacheManager.AcquireLockAsync(userIdAad);
            }
        }

        public Task<bool> UnlockConversationAsync(string userIdAad)
        {
            return _cacheManager.ReleaseLockAsync(userIdAad);
        }

        public async Task<UserState> RetrieveOrCreateUserState(string userId, string displayName)
        {
            string userStateString = await _cacheManager.GetStringAsync(StorageType.UserState, userId);
            if (userStateString == null)
            {
                UserState userState = new UserState(userId, displayName);
                _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}Cache was empty for user {userId}. Creating new user state.");
                return userState;
            }
            else
            {
                try
                {
                    UserState userState = JsonConvert.DeserializeObject<UserState>(userStateString, jsonSerializerSettings);
                    _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}Successfully deserialized user state for user {userId}.");
                    _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}UserState looks like this: {userStateString}");
                    return userState;
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}Failed to deserialize user state for user {userId}. Creating new user state.");
                    UserState userState = new UserState(userId, displayName);
                    return userState;
                }
            }
        }

        public Task<bool> SaveUserState(UserState userState)
        {
            string userStateString = JsonConvert.SerializeObject(userState, jsonSerializerSettings);
            return _cacheManager.SetStringAsync(StorageType.UserState, userState.UserId, userStateString);
        }
    }
}
