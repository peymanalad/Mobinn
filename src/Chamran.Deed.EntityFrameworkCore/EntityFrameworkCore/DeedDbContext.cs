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
        public sealed class DeedDbContext : AbpZeroDbContext<Tenant, Role, User, DeedDbContext>, IAbpPersistedGrantDbContext
    {
        public DbSet<InstagramCrawlerPost> InstagramCrawlerPosts { get; set; }

        public DbSet<DeedChart> DeedCharts { get; set; }

        //modelBuilder.Entity<MixedEntity>().HasNoKey(); // Configure as a shadow property

        public DbSet<GetEntriesDigest> EntriesDigest { get; set; }
        public DbSet<GetEntriesDetail> EntriesDetail { get; set; }
        public DbSet<GetPostsForView> PostsForView { get; set; }
        public DbSet<GetListOfUsers> ListOfUsers { get; set; }

        public DbSet<TaskStat> TaskStats { get; set; }

        public DbSet<TaskEntry> TaskEntries { get; set; }

        public DbSet<OrganizationUser> OrganizationUsers { get; set; }

        public DbSet<OrganizationChart> OrganizationCharts { get; set; }

        public DbSet<UserPostGroup> UserPostGroups { get; set; }

        public DbSet<UserLocation> UserLocations { get; set; }

        public DbSet<UserToken> UserTokens { get; set; }

        public DbSet<FCMQueue> FCMQueues { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DbSet<CommentLike> CommentLikes { get; set; }

        public DbSet<PostLike> PostLikes { get; set; }

        public DbSet<SoftwareUpdate> SoftwareUpdates { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Seen> Seens { get; set; }

        public DbSet<PostCategory> PostCategories { get; set; }

        public DbSet<Hashtag> Hashtags { get; set; }

        public DbSet<PostGroup> PostGroups { get; set; }
        public DbSet<PostSubGroup> PostSubGroups { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }

        //public virtual DbSet<OrganizationGroup> OrganizationGroups { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        /* Define an IDbSet for each entity of the application */

        public DbSet<BinaryObject> BinaryObjects { get; set; }

        public DbSet<Friendship> Friendships { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public DbSet<SubscriptionPaymentExtensionData> SubscriptionPaymentExtensionDatas { get; set; }

        public DbSet<UserDelegation> UserDelegations { get; set; }

        public DbSet<RecentPassword> RecentPasswords { get; set; }
        
        public DeedDbContext(DbContextOptions<DeedDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(60); // Set command timeout to 60 seconds
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(entry =>
            {
                entry.ToTable("Reports", tb => tb.HasTrigger("trgAfterInsertReports"));
            });
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

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.PostKey)
                .IsUnique();

            modelBuilder.ConfigurePersistedGrantEntity();
        }
    }
}