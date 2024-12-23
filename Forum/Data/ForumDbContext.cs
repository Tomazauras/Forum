using Microsoft.EntityFrameworkCore;
using Forum.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Forum.Auth.Model;

namespace Forum.Data
{
    public class ForumDbContext : IdentityDbContext<ForumUser>
    {
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Session> Sessions { get; set; }

        private readonly IConfiguration _configuration;

        public ForumDbContext(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSQL"));
        }
    }
}
