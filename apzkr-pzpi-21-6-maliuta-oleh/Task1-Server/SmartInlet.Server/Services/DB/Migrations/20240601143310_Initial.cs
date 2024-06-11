using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInlet.Server.Services.DB.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanAdministrateDevices = table.Column<bool>(type: "bit", nullable: false),
                    CanAdministrateUsers = table.Column<bool>(type: "bit", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivationCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivationCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JoinOffersFromUsersAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AirSensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    InletDeviceId = table.Column<int>(type: "int", nullable: true),
                    AccessCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aqi = table.Column<short>(type: "smallint", nullable: false),
                    AqiLimitToOpen = table.Column<short>(type: "smallint", nullable: false),
                    AqiLimitToClose = table.Column<short>(type: "smallint", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AirSensors_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CanEditMembers = table.Column<bool>(type: "bit", nullable: false),
                    CanEditDevices = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JoinOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentByGroup = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoinOffers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JoinOffers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TempSensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    InletDeviceId = table.Column<int>(type: "int", nullable: true),
                    AccessCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kelvins = table.Column<short>(type: "smallint", nullable: false),
                    KelvinLimitToOpen = table.Column<short>(type: "smallint", nullable: false),
                    KelvinLimitToClose = table.Column<short>(type: "smallint", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempSensors_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InletDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    AirSensorId = table.Column<int>(type: "int", nullable: true),
                    TempSensorId = table.Column<int>(type: "int", nullable: true),
                    AccessCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ControlType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOpened = table.Column<bool>(type: "bit", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InletDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InletDevices_AirSensors_AirSensorId",
                        column: x => x.AirSensorId,
                        principalTable: "AirSensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InletDevices_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InletDevices_TempSensors_TempSensorId",
                        column: x => x.TempSensorId,
                        principalTable: "TempSensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CanAdministrateDevices", "CanAdministrateUsers", "Email", "FirstName", "IsActivated", "LastName", "PasswordHash", "RegisteredAt", "Username" },
                values: new object[,]
                {
                    { 1, true, true, "1@gmail.com", "Artem", true, "Lavrinenko", "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtPwi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYwMQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uzAJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3mLOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSUHnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefXZ0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4aAXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGfwz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRPGbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1ebY9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJJR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aEpwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmquX2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=", new DateTime(2024, 6, 1, 14, 33, 9, 760, DateTimeKind.Utc).AddTicks(8920), "user1" },
                    { 2, false, true, "2@gmail.com", "Artem", true, "Lavrinenko", "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtPwi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYwMQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uzAJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3mLOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSUHnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefXZ0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4aAXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGfwz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRPGbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1ebY9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJJR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aEpwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmquX2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=", new DateTime(2024, 6, 1, 14, 33, 9, 760, DateTimeKind.Utc).AddTicks(8925), "user2" },
                    { 3, true, false, "3@gmail.com", "Artem", true, "Lavrinenko", "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtPwi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYwMQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uzAJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3mLOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSUHnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefXZ0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4aAXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGfwz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRPGbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1ebY9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJJR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aEpwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmquX2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=", new DateTime(2024, 6, 1, 14, 33, 9, 760, DateTimeKind.Utc).AddTicks(8927), "user3" },
                    { 4, false, false, "4@gmail.com", "Artem", true, "Lavrinenko", "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtPwi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYwMQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uzAJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3mLOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSUHnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefXZ0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4aAXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGfwz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRPGbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1ebY9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJJR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aEpwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmquX2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=", new DateTime(2024, 6, 1, 14, 33, 9, 760, DateTimeKind.Utc).AddTicks(8930), "user4" },
                    { 5, false, false, "5@gmail.com", "Artem", true, "Lavrinenko", "onGj5ihsCZ60LCjul2gTk3hfZBxhR74lkDfBtPwi4EWqN9GJ56cEZd5Go3DcdAV1luSxyxYu7pYwMQnuNKGzZkicYbGjsPKRzxLjfNpkeeJ5gj24uzAJzOePysa8Zysh+YKNppL/7o1UTU5qg83uQNn3QcXvxyQhvwApDK9y8SL00aD1wjeYlzP/YMuY3mLOZskup2QXs5OCslexhK3Z2G9KiihMYwjUONSUHnKcPjIu7YXzRfae+BIFRrfCApmhu2eVAQt2M0/JMNcc95yuRYJGp5X8TfOEKMFKwDwTsMl6X+o/pIRZ2ftneHs1qmECXm74M9SjOhN8rAOmwrbefXZ0EeNfzvIPVOr7nkcoxe1/M7ZYiUuZBdA3BwC1W7eeQ4AV/Hr70vr4TwKWjdu7DcWbFHVxxyDP4aAXPUcCipqILfRJjzGYQYwyVh3WDMVGYQcmBgGfwz+AtG/Liw4azrHvf1Qn/nMoLmoM6OzGHnSzRPGbXErT6a6IgZnbQRw1e7zfr+RKD7Q7TfO6m1ebY9jn5YwVQ2aEpcI1GABiL/+c5ZdXRPP024JLBJJR9Ue943gMvKP89PrMpQaQp1BnNPkfC6G8Z4aEpwWZ+AHkVOicPVlD8TGWi0Ri9q8WQnITzoKmquX2GFft/EbBBwSF5OW3LfjhwB4BMPwdmsQc0h8=", new DateTime(2024, 6, 1, 14, 33, 9, 760, DateTimeKind.Utc).AddTicks(8932), "user5" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivationCodes_UserId",
                table: "ActivationCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AirSensors_GroupId",
                table: "AirSensors",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InletDevices_AirSensorId",
                table: "InletDevices",
                column: "AirSensorId",
                unique: true,
                filter: "[AirSensorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InletDevices_GroupId",
                table: "InletDevices",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_InletDevices_TempSensorId",
                table: "InletDevices",
                column: "TempSensorId",
                unique: true,
                filter: "[TempSensorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JoinOffers_GroupId",
                table: "JoinOffers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinOffers_UserId",
                table: "JoinOffers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TempSensors_GroupId",
                table: "TempSensors",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivationCodes");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "InletDevices");

            migrationBuilder.DropTable(
                name: "JoinOffers");

            migrationBuilder.DropTable(
                name: "AirSensors");

            migrationBuilder.DropTable(
                name: "TempSensors");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
