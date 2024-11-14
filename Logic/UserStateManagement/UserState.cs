using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CoreChatbotApp.Utilities;
using CoreChatbotApp.Logic.Performables;
using CoreChatbotApp.Logic.Model.DTOs;
using CoreChatbotApp.Logic.Performables.UserElectives;

namespace CoreChatbotApp.Logic.UserStateManagement
{
    public enum TurnType
    {
        AdaptiveCardResponse,
        TextInput,
        NoInput
    }

    public class UserState
    {
        [JsonProperty] private string _userId;
        [JsonProperty] private string _displayName;
        [JsonProperty] private List<IUserPerformable> _userPerformables;
        [JsonProperty] private string _lastAdaptiveCardMessageActivityId;

        [JsonProperty] private bool _adaptiveCardOutstanding;
        [JsonProperty] private Guid? _lastAdaptiveCardId;
        [JsonProperty] private DateTime _lastUpdated;

        public UserState(string userId, string displayName)
        {
            _userId = userId;
            _displayName = displayName;
            _userPerformables = new List<IUserPerformable>();
            _lastAdaptiveCardMessageActivityId = null;
            _adaptiveCardOutstanding = false;
            _lastAdaptiveCardId = null;
            _lastUpdated = DateTime.Now;
        }

        [JsonConstructor]
        public UserState(string userId, string displayName, List<IUserPerformable> userPerformables, string lastAdaptiveCardMessageActivityId, bool adaptiveCardOutstanding, Guid? lastAdaptiveCardId, DateTime lastUpdated)
        {
            _userId = userId;
            _displayName = displayName;
            _userPerformables = userPerformables;
            _lastAdaptiveCardMessageActivityId = lastAdaptiveCardMessageActivityId;
            _adaptiveCardOutstanding = adaptiveCardOutstanding;
            _lastAdaptiveCardId = lastAdaptiveCardId;
            _lastUpdated = lastUpdated;
        }

        public async Task<TurnResponseObject> HandleAdaptiveCardResponseAsync(JObject payload)
        {
            if (_userPerformables.Count == 0)
            {
                return await AnswerWithNoTasksScheduledMessage();
            }

            // if an AdaptiveCard is outstanding, We check if the id is a match, if not, we throw an exception
            TurnResponseObject turnResponse;

            turnResponse = await HandleTurnAsync(TurnType.AdaptiveCardResponse, payload, null);

            return turnResponse;
        }

        public async Task<TurnResponseObject> HandleTextInputAsync(string payload)
        {
            if (payload == "help")
            {
                return await GetCurrentHelpMessage();
            }

            if (_userPerformables.Count == 0)
            {
                if (payload == "task")
                {
                    IUserElective userElective = new GenericUserElective(_displayName);
                    ScheduleTask(userElective);
                    return await HandleTurnAsync(TurnType.NoInput, null, payload);
                }
                else
                {
                    TurnResponseObject turnResponse = await AnswerWithNoTasksScheduledMessage();
                    turnResponse.Text.Insert(0, $"I'm sorry, I didn't understand that. Please try again or type 'help' to get a more detailed help message.");
                    return turnResponse;
                }
            }
            else
            {
                TurnResponseObject turnResponse = await HandleTurnAsync(TurnType.TextInput, null, payload);
                return turnResponse;
            }
        }

        private async Task<TurnResponseObject> HandleTurnAsync(TurnType turnType, JObject jsonPayload, string stringPayload)
        {
            PerformableResponseObject performableResponse;
            TurnResponseObject turnResponse = new TurnResponseObject();

            int i = 1;
            do
            {
                if (!TryGetActiveUserPerformable(out IUserPerformable ActiveConversationState))
                {
                    // Likely false 
                    return await AnswerWithNoTasksScheduledMessage();
                }

                if (i == 1)
                {
                    switch (turnType)
                    {
                        case TurnType.AdaptiveCardResponse:
                            performableResponse = await ActiveConversationState.HandleTurnAdaptiveCardResponseInputAsync(jsonPayload);
                            break;
                        case TurnType.TextInput:
                            performableResponse = await ActiveConversationState.HandleTurnTextInputAsync(stringPayload);

                            // Check if the text input was accepted, if not, we only return a list of possible triggers
                            if (performableResponse.AcceptedTextInput == false)
                            {
                                return await GetCurrentHelpMessage(true);
                            }

                            break;
                        case TurnType.NoInput:
                            performableResponse = await ActiveConversationState.HandleTurnNoInputAsync();
                            break;
                        default:
                            throw new Exception("Invalid TurnType");
                    }
                }
                else
                {
                    performableResponse = await ActiveConversationState.HandleTurnNoInputAsync();
                }

                if (!UtilityFunctions.IsNullOrEmpty(performableResponse.Text)) turnResponse.Text.AddRange(performableResponse.Text);
                if (CurrentPerformableCompleted(performableResponse.TaskState))
                {
                    _userPerformables.RemoveAt(0); // we remove the task if it's completed
                }

                if (ContinueWithNextPerformable(performableResponse.TaskState))
                {
                    // we immediately continue with another task, thus we only add the text to the response if there is any
                    turnResponse.SecondsBetweenDifferentMessages = performableResponse.SecondsBetweenDifferentMessages;
                }
                else
                {
                    if (ContinueWithAdaptiveCard(performableResponse)) turnResponse.AdaptiveCard = performableResponse.AdaptiveCard;
                    turnResponse.SecondsBetweenDifferentMessages = performableResponse.SecondsBetweenDifferentMessages;
                }

                // assign to turnresponse and stuff
                i++;
            } while (ContinueWithNextPerformable(performableResponse.TaskState));

            if (turnResponse.Text.Count > 0)
            {
                if (ContinueWithAdaptiveCard(performableResponse))
                {
                    turnResponse.TurnResponseStatus = TurnResponseStatus.RespondWithBoth;
                }
                else
                {
                    turnResponse.TurnResponseStatus = TurnResponseStatus.RespondWithText;
                }
            }
            else if (ContinueWithAdaptiveCard(performableResponse))
            {
                turnResponse.TurnResponseStatus = TurnResponseStatus.RespondWithAdaptiveCard;
            }
            else
            {
                turnResponse.TurnResponseStatus = TurnResponseStatus.RespondWithNothing;
            }

            if (ContinueWithAdaptiveCard(performableResponse))
            {
                _adaptiveCardOutstanding = true;
            }
            else
            {
                _adaptiveCardOutstanding = false;
                _lastAdaptiveCardId = null;
            }

            // if TaskStatus is End, we schedule a trigger for possible follow-up tasks for in 10 seconds or so? Maybe implemeted later


            if (_userPerformables.Count == 0)
            {
                turnResponse.DeleteOutstandingAdaptiveCards = true;
                turnResponse.Text.Add(GetNoTasksScheduledMessage());
                turnResponse.SecondsBetweenDifferentMessages = 1;
            }

            return turnResponse;
        }


