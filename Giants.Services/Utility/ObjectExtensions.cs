using System.Text.Json;

namespace Giants.Services
{
    public static class ObjectExtensions
    {
        public static string SerializeToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
