using Npgsql;
using System.Data;

namespace MockMyDb
{
    public interface IPostgreSqlMockFactory : IMockFactory
    {
        NpgsqlConnection GetNpgsqlConnection();
    }
}