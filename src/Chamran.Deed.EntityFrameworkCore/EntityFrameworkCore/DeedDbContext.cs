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
using Chamran.Deed.Configurations;

namespace Chamran.Deed.EntityFrameworkCore
{
    public class DeedDbContext : AbpZeroDbContext<Tenant, Role, User, DeedDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<InstagramCrawlerPost> InstagramCrawlerPosts { get; set; }

        public virtual DbSet<DeedChart> DeedCharts { get; set; }

        //modelBuilder.Entity<MixedEntity>().HasNoKey(); // Configure as a shadow property

        public virtual DbSet<GetEntriesDigest> EntriesDigest { get; set; }
        public virtual DbSet<GetEntriesDetail> EntriesDetail { get; set; }
        public virtual DbSet<GetPostsForView> PostsForView { get; set; }
        public virtual DbSet<GetListOfUsers> ListOfUsers { get; set; }

        public virtual DbSet<TaskStat> TaskStats { get; set; }

        public virtual DbSet<TaskEntry> TaskEntries { get; set; }

        public virtual DbSet<OrganizationUser> OrganizationUsers { get; set; }

        public virtual DbSet<OrganizationChart> OrganizationCharts { get; set; }

        public virtual DbSet<UserPostGroup> UserPostGroups { get; set; }

        public virtual DbSet<UserLocation> UserLocations { get; set; }

        public virtual DbSet<UserToken> UserTokens { get; set; }

        public virtual DbSet<FCMQueue> FCMQueues { get; set; }

        public virtual DbSet<Report> Reports { get; set; }

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

        //public virtual DbSet<OrganizationGroup> OrganizationGroups { get; set; }

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
            modelBuilder.Entity<GetEntriesDigest>().HasNoKey();
            modelBuilder.Entity<GetEntriesDetail>().HasNoKey();
            modelBuilder.Entity<GetPostsForView>().HasNoKey();
            modelBuilder.Entity<GetListOfUsers>().HasNoKey();

            modelBuilder.Entity<OrganizationChart>()
                .HasOne(node => node.ParentFk)   // Use ParentFk navigation property
                .WithMany(parent => parent.Children)
                .HasForeignKey(node => node.ParentId)
                .OnDelete(DeleteBehavior.ClientCascade);

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CommentLikeConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUserConfiguration());

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

            modelBuilder.Entity<PostLike>()
                .HasIndex(s => new { s.PostId, s.UserId })
                .IsUnique();

            modelBuilder.Entity<CommentLike>()
                .HasIndex(s => new { s.CommentId, s.UserId })
                .IsUnique();

            modelBuilder.Entity<GroupMember>()
                .HasIndex(gm => new { gm.UserId, gm.OrganizationId })
                .IsUnique();

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}