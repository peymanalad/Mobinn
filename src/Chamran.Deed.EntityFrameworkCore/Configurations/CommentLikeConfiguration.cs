using Chamran.Deed.Info;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.Configurations
{
    public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
    {
        public void Configure(EntityTypeBuilder<CommentLike> builder)
        {
            builder.HasOne(c => c.CommentFk)
                .WithMany()
                .HasForeignKey(c => c.CommentId)
                .OnDelete(DeleteBehavior.NoAction); // Specify ON DELETE NO ACTION

            // Add any other configurations for the CommentLike entity
        }
    }
}
