using CoreChatbotApp.Logic.Model.DTOs;
using CoreChatbotApp.Logic.Performables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using static CoreChatbotApp.AdaptiveCards.AdaptiveCardManager;

namespace CoreChatbotApp.Logic.Performables.UserElectives
{
    public enum GenericUserElectiveState
    {
        GenericUserElectiveState1,
        GenericUserElectiveState2,
        GenericUserElectiveState3
    }

    public class GenericUserElective : IUserElective
    {
        [JsonProperty] private GenericUserElectiveState _genericUserElectiveState;
        [JsonProperty] private string _userName;

        public GenericUserElective(string userName)
        {
            _userName = userName;
            _genericUserElectiveState = GenericUserElectiveState.GenericUserElectiveState1;
        }

        [JsonConstructor]
        public GenericUserElective(string userName, GenericUserElectiveState genericUserElectiveState)
        {
            _userName = userName;
            _genericUserElectiveState = genericUserElectiveState;
        }

        public DateTime GetLastModified()
        {
            throw new NotImplementedException();
        }

        public Guid GetPerformableTrackingId()
        {
            throw new NotImplementedException();
        }

        public UserPerformableType GetUserPerformableType()
        {
            throw new NotImplementedException();
        }

        public Task<PerformableResponseObject> HandleTurnAdaptiveCardResponseInputAsync(JObject payload)
        {
            PerformableResponseObject response = new PerformableResponseObject();
            if (_genericUserElectiveState == GenericUserElectiveState.GenericUserElectiveState1)
            {
                return HandleTurnNoInputAsync();
            }
            else if (_genericUserElectiveState == GenericUserElectiveState.GenericUserElectiveState2)
            {
                // Let's retrieve the integer associated with the key 'input' from the payload
                int input = payload["input"].Value<int>();

                JObject newPayload = new JObject
                {
                    ["name"] = _userName,
                    ["input"] = input.ToString()
                };


                _genericUserElectiveState = GenericUserElectiveState.GenericUserElectiveState3;
                response.TaskState = TaskState.ContinueWithAdaptiveCard;
                response.AdaptiveCard = new AdaptiveCardResponseObject(AdaptiveCardType.GenericUserElectiveCardB, Guid.NewGuid(), newPayload);
                return Task.FromResult(response);
            }
            else
            {
                response.TaskState = TaskState.CompletedWithCompletionTextUnspecifiedNextStep;
                response.Text.Add("This generic user elective has been completed.");
                return Task.FromResult(response);
            }
        }

        public Task<PerformableResponseObject> HandleTurnNoInputAsync()
        {
            PerformableResponseObject response = new PerformableResponseObject();
            response.TaskState = TaskState.ContinueWithAdaptiveCard;
            JObject payload = new JObject
            {
                ["name"] = _userName
            };
            response.AdaptiveCard = new AdaptiveCardResponseObject(AdaptiveCardType.GenericUserElectiveCardA, Guid.NewGuid(), payload);
            _genericUserElectiveState = GenericUserElectiveState.GenericUserElectiveState2;
            return Task.FromResult(response);
        }

        public Task<PerformableResponseObject> HandleTurnTextInputAsync(string payload)
        {
            PerformableResponseObject response = new PerformableResponseObject();
            response.Text.Add("This generic user elective does not accept text input.");
            response.TaskState = TaskState.ContinueWithText;
            return Task.FromResult(response);
        }

        public bool IsGenerallyTriggerable()
        {
            throw new NotImplementedException();
        }

        public Task<PerformableResponseObject> RecoverLatestResponseAsync()
        {
            throw new NotImplementedException();
        }

        public void SetLastModifiedToNow()
        {
            throw new NotImplementedException();
        }

        public double TimeBeforeDiscardmentInDays()
        {
            return 5;
        }

        public bool TryGetAcceptedTriggerAndDescription(out string triggerWord, out string description)
        {
            throw new NotImplementedException();
        }
    }
}
