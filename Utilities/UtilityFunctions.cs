using System.Collections;
using System.Collections.Generic;

namespace CoreChatbotApp.Utilities
{
    public interface UtilityFunctions
    {
        public static bool IsNullOrEmpty<T>(List<T> list)
        {
            if (list != null)
            {
                return list.Count == 0;
            }
            return true;
        }
    }
}
