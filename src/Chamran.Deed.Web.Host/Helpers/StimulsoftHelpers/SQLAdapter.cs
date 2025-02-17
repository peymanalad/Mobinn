﻿/*
Stimulsoft.Reports.JS
Version: 2023.2.8
Build date: 2023.06.27
License: https://www.stimulsoft.com/en/licensing/reports
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Chamran.Deed.Web.Helpers.StimulsoftHelpers
{
    public class SQLAdapter
    {
        private static DbConnection connection;
        private static DbDataReader reader;
        private static CommandJson command;

        private static Result End(Result result)
        {
            result.AdapterVersion = "2023.2.8";
            try
            {
                if (reader != null) reader.Close();
                if (connection != null) connection.Close();

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        private static Result OnError(string message)
        {
            return End(new Result { Success = false, Notice = message });
        }

        private static Result Connect()
        {
            try
            {
                connection.Open();
                return OnConnect();
            }
            catch (Exception e)
            {
                return OnError(e.Message);
            }
        }

        private static Result OnConnect()
        {
            if (!string.IsNullOrEmpty(command.QueryString)) return Query();
            else return End(new Result { Success = true });
        }

        private static Result Query()
        {
            try
            {
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandType = command.Command == "Execute" ? CommandType.StoredProcedure : CommandType.Text;
                sqlCommand.CommandText = command.QueryString;

                foreach (var parameter in command.Parameters)
                {
                    var sqlParameter = sqlCommand.CreateParameter();
                    sqlParameter.ParameterName = parameter.Name;
                    sqlParameter.DbType = (DbType)parameter.NetType;
                    sqlParameter.Size = parameter.Size;
                    if (sqlParameter.DbType == DbType.Decimal) sqlParameter.Precision = (byte)parameter.Size;
                    sqlParameter.Value = GetValue((JsonElement)parameter.Value, parameter.TypeGroup);
                    sqlCommand.Parameters.Add(sqlParameter);
                }
                reader = sqlCommand.ExecuteReader();
                return OnQuery();
            }
            catch (Exception e)
            {
                return OnError(e.Message);
            }
        }

        private static Result OnQuery()
        {
            var columns = new List<string>();
            var rows = new List<string[]>();
            var types = new List<string>();

            for (var index = 0; index < reader.FieldCount; index++)
            {
                var columnName = reader.GetName(index);
                var columnType = GetType(reader.GetDataTypeName(index));
                if (columnType == "string" && reader.GetFieldType(index).Equals(typeof(byte[])))
                    columnType = "array";

                columns.Add(columnName);
                types.Add(columnType);
            }

            while (reader.Read())
            {
                var row = new string[reader.FieldCount];
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    object value = null;
                    try
                    {
                        if (!reader.IsDBNull(index))
                        {
                            value = reader.GetValue(index);
                        }
                    }
                    catch
                    {
                        value = null;
                    }

                    if (value == null) value = "";
                    if (value is DateTime)
                    {
                        row[index] = ((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
                        types[index] = "datetime";
                    }
                    else if (value is DateTimeOffset)
                    {
                        row[index] = ((DateTimeOffset)value).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
                        types[index] = "datetimeoffset";
                    }
                    else if (value is TimeSpan)
                    {
                        row[index] = Math.Truncate(((TimeSpan)value).TotalHours) + ":" + ((TimeSpan)value).Minutes + ":" + ((TimeSpan)value).Seconds;
                        types[index] = "time";
                    }
                    else
                    {
                        if (types[index] == "array")
                            value = GetBytes(index);

                        row[index] = value.ToString();
                    }
                }
                rows.Add(row);
            }

            return End(new Result { Success = true, Columns = columns.ToArray(), Rows = rows.ToArray(), Types = types.ToArray() });
        }

        private static string GetBytes(int index)
        {
            var size = reader.GetBytes(index, 0, null, 0, 0);
            var destination = new MemoryStream();
            var buffer = new byte[8040];
            long offset = 0;
            long read;

            while ((read = reader.GetBytes(index, offset, buffer, 0, buffer.Length)) > 0)
            {
                offset += read;
                destination.Write(buffer, 0, (int)read);
                if (size == offset) break;
            }

            return Convert.ToBase64String(destination.ToArray());
        }

        private static string GetType(string dbType)
        {
            if (connection is not SqlConnection) return "string";
            switch (dbType.ToLowerInvariant())
            {
                case "uniqueidentifier":
                case "bigint":
                case "timestamp":
                case "int":
                case "smallint":
                case "tinyint":
                    return "int";

                case "decimal":
                case "money":
                case "smallmoney":
                case "float":
                case "real":
                    return "number";

                case "datetime":
                case "date":
                case "datetime2":
                case "smalldatetime":
                    return "datetime";

                case "time":
                    return "time";

                case "datetimeoffset":
                    return "datetimeoffset";

                case "bit":
                    return "boolean";

                case "binary":
                case "image":
                    return "array";
            }

            return "string";
        }

        private static object GetValue(JsonElement json, string type)
        {
            try
            {
                switch (type)
                {
                    case "string": return json.GetString();
                    case "number": return json.GetDecimal();
                    case "datetime": return json.GetDateTime();
                }
            }
            catch
            {
            }
            return json.GetString();
        }

        public static Result Process(CommandJson command, DbConnection connection)
        {
            SQLAdapter.connection = connection;
            SQLAdapter.command = command;
            return Connect();
        }
    }
}