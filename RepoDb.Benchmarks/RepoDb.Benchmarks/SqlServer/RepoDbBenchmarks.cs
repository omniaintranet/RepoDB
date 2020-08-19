﻿using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RepoDb.Benchmarks.Models;
using RepoDb.Benchmarks.SqlServer.Setup;

namespace RepoDb.Benchmarks.SqlServer
{
    [Description("RepoDb")]
    public class RepoDbBenchmarks : BaseBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();

            SqlServerBootstrap.Initialize();
            TypeMapper.Add(typeof(DateTime), DbType.DateTime2, true);
        }

        [Benchmark]
        public async Task<Person> FirstAsync()
        {
            using IDbConnection connection = await new SqlConnection(DatabaseHelper.ConnectionString).EnsureOpenAsync();
            var person = await connection.QueryAsync<Person>(x => x.Id == CurrentId);

            return person.First();
        }

        [Benchmark]
        public Person First()
        {
            using IDbConnection connection = new SqlConnection(DatabaseHelper.ConnectionString).EnsureOpen();

            return connection.Query<Person>(x => x.Id == CurrentId).First();
        }
    }
}