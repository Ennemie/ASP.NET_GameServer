using Microsoft.EntityFrameworkCore;
using Minecraft.Models;

namespace Minecraft.Data
{
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }
            public DbSet<Player> Players { get; set; }
            public DbSet<GameMode> GameModes { get; set; }
            public DbSet<Item> Items { get; set; }
            public DbSet<Purchase> Purchases { get; set; }
            public DbSet<Quest> Quests { get; set; }
            public DbSet<PlayerQuest> PlayerQuests { get; set; }
            public DbSet<Vehicle> Vehicles { get; set; }
            public DbSet<MonsterKill> MonsterKills { get; set; }
            public DbSet<Monster> Monsters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.GameMode)
                .WithMany(g => g.Players)
                .HasForeignKey(p => p.ModeID)
                .IsRequired();

            //GameMode
            modelBuilder.Entity<GameMode>().HasData(
                new GameMode
                {
                    ModeID = 1,
                    ModeName = "Survival",
                    Description = "Gather resources, maintain health, and survive against monsters."
                },
                new GameMode
                {
                    ModeID = 2,
                    ModeName = "Creative",
                    Description = "Unlimited resources to build and create without restrictions."
                },
                new GameMode
                {
                    ModeID = 3,
                    ModeName = "Adventure",
                    Description = "Play custom maps and adventures created by other players."
                }
            );

            //Player
            modelBuilder.Entity<Player>().HasData(
                    new Player
                    {
                        PlayerID = 1,
                        ModeID = 1,
                        Email = "nammv1402@gmail.com",
                        Password = "password123",
                        CharacterName = "Nam Mai",
                        ExperiencePoints = 1500,
                        WalletBalance = 500
                    },
                    new Player
                    {
                        PlayerID = 2,
                        ModeID = 2,
                        Email = "helloworld0101@gmail.com",
                        Password = "password456",
                        CharacterName = "Hello World",
                        ExperiencePoints = 1200,
                        WalletBalance = 300
                    },
                    new Player
                    {
                        PlayerID = 3,
                        ModeID = 3,
                        Email = "oneaboveall999@gmail.com",
                        Password = "password789",
                        CharacterName = "One Above All",
                        ExperiencePoints = 2000,
                        WalletBalance = 800
                    }
                );

            //Item
            modelBuilder.Entity<Item>().HasData(
                    new Item
                    {
                        ItemID = 1,
                        ItemName = "Apple",
                        ItemType = "Food",
                        ImageURL = "apple.png",
                        Price = 10
                    },
                    new Item
                    {
                        ItemID = 2,
                        ItemName = "Sword",
                        ItemType = "Weapon",
                        ImageURL = "sword.png",
                        Price = 100
                    },
                    new Item
                    {
                        ItemID = 3,
                        ItemName = "Axe",
                        ItemType = "Tool",
                        ImageURL = "axe.png",
                        Price = 80
                    }
                );

                //Vehicle
                modelBuilder.Entity<Vehicle>().HasData(
                    new Vehicle
                    {
                        VehicleID = 1,
                        VehicleName = "Horse",
                        VehicleType = "Mount",
                        ImageURL = "horse.png",
                        Price = 200
                    },
                    new Vehicle
                    {
                        VehicleID = 2,
                        VehicleName = "Boat",
                        VehicleType = "Watercraft",
                        ImageURL = "boat.png",
                        Price = 150
                    },
                    new Vehicle
                    {
                        VehicleID = 3,
                        VehicleName = "Minecart",
                        VehicleType = "Rail Transport",
                        ImageURL = "minecart.png",
                        Price = 300
                    }
                );
                //Monster
                modelBuilder.Entity<Monster>().HasData(
                    new Monster
                    {
                        MonsterID = 1,
                        MonsterName = "Zombie",
                        Health = 20,
                        AttackDamage = 5
                    },
                    new Monster
                    {
                        MonsterID = 2,
                        MonsterName = "Skeleton",
                        Health = 15,
                        AttackDamage = 7
                    },
                    new Monster
                    {
                        MonsterID = 3,
                        MonsterName = "Creeper",
                        Health = 10,
                        AttackDamage = 20
                    }
                );

            //Quest
            modelBuilder.Entity<Quest>().HasData(
                    new Quest
                    {
                        QuestID = 1,
                        QuestName = "Collect Wood",
                        Description = "Gather 10 pieces of wood from trees.",
                        RewardExp = 100
                    },
                    new Quest
                    {
                        QuestID = 2,
                        QuestName = "Defeat Zombies",
                        Description = "Eliminate 5 zombies in the night.",
                        RewardExp = 200
                    },
                    new Quest
                    {
                        QuestID = 3,
                        QuestName = "Build a Shelter",
                        Description = "Construct a basic shelter to survive the night.",
                        RewardExp = 150
                    }
                );

                //Purchase
                modelBuilder.Entity<Purchase>().HasData(
                    new Purchase
                    {
                        PurchaseID = 1,
                        PurchaseDate = DateTime.Parse("2025-10-10"),
                        PlayerID = 1,
                        ItemID = 1,
                        Amount = 5
                    },
                    new Purchase
                    {
                        PurchaseID = 2,
                        PurchaseDate = DateTime.Parse("2025-10-11"),
                        PlayerID = 2,
                        VehicleID = 2,
                        Amount = 1
                    },
                    new Purchase
                    {
                        PurchaseID = 3,
                        PurchaseDate = DateTime.Parse("2025-10-12"),
                        PlayerID = 3,
                        ItemID = 3,
                        Amount = 2
                    }
                );

                //MonsterKill
                modelBuilder.Entity<MonsterKill>().HasData(
                    new MonsterKill
                    {
                        KillID = 1,
                        PlayerID = 1,
                        MonsterID = 1,
                        RewardExp = 50,
                        KillTime = DateTime.Parse("2025-10-10T14:30:00"),
                    },
                    new MonsterKill
                    {
                        KillID = 2,
                        PlayerID = 2,
                        MonsterID = 2,
                        RewardExp = 70,
                        KillTime = DateTime.Parse("2025-10-11T16:00:00"),
                    },
                    new MonsterKill
                    {
                        KillID = 3,
                        PlayerID = 3,
                        MonsterID = 3,
                        RewardExp = 100,
                        KillTime = DateTime.Parse("2025-10-12T18:15:00"),
                    }
                );

            //PlayerQuest
            modelBuilder.Entity<PlayerQuest>().HasData(
                    new PlayerQuest
                    {
                        PlayerQuestID = 1,
                        PlayerID = 1,
                        QuestID = 1,
                        Status = "In Progress",
                    },
                    new PlayerQuest
                    {
                        PlayerQuestID = 2,
                        PlayerID = 2,
                        QuestID = 2,
                        Status = "Completed",
                    },
                    new PlayerQuest
                    {
                        PlayerQuestID = 3,
                        PlayerID = 3,
                        QuestID = 3,
                        Status = "Not Started",
                    }
                );
            }
        }
}
