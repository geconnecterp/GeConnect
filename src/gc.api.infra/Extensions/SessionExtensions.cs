using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;


public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }

    public static object GetObjectFromJson(this ISession session, string key, Type type)
    {
        var value = session.GetString(key);
        return value == null ? null : JsonConvert.DeserializeObject(value, type);
    }
}

