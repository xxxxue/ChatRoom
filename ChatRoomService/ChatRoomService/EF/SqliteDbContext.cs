using System;
using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace ChatRoomService.EF
{
    [AppDbContext("SqliteConnectionString",DbProvider.Sqlite)]
    public class SqliteDbContext : AppDbContext<SqliteDbContext>
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
          
        }

    }
    
}