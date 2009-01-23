using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// A column that uses a deprecated data type, and is a candidate for conversion to the new var*(max) data types.
	/// </summary>
	public struct MaxableColumn
	{
		#region Public Fields
		/// <summary>
		/// The name of the database.
		/// </summary>
		public readonly string DatabaseName;

		/// <summary>
		/// The schema name of the table.
		/// </summary>
		public readonly string TableSchema;

		/// <summary>
		/// The name of the table.
		/// </summary>
		public readonly string TableName;

		/// <summary>
		/// The name of the column.
		/// </summary>
		public readonly string ColumnName;

		/// <summary>
		/// The current SQL datatype of the column.
		/// </summary>
		public readonly string CurrentDataTypeName;

		/// <summary>
		/// The corresponding var*(max) datatype to convert the column to.
		/// </summary>
		public readonly string MaxDataTypeName;

		/// <summary>
		/// Can the column accept null values?
		/// </summary>
		public readonly bool Nullable;

		/// <summary>
		/// The column's nullability in SQL syntax: "null" or "not null".
		/// </summary>
		public readonly string Nullability;
		
		/// <summary>
		/// The number of values in this column.
		/// </summary>
		public readonly int RowCount;
		
		/// <summary>
		/// The number of values in this column that are fewer than 8,000 bytes in length.
		/// </summary>
		public readonly int RowsUnder8000BytesCount;
		
		/// <summary>
		/// The minimum length of the data in the column.
		/// </summary>
		public readonly int MinDataLength;
		
		/// <summary>
		/// The mean average data length in the column.
		/// </summary>
		public readonly int AvgDataLength;
		
		/// <summary>
		/// The maximum data length in the column.
		/// </summary>
		public readonly int MaxDataLength;
		
		/// <summary>
		/// The SQL script for converting the column to the var*(max) data type.
		/// </summary>
		public readonly string SqlConversionString;
		#endregion

		#region Public Constructors
		/// <summary>
		/// The constructor, which notes the location and statistical data about the column, and builds the SQL conversion script.
		/// </summary>
		/// <param name="column">The database column to convert.</param>
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
			var stats = database.ExecuteWithResults(String.Format("select count(*) as [RowCount], "
				+"(select count(*) from [{0}].[{1}] where datalength([{2}]) < 8000) as [RowsUnder8000BytesCount], "
				+ "coalesce(min(datalength([{2}])),0) as [MinDataLength], coalesce(avg(datalength([{2}])),0) as [AvgDataLength], "
				+ "coalesce(max(datalength([{2}])),0) as [MaxDataLength] "
				+ "from [{0}].[{1}]", TableSchema, TableName, ColumnName)).Tables[0].Rows[0];
			RowCount = (int)stats["RowCount"];
			RowsUnder8000BytesCount = (int)stats["RowsUnder8000BytesCount"];
			MinDataLength = (int)stats["MinDataLength"];
			AvgDataLength = (int)stats["AvgDataLength"];
			MaxDataLength = (int)stats["MaxDataLength"];
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Searches a collection of columns, looking for those that can be converted from deprecated data types.
		/// </summary>
		/// <param name="columns">The collection of columns to search.</param>
		/// <returns>A list of maxable columns.</returns>
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
		#endregion
	}
}
