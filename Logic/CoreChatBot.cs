// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using static CoreChatbotApp.Infrastructure.Clients.Timelog.ResponseObjects.CustomerResponseObjects;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using UserState = CoreChatbotApp.Logic.UserStateManagement.UserState;
using CoreChatbotApp.AdaptiveCards;
using CoreChatbotApp.Utilities;
using CoreChatbotApp.Infrastructure.DataLayer;
using CoreChatbotApp.Infrastructure.Clients.SharePoint;
using CoreChatbotApp.Infrastructure.Clients.Timelog;
using CoreChatbotApp.Logic.Model.DTOs;

namespace CoreChatbotApp.Logic
{
    public class CoreChatBot : TeamsActivityHandler
    {
        private readonly IDataManager _dataManager;
        private readonly ILogger _logger;

        private readonly TimelogClient timelogClient;
        private readonly SharePointClient sharePointClient;

        public CoreChatBot(IDataManager dataManager, ILogger<CoreChatBot> logger, IConfiguration config, TimelogClient timelogClient, SharePointClient sharePointClient)
        {
            _dataManager = dataManager;
            _logger = logger;
            this.timelogClient = timelogClient;
            this.sharePointClient = sharePointClient;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            UserState userState = await _dataManager.RetrieveOrCreateUserState(turnContext.Activity.From.AadObjectId, turnContext.Activity.From.Name);

            if (!IsAdaptiveCardResponse(turnContext.Activity))
            {
                string command = turnContext.Activity.Text.Split(" ")[0].ToLower();
                if (command == "cmd")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Command received"), cancellationToken);
                    return;
                }
            }

            TurnResponseObject response = null;
            if (IsAdaptiveCardResponse(turnContext.Activity))
            {
                // Let's first delete the card activity
                await DeleteCardActivityAsync(turnContext, cancellationToken);

                response = await userState.HandleAdaptiveCardResponseAsync(turnContext.Activity.Value as JObject);
            }
            else
            {
                response = await userState.HandleTextInputAsync(turnContext.Activity.Text);
            }

            if (response.AdaptiveCard != null)
            {
                Attachment attachment = AdaptiveCardManager.CreateAdaptiveCardAttachment(_logger, response.AdaptiveCard.AdaptiveCardType, response.AdaptiveCard.AdaptiveCardTrackingId, response.AdaptiveCard.Payload);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            }

            foreach (string message in response.Text)
            {
                await Task.Delay(1000);
                await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
            }

            bool saved = await _dataManager.SaveUserState(userState);
            _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}User state saved: {saved}");
            _logger.LogInformation($"{Constants.SystemConstants.LoggingPrefix}Saved user state: {userState}");
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var teamMember in membersAdded)
            {
                if (teamMember.Id != turnContext.Activity.Recipient.Id && turnContext.Activity.Conversation.ConversationType != "personal")
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to the team {teamMember.GivenName} {teamMember.Surname}."), cancellationToken);
                }
            }
        }

        protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.ConversationType == "channel")
            {
                await turnContext.SendActivityAsync($"Welcome to Microsoft Teams conversationUpdate events demo bot. This bot is configured in {turnContext.Activity.Conversation.Name}");
            }
            else
            {
                await turnContext.SendActivityAsync("Welcome to Microsoft Teams conversationUpdate events demo bot.");
            }
        }



        private async Task DeleteCardActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.DeleteActivityAsync(turnContext.Activity.ReplyToId, cancellationToken);
        }

        public static bool IsAdaptiveCardResponse(IMessageActivity activity)
        {
            if (activity.Value != null)
            {
                return true;
            }

            // If activity.Value is null or not a JObject, it's not an AdaptiveCard response
            return false;
        }
    }
}
