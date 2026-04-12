using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Minecraft.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameModes",
                columns: table => new
                {
                    ModeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameModes", x => x.ModeID);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ImageURL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "Monsters",
                columns: table => new
                {
                    MonsterID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MonsterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Health = table.Column<int>(type: "INTEGER", nullable: false),
                    AttackDamage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monsters", x => x.MonsterID);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    QuestID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    RewardExp = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.QuestID);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VehicleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ImageURL = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleID);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CharacterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExperiencePoints = table.Column<int>(type: "INTEGER", nullable: false),
                    WalletBalance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_Players_GameModes_ModeID",
                        column: x => x.ModeID,
                        principalTable: "GameModes",
                        principalColumn: "ModeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonsterKills",
                columns: table => new
                {
                    KillID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false),
                    MonsterID = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardExp = table.Column<int>(type: "INTEGER", nullable: false),
                    KillTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterKills", x => x.KillID);
                    table.ForeignKey(
                        name: "FK_MonsterKills_Monsters_MonsterID",
                        column: x => x.MonsterID,
                        principalTable: "Monsters",
                        principalColumn: "MonsterID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonsterKills_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerQuests",
                columns: table => new
                {
                    PlayerQuestID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestID = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerQuests", x => x.PlayerQuestID);
                    table.ForeignKey(
                        name: "FK_PlayerQuests_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerQuests_Quests_QuestID",
                        column: x => x.QuestID,
                        principalTable: "Quests",
                        principalColumn: "QuestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemID = table.Column<int>(type: "INTEGER", nullable: true),
                    VehicleID = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "FK_Purchases_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ItemID");
                    table.ForeignKey(
                        name: "FK_Purchases_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID");
                });

            migrationBuilder.InsertData(
                table: "GameModes",
                columns: new[] { "ModeID", "Description", "ModeName" },
                values: new object[,]
                {
                    { 1, "Gather resources, maintain health, and survive against monsters.", "Survival" },
                    { 2, "Unlimited resources to build and create without restrictions.", "Creative" },
                    { 3, "Play custom maps and adventures created by other players.", "Adventure" }
                });

            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "ItemID", "ImageURL", "ItemName", "ItemType", "Price" },
                values: new object[,]
                {
                    { 1, "apple.png", "Apple", "Food", 10 },
                    { 2, "sword.png", "Sword", "Weapon", 100 },
                    { 3, "axe.png", "Axe", "Tool", 80 }
                });

            migrationBuilder.InsertData(
                table: "Monsters",
                columns: new[] { "MonsterID", "AttackDamage", "Health", "MonsterName" },
                values: new object[,]
                {
                    { 1, 5, 20, "Zombie" },
                    { 2, 7, 15, "Skeleton" },
                    { 3, 20, 10, "Creeper" }
                });

            migrationBuilder.InsertData(
                table: "Quests",
                columns: new[] { "QuestID", "Description", "QuestName", "RewardExp" },
                values: new object[,]
                {
                    { 1, "Gather 10 pieces of wood from trees.", "Collect Wood", 100 },
                    { 2, "Eliminate 5 zombies in the night.", "Defeat Zombies", 200 },
                    { 3, "Construct a basic shelter to survive the night.", "Build a Shelter", 150 }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "VehicleID", "ImageURL", "Price", "VehicleName", "VehicleType" },
                values: new object[,]
                {
                    { 1, "horse.png", 200, "Horse", "Mount" },
                    { 2, "boat.png", 150, "Boat", "Watercraft" },
                    { 3, "minecart.png", 300, "Minecart", "Rail Transport" }
                });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "PlayerID", "CharacterName", "Email", "ExperiencePoints", "ModeID", "Password", "WalletBalance" },
                values: new object[,]
                {
                    { 1, "Nam Mai", "nammv1402@gmail.com", 1500, 1, "password123", 500 },
                    { 2, "Hello World", "helloworld0101@gmail.com", 1200, 2, "password456", 300 },
                    { 3, "One Above All", "oneaboveall999@gmail.com", 2000, 3, "password789", 800 }
                });

            migrationBuilder.InsertData(
                table: "MonsterKills",
                columns: new[] { "KillID", "KillTime", "MonsterID", "PlayerID", "RewardExp" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 10, 14, 30, 0, 0, DateTimeKind.Unspecified), 1, 1, 50 },
                    { 2, new DateTime(2025, 10, 11, 16, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, 70 },
                    { 3, new DateTime(2025, 10, 12, 18, 15, 0, 0, DateTimeKind.Unspecified), 3, 3, 100 }
                });

            migrationBuilder.InsertData(
                table: "PlayerQuests",
                columns: new[] { "PlayerQuestID", "PlayerID", "QuestID", "Status" },
                values: new object[,]
                {
                    { 1, 1, 1, "In Progress" },
                    { 2, 2, 2, "Completed" },
                    { 3, 3, 3, "Not Started" }
                });

            migrationBuilder.InsertData(
                table: "Purchases",
                columns: new[] { "PurchaseID", "Amount", "ItemID", "PlayerID", "PurchaseDate", "VehicleID" },
                values: new object[,]
                {
                    { 1, 5, 1, 1, new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, 1, null, 2, new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 3, 2, 3, 3, new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonsterKills_MonsterID",
                table: "MonsterKills",
                column: "MonsterID");

            migrationBuilder.CreateIndex(
                name: "IX_MonsterKills_PlayerID",
                table: "MonsterKills",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_PlayerID",
                table: "PlayerQuests",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_QuestID",
                table: "PlayerQuests",
                column: "QuestID");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ModeID",
                table: "Players",
                column: "ModeID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ItemID",
                table: "Purchases",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PlayerID",
                table: "Purchases",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_VehicleID",
                table: "Purchases",
                column: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonsterKills");

            migrationBuilder.DropTable(
                name: "PlayerQuests");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Monsters");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "GameModes");
        }
    }
}
