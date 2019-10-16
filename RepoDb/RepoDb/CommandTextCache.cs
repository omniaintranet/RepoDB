﻿using RepoDb.Exceptions;
using RepoDb.Extensions;
using RepoDb.Interfaces;
using RepoDb.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RepoDb
{
    /// <summary>
    /// A class used to cache the already-built command texts.
    /// </summary>
    public static class CommandTextCache
    {
        private static readonly ConcurrentDictionary<BaseRequest, string> m_cache = new ConcurrentDictionary<BaseRequest, string>();

        #region GetBatchQueryText

        /// <summary>
        /// Gets a command text from the cache for the batch query operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetBatchQueryText(BatchQueryRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                commandText = statementBuilder.CreateBatchQuery(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Page,
                    request.RowsPerBatch,
                    request.OrderBy,
                    request.Where,
                    request.Hints);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetCountText

        /// <summary>
        /// Gets a command text from the cache for the count operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetCountText(CountRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                commandText = statementBuilder.CreateCount(new QueryBuilder(),
                    request.Name,
                    request.Where);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetCountAllText

        /// <summary>
        /// Gets a command text from the cache for the count-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetCountAllText(CountAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                commandText = statementBuilder.CreateCountAll(new QueryBuilder(),
                    request.Name);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetDeleteText

        /// <summary>
        /// Gets a command text from the cache for the delete operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetDeleteText(DeleteRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                commandText = statementBuilder.CreateDelete(new QueryBuilder(),
                    request.Name,
                    request.Where);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetDeleteAllText

        /// <summary>
        /// Gets a command text from the cache for the delete-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetDeleteAllText(DeleteAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                commandText = statementBuilder.CreateDeleteAll(new QueryBuilder(),
                    request.Name);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetInsertText

        /// <summary>
        /// Gets a command text from the cache for the insert operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetInsertText(InsertRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateInsert(new QueryBuilder(),
                    request.Name,
                    fields,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetInsertAllText

        /// <summary>
        /// Gets a command text from the cache for the insert-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetInsertAllText(InsertAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateInsertAll(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.BatchSize,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetMergeText

        /// <summary>
        /// Gets a command text from the cache for the merge operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetMergeText(MergeRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateMerge(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Qualifiers,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetMergeAllText

        /// <summary>
        /// Gets a command text from the cache for the merge-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetMergeAllText(MergeAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var qualifiers = request.Qualifiers;
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateMergeAll(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Qualifiers,
                    request.BatchSize,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetQueryText

        /// <summary>
        /// Gets a command text from the cache for the query operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetQueryText(QueryRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                commandText = statementBuilder.CreateQuery(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Where,
                    request.OrderBy,
                    request.Top,
                    request.Hints);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetQueryAllText

        /// <summary>
        /// Gets a command text from the cache for the query-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetQueryAllText(QueryAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                commandText = statementBuilder.CreateQueryAll(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.OrderBy,
                    request.Hints);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetQueryMultipleText

        /// <summary>
        /// Gets a command text from the cache for the query-multiple operation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetQueryMultipleText<TEntity>(QueryMultipleRequest request)
            where TEntity : class
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                commandText = statementBuilder.CreateQuery(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Where,
                    request.OrderBy,
                    request.Top,
                    request.Hints);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetTruncateText

        /// <summary>
        /// Gets a command text from the cache for the truncate operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetTruncateText(TruncateRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                commandText = statementBuilder.CreateTruncate(new QueryBuilder(),
                    request.Name);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetUpdateText

        /// <summary>
        /// Gets a command text from the cache for the update operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetUpdateText(UpdateRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateUpdate(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Where,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region GetUpdateAllText

        /// <summary>
        /// Gets a command text from the cache for the update-all operation.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The cached command text.</returns>
        internal static string GetUpdateAllText(UpdateAllRequest request)
        {
            var commandText = (string)null;
            if (m_cache.TryGetValue(request, out commandText) == false)
            {
                var statementBuilder = EnsureStatementBuilder(request.Connection, request.StatementBuilder);
                var fields = GetActualFields(request.Connection, request.Name, request.Fields, request.Transaction);
                var qualifiers = request.Qualifiers;
                var primaryField = GetPrimaryField(request);
                var identityField = GetIdentityField(request);
                commandText = statementBuilder.CreateUpdateAll(new QueryBuilder(),
                    request.Name,
                    fields,
                    request.Qualifiers,
                    request.BatchSize,
                    primaryField,
                    identityField);
                m_cache.TryAdd(request, commandText);
            }
            return commandText;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Flushes all the existing cached command texts.
        /// </summary>
        public static void Flush()
        {
            m_cache.Clear();
        }

        /// <summary>
        /// Get the actual list of <see cref="Field"/> objects of the table based on the actual list of <see cref="DbField"/> objects.
        /// </summary>
        /// <param name="connection">The connection object to be used.</param>
        /// <param name="fields">The target name of the table.</param>
        /// <param name="tableName">The list of fields from the data entity object.</param>
        /// <param name="transaction">The transaction object that is currently in used.</param>
        /// <returns>The actual list of <see cref="Field"/> objects of the table.</returns>
        private static IEnumerable<Field> GetActualFields(IDbConnection connection,
            string tableName,
            IEnumerable<Field> fields,
            IDbTransaction transaction)
        {
            if (fields?.Any() != true)
            {
                return null;
            }

            // Get all the fields from the database, and the setting
            var dbFields = DbFieldCache.Get(connection, tableName, transaction);
            var dbSetting = connection.GetDbSetting();

            // Return the filtered one
            return dbFields?.Any() == true ?
                fields.Where(f => dbFields.FirstOrDefault(df => string.Equals(df.Name, f.Name, StringComparison.OrdinalIgnoreCase)) != null) :
                fields;
        }

        /// <summary>
        /// Gets the primary <see cref="DbField"/> object.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The primary <see cref="DbField"/> object.</returns>
        private static DbField GetPrimaryField(BaseRequest request)
        {
            if (request.Type != null && request.Type != typeof(object))
            {
                var primaryProperty = PrimaryCache.Get(request.Type, DbConnectionExtension.GetDbSetting(request.Connection));
                if (primaryProperty != null)
                {
                    var identityProperty = IdentityCache.Get(request.Type, DbConnectionExtension.GetDbSetting(request.Connection));
                    var isIdentity = false;
                    if (identityProperty != null)
                    {
                        isIdentity = string.Equals(identityProperty.GetUnquotedMappedName(), primaryProperty.GetUnquotedMappedName(), StringComparison.OrdinalIgnoreCase);
                    }
                    return new DbField(primaryProperty.GetUnquotedMappedName(),
                        true,
                        isIdentity,
                        false,
                        primaryProperty.PropertyInfo.PropertyType,
                        null,
                        null,
                        null,
                        null);
                }
            }
            return DbFieldCache.Get(request.Connection, request.Name, request.Transaction)?.FirstOrDefault(f => f.IsPrimary);
        }

        /// <summary>
        /// Gets the identity <see cref="DbField"/> object.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>The identity <see cref="DbField"/> object.</returns>
        private static DbField GetIdentityField(BaseRequest request)
        {
            if (request.Type != null && request.Type != typeof(object))
            {
                var identityProperty = IdentityCache.Get(request.Type, DbConnectionExtension.GetDbSetting(request.Connection));
                if (identityProperty != null)
                {
                    var primaryProperty = PrimaryCache.Get(request.Type, DbConnectionExtension.GetDbSetting(request.Connection));
                    var isPrimary = false;
                    if (primaryProperty != null)
                    {
                        isPrimary = string.Equals(primaryProperty.GetUnquotedMappedName(), identityProperty.GetUnquotedMappedName(), StringComparison.OrdinalIgnoreCase);
                    }
                    return new DbField(identityProperty.GetUnquotedMappedName(),
                        isPrimary,
                        true,
                        false,
                        identityProperty.PropertyInfo.PropertyType,
                        null,
                        null,
                        null,
                        null);
                }
            }
            return DbFieldCache.Get(request.Connection, request.Name, request.Transaction)?.FirstOrDefault(f => f.IsIdentity);
        }

        /// <summary>
        /// Throws an exception of the builder is not defined.
        /// </summary>
        /// <param name="connection">The connection object to identified.</param>
        /// <param name="builder">The builder to be checked.</param>
        /// <returns>The instance of available statement builder.</returns>
        private static IStatementBuilder EnsureStatementBuilder(IDbConnection connection,
            IStatementBuilder builder)
        {
            builder = builder ?? StatementBuilderMapper.Get(connection.GetType());
            if (builder == null)
            {
                throw new MissingMappingException($"There is no statement builder mapping found for '{connection.GetType().FullName}' object.");
            }
            return builder;
        }

        #endregion
    }
}
