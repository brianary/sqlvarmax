using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	public struct MaxableColumn
	{
		public readonly string DatabaseName;
		public readonly string TableSchema;
		public readonly string TableName;
		public readonly string ColumnName;
		public readonly string CurrentDataTypeName;
		public readonly string MaxDataTypeName;
		public readonly bool Nullable;
		public readonly string Nullability;
		public readonly string SqlConversionString;

		public MaxableColumn(Column column)
		{
			Table table = column.Parent as Table;
			Database database = table.Parent as Database;
			DatabaseName = database.Name;
			TableSchema = table.Schema;
			TableName = table.Name;
			ColumnName = column.Name;
			DataType currentdatatype = column.DataType;
			CurrentDataTypeName = column.DataType.ToSqlString();
			DataType maxdatatype = DataType.Int;
			switch (currentdatatype.SqlDataType)
			{
				case SqlDataType.Image: maxdatatype = new DataType(SqlDataType.VarBinaryMax); break;
				case SqlDataType.NText: maxdatatype = new DataType(SqlDataType.NVarCharMax); break;
				case SqlDataType.Text: maxdatatype = new DataType(SqlDataType.VarCharMax); break;
				default: maxdatatype = currentdatatype; break;
			}
			MaxDataTypeName = maxdatatype.ToSqlString();
			Nullable = column.Nullable;
			Nullability = Nullable ? "null" : "not null";
			SqlConversionString = String.Format("alter table [{0}].[{1}].[{2}] alter column [{3}] {4} {5}; -- was {6}\nGO\n"
					+ "update [{0}].[{1}].[{2}] set [{3}]= [{3}]\nGO\n",
					DatabaseName, TableSchema, TableName, ColumnName, MaxDataTypeName, Nullability, CurrentDataTypeName);
		}

		public static List<MaxableColumn> FindMaxableColumns(ColumnCollection columns)
		{
			var maxables = new List<MaxableColumn>();
			foreach(Column column in columns)
				switch (column.DataType.SqlDataType)
				{
					case SqlDataType.Image:
					case SqlDataType.NText:
					case SqlDataType.Text:
						maxables.Add(new MaxableColumn(column));
						break;
				}
			return maxables;
		}
	}
}
