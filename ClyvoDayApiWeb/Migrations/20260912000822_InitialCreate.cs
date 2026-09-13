using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClyvoDayApiWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FullName = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    TypeUser = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "COMMUNITY_POSTS",
                columns: table => new
                {
                    CommunityPostId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Category = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UserId1 = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMMUNITY_POSTS", x => x.CommunityPostId);
                    table.ForeignKey(
                        name: "FK_COMMUNITY_POSTS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_COMMUNITY_POSTS_USERS_UserId1",
                        column: x => x.UserId1,
                        principalTable: "USERS",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TUTORS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ScoreEngagement = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Achievement = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TUTORS", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_TUTORS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VETERINARIANS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Crmv = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    Specialty = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VETERINARIANS", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_VETERINARIANS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PETS",
                columns: table => new
                {
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TutorId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Breed = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Sex = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Age = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PETS", x => x.PetId);
                    table.ForeignKey(
                        name: "FK_PETS_TUTORS_TutorId",
                        column: x => x.TutorId,
                        principalTable: "TUTORS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CARE_EVENTS",
                columns: table => new
                {
                    CareEventId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TypeEvent = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    EventDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Observations = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARE_EVENTS", x => x.CareEventId);
                    table.ForeignKey(
                        name: "FK_CARE_EVENTS_PETS_PetId",
                        column: x => x.PetId,
                        principalTable: "PETS",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DAILY_PET_LOGS",
                columns: table => new
                {
                    DailyPetLogId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DailyPetLogType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Content = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Privacy = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DAILY_PET_LOGS", x => x.DailyPetLogId);
                    table.ForeignKey(
                        name: "FK_DAILY_PET_LOGS_PETS_PetId",
                        column: x => x.PetId,
                        principalTable: "PETS",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DAILY_PET_LOGS_USERS_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PET_MONITORINGS",
                columns: table => new
                {
                    PetMonitoringId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PetId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Mood = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    EnergyLevel = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    HydrationLevel = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    Food = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    SleepQuality = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    RecentActivities = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    Sociability = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    TookMedication = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    Weight = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: true),
                    Observations = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PET_MONITORINGS", x => x.PetMonitoringId);
                    table.ForeignKey(
                        name: "FK_PET_MONITORINGS_PETS_PetId",
                        column: x => x.PetId,
                        principalTable: "PETS",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CARE_EVENTS_PetId",
                table: "CARE_EVENTS",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_COMMUNITY_POSTS_UserId",
                table: "COMMUNITY_POSTS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_COMMUNITY_POSTS_UserId1",
                table: "COMMUNITY_POSTS",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_DAILY_PET_LOGS_CreatedByUserId",
                table: "DAILY_PET_LOGS",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DAILY_PET_LOGS_PetId",
                table: "DAILY_PET_LOGS",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PET_MONITORINGS_PetId",
                table: "PET_MONITORINGS",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PETS_TutorId",
                table: "PETS",
                column: "TutorId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                table: "USERS",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VETERINARIANS_Crmv_State",
                table: "VETERINARIANS",
                columns: new[] { "Crmv", "State" },
                unique: true,
                filter: "\"Crmv\" IS NOT NULL AND \"State\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CARE_EVENTS");

            migrationBuilder.DropTable(
                name: "COMMUNITY_POSTS");

            migrationBuilder.DropTable(
                name: "DAILY_PET_LOGS");

            migrationBuilder.DropTable(
                name: "PET_MONITORINGS");

            migrationBuilder.DropTable(
                name: "VETERINARIANS");

            migrationBuilder.DropTable(
                name: "PETS");

            migrationBuilder.DropTable(
                name: "TUTORS");

            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}
