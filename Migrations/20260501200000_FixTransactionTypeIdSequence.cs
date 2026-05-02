using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class FixTransactionTypeIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reseta a sequence do Id para o próximo valor disponível após o MAX existente.
            // Necessário porque a migration anterior (AlterColumn via SQL raw) pode ter
            // dessincronizado a sequence IDENTITY do PostgreSQL.
            migrationBuilder.Sql(@"
                SELECT setval(
                    pg_get_serial_sequence('""TransactionTypes""', 'Id'),
                    COALESCE((SELECT MAX(""Id"") FROM ""TransactionTypes""), 0) + 1,
                    false
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não há como reverter de forma segura uma correção de sequence.
        }
    }
}
