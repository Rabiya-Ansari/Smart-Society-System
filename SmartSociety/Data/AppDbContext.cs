using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<SmartSociety.Data.ApplicationUser>(options)
{
    public DbSet<Flat> Flats { get; set; }
    public DbSet<ResidentProfile> ResidentProfiles { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    public DbSet<Visitor> Visitors { get; set; }
    public DbSet<GateLog> GateLogs { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<MaintenanceBill> MaintenanceBills { get; set; }
    public DbSet<BillItem> BillItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<AmenityBooking> AmenityBookings { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<PollVote> PollVotes { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // =====================================================
        // Flat → ResidentProfile
        // =====================================================

        builder.Entity<ResidentProfile>()
            .HasOne(r => r.Flat)
            .WithMany(f => f.Residents)
            .HasForeignKey(r => r.FlatId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ApplicationUser → ResidentProfile
        // One ApplicationUser = One ResidentProfile
        // =====================================================

        builder.Entity<ResidentProfile>()
            .HasOne(r => r.ApplicationUser)
            .WithOne(u => u.ResidentProfile)
            .HasForeignKey<ResidentProfile>(r => r.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ResidentProfile → Vehicle
        // =====================================================

        builder.Entity<Vehicle>()
            .HasOne(v => v.ResidentProfile)
            .WithMany(r => r.Vehicles)
            .HasForeignKey(v => v.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // ResidentProfile → EmergencyContact
        // =====================================================

        builder.Entity<EmergencyContact>()
            .HasOne(e => e.ResidentProfile)
            .WithMany(r => r.EmergencyContacts)
            .HasForeignKey(e => e.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // ResidentProfile → FamilyMember
        // =====================================================

        builder.Entity<FamilyMember>()
            .HasOne(f => f.ResidentProfile)
            .WithMany(r => r.FamilyMembers)
            .HasForeignKey(f => f.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // Flat → Visitor
        // =====================================================

        builder.Entity<Visitor>()
            .HasOne(v => v.Flat)
            .WithMany(f => f.Visitors)
            .HasForeignKey(v => v.FlatId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // Visitor → GateLog
        // =====================================================

        builder.Entity<GateLog>()
            .HasOne(g => g.Visitor)
            .WithMany(v => v.GateLogs)
            .HasForeignKey(g => g.VisitorId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // ApplicationUser → GateLog
        // Security Guard
        // =====================================================

        builder.Entity<GateLog>()
            .HasOne(g => g.SecurityGuard)
            .WithMany()
            .HasForeignKey(g => g.SecurityGuardId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ResidentProfile → Complaint
        // =====================================================

        builder.Entity<Complaint>()
            .HasOne(c => c.ResidentProfile)
            .WithMany(r => r.Complaints)
            .HasForeignKey(c => c.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // ApplicationUser → Complaint
        // Assigned Staff
        // =====================================================

        builder.Entity<Complaint>()
            .HasOne(c => c.AssignedStaff)
            .WithMany()
            .HasForeignKey(c => c.AssignedStaffId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // Flat → MaintenanceBill
        // =====================================================

        builder.Entity<MaintenanceBill>()
            .HasOne(m => m.Flat)
            .WithMany(f => f.MaintenanceBills)
            .HasForeignKey(m => m.FlatId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // MaintenanceBill → BillItem
        // =====================================================

        builder.Entity<BillItem>()
            .HasOne(b => b.MaintenanceBill)
            .WithMany(m => m.BillItems)
            .HasForeignKey(b => b.MaintenanceBillId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // MaintenanceBill → Payment
        // =====================================================

        builder.Entity<Payment>()
            .HasOne(p => p.MaintenanceBill)
            .WithMany(m => m.Payments)
            .HasForeignKey(p => p.MaintenanceBillId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ApplicationUser → Payment
        // =====================================================

        builder.Entity<Payment>()
            .HasOne(p => p.ApplicationUser)
            .WithMany()
            .HasForeignKey(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ResidentProfile → AmenityBooking
        // =====================================================

        builder.Entity<AmenityBooking>()
            .HasOne(a => a.ResidentProfile)
            .WithMany(r => r.AmenityBookings)
            .HasForeignKey(a => a.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // Amenity → AmenityBooking
        // =====================================================

        builder.Entity<AmenityBooking>()
            .HasOne(a => a.Amenity)
            .WithMany(a => a.AmenityBookings)
            .HasForeignKey(a => a.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // Poll → PollOption
        // =====================================================

        builder.Entity<PollOption>()
            .HasOne(p => p.Poll)
            .WithMany(p => p.PollOptions)
            .HasForeignKey(p => p.PollId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // Poll → PollVote
        // =====================================================

        builder.Entity<PollVote>()
            .HasOne(p => p.Poll)
            .WithMany(p => p.PollVotes)
            .HasForeignKey(p => p.PollId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // PollOption → PollVote
        // =====================================================

        builder.Entity<PollVote>()
            .HasOne(p => p.PollOption)
            .WithMany(p => p.PollVotes)
            .HasForeignKey(p => p.PollOptionId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ResidentProfile → PollVote
        // =====================================================

        builder.Entity<PollVote>()
            .HasOne(p => p.ResidentProfile)
            .WithMany()
            .HasForeignKey(p => p.ResidentProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        // =====================================================
        // One Resident = One Vote Per Poll
        // =====================================================

        builder.Entity<PollVote>()
            .HasIndex(p => new
            {
                p.PollId,
                p.ResidentProfileId
            })
            .IsUnique();


        // =====================================================
        // ApplicationUser → AuditLog
        // =====================================================

        builder.Entity<AuditLog>()
            .HasOne(a => a.ApplicationUser)
            .WithMany()
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // Unique Resident CNIC
        // =====================================================

        builder.Entity<ResidentProfile>()
            .HasIndex(r => r.CNIC)
            .IsUnique();


        // =====================================================
        // Unique Vehicle Registration Number
        // =====================================================

        builder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();


        // =====================================================
        // Unique Visitor Gate Pass
        // =====================================================

        builder.Entity<Visitor>()
            .HasIndex(v => v.GatePassCode)
            .IsUnique();
    }
}