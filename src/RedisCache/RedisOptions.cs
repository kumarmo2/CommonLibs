namespace CommonLibs.RedisCache
{
    public class RedisOptions
    {
        public static string Key { get; } = "Redis";
        public string ConnectionString { get; set; }
        public bool UseCache { get; set; }
    }
}
