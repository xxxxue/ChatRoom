using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChatRoomService.Utils;
using Furion.DatabaseAccessor;
using Furion.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatRoomService.DbModel
{
    public class User :Entity , IEntitySeedData<User>
    {
        public string UserName { get; set; }
        
        [JsonIgnore]
        public string PassWord { get; set; }
        public string NickName { get; set; }
        public string HeadImgUrl { get; set; } = "";

        public List<Message> Messages { get; set; }

        public IEnumerable<User> HasData(DbContext dbContext, Type dbContextLocator)
        {
            return new[]
            {
                new User{ Id = 1,UserName = "system",PassWord = "123",NickName ="system",HeadImgUrl = "",CreatedTime = DateTime.Now,UpdatedTime = DateTime.Now},
                new User{ Id = 2,UserName = "test1",PassWord = "123",NickName ="test1",HeadImgUrl = "",CreatedTime = DateTime.Now,UpdatedTime = DateTime.Now},
                new User{ Id = 3,UserName = "test2",PassWord = "123",NickName ="test2",HeadImgUrl = "",CreatedTime = DateTime.Now,UpdatedTime = DateTime.Now},
            };
        }
    }
}