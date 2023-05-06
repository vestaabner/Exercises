using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence
{
    //public class VestaDbContext : BaseDbContext, IDb<DbContextChatMessage>
    //{
    //    public class ApplicationContext : DbContext, IDesignTimeDbContextFactory<ApplicationContext>

    //}


    public class VestaDbContext : DbContext, IDesignTimeDbContextFactory<VestaDbContext>
    {
        public VestaDbContext(DbContextOptions<VestaDbContext> opto) : base(opto) { }

        public VestaDbContext GetDbContext() => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VestaDbContext).Assembly);
        }
        }
    }

