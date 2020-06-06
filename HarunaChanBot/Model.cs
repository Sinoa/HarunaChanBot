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
            modelBuilder.Entity<MessageLog>()
                .HasIndex(x => new { x.ChannelID, x.UserID, x.PostTimestamp });


            modelBuilder.Entity<MessageAttachement>()
                .HasIndex(x => new { x.MessageID });


            modelBuilder.Entity<UserInfo>()
                .Property(x => x.Gender)
                .HasConversion(x => (int)x, x => (UserGender)x);
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
        public uint ID { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong DiscordID { get; set; }

        public string Name { get; set; }

        public UserGender Gender { get; set; }

        public DateTimeOffset LastActiveTimestamp { get; set; }
    }



    public class MessageLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }

        public DateTimeOffset PostTimestamp { get; set; }

        public bool IsBot { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong ChannelID { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong UserID { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong MessageID { get; set; }

        public string Message { get; set; }
    }



    public class MessageAttachement
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong MessageID { get; set; }

        [Column(TypeName = "VARCHAR(20)")]
        public ulong AttachmentID { get; set; }

        public string AttachmentURL { get; set; }

        public string FileName { get; set; }
    }
}