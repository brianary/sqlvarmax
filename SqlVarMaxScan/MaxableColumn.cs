﻿using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// A column that uses a deprecated data type, and is a candidate for conversion to the new var*(max) data types.
	/// </summary>
	public class MaxableColumn : MaxableItem
	{
		#region Public Fields
		/// <summary>
		/// The name of the table.
		/// </summary>
		public readonly string TableName;

		/// <summary>
		/// The name of the column.
		/// </summary>
		public readonly string ColumnName;

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
		#endregion

		#region Public Constructors
		/// <summary>
		/// The constructor, which notes the location of, and statistical data about, the column, 
		/// and builds the SQL conversion script.
		/// </summary>
		/// <param name="column">The database column to convert.</param>
		public MaxableColumn(Column column)
		{
			Table table = column.Parent as Table;
			if (table == null)
				throw new ArgumentException("Column argument must be a table column. Column passed has a parent of type "
					+ column.Parent.GetType().FullName);
			Database = table.Parent;
			databaseName = Database.Name;
			schemaName = table.Schema;
			TableName = table.Name;
			ColumnName = column.Name;
			DataType currentdatatype = column.DataType;
			currentDataTypeName = column.DataType.ToSqlString();
			DataType maxdatatype = DataType.Int;
			switch (currentdatatype.SqlDataType)
			{
				case SqlDataType.Image: maxdatatype = new DataType(SqlDataType.VarBinaryMax); break;
				case SqlDataType.NText: maxdatatype = new DataType(SqlDataType.NVarCharMax); break;
				case SqlDataType.Text: maxdatatype = new DataType(SqlDataType.VarCharMax); break;
				default: maxdatatype = currentdatatype; break;
			}
			maxDataTypeName = maxdatatype.ToSqlString();
			Nullable = column.Nullable;
			Nullability = Nullable ? "null" : "not null";
			sqlConversionString = String.Format("alter table [{0}].[{1}].[{2}] alter column [{3}] {4} {5}; -- was {6}\nGO\n"
					+ "update [{0}].[{1}].[{2}] set [{3}]= [{3}]\nGO\n",
					DatabaseName, SchemaName, TableName, ColumnName, MaxDataTypeName, Nullability, CurrentDataTypeName);
			var stats = Database.ExecuteWithResults(String.Format("select count(*) as [RowCount], "
				+"(select count(*) from [{0}].[{1}] where datalength([{2}]) < 8000) as [RowsUnder8000BytesCount], "
				+ "coalesce(min(datalength([{2}])),0) as [MinDataLength], coalesce(avg(datalength([{2}])),0) as [AvgDataLength], "
				+ "coalesce(max(datalength([{2}])),0) as [MaxDataLength] "
				+ "from [{0}].[{1}]", SchemaName, TableName, ColumnName)).Tables[0].Rows[0];
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
