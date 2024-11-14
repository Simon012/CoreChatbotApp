using Newtonsoft.Json.Linq;
using System;
using static CoreChatbotApp.AdaptiveCards.AdaptiveCardManager;

namespace CoreChatbotApp.Logic.Model.DTOs
{
    /// <summary>
    /// This class represents the payload needed to construct an adaptive card with the help of the AdaptiveCardManager.
    /// </summary>
    public class AdaptiveCardResponseObject
    {
        private AdaptiveCardType adaptiveCardType;
        private Guid adaptiveCardTrackingId;
        private JObject payload;

        public AdaptiveCardResponseObject(AdaptiveCardType adaptiveCardType, Guid adaptiveCardTrackingId, JObject payload = null)
        {
            this.adaptiveCardType = adaptiveCardType;
            this.adaptiveCardTrackingId = adaptiveCardTrackingId;
            this.payload = payload;
        }

        public AdaptiveCardType AdaptiveCardType { get => adaptiveCardType; }
        public Guid AdaptiveCardTrackingId { get => adaptiveCardTrackingId; }
        public JObject Payload { get => payload; }
    }
}
