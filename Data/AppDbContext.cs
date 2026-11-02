using Microsoft.EntityFrameworkCore;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<MenuGroup> MenuGroups { get; set; }
        public DbSet<MenuRole> MenuRoles { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Menu configuration
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("menus");
                entity.HasKey(e => e.Id);
            });

            // Group configuration
            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("groups");
                entity.HasKey(e => e.Id);
            });

            // MenuGroup configuration
            modelBuilder.Entity<MenuGroup>(entity =>
            {
                entity.ToTable("menu_groups");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.GroupId, e.MenuId }).IsUnique();
            });

            // MenuRole configuration
            modelBuilder.Entity<MenuRole>(entity =>
            {
                entity.ToTable("menu_roles");
                entity.HasKey(e => e.Id);
            });

            // Report configuration
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("reports");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReportId).IsUnique();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Attachment configuration
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("attachments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AttachmentId).IsUnique();
                entity.HasOne(e => e.Report)
                      .WithMany(r => r.Attachments)
                      .HasPrincipalKey(r => r.ReportId)
                      .HasForeignKey(e => e.ReportId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
