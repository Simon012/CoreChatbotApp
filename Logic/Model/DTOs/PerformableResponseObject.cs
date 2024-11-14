using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;

namespace CoreChatbotApp.Logic.Model.DTOs
{
    public enum TaskState
    {
        ContinueWithAdaptiveCard = 1, // task continues
        ContinueWithText = 2, // task continues
        ContinueWithBoth = 3, // task continues
        ContinueWithNothing = 4, // task continues

        CompletedWithCompletionTextContinueWithNextTask = 11, // task ends but a follow-up task is started immediately
        CompletedWithoutCompletionTextContinueWithNextTask = 12, // task ends but a follow-up task is started immediately

        CompletedWithCompletionTextUnspecifiedNextStep = 21, // task ends but NO follow-up task is started immediately
        CompletedWithoutCompletionTextUnspecifiedNextStep = 22, // task ends but NO follow-up task is started immediately
    }

    public class PerformableResponseObject
    {
        public AdaptiveCardResponseObject AdaptiveCard { get; set; }
        public Guid? AdaptiveCardId { get; set; }
        public List<string> Text { get; set; } = new List<string>();
        public int? SecondsBetweenDifferentMessages { get; set; } = 2;
        public bool AcceptedTextInput { get; set; } = true;
        public TaskState TaskState { get; set; }
    }
}
