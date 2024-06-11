using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartInlet.Server.Models;

namespace SmartInlet.Server.Services.DB
{
    /// <summary>
    /// ORM class to manage the database.
    /// </summary>
    public class DbApp : DbContext
    {
        public DbSet<ActivationCode> ActivationCodes { get; set; }
        public DbSet<AirSensor> AirSensors { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<InletDevice> InletDevices { get; set; }
        public DbSet<JoinOffer> JoinOffers { get; set; }
        public DbSet<TempSensor> TempSensors { get; set; }
        public DbSet<User> Users { get; set; }

        public DbApp(DbContextOptions<DbApp> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // column properties

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Group>()
                .HasIndex(o => o.Name)
                .IsUnique();

            // 1-to-1 relationships

            modelBuilder.Entity<InletDevice>()
                .HasOne(d => d.AirSensor)
                .WithOne(vk => vk.InletDevice)
                .HasForeignKey<AirSensor>(vk => vk.InletDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AirSensor>()
                .HasOne(vk => vk.InletDevice)
                .WithOne(d => d.AirSensor)
                .HasForeignKey<InletDevice>(d => d.AirSensorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InletDevice>()
                .HasOne(d => d.TempSensor)
                .WithOne(vk => vk.InletDevice)
                .HasForeignKey<TempSensor>(vk => vk.InletDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TempSensor>()
                .HasOne(vk => vk.InletDevice)
                .WithOne(d => d.TempSensor)
                .HasForeignKey<InletDevice>(d => d.TempSensorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-to-many relationships

            modelBuilder.Entity<User>()
                .HasMany(u => u.Groups)
                .WithOne(o => o.Owner)
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.GroupMembers)
                .WithOne(om => om.User)
                .HasForeignKey(om => om.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ActivationCodes)
                .WithOne(ac => ac.User)
                .HasForeignKey(ac => ac.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(o => o.JoinOffers)
                .WithOne(ic => ic.User)
                .HasForeignKey(ic => ic.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasMany(g => g.GroupMembers)
                .WithOne(m => m.Group)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Group>()
               .HasMany(o => o.InletDevices)
               .WithOne(d => d.Group)
               .HasForeignKey(d => d.GroupId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
               .HasMany(o => o.AirSensors)
               .WithOne(d => d.Group)
               .HasForeignKey(d => d.GroupId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasMany(o => o.JoinOffers)
                .WithOne(ic => ic.Group)
                .HasForeignKey(ic => ic.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            const string passwordHash = "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtP" +
                            "wi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYw" +
                            "MQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uz" +
                            "AJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3" +
                            "QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3m" +
                            "LOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSU" +
                            "HnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0" +
                            "/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/" +
                            "pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefX" +
                            "Z0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1" +
                            "W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4a" +
                            "AXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGf" +
                            "wz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRP" +
                            "GbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1eb" +
                            "Y9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJ" +
                            "JR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aE" +
                            "pwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmqu" +
                            "X2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "user1",
                    FirstName = "Artem",
                    LastName = "Lavrinenko",
                    Email = "1@gmail.com",
                    PasswordHash = passwordHash,
                    CanAdministrateDevices = true,
                    CanAdministrateUsers = true,
                    IsActivated = true,
                    RegisteredAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    FirstName = "Artem",
                    LastName = "Lavrinenko",
                    Email = "2@gmail.com",
                    PasswordHash = passwordHash,
                    CanAdministrateDevices = false,
                    CanAdministrateUsers = true,
                    IsActivated = true,
                    RegisteredAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 3,
                    Username = "user3",
                    FirstName = "Artem",
                    LastName = "Lavrinenko",
                    Email = "3@gmail.com",
                    PasswordHash = passwordHash,
                    CanAdministrateDevices = true,
                    CanAdministrateUsers = false,
                    IsActivated = true,
                    RegisteredAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 4,
                    Username = "user4",
                    FirstName = "Artem",
                    LastName = "Lavrinenko",
                    Email = "4@gmail.com",
                    PasswordHash = passwordHash,
                    CanAdministrateDevices = false,
                    CanAdministrateUsers = false,
                    IsActivated = true,
                    RegisteredAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 5,
                    Username = "user5",
                    FirstName = "Artem",
                    LastName = "Lavrinenko",
                    Email = "5@gmail.com",
                    PasswordHash = passwordHash,
                    CanAdministrateDevices = false,
                    CanAdministrateUsers = false,
                    IsActivated = true,
                    RegisteredAt = DateTime.UtcNow
                }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
