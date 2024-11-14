namespace CoreChatbotApp.Utilities.Configurations
{
    public class CacheConfig
    {
        public string RedisCacheConnectionString { get; set; }
        public string CacheAppPrefix { get; set; }
        public string CacheSlotPrefix { get; set; }
    }
}
