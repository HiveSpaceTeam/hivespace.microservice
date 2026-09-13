using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HiveSpace.CatalogService.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20260905162238_AddCatalogImportWorkflowAndStoreIdRename")]
    public partial class AddCatalogImportWorkflowAndStoreIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "categories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "catalog_import_bundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SourceValue = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceFingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CheckpointId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CrawledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalProducts = table.Column<int>(type: "int", nullable: false),
                    ReadyProducts = table.Column<int>(type: "int", nullable: false),
                    BlockedProducts = table.Column<int>(type: "int", nullable: false),
                    WarningCount = table.Column<int>(type: "int", nullable: false),
                    DuplicateCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_bundles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    operation_type = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    source_system = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    source_fingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    source_file_name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    bundle_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    requested_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    last_activity_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    total_count = table.Column<int>(type: "int", nullable: false),
                    processed_count = table.Column<int>(type: "int", nullable: false),
                    created_count = table.Column<int>(type: "int", nullable: false),
                    matched_count = table.Column<int>(type: "int", nullable: false),
                    skipped_count = table.Column<int>(type: "int", nullable: false),
                    blocked_count = table.Column<int>(type: "int", nullable: false),
                    warning_count = table.Column<int>(type: "int", nullable: false),
                    duplicate_count = table.Column<int>(type: "int", nullable: false),
                    failed_count = table.Column<int>(type: "int", nullable: false),
                    conflict_count = table.Column<int>(type: "int", nullable: false),
                    result_summary_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    correlation_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    request_payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_category_attribute_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExternalCategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SourceAttributeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalAttributeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InputType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    HiveSpaceCategoryId = table.Column<int>(type: "int", nullable: false),
                    HiveSpaceAttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    SelectableValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProvisionedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisionedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_category_attribute_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "external_category_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_system = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    external_category_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    external_category_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    external_parent_category_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    path_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hive_space_category_id = table.Column<int>(type: "int", nullable: false),
                    provisioning_status = table.Column<int>(type: "int", nullable: false),
                    conflict_reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    source_fingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    provisioned_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provisioned_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_category_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_category_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalCategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalParentCategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExternalCategoryName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PathJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HiveSpaceCategoryId = table.Column<int>(type: "int", nullable: true),
                    MappingStatus = table.Column<int>(type: "int", nullable: false),
                    MappedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MappedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_category_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_category_mappings_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_duplicate_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuplicateKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExternalProductIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberImportedProductIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepresentativeImportedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_duplicate_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_duplicate_groups_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalProductId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalProductUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalSellerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalCategoryIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailImageExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReadinessStatus = table.Column<int>(type: "int", nullable: false),
                    ImportStatus = table.Column<int>(type: "int", nullable: false),
                    ImportedProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_products_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_seller_ownership_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExternalSellerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ImportedSellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiveSpaceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HiveSpaceStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovalReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_seller_ownership_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_seller_ownership_links_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_sellers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSellerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalSellerSlug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvisioningStatus = table.Column<int>(type: "int", nullable: false),
                    HiveSpaceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HiveSpaceStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConflictReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    SuggestedHiveSpaceStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SuggestedHiveSpaceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_sellers_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_validation_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntitySourceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Field = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_validation_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_validation_issues_catalog_import_bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "catalog_import_bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_attributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalAttributeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExternalAttributeValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceAttributeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SourceValueId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HiveSpaceAttributeDefinitionId = table.Column<int>(type: "int", nullable: true),
                    MatchedAttributeValueIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_attributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_attributes_catalog_import_products_ImportedProductId",
                        column: x => x.ImportedProductId,
                        principalTable: "catalog_import_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_image_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedSkuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SourceImageId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MediaStatus = table.Column<int>(type: "int", nullable: false),
                    MediaFileId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_image_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_image_references_catalog_import_products_ImportedProductId",
                        column: x => x.ImportedProductId,
                        principalTable: "catalog_import_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_import_skus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalSkuId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SkuNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    VariantSelectionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceAmount = table.Column<long>(type: "bigint", nullable: true),
                    SourceRawPrice = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    ImageUrlsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: true),
                    IsActiveCandidate = table.Column<bool>(type: "bit", nullable: false),
                    ReadinessStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_import_skus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_import_skus_catalog_import_products_ImportedProductId",
                        column: x => x.ImportedProductId,
                        principalTable: "catalog_import_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_Name_ParentId",
                table: "categories",
                columns: new[] { "Name", "ParentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_attributes_ImportedProductId",
                table: "catalog_import_attributes",
                column: "ImportedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_bundles_SourceFingerprint",
                table: "catalog_import_bundles",
                column: "SourceFingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_category_mappings_BundleId_ExternalCategoryId",
                table: "catalog_import_category_mappings",
                columns: new[] { "BundleId", "ExternalCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_duplicate_groups_BundleId_DuplicateKey",
                table: "catalog_import_duplicate_groups",
                columns: new[] { "BundleId", "DuplicateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_image_references_ImportedProductId",
                table: "catalog_import_image_references",
                column: "ImportedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_jobs_operation_type_source_system_source_fingerprint",
                table: "catalog_import_jobs",
                columns: new[] { "operation_type", "source_system", "source_fingerprint" },
                filter: "[source_fingerprint] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_jobs_status",
                table: "catalog_import_jobs",
                column: "status",
                filter: "[status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_products_BundleId_ExternalProductId",
                table: "catalog_import_products",
                columns: new[] { "BundleId", "ExternalProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_seller_ownership_links_BundleId",
                table: "catalog_import_seller_ownership_links",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_seller_ownership_links_SourceSystem_ExternalSellerId_LinkStatus",
                table: "catalog_import_seller_ownership_links",
                columns: new[] { "SourceSystem", "ExternalSellerId", "LinkStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_sellers_BundleId_ExternalSellerId",
                table: "catalog_import_sellers",
                columns: new[] { "BundleId", "ExternalSellerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_skus_ImportedProductId_ExternalSkuId",
                table: "catalog_import_skus",
                columns: new[] { "ImportedProductId", "ExternalSkuId" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_import_validation_issues_BundleId_EntityType_EntitySourceId",
                table: "catalog_import_validation_issues",
                columns: new[] { "BundleId", "EntityType", "EntitySourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_external_category_attribute_links_SourceSystem_ExternalCategoryId_SourceAttributeId",
                table: "external_category_attribute_links",
                columns: new[] { "SourceSystem", "ExternalCategoryId", "SourceAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_category_links_source_system_external_category_id",
                table: "external_category_links",
                columns: new[] { "source_system", "external_category_id" },
                unique: true);

            migrationBuilder.RenameColumn(
                name: "SellerId",
                table: "products",
                newName: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StoreId",
                table: "products",
                newName: "SellerId");

            migrationBuilder.DropTable(
                name: "catalog_import_attributes");

            migrationBuilder.DropTable(
                name: "catalog_import_category_mappings");

            migrationBuilder.DropTable(
                name: "catalog_import_duplicate_groups");

            migrationBuilder.DropTable(
                name: "catalog_import_image_references");

            migrationBuilder.DropTable(
                name: "catalog_import_jobs");

            migrationBuilder.DropTable(
                name: "catalog_import_seller_ownership_links");

            migrationBuilder.DropTable(
                name: "catalog_import_sellers");

            migrationBuilder.DropTable(
                name: "catalog_import_skus");

            migrationBuilder.DropTable(
                name: "catalog_import_validation_issues");

            migrationBuilder.DropTable(
                name: "external_category_attribute_links");

            migrationBuilder.DropTable(
                name: "external_category_links");

            migrationBuilder.DropTable(
                name: "catalog_import_products");

            migrationBuilder.DropTable(
                name: "catalog_import_bundles");

            migrationBuilder.DropIndex(
                name: "IX_categories_Name_ParentId",
                table: "categories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
