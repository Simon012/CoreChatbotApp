using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using AdaptiveCards;
using AdaptiveCards.Templating;
using System.IO;
using System;
using Microsoft.Extensions.Logging;
using AdaptiveExpressions.TriggerTrees;

namespace CoreChatbotApp.AdaptiveCards
{
    public class AdaptiveCardManager
    {
        public enum AdaptiveCardType
        {
            GenericUserElectiveCardA,
            GenericUserElectiveCardB
        }

        public static Attachment CreateAdaptiveCardAttachment(ILogger logger, AdaptiveCardType adaptiveCardType, Guid adaptiveCardTrackingId, JObject payload = null)
        {
            // Get the path to the Adaptive Card JSON template
            string cardResourcePath = GetPath(adaptiveCardType);
            // Read the content of the Adaptive Card JSON template
            string adaptiveCardTemplateJson = File.ReadAllText(cardResourcePath);

            // Create a template from the JSON string
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardTemplateJson);

            // Initialize payload if null
            if (payload == null) payload = new JObject();
            // Add or update the cardId in the payload
            payload["cardId"] = adaptiveCardTrackingId.ToString();

            // Expand the template with the payload and create the card
            string cardJson = template.Expand(payload);
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            // Create the attachment
            Attachment attachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",

                Content = card
            };
            return attachment;
        }

        private static string GetPath(AdaptiveCardType adaptiveCardType)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(basePath, "AdaptiveCards", $"{Enum.GetName(typeof(AdaptiveCardType), adaptiveCardType)}.json");
            return filePath;
        }

    }
}
