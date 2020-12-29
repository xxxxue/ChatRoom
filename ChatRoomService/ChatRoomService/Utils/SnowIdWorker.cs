using System.Dynamic;
using Furion.Utilities;

namespace ChatRoomService.Utils
{
    public class SnowIdWorker
    {

        private static Snowflake _woker = new Snowflake(1, 1);

        public static long GetId()
        {
          return  _woker.GetId();
        }
    }
}