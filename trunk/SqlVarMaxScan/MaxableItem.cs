using Microsoft.SqlServer.Management.Smo;
using System.Text.RegularExpressions;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// An object in the database that uses a deprecated data type.
	/// </summary>
	public abstract class MaxableItem
	{
		#region Protected Fields
		/// <summary>
		/// Used to strip off the creation header of the subroutine.
		/// </summary>
		protected static readonly Regex CreateHeader = new
			Regex(@"\A\s*CREATE\s+(?<object>proc(?:edure)|function)\s+(?<schema>\[[^\]]+\].|\w+.)?",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// The database this parameter is contained within.
		/// </summary>
		protected Database Database;

		/// <summary>
		/// The name of the database.
		/// </summary>
		protected string databaseName;

		/// <summary>
		/// The schema the object belongs to.
		/// </summary>
		protected string schemaName;

		/// <summary>
		/// The current SQL datatype of the item.
		/// </summary>
		protected string currentDataTypeName;

		/// <summary>
		/// The corresponding var*(max) datatype to convert the item to.
		/// </summary>
		protected string maxDataTypeName;

		/// <summary>
		/// The SQL script for converting the item to the var*(max) data type.
		/// </summary>
		protected string sqlConversionString;
		#endregion

		#region Public Properties
		/// <summary>
		/// The name of the database.
		/// </summary>
		public string DatabaseName { get { return databaseName; } }

		/// <summary>
		/// The schema the object belongs to.
		/// </summary>
		public string SchemaName { get { return schemaName; } }

		/// <summary>
		/// The current SQL datatype of the item.
		/// </summary>
		public string CurrentDataTypeName { get { return currentDataTypeName; } }

		/// <summary>
		/// The corresponding var*(max) datatype to convert the item to.
		/// </summary>
		public string MaxDataTypeName { get { return maxDataTypeName; } }

		/// <summary>
		/// The SQL script for converting the item to the var*(max) data type.
		/// </summary>
		public string SqlConversionString { get { return sqlConversionString; } }
		#endregion

		#region Public Methods
		/// <summary>
		/// Convert the item.
		/// </summary>
		public void ExecuteConversion()
		{
			Database.ExecuteNonQuery(SqlConversionString);
		}
		#endregion
	}
}
