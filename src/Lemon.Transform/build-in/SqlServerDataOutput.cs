﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace Lemon.Transform
{
    public class SqlServerDataOutput : AbstractDataOutput, IDisposable
    {
        private string _connectionString;

        private string _tableName;

        private string _primaryKey;

        private IEnumerable<string> _columnNames;

        private SqlConnection _connection;

        private string _sql;

        private SQLInserOrUpdateQueryBuilder _builder;

        public SqlServerDataOutput(DataOutputModel model)
        {
            _connectionString = model.Connection;

            _tableName = model.ObjectName;

            _primaryKey = model.PrimaryKey;

            _columnNames = model.ColumnNames;
            
            _builder = new SQLInserOrUpdateQueryBuilder(_tableName, _primaryKey);

            _sql = _builder.Build(model.ColumnNames, model.IsUpsert);

            DetermineWriteOrNot = BuildDetermineWriteOrNotFunction(model.WriteOnChange);

            Connect();
        }

        private void Connect()
        {
            _connection = new SqlConnection(_connectionString);
        }

        protected override DataRowStatusContext BuildDataRowStatusContext(string[] excludes)
        {
            var columns = new List<string>();

            foreach (var column in _columnNames)
            {
                if (column == _primaryKey || excludes.Any(exclue => exclue == column))
                {
                    continue;
                }

                columns.Add(column);
            }

            return new SqlServerRecordStatusContext(
                    _connectionString,
                    _tableName,
                    new string[] { _primaryKey },
                    columns.ToArray());
        }

        private void Input(BsonDataRow inputRow)
        {
            if(!DetermineWriteOrNot(inputRow))
            {
                return;
            }

            var dict = new Dictionary<string, object>();

            foreach (var columName in _columnNames)
            {
                dict.Add(columName, CastUtil.Cast(inputRow.GetValue(columName)));
            }

            _connection.Execute(_sql, dict);
        }

        public void Dispose()
        {
            _connection.Close();
        }

        protected override void OnReceive(BsonDataRow row)
        {
            Input(row);
        }
    }
}
