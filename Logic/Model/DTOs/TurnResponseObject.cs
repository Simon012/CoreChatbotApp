using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace CoreChatbotApp.Logic.Model.DTOs
{
    public enum TurnResponseStatus
    {
        RespondWithAdaptiveCard,
        RespondWithText,
        RespondWithBoth,
        RespondWithNothing
    }


    public class TurnResponseObject
    {
        public TurnResponseStatus TurnResponseStatus { get; set; }
        public AdaptiveCardResponseObject AdaptiveCard { get; set; }
        public List<string> Text { get; set; } = new List<string>();
        public int? SecondsBetweenDifferentMessages { get; set; } = 2;
        public bool DeleteOutstandingAdaptiveCards { get; set; } = false;
    }
}
