using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDS.Migrations
{
    /// <inheritdoc />
    public partial class fixSomeModelsAndAddSdsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Products",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Base64Image",
                table: "HeaderHImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SdsModelId",
                table: "HeaderHImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SdsModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BiologicalDefinition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InciName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CasNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FemaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EinecsNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentifiedUses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassificationContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignalWord = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContainsInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HazardStatements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Precautionary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Supplementary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherHazards = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubstancesContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InhalationFirstAid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngestionFirstAid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinContactFirstAid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EyeContactFirstAid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SymptomsEffects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalAttention = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtinguishingMediaContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialHazardsContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirefighterAdviceContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalPrecautions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentalPrecautions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContainmentMethods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionReferences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafeHandlingPrecautions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafeStorageConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificEndUses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControlParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProtectiveEquipmentImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineeringMeasures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RespiratoryEquipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HandProtection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EyeProtection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherProtection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HygieneMeasures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalProtection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinProtection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentalExposure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Appearance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Odour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativeDensity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlashPoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeltingPoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefractiveIndex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoilingPoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VapourPressure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolubilityInWater = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoIgnitionTemp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherChemicalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReactivityInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalStability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HazardousReactions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConditionsToAvoid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncompatibleMaterials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HazardousDecomposition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToxicologicalEffects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcuteToxicity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinCorrosion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EyeDamage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SkinSensitization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GermCell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Carcinogenicity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReproductiveToxicity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SingleExposure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepeatedExposure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AspirationHazard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoToxicity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EcoToxicity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistenceDegradability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BioaccumulationPotential = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoilMobility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PbtAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherAdverseEffects = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasteTreatmentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnRoad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnSea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnAir = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HazardClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackingGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentalHazards = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialPrecautions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BulkTranprt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IbcCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafetyRegulations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChemicalSafetyAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrecautionaryStatements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SdsModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeaderHImages_SdsModelId",
                table: "HeaderHImages",
                column: "SdsModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeaderHImages_SdsModels_SdsModelId",
                table: "HeaderHImages",
                column: "SdsModelId",
                principalTable: "SdsModels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeaderHImages_SdsModels_SdsModelId",
                table: "HeaderHImages");

            migrationBuilder.DropTable(
                name: "SdsModels");

            migrationBuilder.DropIndex(
                name: "IX_HeaderHImages_SdsModelId",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "Base64Image",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "SdsModelId",
                table: "HeaderHImages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
