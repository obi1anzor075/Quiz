using System;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.DataContext
{
    public partial class DataStoreDbContext : DbContext
    {
        public DataStoreDbContext()
        {
        }

        public DataStoreDbContext(DbContextOptions<DataStoreDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<HardQuestion> HardQuestions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomUser> RoomUsers { get; set; }
        public virtual DbSet<GameResult> GameResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.QuestionId).HasName("PK__Question__2EC21549E80C6BEB");

                entity.Property(e => e.QuestionId).HasColumnName("question_id");
                entity.Property(e => e.Answer1)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("answer1");
                entity.Property(e => e.Answer2)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("answer2");
                entity.Property(e => e.Answer3)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("answer3");
                entity.Property(e => e.Answer4)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("answer4");
                entity.Property(e => e.CorrectAnswer)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("correct_answer");
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("image_url");
                entity.Property(e => e.QuestionText)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("question_text");
            });

            modelBuilder.Entity<HardQuestion>(entity =>
            {
                entity.HasKey(e => e.QuestionId).HasName("PK__HardQuestion__2EC21549E80C6BEB");

                entity.Property(e => e.QuestionId).HasColumnName("question_id");
                entity.Property(e => e.QuestionText)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("question_text");
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("image_url");
                entity.Property(e => e.CorrectAnswer)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("correct_answer");
                entity.Property(e => e.CorrectAnswer2)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("correct_answer2");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C97A6C5B4");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("user_name");
                entity.Property(e => e.Score)
                    .HasColumnName("score")
                    .HasDefaultValue(0);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId).HasName("PK__Room__08EA57936F4AAB62");

                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.RoomName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("room_name");
                entity.Property(e => e.IsGameStarted)
                    .HasColumnName("is_game_started")
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<RoomUser>(entity =>
            {
                entity.HasKey(e => new { e.RoomId, e.UserId });

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.RoomUsers)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GameResult>(entity =>
            {
                entity.HasKey(e => e.GameResultId).HasName("PK__GameResu__CA8EFA8E7E7F75C2");

                entity.Property(e => e.GameResultId).HasColumnName("game_result_id");
                entity.Property(e => e.CorrectAnswers).HasColumnName("correct_answers");
                entity.Property(e => e.GameDate)
                    .HasColumnName("game_date")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Room)
                    .WithMany()
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
