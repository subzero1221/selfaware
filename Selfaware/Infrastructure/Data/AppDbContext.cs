using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.User.Entities;


namespace Selfaware.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserSubmission> UserSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserSubmission>()
                .Property(b => b.RawAnswersJson)
                .HasColumnType("jsonb");
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
         
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
       
                if (entityEntry.Entity is ApplicationUser user)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        user.CreatedAt = DateTime.UtcNow;
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
