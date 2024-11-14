using CoreChatbotApp.Logic.UserStateManagement;
using System.Threading.Tasks;

namespace CoreChatbotApp.Infrastructure.DataLayer
{
    public interface IDataManager
    {
        /// <summary>
        /// Checks, whether or not the user's conversation is locked. Locks the conversation for a set number of seconds if it is not locked.
        /// </summary>
        /// <returns>A boolean value, indicating whether or not the user's conversation is locked.</returns>
        /// <remarks>None.</remarks>
        Task<bool> IsConversationUnlockedAsync(string userIdAad, int? timeoutInSeconds);
        /// <summary>
        /// Deletes any lock on the user's conversation.
        /// </summary>
        /// <remarks>None.</remarks>
        Task<bool> UnlockConversationAsync(string userIdAad);

        Task<UserState> RetrieveOrCreateUserState(string userId, string displayName);
        Task<bool> SaveUserState(UserState userState);
    }
}
