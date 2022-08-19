using Microsoft.EntityFrameworkCore;
using SftpDownloader.Models;

namespace SftpDownloader.Database
{
    public class DatabaseContext : DbContext
    {
        IConfiguration _configuration;
        public DatabaseContext(IConfiguration configuration) : base()
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PGSQLConnection"));
        }

        public DbSet<FileRecord> FileRecords { get; set; }
    }
}
