using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace ChatRoomService.Utils
{
    public class JsonUtils
    {
        public static string ToJsonString(object model)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                // 设置忽略
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // 转string
            var jsonData = JsonConvert.SerializeObject(model, jsonSettings);
            return jsonData;
        }

        public static T ToObject<T>(string jsonData)
        {
           return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}