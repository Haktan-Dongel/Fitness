using Microsoft.EntityFrameworkCore;
using Fitness.Business.Models;

namespace Fitness.Data.Context
{
    public class FitnessContext : DbContext
    {
        public FitnessContext(DbContextOptions<FitnessContext> options) : base(options) { }

        // Add parameterless constructor for design-time context creation
        public FitnessContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=GymTest;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
            }
        }

        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Equipment> Equipment { get; set; } = null!;
        public DbSet<FitnessProgram> Programs { get; set; } = null!;
        public DbSet<ProgramMembers> ProgramMembers { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<CyclingSession> CyclingSessions { get; set; } = null!;
        public DbSet<RunningSessionMain> RunningSessionMains { get; set; } = null!;
        public DbSet<RunningSessionDetail> RunningSessionDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(entity =>
            {
                entity.ToTable("members");
                entity.HasKey(e => e.MemberId);

                entity.HasMany(m => m.Reservations)
                    .WithOne(r => r.Member)
                    .HasForeignKey(r => r.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.CyclingSessions)
                    .WithOne(cs => cs.Member)
                    .HasForeignKey(cs => cs.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.RunningSessions)
                    .WithOne(rs => rs.Member)
                    .HasForeignKey(rs => rs.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(m => m.ProgramMemberships)
                    .WithOne(pm => pm.Member)
                    .HasForeignKey(pm => pm.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.ToTable("equipment");
                entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
                entity.Property(e => e.DeviceType).HasColumnName("device_type")
                    .IsRequired()
                    .HasMaxLength(45);
            });
            modelBuilder.Entity<FitnessProgram>(entity =>
            {
                entity.ToTable("program");
                entity.HasKey(e => e.ProgramCode);
            });
            modelBuilder.Entity<ProgramMembers>().ToTable("programmembers");
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("reservation");
                entity.HasKey(e => e.ReservationId);
                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
                entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.HasOne(r => r.Member)
                    .WithMany(m => m.Reservations)
                    .HasForeignKey(r => r.MemberId);

                entity.HasOne(r => r.Equipment)
                    .WithMany(e => e.Reservations)
                    .HasForeignKey(r => r.EquipmentId);
            });
            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.ToTable("time_slot");
                entity.HasKey(e => e.TimeSlotId);
                entity.Property(e => e.TimeSlotId).HasColumnName("time_slot_id");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.PartOfDay).HasColumnName("part_of_day");
            });
            modelBuilder.Entity<CyclingSession>(entity =>
            {
                entity.ToTable("cyclingsession");
                entity.HasKey(e => e.CyclingSessionId);
                entity.Property(e => e.CyclingSessionId).HasColumnName("cyclingsession_id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.Duration).HasColumnName("duration");
                entity.Property(e => e.AvgWatt).HasColumnName("avg_watt");
                entity.Property(e => e.MaxWatt).HasColumnName("max_watt");
                entity.Property(e => e.AvgCadence).HasColumnName("avg_cadence");
                entity.Property(e => e.MaxCadence).HasColumnName("max_cadence");
                entity.Property(e => e.TrainingType).HasColumnName("trainingtype");
                entity.Property(e => e.Comment).HasColumnName("comment");
                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.HasOne(d => d.Member)
                    .WithMany(m => m.CyclingSessions)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RunningSessionMain>(entity =>
            {
                entity.ToTable("runningsession_main");
                entity.HasKey(e => e.RunningSessionId);
                entity.Property(e => e.RunningSessionId).HasColumnName("runningsession_id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.MemberId).HasColumnName("member_id");
                entity.Property(e => e.Duration).HasColumnName("duration");
                entity.Property(e => e.AvgSpeed).HasColumnName("avg_speed");


                entity.HasOne(d => d.Member)
                    .WithMany(m => m.RunningSessions)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RunningSessionDetail>(entity =>
            {
                entity.ToTable("runningsession_detail");
                entity.HasKey(e => new { e.RunningSessionId, e.SeqNr });
                entity.Property(e => e.RunningSessionId).HasColumnName("runningsession_id");
                entity.Property(e => e.SeqNr).HasColumnName("seq_nr");
                entity.Property(e => e.IntervalTime).HasColumnName("interval_time");
                entity.Property(e => e.IntervalSpeed).HasColumnName("interval_speed");

                entity.HasOne(d => d.RunningSession)
                    .WithMany(r => r.Details)
                    .HasForeignKey(d => d.RunningSessionId);
            });

            modelBuilder.Entity<ProgramMembers>()
                .HasKey(pm => new { pm.ProgramCode, pm.MemberId });

            modelBuilder.Entity<RunningSessionDetail>()
                .HasKey(rd => new { rd.RunningSessionId, rd.SeqNr });

            // Configure many-to-many relationship between Reservation and TimeSlot
            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.TimeSlots)
                .WithMany(t => t.Reservations)
                .UsingEntity<Dictionary<string, object>>(
                    "reservation_timeslot",
                    j => j
                        .HasOne<TimeSlot>()
                        .WithMany()
                        .HasForeignKey("time_slot_id"),
                    j => j
                        .HasOne<Reservation>()
                        .WithMany()
                        .HasForeignKey("reservation_id"),
                    j =>
                    {
                        j.HasKey("reservation_id", "time_slot_id");
                        j.ToTable("reservation_timeslot");
                    });
        }
    }
}