        public string UserId { get => _userId; }

        private void ScheduleTask(IUserPerformable userPerformable, int index = 0)
        {
            // Let's check if we have enough tasks in the queue to insert the task at the specified index
            if (index >= _userPerformables.Count)
            {
                _userPerformables.Add(userPerformable);
            }
            else
            {
                _userPerformables.Insert(index, userPerformable);
            }
        }

        private bool TryGetActiveUserPerformable(out IUserPerformable userPerformable)
        {
            if (_userPerformables.Count > 0)
            {
                userPerformable = _userPerformables[0];
                return true;
            }
            else
            {
                userPerformable = null;
                return false;
            }
        }

        private async Task<TurnResponseObject> GetCurrentHelpMessage(bool fromWithinActivePerformable = false)
        {
            TurnResponseObject turnResponseObject = new TurnResponseObject();

            turnResponseObject.TurnResponseStatus = TurnResponseStatus.RespondWithText;

            if (fromWithinActivePerformable)
            {
                turnResponseObject.Text.Add($"I currently don't accept text input while your open task is still ongoing. " +
                    $"Please complete the task first. If you need any further help, **consult the** [InsertUserGuideHere]().");
            }
            else
            {
                turnResponseObject.Text.Add($"You can use the set of commands stated to perform the tasks you'd like to. " +
                    $"The commands available to you depend on your user role and state. If you need any further help, **consult the** [InsertUserGuideHere]().");
            }

            return turnResponseObject;
        }

        private async Task<TurnResponseObject> AnswerWithNoTasksScheduledMessage()
        {
            TurnResponseObject turnResponseObject = new TurnResponseObject();
            turnResponseObject.TurnResponseStatus = TurnResponseStatus.RespondWithText;

            turnResponseObject.Text.Add(GetNoTasksScheduledMessage());
            return turnResponseObject;
        }

        private string GetNoTasksScheduledMessage()
        {
            string message = $"You can (re-)trigger the following tasks / processes:";
            message += $"{Environment.NewLine} To trigger 'Generic Task A', type 'task'";
            return message;
        }

        private bool ContinueWithNextPerformable(TaskState taskState)
        {
            bool cont = false;

            // If we definitely continue with the next task, we clearly return true
            cont = taskState >= TaskState.CompletedWithCompletionTextContinueWithNextTask && taskState <= TaskState.CompletedWithoutCompletionTextContinueWithNextTask;

            // If we don't definitively continue with the next task (= uncertain next step), we check if there are any performables left
            if (!cont && taskState >= TaskState.CompletedWithCompletionTextContinueWithNextTask)
            {
                cont = _userPerformables.Count >= 1;
            }

            return cont;
        }

        private bool CurrentPerformableCompleted(TaskState taskState)
        {
            return taskState >= TaskState.CompletedWithCompletionTextContinueWithNextTask;
        }

        private bool ContinueWithAdaptiveCard(PerformableResponseObject performableResponseObject)
        {
            return performableResponseObject.TaskState == TaskState.ContinueWithAdaptiveCard || performableResponseObject.TaskState == TaskState.ContinueWithBoth;
        }

        private bool ContinueWithText(PerformableResponseObject performableResponseObject)
        {
            return !(performableResponseObject.TaskState == TaskState.ContinueWithAdaptiveCard || performableResponseObject.TaskState == TaskState.ContinueWithNothing);
        }
    }
}
