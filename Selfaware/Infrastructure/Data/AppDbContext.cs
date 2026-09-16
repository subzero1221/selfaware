using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Selfaware.Features.Game.GameSession.Entities;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Survey.Entities;
using Selfaware.Features.Survey.SurveySession.Entities;
using Selfaware.Features.User.Entities;

namespace Selfaware.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }

        public DbSet<Option> Options { get; set; }
        public DbSet<Survey> Surveys { get; set; }


        public DbSet<UserAnswer> Answers { get; set; }
        public DbSet<GameSessionEntity> GameSessionEntities { get; set; }

        public DbSet<SurveySession> SurveySessions { get; set; }

        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           

            modelBuilder.Entity<Quiz>()
    .Property(q => q.QuizType)
    .HasConversion<string>();
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default
        )
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                var entityType = entityEntry.Entity.GetType();

                var createdAtProp = entityType.GetProperty("CreatedAt");
                if (createdAtProp != null && entityEntry.State == EntityState.Added)
                {
                    createdAtProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                }

                var updatedAtProp = entityType.GetProperty("UpdatedAt");
                if (updatedAtProp != null)
                {
                    updatedAtProp.SetValue(entityEntry.Entity, DateTime.UtcNow);
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
