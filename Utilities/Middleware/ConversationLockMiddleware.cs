using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using CoreChatbotApp.Utilities;
using CoreChatbotApp.Infrastructure.DataLayer;

namespace CoreChatbotApp.Utilities.Middleware
{
    public class ConversationLockMiddleware : IMiddleware
    {
        private readonly IDataManager _dataManager;
        private readonly ILogger _logger;

        public ConversationLockMiddleware(IDataManager dataManager, ILogger<ConversationLockMiddleware> logger)
        {
            _dataManager = dataManager;
            _logger = logger;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message || turnContext.Activity.Type == ActivityTypes.InstallationUpdate)
            {
                string userAadId = turnContext.Activity.From.AadObjectId;

                if (await _dataManager.IsConversationUnlockedAsync(userAadId, null))
                {
                    try
                    {
                        // If lock is acquired, execute the next middleware in the pipeline
                        await next(cancellationToken);
                    }
                    finally
                    {
                        // Release the lock after processing
                        bool unlockSuccess = await _dataManager.UnlockConversationAsync(userAadId);
                        if (!unlockSuccess)
                        {
                            _logger.LogError($"{Constants.SystemConstants.LoggingPrefix}Failed to unlock conversation for user {userAadId}");
                        }
                    }
                }
                else
                {
                    // If the lock could not be acquired because another message is being processed,
                    // you could either respond to the user or simply do nothing.
                    // await turnContext.SendActivityAsync("I'm currently busy handling another request. Please try again in a moment.");
                    return; // Dropping the message
                }
            }
            else
            {
                // If it's not a message activity, just pass through the middleware pipeline
                await next(cancellationToken);
            }
        }
    }
}
