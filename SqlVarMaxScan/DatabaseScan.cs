using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// Scans a database for use of deprecated data types.
	/// </summary>
	public class DatabaseScan
	{
		#region Private Fields
		/// <summary>
		/// The database to scan.
		/// </summary>
		Database Database;
		#endregion

		#region Public Fields
		/// <summary>
		/// The list of columns that use a deprecated data type.
		/// </summary>
		public List<MaxableColumn> MaxableColumns = new List<MaxableColumn>();
		#endregion

		#region Public Properties
		/// <summary>
		/// The name of the database to scan.
		/// </summary>
		public string Name
		{
			get { return Database.Name; }
		}

		/// <summary>
		/// Does this database contain maxable columns?
		/// </summary>
		public bool HasMaxables
		{
			get { return MaxableColumns.Count > 0; }
		}
		#endregion

		#region Public Constructors
		/// <summary>
		/// Creates a new DatabaseScan object, given a database.
		/// </summary>
		/// <param name="database">The database to scan.</param>
		public DatabaseScan(Database database)
		{
			Database = database;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Performs the database scan, populating the maxable collections.
		/// </summary>
		public void PerformScan()
		{
			foreach (Table table in Database.Tables)
				MaxableColumns.AddRange(MaxableColumn.FindMaxableColumns(table.Columns));
		}

		/// <summary>
		/// Concatenates the SQL conversion strings from all of the maxable objects found in the database.
		/// </summary>
		/// <returns>A SQL conversion string.</returns>
		public string GetSqlConversionString()
		{
			var sql = new StringBuilder();
			sql.AppendFormat("use [{0}]\nGO\n-- Columns\n", Database.Name);
			foreach (var maxcol in MaxableColumns)
				sql.Append(maxcol.SqlConversionString);
			return sql.ToString();
		}
		#endregion
	}
}
