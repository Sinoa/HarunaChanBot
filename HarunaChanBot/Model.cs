using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HarunaChanBot
{
    public class DatabaseContext : DbContext
    {
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<MessageLog> MessageLog { get; set; }
        public DbSet<MessageAttachement> MessageAttachement { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CON_STR");
            optionsBuilder.UseMySQL(connectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index
            modelBuilder.Entity<MessageLog>()
                .HasIndex(x => new { x.UserID, x.ChannelID, x.PostTimestamp });


            modelBuilder.Entity<MessageAttachement>()
                .HasIndex(x => new { x.ChannelID, x.MessageID });


            modelBuilder.Entity<UserInfo>()
                .HasIndex(x => x.DiscordID);


            modelBuilder.Entity<UserInfo>()
                .HasIndex(x => x.Gender);


            // Convert
            modelBuilder.Entity<UserInfo>()
                .Property(x => x.Gender)
                .HasConversion(x => (int)x, x => (UserGender)x);


            modelBuilder.Entity<UserInfo>()
                .Property(x => x.DiscordID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageLog>()
                .Property(x => x.ChannelID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageLog>()
                .Property(x => x.UserID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageLog>()
                .Property(x => x.MessageID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageAttachement>()
                .Property(x => x.ChannelID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageAttachement>()
                .Property(x => x.MessageID)
                .HasConversion(x => (long)x, x => (ulong)x);


            modelBuilder.Entity<MessageAttachement>()
                .Property(x => x.AttachmentID)
                .HasConversion(x => (long)x, x => (ulong)x);


            // AutoIncrement
            modelBuilder.Entity<MessageLog>()
                .Property(x => x.ID)
                .ValueGeneratedOnAdd();


            modelBuilder.Entity<MessageAttachement>()
                .Property(x => x.ID)
                .ValueGeneratedOnAdd();


            modelBuilder.Entity<UserInfo>()
                .Property(x => x.ID)
                .ValueGeneratedOnAdd();
        }
    }



    public enum UserGender
    {
        Male = 0,
        Female = 1,
    }



    public class UserInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public ulong DiscordID { get; set; }

        public string Name { get; set; }

        public UserGender Gender { get; set; }

        public DateTimeOffset LastActiveTimestamp { get; set; }
    }



    public class MessageLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTimeOffset PostTimestamp { get; set; }

        public bool IsBot { get; set; }

        public ulong ChannelID { get; set; }

        public ulong UserID { get; set; }

        public ulong MessageID { get; set; }

        public string Message { get; set; }
    }



    public class MessageAttachement
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public ulong ChannelID { get; set; }

        public ulong MessageID { get; set; }

        public ulong AttachmentID { get; set; }

        public string AttachmentURL { get; set; }

        public string FileName { get; set; }
    }
}