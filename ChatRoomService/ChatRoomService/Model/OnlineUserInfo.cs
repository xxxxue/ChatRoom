namespace ChatRoomService.Model
{
    public class OnlineUserInfo
    {
        public string ConnectionId { get; set; }
        
        public int UserId { get; set; }
        public string NickName { get; set; }
        public string UserName { get; set; }
    }
}