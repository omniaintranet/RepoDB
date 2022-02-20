﻿using RepoDb.Contexts.Cachers;
using RepoDb.Contexts.Execution;
using RepoDb.Extensions;
using RepoDb.Interfaces;
using RepoDb.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RepoDb.Contexts.Providers
{
    /// <summary>
    /// 
    /// </summary>
    internal static class MergeExecutionContextProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="tableName"></param>
        /// <param name="qualifiers"></param>
        /// <param name="fields"></param>
        /// <param name="hints"></param>
        /// <returns></returns>
        private static string GetKey(Type entityType,
            string tableName,
            IEnumerable<Field> qualifiers,
            IEnumerable<Field> fields,
            string hints)
        {
            return string.Concat(entityType.FullName,
                ";",
                tableName,
                ";",
                qualifiers?.Select(f => f.Name).Join(","),
                ";",
                fields?.Select(f => f.Name).Join(","),
                ";",
                hints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <param name="qualifiers"></param>
        /// <param name="fields"></param>
        /// <param name="hints"></param>
        /// <param name="transaction"></param>
        /// <param name="statementBuilder"></param>
        /// <returns></returns>
        public static MergeExecutionContext Create(Type entityType,
            IDbConnection connection,
            string tableName,
            IEnumerable<Field> qualifiers,
            IEnumerable<Field> fields,
            string hints = null,
            IDbTransaction transaction = null,
            IStatementBuilder statementBuilder = null)
        {
            var key = GetKey(entityType, tableName, qualifiers, fields, hints);

            // Get from cache
            var context = MergeExecutionContextCache.Get(key);
            if (context != null)
            {
                return context;
            }

            // Create
            var dbFields = DbFieldCache.Get(connection, tableName, transaction);
            var request = new MergeRequest(tableName,
                connection,
                transaction,
                fields,
                qualifiers,
                hints,
                statementBuilder);
            var commandText = CommandTextCache.GetMergeText(request);

            // Call
            context = CreateInternal(entityType,
                connection,
                dbFields,
                tableName,
                fields,
                commandText);

            // Add to cache
            MergeExecutionContextCache.Add(key, context);

            // Return
            return context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <param name="qualifiers"></param>
        /// <param name="fields"></param>
        /// <param name="hints"></param>
        /// <param name="transaction"></param>
        /// <param name="statementBuilder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<MergeExecutionContext> CreateAsync(Type entityType,
            IDbConnection connection,
            string tableName,
            IEnumerable<Field> qualifiers,
            IEnumerable<Field> fields,
            string hints = null,
            IDbTransaction transaction = null,
            IStatementBuilder statementBuilder = null,
            CancellationToken cancellationToken = default)
        {
            var key = GetKey(entityType, tableName, qualifiers, fields, hints);

            // Get from cache
            var context = MergeExecutionContextCache.Get(key);
            if (context != null)
            {
                return context;
            }

            // Create
            var dbFields = await DbFieldCache.GetAsync(connection, tableName, transaction, cancellationToken);
            var request = new MergeRequest(tableName,
                connection,
                transaction,
                fields,
                qualifiers,
                hints,
                statementBuilder);
            var commandText = await CommandTextCache.GetMergeTextAsync(request, cancellationToken);

            // Call
            context = CreateInternal(entityType,
                connection,
                dbFields,
                tableName,
                fields,
                commandText);

            // Add to cache
            MergeExecutionContextCache.Add(key, context);

            // Return
            return context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="connection"></param>
        /// <param name="dbFields"></param>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private static MergeExecutionContext CreateInternal(Type entityType,
            IDbConnection connection,
            IEnumerable<DbField> dbFields,
            string tableName,
            IEnumerable<Field> fields,
            string commandText)
        {
            var dbSetting = connection.GetDbSetting();
            var primary = (Field)null;
            var inputFields = new List<DbField>();
            var primaryDbField = dbFields?.FirstOrDefault(f => f.IsPrimary);

            // Set the primary field
            primary = PrimaryCache.Get(entityType)?.AsField() ??
                FieldCache
                    .Get(entityType)?
                    .FirstOrDefault(field =>
                        string.Equals(field.Name.AsUnquoted(true, dbSetting), primaryDbField?.Name.AsUnquoted(true, dbSetting), StringComparison.OrdinalIgnoreCase)) ??
                primaryDbField?.AsField();

            // Filter the actual properties for input fields
            inputFields = dbFields?
                .Where(dbField =>
                    fields.FirstOrDefault(field =>
                        string.Equals(field.Name.AsUnquoted(true, dbSetting), dbField.Name.AsUnquoted(true, dbSetting), StringComparison.OrdinalIgnoreCase)) != null)
                .AsList();

            // Variables for the entity action
            var primaryPropertySetter = (Action<object, object>)null;

            // Get the identity setter
            if (primary != null)
            {
                primaryPropertySetter = FunctionCache
                    .GetDataEntityPropertySetterCompiledFunction(entityType, primary);
            }

            // Return the value
            return new MergeExecutionContext
            {
                CommandText = commandText,
                InputFields = inputFields,
                ParametersSetterFunc = FunctionCache
                    .GetDataEntityDbParameterSetterCompiledFunction(entityType,
                        string.Concat(entityType.FullName, StringConstant.Period, tableName, ".Merge"),
                        inputFields?.AsList(),
                        null,
                        dbSetting),
                PrimaryPropertySetterFunc = primaryPropertySetter
            };
        }
    }
}
