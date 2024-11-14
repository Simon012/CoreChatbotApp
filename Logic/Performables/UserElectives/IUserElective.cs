using CoreChatbotApp.Logic.Performables;

namespace CoreChatbotApp.Logic.Performables.UserElectives
{
    public interface IUserElective : IUserPerformable
    {
        /// <summary>
        /// Since IUserElectives are not reminded but simply discarded, this function provides the time before this discardment in days.
        /// However, the user of this function should set a maximum value sensible to their context, meaning that this value should be 
        /// disregarded if it is higher than said maximum.
        /// </summary>
        /// <returns> A double value representing the time before discardment in days.</returns>
        public double TimeBeforeDiscardmentInDays();
    }
}
