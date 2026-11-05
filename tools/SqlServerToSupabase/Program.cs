using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;

var sourceConnectionString =
    Environment.GetEnvironmentVariable("SQLSERVER_SOURCE_CONNECTION_STRING");
var targetConnectionString =
    Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(sourceConnectionString) ||
    string.IsNullOrWhiteSpace(targetConnectionString))
{
    throw new InvalidOperationException(
        "Set SQLSERVER_SOURCE_CONNECTION_STRING and SUPABASE_CONNECTION_STRING before running this tool.");
}

var tableNames = new[]
{
    "AspNetRoles",
    "AspNetUsers",
    "AspNetRoleClaims",
    "AspNetUserClaims",
    "AspNetUserLogins",
    "AspNetUserRoles",
    "AspNetUserTokens",
    "Business",
    "Categories",
    "Suppliers",
    "Products",
    "Orders",
    "OrderItems"
};

var identityTables = new[]
{
    "AspNetRoleClaims",
    "AspNetUserClaims",
    "Business",
    "Categories",
    "Suppliers",
    "Products",
    "Orders",
    "OrderItems"
};

await using var source = new SqlConnection(sourceConnectionString);
await using var target = new NpgsqlConnection(targetConnectionString);

await source.OpenAsync();
await target.OpenAsync();
await using var transaction = await target.BeginTransactionAsync(IsolationLevel.Serializable);

foreach (var tableName in tableNames)
{
    var countCommand = new NpgsqlCommand(
        $"SELECT COUNT(*) FROM {QuoteIdentifier(tableName)}",
        target,
        transaction);
    var targetCount = (long)(await countCommand.ExecuteScalarAsync()
        ?? throw new InvalidOperationException($"Could not check target table '{tableName}'."));

    if (targetCount != 0)
    {
        throw new InvalidOperationException(
            $"Target table '{tableName}' is not empty. No data was copied; use an empty Supabase database.");
    }
}

foreach (var tableName in tableNames)
{
    await CopyTableAsync(source, target, transaction, tableName);
}

foreach (var tableName in identityTables)
{
    var sequenceCommand = new NpgsqlCommand(
        $"SELECT setval(pg_get_serial_sequence('{QuoteIdentifier(tableName)}', 'Id'), " +
        $"COALESCE(MAX(\"Id\"), 1), MAX(\"Id\") IS NOT NULL) FROM {QuoteIdentifier(tableName)}",
        target,
        transaction);
    await sequenceCommand.ExecuteNonQueryAsync();
}

await transaction.CommitAsync();
Console.WriteLine("Data transfer completed. Verify the copied records before switching production traffic.");

static async Task CopyTableAsync(
    SqlConnection source,
    NpgsqlConnection target,
    NpgsqlTransaction transaction,
    string tableName)
{
    var quotedTableName = QuoteIdentifier(tableName);

    await using var sourceCommand = new SqlCommand(
        $"SELECT * FROM [dbo].[{tableName}]",
        source);
    await using var sourceReader = await sourceCommand.ExecuteReaderAsync(
        CommandBehavior.SequentialAccess);

    var columnNames = Enumerable.Range(0, sourceReader.FieldCount)
        .Select(sourceReader.GetName)
        .ToArray();

    await using (var targetSchemaCommand = new NpgsqlCommand(
        $"SELECT * FROM {quotedTableName} LIMIT 0",
        target,
        transaction))
    await using (var targetSchemaReader = await targetSchemaCommand.ExecuteReaderAsync())
    {
        var targetColumnNames = Enumerable.Range(0, targetSchemaReader.FieldCount)
            .Select(targetSchemaReader.GetName)
            .ToArray();

        if (!columnNames.OrderBy(name => name, StringComparer.Ordinal)
                .SequenceEqual(
                    targetColumnNames.OrderBy(name => name, StringComparer.Ordinal),
                    StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Columns differ for '{tableName}'. SQL Server: [{string.Join(", ", columnNames)}]; " +
                $"Supabase: [{string.Join(", ", targetColumnNames)}]. No data was committed.");
        }
    }

    var insertSql =
        $"INSERT INTO {quotedTableName} ({string.Join(", ", columnNames.Select(QuoteIdentifier))}) " +
        $"OVERRIDING SYSTEM VALUE VALUES ({string.Join(", ", columnNames.Select((_, index) => $"@p{index}"))})";
    await using var insertCommand = new NpgsqlCommand(insertSql, target, transaction);
    var parameters = columnNames
        .Select((_, index) =>
        {
            var parameter = new NpgsqlParameter($"p{index}", DBNull.Value);
            insertCommand.Parameters.Add(parameter);
            return parameter;
        })
        .ToArray();

    long copiedCount = 0;
    while (await sourceReader.ReadAsync())
    {
        for (var index = 0; index < parameters.Length; index++)
        {
            var value = sourceReader.GetValue(index);
            parameters[index].Value = value is DateTime dateTime
                ? NormalizeUtc(dateTime)
                : value;
        }

        await insertCommand.ExecuteNonQueryAsync();
        copiedCount++;
    }

    Console.WriteLine($"{tableName}: copied {copiedCount} row(s).");
}

static DateTime NormalizeUtc(DateTime value) =>
    value.Kind == DateTimeKind.Local
        ? value.ToUniversalTime()
        : DateTime.SpecifyKind(value, DateTimeKind.Utc);

static string QuoteIdentifier(string value) =>
    $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
