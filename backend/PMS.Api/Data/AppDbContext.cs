using Microsoft.EntityFrameworkCore;
using PMS.Api.Models;

namespace PMS.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ======================
        // CORE TABLES
        // ======================
        public DbSet<User> Users => Set<User>();
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

        // ======================
        // PROJECTS
        // ======================
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        // ======================
        // TASKS
        // ======================
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<TaskComment> TaskComments => Set<TaskComment>();

        // ======================
        // COLLAB / SYSTEM
        // ======================
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // ======================
        // FILES / TIME / AUTH
        // ======================
        public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Integration> Integrations => Set<Integration>();

        // ======================
        // MODEL CONFIGURATION
        // ======================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- USER ----------
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ---------- WORKSPACE MEMBER (RBAC CORE) ----------
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.UserId, wm.WorkspaceId });

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMembers)
                .HasForeignKey(wm => wm.UserId);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId);

            // ---------- PROJECT MEMBER ----------
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.UserId, pm.ProjectId });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);

            // ---------- TASK ----------
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- FILE ATTACHMENT ----------
            modelBuilder.Entity<FileAttachment>()
                .HasOne(f => f.Project)
                .WithMany()
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FileAttachment>()
                .HasOne(f => f.Task)
                .WithMany()
                .HasForeignKey(f => f.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- TIME ENTRY ----------
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany()
                .HasForeignKey(te => te.TaskId);

            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany()
                .HasForeignKey(te => te.UserId);

            // ---------- REFRESH TOKEN ----------
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);

            // ---------- INTEGRATION ----------
            modelBuilder.Entity<Integration>()
                .HasOne(i => i.Workspace)
                .WithMany()
                .HasForeignKey(i => i.WorkspaceId);
        }
    }
}
