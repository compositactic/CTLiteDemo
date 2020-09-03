// CTLite - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

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

using CTLite.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace CTLite.Data
{
    public abstract class SqlRepository : ISqlRepository
    {
        public CompositeRoot CompositeRoot { get; set; }

        public DbConnection OpenConnection(string connectionString)
        {
            return OnOpenNewConnection(connectionString);
        }

        public DbTransaction BeginTransaction(DbConnection connection)
        {
            return connection.BeginTransaction();
        }

        public void CommitTransaction(DbTransaction transaction)
        {
            transaction.Commit();
        }

        public void CloseConnection(DbConnection connection)
        {
            connection.Close();
        }

        public T Execute<T>(DbConnection connection, DbTransaction transaction, string statement, IEnumerable<DbParameter> parameters)
        {
            return OnExecute<T>(connection, transaction, statement, parameters);
        }

        public IEnumerable<T> Load<T>(DbConnection connection, DbTransaction transaction, string query, IEnumerable<DbParameter> parameters, Func<T> newModelFunc)
        {
            return OnLoad(connection, transaction, query, parameters, newModelFunc);
        }

        public void Save(DbConnection connection, DbTransaction transaction, Composite composite)
        {
            Save(connection, transaction, composite, true);
        }

        public void Save(DbConnection connection, DbTransaction transaction, Composite composite, bool shouldUpdateInsertedIds)
        {
            var newComposites = new List<Composite>();

            composite.TraverseDepthFirst((c) =>
            {
                CompositeContainerAttribute compositeContainerAttribute;

                var compositeType = c.GetType();
                CompositeModelAttribute compositeModelAttribute;

                if ((compositeContainerAttribute = compositeType.FindCustomAttribute<CompositeContainerAttribute>()) != null)
                {
                    var removedIdsProperty = compositeType
                        .GetProperty(compositeContainerAttribute.CompositeContainerDictionaryPropertyName)
                        .GetValue(c)
                        .GetType().GetProperty(nameof(CompositeDictionary<object, Composite>.RemovedIds));

                    var compositeDictionary = compositeType
                        .GetProperty(compositeContainerAttribute.CompositeContainerDictionaryPropertyName)
                        .GetValue(c);

                    dynamic deletedIds = removedIdsProperty.GetValue(compositeDictionary);

                    if (deletedIds.Count > 0)
                    {
                        var deletedCompositeType = compositeDictionary.GetType().GetGenericArguments()[1];
                        var deletedModelType = deletedCompositeType.GetField(deletedCompositeType.GetCustomAttribute<CompositeModelAttribute>().ModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic).FieldType;
                        var tableName = deletedModelType.GetCustomAttribute<DataContractAttribute>().Name ?? deletedModelType.Name;
                        var tableKeyPropertyName = deletedModelType.GetCustomAttribute<KeyPropertyAttribute>().KeyPropertyName;
                        OnDelete(connection, transaction, tableName, tableKeyPropertyName, deletedIds);
                        dynamic internalCompositeDictionary = compositeType.GetField(compositeType.GetCustomAttribute<CompositeContainerAttribute>().InternalCompositeContainerDictionaryPropertyName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
                        internalCompositeDictionary.ClearRemovedIds();
                    }
                }

                if ((compositeModelAttribute = compositeType.FindCustomAttribute<CompositeModelAttribute>()) != null)
                {
                    switch (c.State)
                    {
                        case CompositeState.Modified:

                            var modelFieldInfo = compositeType.GetField(compositeModelAttribute?.ModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                            var dataContractAttribute = modelFieldInfo?.FieldType.GetCustomAttribute<DataContractAttribute>();
                            var modelKeyPropertyAttribute = modelFieldInfo?.FieldType.GetCustomAttribute<KeyPropertyAttribute>();
                            var modelKeyProperty = modelFieldInfo?.FieldType.GetProperty(modelKeyPropertyAttribute.KeyPropertyName);
                            var modelKeyDataMemberAttribute = modelKeyProperty?.GetCustomAttribute<DataMemberAttribute>();

                            if (modelFieldInfo == null)
                                throw new MemberAccessException(Resources.CannotFindCompositeModelProperty);

                            if (dataContractAttribute == null)
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MustHaveDataContractAttribute, modelFieldInfo.FieldType.Name));

                            if (modelKeyPropertyAttribute == null)
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MustHaveKeyPropertyAttribute, modelFieldInfo.FieldType.Name));

                            if (modelKeyProperty == null)
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidPropertyName, modelKeyPropertyAttribute.KeyPropertyName));

                            if (modelKeyDataMemberAttribute == null)
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MustHaveDataMemberAttribute, modelKeyPropertyAttribute.KeyPropertyName));

                            var dataRow = new Composite[] { c }.ToDataTable().Rows[0];
                            var columnValues = dataRow.Table.Columns.Cast<DataColumn>().Where(column => column.ColumnName != "__model").ToDictionary(column => column.ColumnName, column => dataRow[column]);

                            var keyColumnName = modelKeyDataMemberAttribute.Name ?? modelKeyProperty.Name;
                            var keyValue = dataRow[keyColumnName];

                            var tableName = dataContractAttribute.Name ?? modelFieldInfo.FieldType.Name;
                            var tableKeyPropertyName = modelKeyDataMemberAttribute.Name ?? modelKeyProperty.Name;

                            if (!Regex.IsMatch(tableName, @"^[A-Za-z0-9_]+$"))
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidTableName, tableName));

                            if (!Regex.IsMatch(tableKeyPropertyName, @"^[A-Za-z0-9_]+$"))
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidColumnName, tableKeyPropertyName));

                            string invalidColumnName = null;
                            if ((invalidColumnName = columnValues.Keys.FirstOrDefault(column => !Regex.IsMatch(column, @"^[A-Za-z0-9_]+$"))) != null)
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidColumnName, invalidColumnName));

                            OnUpdate(connection, transaction, tableName, tableKeyPropertyName, keyValue, columnValues);
                            c.State = CompositeState.Unchanged;

                            break;
                        case CompositeState.New:
                            newComposites.Add(c);
                            break;
                        default:
                            break;
                    }
                }
            });

            if (newComposites.Count() > 0)
            {
                SaveNewComposites(connection, transaction, newComposites, shouldUpdateInsertedIds);
                if(shouldUpdateInsertedIds)
                    UpdateNewKeyValues(composite);
            }

        }

        private static void UpdateNewKeyValues(Composite composite)
        {
            composite.TraverseDepthFirst((c) =>
            {
                if (c.GetType().GetCustomAttribute<CompositeModelAttribute>() != null && c.GetType().GetCustomAttribute<KeyPropertyAttribute>() != null)
                {
                    var compositeType = c.GetType();
                    var keyPropertyAttribute = c.GetType().GetCustomAttribute<KeyPropertyAttribute>();

                    var compositeDictionaryContainer = compositeType.GetProperty(compositeType.GetCustomAttribute<ParentPropertyAttribute>().ParentPropertyName).GetValue(c);
                    var compositeDictionaryContainerType = compositeDictionaryContainer.GetType();
                    var compositeOriginalId = compositeType.GetProperty(keyPropertyAttribute.OriginalKeyPropertyName).GetValue(c);
                    var compositeNewId = compositeType.GetProperty(keyPropertyAttribute.KeyPropertyName).GetValue(c);
                    
                    dynamic compositeDictionary = compositeDictionaryContainerType.GetField(compositeDictionaryContainerType.GetCustomAttribute<CompositeContainerAttribute>().InternalCompositeContainerDictionaryPropertyName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compositeDictionaryContainer);
                    compositeDictionary.Remove(compositeOriginalId);
                    compositeDictionary.Add(compositeNewId, c);
                }
            });
        }

        private void SaveNewComposites(DbConnection connection, DbTransaction transaction, List<Composite> newComposites, bool shouldUpdateInsertedIds)
        {
            DataTable dataTable = null;

            try
            {
                var dataTablesToInsert = new List<DataTable>();
                var newCompositeTypes = newComposites.Select(nc => nc.GetType()).Distinct();

                var sqlColumnList = string.Empty;
                var sqlInsertColumnList = string.Empty;

                foreach (var compositeType in newCompositeTypes)
                {
                    var compositeModelAttribute = compositeType.FindCustomAttribute<CompositeModelAttribute>();
                    if (compositeModelAttribute == null)
                        throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, Resources.MustHaveCompositeModelAttribute, compositeType.Name));

                    FieldInfo modelFieldInfo;
                    modelFieldInfo = compositeType.GetField(compositeModelAttribute.ModelFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                    if (modelFieldInfo == null)
                        throw new MissingMemberException(string.Format(CultureInfo.CurrentCulture, Resources.CannotFindCompositeModelProperty, compositeModelAttribute.ModelFieldName));

                    var keyName = modelFieldInfo.FieldType.GetCustomAttribute<KeyPropertyAttribute>().KeyPropertyName;

                    var columnProperties = modelFieldInfo
                                            .FieldType
                                            .GetProperties()
                                            .Where(p => p.GetCustomAttribute<DataMemberAttribute>() != null && p.PropertyType != typeof(CompositeState) && p.Name != keyName);

                    var columnList = columnProperties.Select(dataMemberProperty => dataMemberProperty.GetCustomAttribute<DataMemberAttribute>().Name ?? dataMemberProperty.Name);

                    var invalidColumnName = string.Empty;
                    if ((invalidColumnName = columnList.FirstOrDefault(c => !Regex.IsMatch(c, @"^[A-Za-z0-9_]+$"))) != null)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidColumnName, invalidColumnName));

                    sqlColumnList = string.Join(',', columnList);
                    sqlInsertColumnList = string.Join(',', columnList.Select(c => "tableToInsert." + c));

                    dataTable = newComposites.Where(nc => nc.GetType() == compositeType).ToDataTable();

                    var modelKeyPropertyName = modelFieldInfo.FieldType.GetCustomAttribute<KeyPropertyAttribute>()?.KeyPropertyName;
                    if (string.IsNullOrEmpty(modelKeyPropertyName))
                        throw new InvalidOperationException();

                    var modelKeyName = modelFieldInfo.FieldType.GetProperty(modelKeyPropertyName)?.GetCustomAttribute<DataMemberAttribute>()?.Name ?? modelKeyPropertyName;

                    dataTable.ExtendedProperties[nameof(SaveParameters.ModelOriginalKeyPropertyName)] = modelFieldInfo.FieldType.GetCustomAttribute<KeyPropertyAttribute>().OriginalKeyPropertyName;
                    dataTable.ExtendedProperties[nameof(SaveParameters.ModelKeyPropertyName)] = modelKeyName;
                    dataTable.ExtendedProperties[nameof(SaveParameters.SqlColumnList)] = sqlColumnList;
                    dataTable.ExtendedProperties[nameof(SaveParameters.SqlInsertColumnList)] = sqlInsertColumnList;

                    dataTablesToInsert.Add(dataTable);
                }

                string invalidTableName = null;
                if ((invalidTableName = dataTablesToInsert.FirstOrDefault(t => !Regex.IsMatch(t.TableName, @"^[A-Za-z0-9_]+$"))?.TableName) != null)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidTableName, invalidTableName));

                OnInsert(connection, transaction, dataTablesToInsert, shouldUpdateInsertedIds);

                foreach (var composite in newComposites)
                    composite.State = CompositeState.Unchanged;
            }
            catch
            {
                dataTable?.Dispose();
                throw;
            }
            finally
            {
                dataTable?.Dispose();
            }
        }

        protected abstract DbConnection OnOpenNewConnection(string connectionString);
        protected abstract DbTransaction OnBeginNewTransaction(DbConnection connection);
        protected abstract void OnDelete(DbConnection connection, DbTransaction transaction, string tableName, string tableKeyPropertyName, IEnumerable<object> idValues);
        protected abstract void OnInsert(DbConnection connection, DbTransaction transaction, IReadOnlyList<DataTable> dataTablesToInsert, bool shouldUpdatedInsertedIds);
        protected abstract void OnUpdate(DbConnection connection, DbTransaction transaction, string tableName, string tableKeyPropertyName, object tableKeyValue, IReadOnlyDictionary<string, object> columnValues);
        protected abstract void OnCommit(DbConnection connection, DbTransaction transaction);
        protected abstract IEnumerable<T> OnLoad<T>(DbConnection connection, DbTransaction transaction, string query, IEnumerable<DbParameter> parameters, Func<T> newModelFunc);
        protected abstract T OnExecute<T>(DbConnection connection, DbTransaction transaction, string statement, IEnumerable<DbParameter> parameters);

    }
}
