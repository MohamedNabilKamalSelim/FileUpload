using Microsoft.EntityFrameworkCore;
using FileUpload.Models;

namespace FileUpload.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<FileOnDatabase> FilesOnDatabase { get; set; }
        public DbSet<FileOnFileSystem> FilesOnFileSystem { get; set; }
    }
}