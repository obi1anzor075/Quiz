using System;
using System.Collections.Generic;
using DataAccessLayer.Models;
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
        public virtual DbSet<User> Users { get; set; }  // Добавлено

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
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07E80C6BEB");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.GoogleId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("googleid");
                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdat")
                    .HasDefaultValueSql("GETDATE()");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
