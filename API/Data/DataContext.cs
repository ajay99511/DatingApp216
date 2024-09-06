using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;
// DbContext(options)
public class DataContext(DbContextOptions options): IdentityDbContext<AppUser,AppRole,int,
IdentityUserClaim<int>,AppUserRole,IdentityUserLogin<int>,IdentityRoleClaim<int>,IdentityUserToken<int>>(options)
{
    // public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> groups{ get; set; }
    public DbSet<Connection> connections{ get; set; }
    public DbSet<Post> Posts{ get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
        .HasMany(ur=> ur.UserRoles)
        .WithOne(u=>u.User)
        .HasForeignKey(u=>u.UserId)
        .IsRequired();

        builder.Entity<AppRole>()
        .HasMany(ur=> ur.UserRoles)
        .WithOne(u=>u.Role)
        .HasForeignKey(u=>u.RoleId)
        .IsRequired();

        builder.Entity<UserLike>()
            .HasKey(k=>new{k.SourceUserId,k.TargetUserId});
        
        builder.Entity<UserLike>()
            .HasOne(s=>s.SourceUser)
            .WithMany(l=>l.LikedUsers)
            .HasForeignKey(s=>s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLike>()
            .HasOne(s=>s.TargetUser)
            .WithMany(l=>l.LikedByUsers)
            .HasForeignKey(s=>s.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Message>()
            .HasOne(s=>s.Sender)
            .WithMany(x=>x.MessageSent)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Message>()
            .HasOne(s=>s.Recipient)
            .WithMany(x=>x.MessageRecieved)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Post>()
            .HasOne(c=>c.Creator)
            .WithMany(x=>x.Posts)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Photo>()
        .HasQueryFilter(x=>x.IsApproved);
    }
}
