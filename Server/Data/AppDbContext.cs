using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User {get;set;}
        public DbSet<Todo> Todo {get;set;}

        public AppDbContext(DbContextOptions options) : base (options)
        {
            
        }
    }
}