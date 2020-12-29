using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ChatRoomService.Model
{
    public class Result
    {
        public ResultCodeEnum Code { get; set; } = ResultCodeEnum.Success;
        public object Data { get; set; } = null;
        public string Msg { get; set; } = string.Empty;
    }
}