using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ForumWrapUp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "raw");

            migrationBuilder.CreateTable(
                name: "forum_instances",
                schema: "raw",
                columns: table => new
                {
                    ApiForumId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_instances", x => x.ApiForumId);
                });

            migrationBuilder.CreateTable(
                name: "forum_profiles",
                schema: "raw",
                columns: table => new
                {
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ForumRegisteredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ForumMessageCount = table.Column<int>(type: "integer", nullable: true),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: true),
                    LastSyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_profiles", x => x.Mid);
                });

            migrationBuilder.CreateTable(
                name: "registration_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    VerificationCode = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProfileVerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registration_attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wrap_up_years",
                columns: table => new
                {
                    Year = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wrap_up_years", x => x.Year);
                });

            migrationBuilder.CreateTable(
                name: "forum_sections",
                schema: "raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiForumId = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_sections_forum_instances_ApiForumId",
                        column: x => x.ApiForumId,
                        principalSchema: "raw",
                        principalTable: "forum_instances",
                        principalColumn: "ApiForumId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_topics",
                schema: "raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiForumId = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    TitleHtml = table.Column<string>(type: "text", nullable: true),
                    PostCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_topics_forum_instances_ApiForumId",
                        column: x => x.ApiForumId,
                        principalSchema: "raw",
                        principalTable: "forum_instances",
                        principalColumn: "ApiForumId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_accounts",
                columns: table => new
                {
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProfileVerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_accounts", x => x.Mid);
                    table.ForeignKey(
                        name: "FK_user_accounts_forum_profiles_Mid",
                        column: x => x.Mid,
                        principalSchema: "raw",
                        principalTable: "forum_profiles",
                        principalColumn: "Mid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "annual_wrap_ups",
                columns: table => new
                {
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    IsReady = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPosts = table.Column<int>(type: "integer", nullable: false),
                    TotalPoints = table.Column<long>(type: "bigint", nullable: false),
                    TopTopicExternalId = table.Column<int>(type: "integer", nullable: true),
                    TopTopicTitleHtml = table.Column<string>(type: "text", nullable: true),
                    TopTopicPostCount = table.Column<int>(type: "integer", nullable: false),
                    MostSuccessfulPostExternalId = table.Column<int>(type: "integer", nullable: true),
                    MostSuccessfulPostPoints = table.Column<int>(type: "integer", nullable: true),
                    MostSuccessfulPostContentHtml = table.Column<string>(type: "text", nullable: true),
                    MostSuccessfulPostDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalculatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annual_wrap_ups", x => new { x.Mid, x.Year });
                    table.ForeignKey(
                        name: "FK_annual_wrap_ups_user_accounts_Mid",
                        column: x => x.Mid,
                        principalTable: "user_accounts",
                        principalColumn: "Mid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_annual_wrap_ups_wrap_up_years_Year",
                        column: x => x.Year,
                        principalTable: "wrap_up_years",
                        principalColumn: "Year",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_posts",
                schema: "raw",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiForumId = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    AuthorMid = table.Column<int>(type: "integer", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: true),
                    RawPoints = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContentHtml = table.Column<string>(type: "text", nullable: true),
                    TopicId = table.Column<long>(type: "bigint", nullable: true),
                    SectionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_posts_forum_instances_ApiForumId",
                        column: x => x.ApiForumId,
                        principalSchema: "raw",
                        principalTable: "forum_instances",
                        principalColumn: "ApiForumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_posts_forum_sections_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "raw",
                        principalTable: "forum_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_forum_posts_forum_topics_TopicId",
                        column: x => x.TopicId,
                        principalSchema: "raw",
                        principalTable: "forum_topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_forum_posts_user_accounts_AuthorMid",
                        column: x => x.AuthorMid,
                        principalTable: "user_accounts",
                        principalColumn: "Mid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PagesFetched = table.Column<int>(type: "integer", nullable: false),
                    TotalPagesExpected = table.Column<int>(type: "integer", nullable: true),
                    PostsFetched = table.Column<int>(type: "integer", nullable: false),
                    TotalPostsExpected = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_operations", x => x.Id);
                    table.CheckConstraint("ck_import_operations_date_range", "\"FromDate\" <= \"ToDate\"");
                    table.ForeignKey(
                        name: "FK_import_operations_user_accounts_Mid",
                        column: x => x.Mid,
                        principalTable: "user_accounts",
                        principalColumn: "Mid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_post_statistics",
                columns: table => new
                {
                    Mid = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    PostCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_post_statistics", x => new { x.Mid, x.Year, x.Day });
                    table.ForeignKey(
                        name: "FK_daily_post_statistics_annual_wrap_ups_Mid_Year",
                        columns: x => new { x.Mid, x.Year },
                        principalTable: "annual_wrap_ups",
                        principalColumns: new[] { "Mid", "Year" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_annual_wrap_ups_Year",
                table: "annual_wrap_ups",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_ApiForumId_ExternalId",
                schema: "raw",
                table: "forum_posts",
                columns: new[] { "ApiForumId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_AuthorMid_PublishedAtUtc",
                schema: "raw",
                table: "forum_posts",
                columns: new[] { "AuthorMid", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_SectionId",
                schema: "raw",
                table: "forum_posts",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_TopicId",
                schema: "raw",
                table: "forum_posts",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_sections_ApiForumId_ExternalId",
                schema: "raw",
                table: "forum_sections",
                columns: new[] { "ApiForumId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_forum_topics_ApiForumId_ExternalId",
                schema: "raw",
                table: "forum_topics",
                columns: new[] { "ApiForumId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_operations_Mid_RequestedAtUtc",
                table: "import_operations",
                columns: new[] { "Mid", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_registration_attempts_ExpiresAtUtc",
                table: "registration_attempts",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_registration_attempts_Mid",
                table: "registration_attempts",
                column: "Mid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_post_statistics");

            migrationBuilder.DropTable(
                name: "forum_posts",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "import_operations");

            migrationBuilder.DropTable(
                name: "registration_attempts");

            migrationBuilder.DropTable(
                name: "annual_wrap_ups");

            migrationBuilder.DropTable(
                name: "forum_sections",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "forum_topics",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "user_accounts");

            migrationBuilder.DropTable(
                name: "wrap_up_years");

            migrationBuilder.DropTable(
                name: "forum_instances",
                schema: "raw");

            migrationBuilder.DropTable(
                name: "forum_profiles",
                schema: "raw");
        }
    }
}
