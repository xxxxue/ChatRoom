using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatRoomService.DbModel;
using ChatRoomService.Model;
using ChatRoomService.Utils;
using Furion.DatabaseAccessor;
using Furion.LinqBuilder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;

namespace ChatRoomService.Hubs
{
    public class ChatHub : Hub
    {
        // log
        private readonly ILogger<ChatHub> _logger;

        // 数据库相关
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Message> _messageRepository;

        // 在线用户列表
        private static List<OnlineUserInfo> _onlineUserList = new();

        // 锁
        private static object _lock = new();

        public ChatHub(ILogger<ChatHub> logger, IRepository<User> userRepository,
            IRepository<Message> messageRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        /// <summary>
        /// 注册 (待完善)
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public Result Register(string userName, string pwd)
        {
            var result = new Result();
            
            return result;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public Result Login(string userName, string pwd)
        {
            var result = new Result();

            if (userName.IsNullOrEmpty() || pwd.IsNullOrEmpty())
            {
                result.Code = ResultCodeEnum.Fail;
                result.Msg = "账号/密码为空,请检查后再登录.";
                return result;
            }

            // if (_onlineUserList.Any(item => item.UserName == userName))
            // {
            //     result.Code = ResultCodeEnum.Fail;
            //     result.Msg = "请勿重复登录,您已经在线了.";
            //     return result;
            // }

            var user = _userRepository.Where(item => item.UserName == userName).FirstOrDefault();
 
            if (user == null)
            {
                result.Code = ResultCodeEnum.Fail;
                result.Msg = "账号未注册";
                return result;
            }

            if (user.PassWord != pwd)
            {
                result.Code = ResultCodeEnum.Fail;
                result.Msg = "密码错误";
                return result;
            }

            var message = $"{userName} 加入群聊..";
            var userInfo = new OnlineUserInfo()
            {
                ConnectionId = Context.ConnectionId,
                UserId = user.Id,
                UserName = user.UserName,
                NickName = user.NickName,
            };
            _onlineUserList.Add(userInfo);

            _logger.LogInformation(message);

            SendSystemMessage(message);

            result.Data = userInfo;

            return result;
        }

        // 获取 system用户信息.并放入在线列表.用来发系统通知
        private OnlineUserInfo GetSystemOnlineInfo()
        {
            OnlineUserInfo result;

            lock (_lock)
            {
                var systemOnlineInfo = _onlineUserList.FirstOrDefault(item => item.UserName == Const.SystemUserName);
                var user = new User();
                if (systemOnlineInfo == null)
                {
                    user = _userRepository.Where(item => item.UserName == Const.SystemUserName)
                        .FirstOrDefault();
                    if (user == null)
                    {
                        _logger.LogError("system 账户查找失败");
                        return null;
                    }
                }

                var onlineInfo = new OnlineUserInfo()
                {
                    ConnectionId = "000",
                    UserId = user.Id,
                    UserName = user.UserName,
                    NickName = user.NickName,
                };
                _onlineUserList.Add(onlineInfo);
                result = onlineInfo;
            }

            return result;
        }

        /// <summary>
        /// 发出系统广播 
        /// </summary>
        /// <param name="msg"></param>
        private async void SendSystemMessage(string msg)
        {
            var systemOnlineInfo = _onlineUserList.FirstOrDefault(item => item.UserName == Const.SystemUserName);

            if (systemOnlineInfo == null)
            {
                var onlineInfo = GetSystemOnlineInfo();
                if (onlineInfo == null)
                {
                    return;
                }

                systemOnlineInfo = onlineInfo;
            }

            var message = new Message()
            {
                Content = msg,
                IsSystemMessage = true,
                UserId = systemOnlineInfo.UserId,
                CreatedTime = DateTime.Now
            };
            // 存入数据库
            var dbResult = await _messageRepository.InsertNowAsync(message);

            if (dbResult.Entity.User == null)
            {
                dbResult.Entity.User =
                    _userRepository.Where(item => item.Id == dbResult.Entity.UserId).FirstOrDefault();
            }

            _logger.LogInformation(JsonUtils.ToJsonString(dbResult.Entity.User));

            //发送
            await Clients.All.SendAsync(Const.ReceiveSystemMessage, dbResult.Entity);
        }


        /// <summary>
        /// 发送消息给群里其他人
        /// </summary>
        public async void SendMessageToElse(string msg)
        {
            var message = new Message()
            {
                Content = msg,
                IsSystemMessage = false,
                UserId = _onlineUserList.FirstOrDefault(item => item.ConnectionId == Context.ConnectionId)?.UserId ?? 0,
            };
            // 存入数据库
            var dbResult = await _messageRepository.InsertNowAsync(message);

            if (dbResult.Entity.User == null)
            {
                dbResult.Entity.User =
                    _userRepository.Where(item => item.Id == dbResult.Entity.UserId).FirstOrDefault();
            }

            _logger.LogInformation(JsonUtils.ToJsonString(dbResult.Entity));

            await Clients.Others.SendAsync(Const.ReceiveMessageForElse, dbResult.Entity);
        }

        /// <summary>
        /// 通过Id发送私聊消息 (待完善)
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="msg"></param>
        public async void SendMessageById(string connectionId, string msg)
        {
            var currId = Context.ConnectionId;
            // 待完善
            await Clients.Client(connectionId).SendAsync(Const.ReceiveMyMessage, currId, msg);
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        /// <returns></returns>
        public Result GetOnlineUserList()
        {
            return new Result {Data = _onlineUserList};
        }


#pragma warning disable 8632
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
#pragma warning restore 8632
        {
            var connectionId = Context.ConnectionId;

            var userInfo = _onlineUserList.FirstOrDefault(item => item.ConnectionId == connectionId);
            if (userInfo != null)
            {
                _onlineUserList.Remove(userInfo); // 移除用户

                _logger.LogWarning($"{userInfo.UserId} {userInfo.UserName} {userInfo.NickName} 异常断开连接");
            }


            return base.OnDisconnectedAsync(exception);
        }
    }
}