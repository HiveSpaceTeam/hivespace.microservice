using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryProductSetIdIsActiveFilePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server does not support removing IDENTITY via ALTER COLUMN.
            // Use raw SQL to drop and recreate the tables so that external Tiki IDs
            // can be used directly as primary keys (ValueGeneratedNever).
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CategoryAttributes_Categories_CategoryId', 'F') IS NOT NULL
                    ALTER TABLE [CategoryAttributes] DROP CONSTRAINT [FK_CategoryAttributes_Categories_CategoryId];
                IF OBJECT_ID('FK_ProductCategories_Categories_CategoryId', 'F') IS NOT NULL
                    ALTER TABLE [ProductCategories] DROP CONSTRAINT [FK_ProductCategories_Categories_CategoryId];
                IF OBJECT_ID('FK_Categories_Categories_ParentId', 'F') IS NOT NULL
                    ALTER TABLE [Categories] DROP CONSTRAINT [FK_Categories_Categories_ParentId];
                IF OBJECT_ID('CategoryAttributes', 'U') IS NOT NULL
                    DROP TABLE [CategoryAttributes];
                IF OBJECT_ID('Categories', 'U') IS NOT NULL
                    DROP TABLE [Categories];

                CREATE TABLE [Categories] (
                    [Id]           int           NOT NULL,
                    [Name]         nvarchar(max) NOT NULL,
                    [ParentId]     int           NULL,
                    [ProductSetId] int           NULL,
                    [IsActive]     bit           NULL,
                    [FilePath]     nvarchar(500) NULL,
                    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
                );

                CREATE TABLE [CategoryAttributes] (
                    [AttributeId] int NOT NULL,
                    [CategoryId]  int NOT NULL,
                    CONSTRAINT [PK_CategoryAttributes] PRIMARY KEY ([AttributeId], [CategoryId]),
                    CONSTRAINT [FK_CategoryAttributes_Categories_CategoryId]
                        FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_CategoryAttributes_CategoryId] ON [CategoryAttributes] ([CategoryId]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('FK_CategoryAttributes_Categories_CategoryId', 'F') IS NOT NULL
                    ALTER TABLE [CategoryAttributes] DROP CONSTRAINT [FK_CategoryAttributes_Categories_CategoryId];
                IF OBJECT_ID('CategoryAttributes', 'U') IS NOT NULL
                    DROP TABLE [CategoryAttributes];
                IF OBJECT_ID('Categories', 'U') IS NOT NULL
                    DROP TABLE [Categories];

                CREATE TABLE [Categories] (
                    [Id]       int           NOT NULL IDENTITY(1,1),
                    [Name]     nvarchar(max) NOT NULL,
                    [ParentId] int           NULL,
                    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
                );

                CREATE TABLE [CategoryAttributes] (
                    [AttributeId] int NOT NULL IDENTITY(1,1),
                    [CategoryId]  int NOT NULL,
                    CONSTRAINT [PK_CategoryAttributes] PRIMARY KEY ([AttributeId], [CategoryId]),
                    CONSTRAINT [FK_CategoryAttributes_Categories_CategoryId]
                        FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_CategoryAttributes_CategoryId] ON [CategoryAttributes] ([CategoryId]);
            ");
        }
    }
}
