using ItemRating.Data;
using Microsoft.EntityFrameworkCore;

namespace ItemRating
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        //Matching dataset to table information.
        public DbSet<ITEM> ITEM_DBSet { get; set; }
        public DbSet<USER> USER_DBSet { get; set; }
        public DbSet<ITEM_RATING> ITEM_RATING_DBSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Important to db linking.. Table names _MUST_ be accurate or risk not being found.
            modelBuilder.Entity<ITEM>().ToTable("ITEM");
            modelBuilder.Entity<USER>().ToTable("USER");
            modelBuilder.Entity<ITEM_RATING>().ToTable("ITEM_RATING");

            //composit key
            //https://msdn.microsoft.com/en-us/library/jj591617(v=vs.113).aspx
            modelBuilder.Entity<ITEM_RATING>().HasKey(t => new { t.ID_ITEM, t.ID_USER });

        }
    }
}
