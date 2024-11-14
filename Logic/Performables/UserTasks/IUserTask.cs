using CoreChatbotApp.Logic.Model.DTOs;
using CoreChatbotApp.Logic.Performables;
using System.Threading.Tasks;

namespace CoreChatbotApp.Logic.Performables.UserTasks
{
    public enum UserTaskType
    {

    }

    public enum TaskImportance
    {
        Critical, // Should be used only if the task is mission critical. Should be reminded after 1 day if no reminder interval is set.
        Important, // Should be used only if the task is really important. Should be reminded after 3 days if no reminder interval is set.
        Normal, // Default case. Completion is mandatory. Should be reminded. If no reminder interval is set, remind after 1 week.
        Low // Basically not mandatory to be done. Should rarely be used. If a task of higher importance is outstanding or being scheduled, it should be enqueued before this task. 
    }

    public interface IUserTask : IUserPerformable
    {
        public Task<PerformableResponseObject> TriggerTaskReminderAsync();
        public TaskImportance GetTaskImportance();
        public double TimeBeforeReminderInDays();
        public UserTaskType GetUserTaskType();
        public int GetNumberOfRemindersSent();
        public void IncrementNumberOfRemindersSent();
        public void SetNumberOfRemindersSent(int numberOfRemindersSent);
    }
}
