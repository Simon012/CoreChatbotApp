using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using CoreChatbotApp.Logic.Model.DTOs;

namespace CoreChatbotApp.Logic.Performables
{
    public enum UserPerformableType
    {
        // Firstly, the UserTasks (numbers below 100)
        UserTaskLaunchPad = 0,
        UserTaskDecideProvisionMode = 1,
        UserTaskFullSkillProvision = 2,
        UserTaskPartialSkillProvision = 3,

        // Secondly, the UserElectives (numbers above 100)
        UserEleciveModifyDataModel = 100,
        UserElectiveTriggerForOthers = 101
    }


    public interface IUserPerformable
    {
        public Task<PerformableResponseObject> HandleTurnAdaptiveCardResponseInputAsync(JObject payload);
        public Task<PerformableResponseObject> HandleTurnTextInputAsync(string payload);
        // Can be called (only) if it kicks off a new task. If the task is not new, it should throw an exception.

        /// <summary>
        /// should start task if new, otherwise provide the last message (again)
        /// </summary>
        /// <returns></returns>
        public Task<PerformableResponseObject> HandleTurnNoInputAsync();

        /// <summary>
        /// This function should recover the latest proper response, excluding any responses not changing the 
        /// state of the IUserPerformable (e.g. simple help messages, etc.).
        /// Should enforce that IUserPerformables maintain their latest state.
        /// </summary>
        public Task<PerformableResponseObject> RecoverLatestResponseAsync();

        /// <summary>
        /// Provides the trigger word and the corresponding description for the task, usually displayed together in a list of available tasks.
        /// </summary>
        public bool TryGetAcceptedTriggerAndDescription(out string triggerWord, out string description);

        // This function is called after deserialization to inject runtime objects into the task.
        // All of the build-up functions dependant on runtime objects should be called here.
        // public Task InjectRuntimeObjectsAndInitializeAsync(UserStateRuntimeObjects runtimeObjects);

        public DateTime GetLastModified(); // likely not needed due to Timestamp collection which should act as a replacement.
        public void SetLastModifiedToNow(); // likely not needed due to Timestamp collection which should act as a replacement.

        /// <summary>
        /// This function returns the specific UserPerformableType. It also forces the implementer to use the UserPerformableType, adding an entry to the UserPerformableType.
        /// </summary>
        public UserPerformableType GetUserPerformableType();

        /// <summary>
        /// Only answers whether or not the performable is triggerable in general. The corresponding decision for the 
        /// specific user is made by the UserStateFactory.
        /// </summary>
        public bool IsGenerallyTriggerable();

        public Guid GetPerformableTrackingId();
    }
}
