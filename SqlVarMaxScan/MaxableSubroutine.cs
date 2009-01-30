using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// A subroutine that uses a deprecated datatype internally.
	/// </summary>
	public class MaxableSubroutine : MaxableItem
	{
		#region Private Fields
		/// <summary>
		/// Used to find special, deprecated text functions.
		/// </summary>
		public static readonly Regex TextOperations = new
			Regex(@"\b(?<command>readtext|writetext|updatetext|textptr)\b",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Used to find local variable declarations that use deprecated datatypes.
		/// </summary>
		public static readonly Regex VariableDeclaration = new
			Regex(@"\b(?<declare>declare\s+@\w+\s+)(?<datatype>image|ntext|text)\b",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);
		#endregion

		#region Public Fields
		/// <summary>
		/// The name of the stored procedure or user-defined function.
		/// </summary>
		public readonly string SubroutineName;

		/// <summary>
		/// The object type of the subroutine.
		/// </summary>
		public readonly Type SubroutineType;

		/// <summary>
		/// What kind of subroutine is this?
		/// </summary>
		public readonly string SubroutineSpecies;

		/// <summary>
		/// How is this subroutine using a deprecated datatype internally?
		/// </summary>
		public readonly string SubroutineSymptom;
		#endregion

		#region Public Constructors
		/// <summary>
		/// The constructor, which notes any deprecated datatype internals of the stored procedure.
		/// </summary>
		/// <param name="storedprocedure">The stored procedure to examine.</param>
		public MaxableSubroutine(StoredProcedure storedprocedure)
		{
			Database = storedprocedure.Parent;
			databaseName = Database.Name;
			schemaName = storedprocedure.Schema;
			SubroutineName = storedprocedure.Name;
			SubroutineType = storedprocedure.GetType();
			SubroutineSpecies = SubroutineType.Name;
			if (!storedprocedure.TextMode)
			{
				currentDataTypeName = "";
				maxDataTypeName = "";
				SubroutineSymptom = "";
			}
			else if (TextOperations.IsMatch(storedprocedure.TextBody))
			{
				currentDataTypeName = "text";
				maxDataTypeName = "varchar(max)";
				SubroutineSymptom = TextOperations.Match(storedprocedure.TextBody).Value;
			}
			else if (VariableDeclaration.IsMatch(storedprocedure.TextBody))
			{
				var variable = VariableDeclaration.Match(storedprocedure.TextBody);
				currentDataTypeName = variable.Groups["datatype"].Value;
				switch (currentDataTypeName.ToLower())
				{
					case "image": maxDataTypeName = "varbinary(max)"; break;
					case "ntext": maxDataTypeName = "nvarchar(max)"; break;
					case "text": maxDataTypeName = "varchar(max)"; break;
				}
				SubroutineSymptom = variable.Value;
			}
			StringBuilder sql = new StringBuilder(String.Format("ALTER FUNCTION [{0}].", SchemaName));
			sql.Append(CreateHeader.Replace(storedprocedure.TextHeader, ""));
			sql.Append(storedprocedure.TextBody);
			sqlConversionString = sql.ToString();
		}

		/// <summary>
		/// The constructor, which notes any deprecated datatype internals of the user defined function.
		/// </summary>
		/// <param name="storedprocedure">The stored procedure to examine.</param>
		public MaxableSubroutine(UserDefinedFunction udf)
		{
			Database = udf.Parent;
			databaseName = Database.Name;
			schemaName = udf.Schema;
			SubroutineName = udf.Name;
			SubroutineType = udf.GetType();
			SubroutineSpecies = SubroutineType.Name;
			if (!udf.TextMode)
			{
				currentDataTypeName = "";
				maxDataTypeName = "";
				SubroutineSymptom = "";
			}
			else if (TextOperations.IsMatch(udf.TextBody))
			{
				currentDataTypeName = "text";
				maxDataTypeName = "varchar(max)";
				SubroutineSymptom = TextOperations.Match(udf.TextBody).Value;
			}
			else if (VariableDeclaration.IsMatch(udf.TextBody))
			{
				var variable = VariableDeclaration.Match(udf.TextBody);
				currentDataTypeName = variable.Groups["datatype"].Value;
				switch (currentDataTypeName.ToLower())
				{
					case "image": maxDataTypeName = "varbinary(max)"; break;
					case "ntext": maxDataTypeName = "nvarchar(max)"; break;
					case "text": maxDataTypeName = "varchar(max)"; break;
				}
				SubroutineSymptom = variable.Value;
			}
			StringBuilder sql = new StringBuilder(String.Format("ALTER FUNCTION [{0}].", SchemaName));
			sql.Append(CreateHeader.Replace(udf.TextHeader, ""));
			sql.Append(udf.TextBody);
			sqlConversionString = sql.ToString();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Does this stored procedure use text functions internally?
		/// </summary>
		/// <param name="storedprocedure">The stored procedure to check.</param>
		/// <returns>True if the stored procedure uses READTEXT, WRITETEXT, UPDATETEXT, or TEXTPTR; false otherwise.</returns>
		public static bool HasMaxableInternals(StoredProcedure storedprocedure)
		{
			if (!storedprocedure.TextMode) return false;
			if (TextOperations.IsMatch(storedprocedure.TextBody) || VariableDeclaration.IsMatch(storedprocedure.TextBody)) return true;
			else return false;
		}

		/// <summary>
		/// Does this user defined function use text functions internally?
		/// </summary>
		/// <param name="storedprocedure">The user defined function to check.</param>
		/// <returns>True if the user defined function uses READTEXT, WRITETEXT, UPDATETEXT, or TEXTPTR; false otherwise.</returns>
		public static bool HasMaxableInternals(UserDefinedFunction udf)
		{
			if (!udf.TextMode) return false;
			if (TextOperations.IsMatch(udf.TextBody) || VariableDeclaration.IsMatch(udf.TextBody)) return true;
			else return false;
		}
		#endregion
	}
}
