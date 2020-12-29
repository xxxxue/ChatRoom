using System.ComponentModel.DataAnnotations.Schema;
using Furion.DatabaseAccessor;

namespace ChatRoomService.DbModel
{
    public class Message :Entity
    {
        public string Content { get; set; }

        public bool IsSystemMessage { get; set; } = false;
        
        public int UserId { get; set; }
        
        public User User { get; set; }
        
    }
}