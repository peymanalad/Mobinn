using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.EntityFrameworkCore
{
    public static class DeedDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<DeedDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<DeedDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}