using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FindPet.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("54a19741-d703-4a8c-bb31-f4253c643d83"));

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("75b8e78c-312c-402a-aa32-a91f70bc8774"));

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("e97bd204-5db3-46a2-911d-798d92a0a752"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "26bfcd49-d4e0-457c-99cc-c2898c190e51");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f28f9ea7-e858-4ebf-823f-b28f4c524654");

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("3f4e5485-09d0-4376-8c96-3f494b2247af"));

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("8b5c92fc-1730-4b49-adc5-4756d8c9cfeb"));

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("e9ac2afe-0931-4662-b307-0d9f07cf0444"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("29fd2ea4-af27-4abb-b910-f92ff441ebae"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5375a1f9-84db-4d39-b553-f39e8050b7e2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a8d10da4-6965-4458-a1f2-061c702b299e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("e1b9a2c0-a2e4-4fe4-b846-20e8e3569a61"));

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ads",
                columns: new[] { "Id", "DateCreateUpdate", "Description", "Location", "PetId", "Photo", "UserId" },
                values: new object[,]
                {
                    { new Guid("08f4796a-d1a6-493f-acdb-79318a6d7ecb"), new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Found a cat in the entrance of house No. 5", "Mira St., 5", null, "ads/cat_sighting.jpg", null },
                    { new Guid("4ad70196-5345-4de2-b9bb-b04224599e97"), new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Found a cat in the entrance of house No. 5", "Mira St., 5", null, "ads/cat_sighting.jpg", null },
                    { new Guid("7e12b331-a255-4f3b-b709-efc80ba39880"), new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "I saw a similar dog on the street. Shevchenko", "st. Shevchenko, 30", null, "ads/dog_sighting.jpg", null }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4bd206ab-5d0b-4b8a-af53-8fac7c03dad0", null, "User", "USER" },
                    { "d00ea5da-c01c-4ccd-a68c-de82d5895e02", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "DateCreateUpdate", "Description", "FoundDate", "FoundLocation", "Gender", "LostDate", "LostLocation", "Nickname", "Photo", "Size", "SpecialMarks", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("69b56946-ec35-41f5-a5fc-900b6dfecb82"), "American furry sheep", "White", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Female", new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 20", "Woody", "pets/example.jpg", "Small", "Fluffy tail", "Missing", "Rabbit", null },
                    { new Guid("b908b4f3-7c7f-4ca1-bc90-bb74cb10792d"), "Labrador", "Black", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Male", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Sumskaya, 10", "Baron", "pets/dog_example.jpg", "Middle", "White spot on chest", "Missing", "Dog", null },
                    { new Guid("cee0288c-5d1c-4a87-8508-f45205c7f78c"), "British Shorthair", "Redhead", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Female", new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 20", "Myssa", "pets/cat_example.jpg", "Small", "Fluffy tail", "Missing", "Cat", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BirthDate", "DateCreateUpdate", "Email", "Name", "Password", "PhoneNumber", "Photo" },
                values: new object[,]
                {
                    { new Guid("02714c8d-0728-4945-8270-eca958822234"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "vlad2002@gmail.com", "Vlad", "10122002", "+380737303288", "users/vlad_example.jpg" },
                    { new Guid("6a92e446-2553-4b62-b4e7-5bc6e9a3b31d"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "khalin2002@gmail.com", "Andrew", "10122002", "+380737303288", "users/andrew_example.jpg" },
                    { new Guid("878d4555-20dc-426a-895a-6efd0dfce5c6"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "dima2002@gmail.com", "Dima", "10122002", "+380737303288", "users/dima_example.jpg" },
                    { new Guid("8c6ce265-4f0b-49f1-8e32-c7a102b08995"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "vanya2002@gmail.com", "Vanya", "10122002", "+380737303288", "users/vanya_example.jpg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("08f4796a-d1a6-493f-acdb-79318a6d7ecb"));

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("4ad70196-5345-4de2-b9bb-b04224599e97"));

            migrationBuilder.DeleteData(
                table: "Ads",
                keyColumn: "Id",
                keyValue: new Guid("7e12b331-a255-4f3b-b709-efc80ba39880"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4bd206ab-5d0b-4b8a-af53-8fac7c03dad0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d00ea5da-c01c-4ccd-a68c-de82d5895e02");

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("69b56946-ec35-41f5-a5fc-900b6dfecb82"));

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("b908b4f3-7c7f-4ca1-bc90-bb74cb10792d"));

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: new Guid("cee0288c-5d1c-4a87-8508-f45205c7f78c"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("02714c8d-0728-4945-8270-eca958822234"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("6a92e446-2553-4b62-b4e7-5bc6e9a3b31d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("878d4555-20dc-426a-895a-6efd0dfce5c6"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8c6ce265-4f0b-49f1-8e32-c7a102b08995"));

            migrationBuilder.InsertData(
                table: "Ads",
                columns: new[] { "Id", "DateCreateUpdate", "Description", "Location", "PetId", "Photo", "UserId" },
                values: new object[,]
                {
                    { new Guid("54a19741-d703-4a8c-bb31-f4253c643d83"), new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "I saw a similar dog on the street. Shevchenko", "st. Shevchenko, 30", null, "https://example.com/dog_sighting.jpg", null },
                    { new Guid("75b8e78c-312c-402a-aa32-a91f70bc8774"), new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Found a cat in the entrance of house No. 5", "Mira St., 5", null, "https://example.com/cat_sighting.jpg", null },
                    { new Guid("e97bd204-5db3-46a2-911d-798d92a0a752"), new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Found a cat in the entrance of house No. 5", "Mira St., 5", null, "https://example.com/cat_sighting.jpg", null }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "26bfcd49-d4e0-457c-99cc-c2898c190e51", null, "Admin", "ADMIN" },
                    { "f28f9ea7-e858-4ebf-823f-b28f4c524654", null, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "Id", "Breed", "Color", "DateCreateUpdate", "Description", "FoundDate", "FoundLocation", "Gender", "LostDate", "LostLocation", "Nickname", "Photo", "Size", "SpecialMarks", "Status", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("3f4e5485-09d0-4376-8c96-3f494b2247af"), "Labrador", "Black", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Male", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Sumskaya, 10", "Baron", "https://example.com/dog.jpg", "Middle", "White spot on chest", "Missing", "Dog", null },
                    { new Guid("8b5c92fc-1730-4b49-adc5-4756d8c9cfeb"), "British Shorthair", "Redhead", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Female", new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 20", "Myssa", "https://example.com/cat.jpg", "Small", "Fluffy tail", "Missing", "Cat", null },
                    { new Guid("e9ac2afe-0931-4662-b307-0d9f07cf0444"), "American furry sheep", "White", null, null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 25", "Female", new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "st. Petrovskaya, 20", "Woody", "https://example.com/cat.jpg", "Small", "Fluffy tail", "Missing", "Rabbit", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BirthDate", "DateCreateUpdate", "Email", "Name", "Password", "PhoneNumber", "Photo" },
                values: new object[,]
                {
                    { new Guid("29fd2ea4-af27-4abb-b910-f92ff441ebae"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "dima2002@gmail.com", "Dima", "10122002", "+380737303288", null },
                    { new Guid("5375a1f9-84db-4d39-b553-f39e8050b7e2"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "khalin2002@gmail.com", "Andrew", "10122002", "+380737303288", null },
                    { new Guid("a8d10da4-6965-4458-a1f2-061c702b299e"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "vlad2002@gmail.com", "Vlad", "10122002", "+380737303288", null },
                    { new Guid("e1b9a2c0-a2e4-4fe4-b846-20e8e3569a61"), new DateTime(2002, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "vanya2002@gmail.com", "Vanya", "10122002", "+380737303288", null }
                });
        }
    }
}
