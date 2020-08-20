using System.Data.Common;

namespace CTLite.Data.MicrosoftSqlServer
{
    public interface IMicrosoftSqlServerRepository : ISqlRepository
    {
        void CreateHelperStoredProcedures(DbConnection connection, DbTransaction transaction);
    }
}
