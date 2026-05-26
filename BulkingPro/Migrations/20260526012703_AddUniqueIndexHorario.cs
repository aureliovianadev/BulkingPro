using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BulkingPro.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexHorario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeCompleto = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cpf = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CategoriasMusculares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasMusculares", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AlunosHorariosAtendimento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonalId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunosHorariosAtendimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlunosHorariosAtendimento_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlunosHorariosAtendimento_AspNetUsers_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Anamneses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TreinadorId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JaTreinouAntes = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TempoTreinando = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TempoSemAtividade = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Objetivo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FrequenciaSemanal = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TempoPorDia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TemDoenca = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QualDoenca = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TemLimitacaoMovimento = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QualLimitacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TemDorMovimento = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QualDor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FezCirurgia = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QualCirurgia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsaMedicamento = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QualMedicamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FazDieta = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TipoDieta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConsomeAlcool = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fuma = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ObservacoesGerais = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anamneses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anamneses_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anamneses_AspNetUsers_TreinadorId",
                        column: x => x.TreinadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AvaliacoesFisicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TreinadorId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAvaliacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Altura = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Pescoco = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Ombro = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ToraxContrai = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ToraxRelax = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BicepsDireito = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BicepsEsquerdo = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Cintura = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Abdomen = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Quadril = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CoxaDireita = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CoxaEsquerda = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PanturrilhaDireita = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PanturrilhaEsquerda = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesFisicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesFisicas_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvaliacoesFisicas_AspNetUsers_TreinadorId",
                        column: x => x.TreinadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HorariosTrabalhoPersonal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonalId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosTrabalhoPersonal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosTrabalhoPersonal_AspNetUsers_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanosTreino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TreinadorId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Titulo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Objetivo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosTreino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosTreino_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanosTreino_AspNetUsers_TreinadorId",
                        column: x => x.TreinadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GruposMusculares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoriaMuscularId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposMusculares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposMusculares_CategoriasMusculares_CategoriaMuscularId",
                        column: x => x.CategoriaMuscularId,
                        principalTable: "CategoriasMusculares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AgendamentosAlunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonalId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorarioTrabalhoId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendamentosAlunos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendamentosAlunos_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendamentosAlunos_AspNetUsers_PersonalId",
                        column: x => x.PersonalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendamentosAlunos_HorariosTrabalhoPersonal_HorarioTrabalhoId",
                        column: x => x.HorarioTrabalhoId,
                        principalTable: "HorariosTrabalhoPersonal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Treinos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlanoTreinoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrdemDia = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treinos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Treinos_PlanosTreino_PlanoTreinoId",
                        column: x => x.PlanoTreinoId,
                        principalTable: "PlanosTreino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Exercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrupoMuscularId = table.Column<int>(type: "int", nullable: false),
                    InstrucoesExecucao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoExecucao = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercicios_GruposMusculares_GrupoMuscularId",
                        column: x => x.GrupoMuscularId,
                        principalTable: "GruposMusculares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExecucoesTreino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TreinoId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataExecucao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: true),
                    EsforcoPercebido = table.Column<int>(type: "int", nullable: true),
                    ObservacoesGerais = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Concluido = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucoesTreino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecucoesTreino_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExecucoesTreino_Treinos_TreinoId",
                        column: x => x.TreinoId,
                        principalTable: "Treinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TreinoExercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TreinoId = table.Column<int>(type: "int", nullable: false),
                    ExercicioId = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    SeriesPlanejadas = table.Column<int>(type: "int", nullable: false),
                    RepeticoesPlanejadas = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TempoExecucaoSegundos = table.Column<int>(type: "int", nullable: true),
                    CargaPlanejada = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    TempoDescanso = table.Column<int>(type: "int", nullable: true),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreinoExercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreinoExercicios_Exercicios_ExercicioId",
                        column: x => x.ExercicioId,
                        principalTable: "Exercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreinoExercicios_Treinos_TreinoId",
                        column: x => x.TreinoId,
                        principalTable: "Treinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExecucoesTreinoExercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExecucaoTreinoId = table.Column<int>(type: "int", nullable: false),
                    TreinoExercicioId = table.Column<int>(type: "int", nullable: false),
                    SeriesFeitas = table.Column<int>(type: "int", nullable: true),
                    RepeticoesFeitas = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CargaUsada = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Concluido = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecucoesTreinoExercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecucoesTreinoExercicios_ExecucoesTreino_ExecucaoTreinoId",
                        column: x => x.ExecucaoTreinoId,
                        principalTable: "ExecucoesTreino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExecucoesTreinoExercicios_TreinoExercicios_TreinoExercicioId",
                        column: x => x.TreinoExercicioId,
                        principalTable: "TreinoExercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "CategoriasMusculares",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Superior" },
                    { 2, "Inferior" },
                    { 3, "Aeróbico" }
                });

            migrationBuilder.InsertData(
                table: "GruposMusculares",
                columns: new[] { "Id", "CategoriaMuscularId", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "Peito" },
                    { 2, 1, "Costas" },
                    { 3, 1, "Bíceps" },
                    { 4, 1, "Tríceps" },
                    { 5, 1, "Ombro" },
                    { 6, 1, "Abdômen" },
                    { 7, 2, "Perna" },
                    { 8, 2, "Glúteo" },
                    { 9, 2, "Panturrilha" },
                    { 10, 3, "Cardio" }
                });

            migrationBuilder.InsertData(
                table: "Exercicios",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "GrupoMuscularId", "InstrucoesExecucao", "Nome", "TipoExecucao" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício clássico para peitoral", 1, "Deite no banco, desça a barra até o peito e empurre.", "Supino Reto com Barra", 0 },
                    { 2, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Foco na parte superior do peitoral", 1, "Banco inclinado a 45°, mesmo movimento do supino reto.", "Supino Inclinado com Barra", 0 },
                    { 3, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maior amplitude de movimento", 1, "Desça os halteres até sentir o alongamento e empurre.", "Supino com Halteres", 0 },
                    { 4, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento do peitoral", 1, "Puxe os cabos em arco na frente do corpo.", "Crossover", 0 },
                    { 5, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento peitoral na máquina", 1, "Una os braços à frente contraindo o peitoral.", "Peck Deck (Borboleta)", 0 },
                    { 6, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício com peso corporal", 1, "Corpo reto, desça o peito até próximo ao chão.", "Flexão de Braço", 0 },
                    { 7, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dorsais e bíceps", 2, "Puxe a barra até a altura do queixo.", "Puxada Frente", 0 },
                    { 8, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Espessura das costas", 2, "Curvado, puxe a barra em direção ao abdômen.", "Remada Curvada", 0 },
                    { 9, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Trabalha os dorsais isoladamente", 2, "Apoie um joelho no banco, puxe o halter.", "Remada Unilateral", 0 },
                    { 10, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ênfase na parte baixa das costas", 2, "Use o triângulo no cabo e puxe até o abdômen.", "Puxada Triângulo", 0 },
                    { 11, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício com peso corporal", 2, "Puxe o corpo até o queixo ultrapassar a barra.", "Barra Fixa", 0 },
                    { 12, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício composto para costas", 2, "Levante a barra do chão com as costas retas.", "Levantamento Terra", 0 },
                    { 13, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício básico de bíceps", 3, "Curl com barra reta mantendo os cotovelos fixos.", "Rosca Direta com Barra", 0 },
                    { 14, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bíceps com halteres alternando", 3, "Curl com halteres alternando os braços.", "Rosca Alternada", 0 },
                    { 15, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento do bíceps", 3, "Cotovelo apoiado na coxa, faça o curl.", "Rosca Concentrada", 0 },
                    { 16, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bíceps no banco scott", 3, "Braços apoiados no banco inclinado.", "Rosca Scott", 0 },
                    { 17, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento do tríceps no cabo", 4, "Puxe a corda para baixo abrindo as pontas.", "Tríceps Corda", 0 },
                    { 18, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício com barra para tríceps", 4, "Deite, abaixe a barra até a testa e empurre.", "Tríceps Testa", 0 },
                    { 19, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Haltere sobre a cabeça", 4, "Segure halter, abaixe atrás da cabeça.", "Tríceps Francês", 0 },
                    { 20, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Peso corporal para tríceps", 4, "Desça entre as barras paralelas e empurre.", "Mergulho (Paralelas)", 0 },
                    { 21, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício composto para ombros", 5, "Empurre a barra acima da cabeça.", "Desenvolvimento com Barra", 0 },
                    { 22, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Feixe médio do deltoide", 5, "Eleve os halteres lateralmente até a altura dos ombros.", "Elevação Lateral", 0 },
                    { 23, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Feixe anterior do deltoide", 5, "Eleve os halteres à frente até a altura dos ombros.", "Elevação Frontal", 0 },
                    { 24, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Feixe posterior do deltoide", 5, "Curvado, abra os braços para trás.", "Crucifixo Invertido", 0 },
                    { 25, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Estabilização do core", 6, "Mantenha o corpo reto apoiado nos antebraços.", "Prancha", 0 },
                    { 26, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Crunch básico", 6, "Eleve o tronco contraindo o abdômen.", "Abdominal Supra", 0 },
                    { 27, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Elevação de pernas", 6, "Deitado, eleve as pernas estendidas.", "Abdominal Infra", 0 },
                    { 28, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Abdominal em máquina", 6, "Flexione o tronco na máquina de forma controlada.", "Abdominal Máquina", 0 },
                    { 29, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Exercício composto para pernas", 7, "Pés na largura dos ombros, desça até 90°.", "Agachamento Livre", 0 },
                    { 30, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quadríceps e glúteos", 7, "Empurre a plataforma sem travar os joelhos.", "Leg Press 45°", 0 },
                    { 31, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento do quadríceps", 7, "Estenda os joelhos na máquina de forma controlada.", "Cadeira Extensora", 0 },
                    { 32, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isquiotibiais", 7, "Flexione os joelhos puxando os calcanhares.", "Mesa Flexora", 0 },
                    { 33, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Quadríceps e glúteos", 7, "Dê um passo à frente e desça o joelho traseiro.", "Avanço (Lunge)", 0 },
                    { 34, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isquiotibiais e glúteos", 7, "Incline o tronco com as pernas semi-estendidas.", "Stiff", 0 },
                    { 35, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Glúteo máximo", 8, "Apoie os ombros no banco e empurre o quadril.", "Elevação Pélvica (Hip Thrust)", 0 },
                    { 36, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Glúteo médio na máquina", 8, "Abra as pernas contra a resistência.", "Abdução de Quadril", 0 },
                    { 37, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Isolamento do glúteo", 8, "Coloque o cabo no tornozelo e estenda a perna para trás.", "Glúteo no Cabo", 0 },
                    { 38, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Panturrilha em pé", 9, "Eleve os calcanhares o máximo possível.", "Gêmeos em Pé", 0 },
                    { 39, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sóleo (panturrilha sentado)", 9, "Na máquina sentado, eleve os calcanhares.", "Gêmeos Sentado", 0 },
                    { 40, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cardio na esteira", 10, "Caminhe ou corra em ritmo moderado.", "Esteira", 0 },
                    { 41, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cardio de baixo impacto", 10, "Pedale em cadência constante.", "Bicicleta Ergométrica", 0 },
                    { 42, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cardio com baixo impacto articular", 10, "Movimento elíptico contínuo.", "Elíptico", 0 },
                    { 43, true, null, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cardio de alta intensidade", 10, "Pule a corda em ritmo constante.", "Corda (Jump Rope)", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentosAlunos_AlunoId",
                table: "AgendamentosAlunos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentosAlunos_HorarioTrabalhoId",
                table: "AgendamentosAlunos",
                column: "HorarioTrabalhoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentosAlunos_PersonalId",
                table: "AgendamentosAlunos",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoHorarioAtendimento_Unique",
                table: "AlunosHorariosAtendimento",
                columns: new[] { "PersonalId", "DiaSemana", "HoraInicio", "HoraFim" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlunosHorariosAtendimento_AlunoId",
                table: "AlunosHorariosAtendimento",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_AlunoId",
                table: "Anamneses",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_TreinadorId",
                table: "Anamneses",
                column: "TreinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesFisicas_AlunoId",
                table: "AvaliacoesFisicas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesFisicas_TreinadorId",
                table: "AvaliacoesFisicas",
                column: "TreinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucoesTreino_AlunoId",
                table: "ExecucoesTreino",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucoesTreino_TreinoId",
                table: "ExecucoesTreino",
                column: "TreinoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucoesTreinoExercicios_ExecucaoTreinoId",
                table: "ExecucoesTreinoExercicios",
                column: "ExecucaoTreinoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecucoesTreinoExercicios_TreinoExercicioId",
                table: "ExecucoesTreinoExercicios",
                column: "TreinoExercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercicios_GrupoMuscularId",
                table: "Exercicios",
                column: "GrupoMuscularId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposMusculares_CategoriaMuscularId",
                table: "GruposMusculares",
                column: "CategoriaMuscularId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTrabalhoPersonal_PersonalId",
                table: "HorariosTrabalhoPersonal",
                column: "PersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosTreino_AlunoId",
                table: "PlanosTreino",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosTreino_TreinadorId",
                table: "PlanosTreino",
                column: "TreinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TreinoExercicios_ExercicioId",
                table: "TreinoExercicios",
                column: "ExercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_TreinoExercicios_TreinoId",
                table: "TreinoExercicios",
                column: "TreinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Treinos_PlanoTreinoId",
                table: "Treinos",
                column: "PlanoTreinoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendamentosAlunos");

            migrationBuilder.DropTable(
                name: "AlunosHorariosAtendimento");

            migrationBuilder.DropTable(
                name: "Anamneses");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AvaliacoesFisicas");

            migrationBuilder.DropTable(
                name: "ExecucoesTreinoExercicios");

            migrationBuilder.DropTable(
                name: "HorariosTrabalhoPersonal");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ExecucoesTreino");

            migrationBuilder.DropTable(
                name: "TreinoExercicios");

            migrationBuilder.DropTable(
                name: "Exercicios");

            migrationBuilder.DropTable(
                name: "Treinos");

            migrationBuilder.DropTable(
                name: "GruposMusculares");

            migrationBuilder.DropTable(
                name: "PlanosTreino");

            migrationBuilder.DropTable(
                name: "CategoriasMusculares");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
