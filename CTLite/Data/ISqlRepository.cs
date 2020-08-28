using System;
using System.Collections.Generic;
using System.Data.Common;

namespace CTLite.Data
{
    public interface ISqlRepository : IService
    {
        IEnumerable<T> Load<T>(DbConnection connection, DbTransaction transaction, string query, IEnumerable<DbParameter> parameters, Func<T> newModelFunc);
        void Save(DbConnection connection, DbTransaction transaction, Composite composite);
        T Execute<T>(DbConnection connection, DbTransaction transaction, string statement, IEnumerable<DbParameter> parameters);
        DbConnection OpenConnection(string connectionString);
        DbTransaction BeginTransaction(DbConnection connection);
        void CommitTransaction(DbTransaction transaction);
        void CloseConnection(DbConnection connection);
    }
}
