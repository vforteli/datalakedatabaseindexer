using Microsoft.Data.SqlClient;

namespace DatabaseIndexer;

public interface ISqlConnectionFactory
{
    SqlConnection Create();
}

public class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public SqlConnection Create() => new SqlConnection(connectionString);
}
