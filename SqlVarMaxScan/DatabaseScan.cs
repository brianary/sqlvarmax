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

		/// <summary>
		/// A list of stored procedures that contain maxable types.
		/// </summary>
		List<StoredProcedure> StoredProcedures = new List<StoredProcedure>();

		/// <summary>
		/// A list of user defined functions that contain maxable types.
		/// </summary>
		List<UserDefinedFunction> UserDefinedFunctions = new List<UserDefinedFunction>();
		#endregion

		#region Public Fields
		/// <summary>
		/// The list of columns that use a deprecated data type.
		/// </summary>
		public List<MaxableColumn> MaxableColumns = new List<MaxableColumn>();

		/// <summary>
		/// The list of parameters that use a deprecated data type.
		/// </summary>
		public List<MaxableParameter> MaxableParameters = new List<MaxableParameter>();
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
			get { return MaxableColumns.Count > 0 || MaxableParameters.Count > 0; }
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
			int done = 0, total = Database.Tables.Count + Database.StoredProcedures.Count 
				+ Database.UserDefinedFunctions.Count;
			string statusmessage = "Scanning " + Database.Name + ": ";
			foreach (Table table in Database.Tables) if(!table.IsSystemObject)
			{
				Scanning(this, new ScanProgressEventArgs(statusmessage + table.Name, done++, total));
				MaxableColumns.AddRange(MaxableColumn.FindMaxableColumns(table.Columns));
			}
			foreach (StoredProcedure storedprocedure in Database.StoredProcedures) if(!storedprocedure.IsSystemObject)
			{
				Scanning(this, new ScanProgressEventArgs(statusmessage + storedprocedure.Name, done++, total));
				var maxparams = MaxableParameter.FindMaxableParameters(storedprocedure.Parameters);
				if (maxparams.Count > 0)
				{
					StoredProcedures.Add(storedprocedure);
					MaxableParameters.AddRange(maxparams);
				}
			}
			foreach (UserDefinedFunction udf in Database.UserDefinedFunctions)
			{
				Scanning(this, new ScanProgressEventArgs(statusmessage + udf.Name, done++, total));
				var maxparams = MaxableParameter.FindMaxableParameters(udf.Parameters);
				if (maxparams.Count > 0)
				{
					UserDefinedFunctions.Add(udf);
					MaxableParameters.AddRange(maxparams);
				}
			}
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
			foreach (var maxparam in MaxableParameters)
				sql.Append(maxparam.SqlConversionString);
			return sql.ToString();
		}
		#endregion

		#region Public Delegates
		/// <summary>
		/// A handler for a scanning progress update event.
		/// </summary>
		/// <param name="sender">The object that caused the event.</param>
		/// <param name="e">The event details.</param>
		public delegate void ScanProgressHandler(object sender, ScanProgressEventArgs e);
		#endregion

		#region Events
		/// <summary>
		/// Fired when the scanning progress is updated.
		/// </summary>
		public event ScanProgressHandler Scanning;
		#endregion
	}
}
