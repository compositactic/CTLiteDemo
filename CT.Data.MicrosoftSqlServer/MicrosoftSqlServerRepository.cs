﻿// CTLite.Data.MicrosoftSqlServer - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using CTLite.Data.MicrosoftSqlServer.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace CTLite.Data.MicrosoftSqlServer
{
    public class MicrosoftSqlServerRepository : SqlRepository, IMicrosoftSqlServerRepository
    {
        public static MicrosoftSqlServerRepository Create()
        {
            return new MicrosoftSqlServerRepository();
        }

        protected MicrosoftSqlServerRepository() { }

        protected override T OnExecute<T>(DbConnection connection, DbTransaction transaction, string statement, IEnumerable<DbParameter> parameters)
        {
            using var command = new SqlCommand(statement, (SqlConnection)connection) { Transaction = (SqlTransaction)transaction, CommandTimeout = 0 };
            if (parameters != null)
                command.Parameters.AddRange(parameters.ToArray());

            var returnValue = (T)command.ExecuteScalar();
            return returnValue;
        }

        protected override IEnumerable<T> OnLoad<T>(DbConnection connection, DbTransaction transaction, string query, IEnumerable<DbParameter> parameters, Func<T> newModelFunc)
        {
            var results = new List<T>();

            using (var command = new SqlCommand(query, (SqlConnection)connection) { Transaction = (SqlTransaction)transaction, CommandTimeout = 0 })
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters.ToArray());

                using var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                    results.Add(dataReader.ToModel(newModelFunc));
            }

            return results;
        }

        protected override DbConnection OnOpenNewConnection(string connectionString)
        {
            var newConnection = new SqlConnection(connectionString);
            newConnection.Open();
            return newConnection;
        }

        protected override DbTransaction OnBeginNewTransaction(DbConnection connection)
        {
            return connection.BeginTransaction();
        }

        protected override void OnCommit(DbConnection connection, DbTransaction transaction)
        {
            transaction.Commit();
        }

        protected override void OnDelete(DbConnection connection, DbTransaction transaction, string tableName, string tableKeyPropertyName, IEnumerable<object> idValues)
        {
            int batchSize = 500;

            var batches = idValues
                    .Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / batchSize)
                    .Select(g => g.Select(x => x.item));

            var sqlStatement = $@"DELETE FROM {tableName} WHERE {tableKeyPropertyName} IN ";

            var parameterIndex = 0;
            foreach (var batch in batches)
            {
                var parameterList = "(" + string.Join(',', batch.Select(id => "@p" + parameterIndex++)) + ")";
                parameterIndex = 0;
                var parameters = batch.Select(id => new SqlParameter("@p" + parameterIndex++, id.ToString()));
                sqlStatement += parameterList;
                OnExecute<object>(connection, transaction, sqlStatement, parameters);
            }
        }

        protected override void OnUpdate(DbConnection connection, DbTransaction transaction, string tableName, string tableKeyPropertyName, object tableKeyValue, IReadOnlyDictionary<string, object> columnValues)
        {
            var sqlParameterList = new List<SqlParameter>
            (
                columnValues.Keys.Select(columnName => new SqlParameter("@" + columnName, columnValues[columnName]))
            );

            var updateSql =
            $@"
                UPDATE {tableName} 
                SET {string.Join(',', sqlParameterList.Where(p => p.ParameterName != "@" + tableKeyPropertyName).Select(p => p.ParameterName.Substring(1) + " = '" + p.Value.ToString() + "'"))}
                WHERE {tableKeyPropertyName} = @{tableKeyPropertyName} 
            ";

            OnExecute<object>(connection, transaction, updateSql, sqlParameterList);
        }

        protected override void OnInsert(DbConnection connection, DbTransaction transaction, IReadOnlyList<DataTable> dataTablesToInsert, bool shouldUpdatedInsertedIds)
        {
            var keys = new Dictionary<object, object>();
            var foreignKeyColumnName = string.Empty;

            foreach (var dataTable in dataTablesToInsert)
            {
                if(!string.IsNullOrEmpty(foreignKeyColumnName))
                    foreach (var dt in dataTablesToInsert)
                    {
                        if (dt.Columns.Contains(foreignKeyColumnName))
                        {
                            PropertyInfo foreignKeyProperty = null;

                            foreach (DataRow dataRow in dt.Rows)
                            {
                                dataRow[foreignKeyColumnName] = keys[dataRow[foreignKeyColumnName]];
                                var model = dataRow["__model"];
                                foreignKeyProperty ??= model.GetType().GetProperty(foreignKeyColumnName);
                                foreignKeyProperty.SetValue(model, dataRow[foreignKeyColumnName]);
                            }
                        }
                    }

                OnExecute<object>(connection, transaction,
                $@"

                    SELECT * INTO #{dataTable.TableName} FROM {dataTable.TableName} WHERE 1 = 0
                    SET IDENTITY_INSERT #{dataTable.TableName} ON

                ", null);

                var tempDataTable = dataTable.Copy();
                tempDataTable.Columns.Remove("__model");

                using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)transaction))
                {
                    sqlBulkCopy.DestinationTableName = $"#{dataTable.TableName}";
                    sqlBulkCopy.WriteToServer(tempDataTable);
                }

                var mergeSql = $@"

                    MERGE INTO {dataTable.TableName}
                    USING #{dataTable.TableName} AS tableToInsert ON 1 = 0 
                    WHEN NOT MATCHED BY TARGET
                    THEN INSERT({dataTable.ExtendedProperties[nameof(SaveParameters.SqlColumnList)]})
                      VALUES({dataTable.ExtendedProperties[nameof(SaveParameters.SqlInsertColumnList)]})
                    OUTPUT INSERTED.{dataTable.ExtendedProperties[nameof(SaveParameters.ModelKeyPropertyName)]} AS {nameof(InsertKeyPair.InsertedKey)},
                      tableToInsert.{dataTable.ExtendedProperties
                      [nameof(SaveParameters.ModelKeyPropertyName)]} AS {nameof(InsertKeyPair.OriginalKey)};
                ";

                var insertKeyPairs = OnLoad(connection, transaction, mergeSql, null, () => new InsertKeyPair());

                OnExecute<object>(connection, transaction, $@"DROP TABLE #{dataTable.TableName}", null);

                var modelKeyPropertyName = dataTable.ExtendedProperties[nameof(SaveParameters.ModelKeyPropertyName)] as string;
                var modelOriginalKeyPropertyName = dataTable.ExtendedProperties[nameof(SaveParameters.ModelOriginalKeyPropertyName)] as string;
                PropertyInfo modelKeyProperty = null;
                PropertyInfo modelOriginalKeyProperty = null;

                foreignKeyColumnName = dataTable.TableName + dataTable.PrimaryKey[0].ColumnName;

                foreach (var insertKeyPair in insertKeyPairs)
                {
                    keys.Add(insertKeyPair.OriginalKey, insertKeyPair.InsertedKey);

                    if(shouldUpdatedInsertedIds)
                    {
                        var row = dataTable.Rows.Find(insertKeyPair.OriginalKey);
                        var model = row["__model"];

                        modelKeyProperty ??= model.GetType().GetProperty(modelKeyPropertyName);
                        modelOriginalKeyProperty ??= model.GetType().GetProperty(modelOriginalKeyPropertyName);

                        modelOriginalKeyProperty.SetValue(model, modelKeyProperty.GetValue(model));
                        modelKeyProperty.SetValue(model, insertKeyPair.InsertedKey);
                    }
                }
            }
        }

        public void CreateHelperStoredProcedures(DbConnection connection, DbTransaction transaction)
        {
            var resourceSet = new ResourceManager(typeof(Resources)).GetResourceSet(CultureInfo.InvariantCulture, true, true);

            foreach (var helperStoredProcedureScript in resourceSet)
                OnExecute<object>(connection, transaction, ((DictionaryEntry)helperStoredProcedureScript).Value as string, null);
        }
    }

    internal class InsertKeyPair
    {
        public object InsertedKey { get; set; }
        public object OriginalKey { get; set; }
    }
}
