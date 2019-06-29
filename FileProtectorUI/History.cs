using System;
using Microsoft.EntityFrameworkCore;


namespace FileProtectorUI
{

    class HistoryEntry
    {

        public string Id { get; set; }
        public string Path { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public bool Allowed { get; set; }
        public DateTime TimeAccessed { get; set; }
    }

    class FileProtectorContext : DbContext
    {
        public DbSet<HistoryEntry> HistoryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoryEntry>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=FileProtector.db");
        }
    }
}
