namespace Contracts
{
    using Newtonsoft.Json;

    public static class JsonExtentions
    {
        public static string ToJson(this IMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }
}
