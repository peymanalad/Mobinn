using Chamran.Deed.Common;
using Chamran.Deed.Info;
using Chamran.Deed.People;
using Abp.IdentityServer4vNext;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Chamran.Deed.Authorization.Delegation;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Chat;
using Chamran.Deed.Editions;
using Chamran.Deed.Friendships;
using Chamran.Deed.MultiTenancy;
using Chamran.Deed.MultiTenancy.Accounting;
using Chamran.Deed.MultiTenancy.Payments;
using Chamran.Deed.Storage;

namespace Chamran.Deed.EntityFrameworkCore
{
    public class DeedDbContext : AbpZeroDbContext<Tenant, Role, User, DeedDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<CommentLike> CommentLikes { get; set; }

        public virtual DbSet<PostLike> PostLikes { get; set; }

        public virtual DbSet<SoftwareUpdate> SoftwareUpdates { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Seen> Seens { get; set; }

        public virtual DbSet<PostCategory> PostCategories { get; set; }

        public virtual DbSet<Hashtag> Hashtags { get; set; }

        public virtual DbSet<PostGroup> PostGroups { get; set; }

        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<GroupMember> GroupMembers { get; set; }

        public virtual DbSet<OrganizationGroup> OrganizationGroups { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public virtual DbSet<SubscriptionPaymentExtensionData> SubscriptionPaymentExtensionDatas { get; set; }

        public virtual DbSet<UserDelegation> UserDelegations { get; set; }

        public virtual DbSet<RecentPassword> RecentPasswords { get; set; }

        public DeedDbContext(DbContextOptions<DeedDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BinaryObject>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
            });

            modelBuilder.Entity<SubscriptionPaymentExtensionData>(b =>
            {
                b.HasQueryFilter(m => !m.IsDeleted)
                    .HasIndex(e => new { e.SubscriptionPaymentId, e.Key, e.IsDeleted })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<UserDelegation>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.SourceUserId });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId });
            });

            modelBuilder
                .Entity<PostCategory>()
                .ToView("vwPostCategories");
            //.HasKey(t => t.Id);

            modelBuilder.Entity<Seen>()
                .HasIndex(s => new { s.PostId, s.UserId })
                .IsUnique();

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}