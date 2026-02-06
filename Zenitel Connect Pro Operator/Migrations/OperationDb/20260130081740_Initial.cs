using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenitelConnectProOperator.Migrations.OperationDb
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Camera",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FQID = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camera", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    device_ip = table.Column<string>(type: "TEXT", nullable: true),
                    dirno = table.Column<string>(type: "TEXT", nullable: true),
                    location = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    device_type = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceState = table.Column<int>(type: "INTEGER", nullable: true),
                    CallState = table.Column<int>(type: "INTEGER", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    ServerIP = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    MachineName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.MachineName);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCamera",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    CameraId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCamera", x => new { x.DeviceId, x.CameraId });
                    table.ForeignKey(
                        name: "FK_DeviceCamera_Camera_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Camera",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceCamera_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ErrorLogFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    ControllerName = table.Column<string>(type: "TEXT", nullable: true),
                    ServerAddr = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<string>(type: "TEXT", nullable: true),
                    Realm = table.Column<string>(type: "TEXT", nullable: true),
                    OperatorDirNo = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayConfigurationInSmartClient = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnablePopupWindow = table.Column<bool>(type: "INTEGER", nullable: false),
                    MachineName = table.Column<string>(type: "TEXT", nullable: false),
                    OperatorMachineName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Configurations_Operators_OperatorMachineName",
                        column: x => x.OperatorMachineName,
                        principalTable: "Operators",
                        principalColumn: "MachineName",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_MachineName",
                table: "Configurations",
                column: "MachineName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_OperatorMachineName",
                table: "Configurations",
                column: "OperatorMachineName");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCamera_CameraId",
                table: "DeviceCamera",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_device_ip",
                table: "Devices",
                column: "device_ip");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DeviceCamera");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "Camera");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
