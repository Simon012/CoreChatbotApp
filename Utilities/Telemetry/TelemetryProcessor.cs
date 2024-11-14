using CoreChatbotApp.Utilities;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CoreChatbotApp.Utilities.Telemetry
{
    public class TelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // You can pass values from DI through the constructor.
        public TelemetryProcessor(ITelemetryProcessor next)
        {
            Next = next;
        }

        public void Process(ITelemetry item)
        {
            // To filter out an item, return without calling the next processor.
            if (ShouldFilterOut(item))
            {
                return;
            }

            Next.Process(item);
        }

        private static bool ShouldFilterOut(ITelemetry item)
        {
            if (item is TraceTelemetry traceTelemetry)
            {
                // Check if the message has the desired prefix
                if (!traceTelemetry.Message.StartsWith($"{Constants.SystemConstants.LoggingPrefix}"))
                {
                    // Will not log the message and stop processing here
                    return true;
                }
            }
            // Will allow the item to be logged
            return false;
        }
    }
}
