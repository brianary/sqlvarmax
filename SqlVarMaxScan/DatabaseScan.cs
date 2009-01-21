using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	public class DatabaseScan
	{
		Database Database;
		public List<MaxableColumn> MaxableColumns = new List<MaxableColumn>();

		public string Name
		{
			get { return Database.Name; }
		}

		public bool HasMaxables
		{
			get { return MaxableColumns.Count > 0; }
		}

		public DatabaseScan(Database database)
		{
			Database = database;
		}

		public void PerformScan()
		{
			foreach (Table table in Database.Tables)
				MaxableColumns.AddRange(MaxableColumn.FindMaxableColumns(table.Columns));
		}

		public string GetSqlConversionString()
		{
			var sql = new StringBuilder();
			sql.AppendFormat("use [{0}]\nGO\n-- Columns\n", Database.Name);
			foreach (var maxcol in MaxableColumns)
				sql.Append(maxcol.SqlConversionString);
			return sql.ToString();
		}
	}
}
