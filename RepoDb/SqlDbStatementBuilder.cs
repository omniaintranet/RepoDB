﻿using RepoDb.Enumerations;
using RepoDb.Extensions;
using RepoDb.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RepoDb
{
    /// <summary>
    /// A class used to build a SQL Statement for SQL Server. This is the default statement builder used by the library.
    /// </summary>
    public sealed class SqlDbStatementBuilder : IStatementBuilder
    {
        /// <summary>
        /// Creates a new instance of <i>RepoDb.SqlDbStatementBuilder</i> object.
        /// </summary>
        public SqlDbStatementBuilder() { }

        /// <summary>
        /// Creates a SQL Statement for repository <i>BatchQuery</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <param name="page">The page of the batch.</param>
        /// <param name="rowsPerBatch">The number of rows per batch.</param>
        /// <param name="orderBy">The list of fields used for ordering.</param>
        /// <returns>A string containing the composed SQL Statement for <i>BatchQuery</i> operation.</returns>
        public string CreateBatchQuery<TEntity>(QueryBuilder<TEntity> queryBuilder, QueryGroup where, int page, int rowsPerBatch, IEnumerable<OrderField> orderBy)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            var queryProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Query);
            var batchQueryProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.BatchQuery)
                .Where(property => queryProperties.Contains(property));
            var fields = batchQueryProperties.Select(property => new Field(property.Name));
            queryBuilder
                .Clear()
                .With()
                .WriteText("CTE")
                .As()
                .OpenParen()
                .Select()
                .RowNumber()
                .Over()
                .OpenParen()
                .OrderBy(orderBy)
                .CloseParen()
                .As("[RowNumber],")
                .Fields(Command.BatchQuery)
                .From()
                .Table(Command.BatchQuery)
                .Where(where)
                .CloseParen()
                .Select()
                .Fields(fields)
                .From()
                .WriteText("CTE")
                .WriteText($"WHERE ([RowNumber] BETWEEN {(page * rowsPerBatch) + 1} AND {(page + 1) * rowsPerBatch})")
                .OrderBy(orderBy)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Count</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Count</i> operation.</returns>
        public string CreateCount<TEntity>(QueryBuilder<TEntity> queryBuilder, QueryGroup where)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            queryBuilder
                .Clear()
                .Select()
                .CountBig()
                .WriteText("(1) AS [Counted]")
                .From()
                .Table(Command.Count)
                .Where(where)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Delete</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Delete</i> operation.</returns>
        public string CreateDelete<TEntity>(QueryBuilder<TEntity> queryBuilder, QueryGroup where)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            queryBuilder
                .Clear()
                .Delete()
                .From()
                .Table(Command.Delete)
                .Where(where)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>DeleteAll</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <returns>A string containing the composed SQL Statement for <i>DeleteAll</i> operation.</returns>
        public string CreateDeleteAll<TEntity>(QueryBuilder<TEntity> queryBuilder)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            queryBuilder
                .Clear()
                .Delete()
                .From()
                .Table(Command.DeleteAll)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>InlineUpdate</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="fields">The list of fields to be a part of inline update operation on SQL Statement composition.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <param name="overrideIgnore">
        /// Set to <i>true</i> if the defined <i>RepoDb.Attributes.IgnoreAttribute</i> would likely 
        /// be ignored on the inline update operation on SQL Statement composition.
        /// </param>
        /// <returns>A string containing the composed SQL Statement for <i>InlineUpdate</i> operation.</returns>
        public string CreateInlineUpdate<TEntity>(QueryBuilder<TEntity> queryBuilder, IEnumerable<Field> fields,
            QueryGroup where, bool? overrideIgnore = false)
            where TEntity : DataEntity
        {
            if (overrideIgnore == false)
            {
                var updateableProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Update);
                var inlineUpdateableProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.InlineUpdate)
                    .Where(property => property != DataEntityExtension.GetPrimaryProperty<TEntity>() && updateableProperties.Contains(property))
                    .Select(property => property.GetMappedName());
                var unmatchesProperties = fields?.Where(field =>
                    inlineUpdateableProperties?.FirstOrDefault(property =>
                        field.Name.ToLower() == property.ToLower()) == null);
                if (unmatchesProperties?.Count() > 0)
                {
                    throw new InvalidOperationException($"The following columns ({unmatchesProperties.Select(field => field.AsField()).Join(", ")}) " +
                        $"are not updatable for entity ({DataEntityExtension.GetMappedName<TEntity>(Command.InlineUpdate)}).");
                }
            }
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            queryBuilder
                .Clear()
                .Update()
                .Table(Command.InlineUpdate)
                .Set()
                .FieldsAndParameters(fields)
                .Where(where)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Insert</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Insert</i> operation.</returns>
        public string CreateInsert<TEntity>(QueryBuilder<TEntity> queryBuilder)
            where TEntity : DataEntity
        {
            return CreateInsert(queryBuilder, false);
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Insert</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="isPrimaryIdentity">A boolean value indicates whether the primary key is identity in the database.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Insert</i> operation.</returns>
        internal string CreateInsert<TEntity>(QueryBuilder<TEntity> queryBuilder, bool isPrimaryIdentity)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            var primary = DataEntityExtension.GetPrimaryProperty<TEntity>();
            var fields = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Insert)
                .Where(property => !(isPrimaryIdentity && property == primary))
                .Select(p => new Field(p.Name));
            queryBuilder
                .Clear()
                .Insert()
                .Into()
                .Table(Command.Insert)
                .OpenParen()
                .Fields(fields)
                .CloseParen()
                .Values()
                .OpenParen()
                .Parameters(fields)
                .CloseParen()
                .End();
            var result = isPrimaryIdentity ? "SCOPE_IDENTITY()" : $"@{primary.GetMappedName()}";
            queryBuilder
                .Select()
                .WriteText(result)
                .As("[Result]")
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Merge</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="qualifiers">The list of qualifier fields to be used for the <i>Merge</i> operation on SQL Statement composition.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Merge</i> operation.</returns>
        public string CreateMerge<TEntity>(QueryBuilder<TEntity> queryBuilder, IEnumerable<Field> qualifiers)
            where TEntity : DataEntity
        {
            return CreateMerge(queryBuilder, qualifiers);
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Merge</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="qualifiers">The list of qualifier fields to be used for the <i>Merge</i> operation on SQL Statement composition.</param>
        /// <param name="isPrimaryIdentity">A boolean value indicates whether the primary key is identity in the database.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Merge</i> operation.</returns>
        internal string CreateMerge<TEntity>(QueryBuilder<TEntity> queryBuilder, IEnumerable<Field> qualifiers, bool isPrimaryIdentity)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            var primary = DataEntityExtension.GetPrimaryProperty<TEntity>();
            if (qualifiers == null && primary != null)
            {
                qualifiers = new Field(primary?.Name).AsEnumerable();
            }
            var insertProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Insert)
                .Where(property => !(isPrimaryIdentity && property == primary));
            var updateProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Insert)
                .Where(property => property != primary);
            var mergeProperties = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Merge);
            var mergeInsertableFields = mergeProperties
                .Where(property => insertProperties.Contains(property))
                .Select(property => new Field(property.Name));
            var mergeUpdateableFields = mergeProperties
                .Where(property => updateProperties.Contains(property))
                .Select(property => new Field(property.Name));
            queryBuilder
                .Clear()
                // MERGE T USING S
                .Merge()
                .Table(Command.Merge)
                .As("T")
                .Using()
                .OpenParen()
                .Select()
                .ParametersAsFields(Command.None) // All fields must be included for selection
                .CloseParen()
                .As("S")
                // QUALIFIERS
                .On()
                .OpenParen()
                .WriteText(qualifiers?
                    .Select(
                        field => field.AsJoinQualifier("S", "T"))
                            .Join($" {StringConstant.And.ToUpper()} "))
                .CloseParen()
                // WHEN NOT MATCHED THEN INSERT VALUES
                .When()
                .Not()
                .Matched()
                .Then()
                .Insert()
                .OpenParen()
                .Fields(mergeInsertableFields)
                .CloseParen()
                .Values()
                .OpenParen()
                .Parameters(mergeInsertableFields)
                .CloseParen()
                // WHEN MATCHED THEN UPDATE SET
                .When()
                .Matched()
                .Then()
                .Update()
                .Set()
                .FieldsAndAliasFields(mergeUpdateableFields, "S")
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Query</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <param name="top">The number of rows to be returned by the <i>Query</i> operation on SQL Statement composition.</param>
        /// <param name="orderBy">The list of fields  to be used for ordering on SQL Statement composition.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Query</i> operation.</returns>
        public string CreateQuery<TEntity>(QueryBuilder<TEntity> queryBuilder, QueryGroup where, int? top = 0, IEnumerable<OrderField> orderBy = null)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            queryBuilder
                .Clear()
                .Select()
                .Top(top)
                .Fields(Command.Query)
                .From()
                .Table(Command.Query)
                .Where(where)
                .OrderBy(orderBy)
                .End();
            return queryBuilder.GetString();
        }

        /// <summary>
        /// Creates a SQL Statement for repository <i>Update</i> operation that is meant for SQL Server.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The <i>DataEntity</i> object bound for the SQL Statement to be created.
        /// </typeparam>
        /// <param name="queryBuilder">An instance of query builder used to build the SQL statement.</param>
        /// <param name="where">The query expression for SQL statement.</param>
        /// <returns>A string containing the composed SQL Statement for <i>Update</i> operation.</returns>
        public string CreateUpdate<TEntity>(QueryBuilder<TEntity> queryBuilder, QueryGroup where)
            where TEntity : DataEntity
        {
            queryBuilder = queryBuilder ?? new QueryBuilder<TEntity>();
            var fields = DataEntityExtension.GetPropertiesFor<TEntity>(Command.Update)
                .Where(property => property != DataEntityExtension.GetPrimaryProperty<TEntity>())
                .Select(p => new Field(p.Name));
            queryBuilder
                .Clear()
                .Update()
                .Table(Command.Update)
                .Set()
                .FieldsAndParameters(fields)
                .Where(where)
                .End();
            return queryBuilder.GetString();
        }
    }
}
