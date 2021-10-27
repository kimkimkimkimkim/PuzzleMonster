using Newtonsoft.Json;

namespace GameBase 
{
    public static class CloneExtensions 
    {
        public static T Clone<T>(this T t)
        {
            var json = JsonConvert.SerializeObject(t);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
