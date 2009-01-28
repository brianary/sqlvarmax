using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// A parameter to a subroutine that uses a deprecated data type.
	/// </summary>
	public struct MaxableParameter
	{
		#region Private Fields
		/// <summary>
		/// Used to strip off the creation header of the subroutine.
		/// </summary>
		private static readonly Regex CreateHeader = new
			Regex(@"\A\s*CREATE\s+(?<object>proc(?:edure)|function)\s+(?<schema>\[[^\]]+\].|\w+.)?",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);
		#endregion

		#region Public Fields
		/// <summary>
		/// The name of the database.
		/// </summary>
		public readonly string DatabaseName;

		/// <summary>
		/// The schema the object belongs to.
		/// </summary>
		public readonly string SchemaName;

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
		/// The name of the subroutine parameter.
		/// </summary>
		public readonly string ParameterName;

		/// <summary>
		/// The current SQL datatype of the parameter.
		/// </summary>
		public readonly string CurrentDataTypeName;

		/// <summary>
		/// The corresponding var*(max) datatype to convert the parameter to.
		/// </summary>
		public readonly string MaxDataTypeName;

		/// <summary>
		/// The direction (input or output) of the parameter.
		/// </summary>
		public readonly string ParameterDirectionName;

		/// <summary>
		/// The SQL script for converting the parameter to the var*(max) data type.
		/// </summary>
		public readonly string SqlConversionString;
		#endregion

		#region Public Constructors
		/// <summary>
		/// The constructor, which notes the stored procedure of the parameter, and builds the SQL conversion script.
		/// </summary>
		/// <param name="parameter">The database column to convert.</param>
		public MaxableParameter(StoredProcedureParameter parameter)
		{
			StoredProcedure storedprocedure = parameter.Parent;
			Database database = storedprocedure.Parent;
			DatabaseName = database.Name;
			SchemaName = storedprocedure.Schema;
			SubroutineName = storedprocedure.Name;
			SubroutineType = storedprocedure.GetType();
			SubroutineSpecies = SubroutineType.Name;
			ParameterName = parameter.Name;
			DataType currentdatatype = parameter.DataType;
			CurrentDataTypeName = currentdatatype.ToSqlString();
			DataType maxdatatype = DataType.Int;
			switch (currentdatatype.SqlDataType)
			{
				case SqlDataType.Image: maxdatatype = new DataType(SqlDataType.VarBinaryMax); break;
				case SqlDataType.NText: maxdatatype = new DataType(SqlDataType.NVarCharMax); break;
				case SqlDataType.Text: maxdatatype = new DataType(SqlDataType.VarCharMax); break;
				default: maxdatatype = currentdatatype; break;
			}
			MaxDataTypeName = maxdatatype.ToSqlString();
			ParameterDirectionName = parameter.IsOutputParameter ? "output" : "input";
			if (storedprocedure.TextMode)
			{
				StringBuilder sql = new StringBuilder(String.Format("ALTER PROC [{0}].", SchemaName));
				sql.Append(Regex.Replace(CreateHeader.Replace(storedprocedure.TextHeader, ""), 
					Regex.Escape(ParameterName) + @"\s+" + Regex.Escape(CurrentDataTypeName) + @"\b", 
					ParameterName + " " + MaxDataTypeName, RegexOptions.IgnoreCase));
				sql.Append(storedprocedure.TextBody);
				SqlConversionString = sql.ToString();
			}
			else
			{
				SqlConversionString = "";
			}
		}

		/// <summary>
		/// The constructor, which notes the user defined function of the return value
		/// </summary>
		/// <param name="function">The user defined function with a return to convert.</param>
		public MaxableParameter(UserDefinedFunction udf)
		{
			Database database = udf.Parent;
			DatabaseName = database.Name;
			SchemaName = udf.Schema;
			SubroutineName = udf.Name;
			SubroutineType = udf.GetType();
			SubroutineSpecies = SubroutineType.Name;
			ParameterName = "(return value)";
			ParameterDirectionName = "output";
			if (udf.FunctionType != UserDefinedFunctionType.Scalar)
			{
				CurrentDataTypeName = "(not scalar)";
				MaxDataTypeName = "(not scalar)";
				SqlConversionString = "";
			}
			else
			{
				DataType currentdatatype = udf.DataType;
				CurrentDataTypeName = currentdatatype.Name;
				DataType maxdatatype = DataType.Int;
				switch (currentdatatype.SqlDataType)
				{
					case SqlDataType.Image: maxdatatype = new DataType(SqlDataType.VarBinaryMax); break;
					case SqlDataType.NText: maxdatatype = new DataType(SqlDataType.NVarCharMax); break;
					case SqlDataType.Text: maxdatatype = new DataType(SqlDataType.VarCharMax); break;
					default: maxdatatype = currentdatatype; break;
				}
				MaxDataTypeName = maxdatatype.ToSqlString();
				if (udf.TextMode)
				{
					StringBuilder sql = new StringBuilder(String.Format("ALTER FUNCTION [{0}].", SchemaName));
					sql.Append(Regex.Replace(CreateHeader.Replace(udf.TextHeader, ""), 
						@"\bRETURNS\s+" + Regex.Escape(MaxDataTypeName) + @"\b",
						"RETURNS " + MaxDataTypeName, RegexOptions.IgnoreCase));
					sql.Append(udf.TextBody);
					SqlConversionString = sql.ToString();
				}
				else
				{
					SqlConversionString = "";
				}
			}
		}

		/// <summary>
		/// The constructor, which notes the user defined function of the parameter, and builds the SQL conversion script.
		/// </summary>
		/// <param name="parameter">The parameter to convert.</param>
		public MaxableParameter(UserDefinedFunctionParameter parameter)
		{
			UserDefinedFunction udf = parameter.Parent;
			Database database = udf.Parent;
			DatabaseName = database.Name;
			SchemaName = udf.Schema;
			SubroutineName = udf.Name;
			SubroutineType = udf.GetType();
			SubroutineSpecies = SubroutineType.Name;
			ParameterName = parameter.Name;
			DataType currentdatatype = parameter.DataType;
			CurrentDataTypeName = currentdatatype.ToSqlString();
			DataType maxdatatype = DataType.Int;
			switch (currentdatatype.SqlDataType)
			{
				case SqlDataType.Image: maxdatatype = new DataType(SqlDataType.VarBinaryMax); break;
				case SqlDataType.NText: maxdatatype = new DataType(SqlDataType.NVarCharMax); break;
				case SqlDataType.Text: maxdatatype = new DataType(SqlDataType.VarCharMax); break;
				default: maxdatatype = currentdatatype; break;
			}
			MaxDataTypeName = maxdatatype.ToSqlString();
			ParameterDirectionName = "input";
			if (udf.TextMode)
			{
				StringBuilder sql = new StringBuilder(String.Format("ALTER FUNCTION [{0}].", SchemaName));
				sql.Append(Regex.Replace(CreateHeader.Replace(udf.TextHeader, ""),
					Regex.Escape(ParameterName) + @"\s+" + Regex.Escape(CurrentDataTypeName) + @"\b",
					ParameterName + " " + MaxDataTypeName, RegexOptions.IgnoreCase));
				sql.Append(udf.TextBody);
				SqlConversionString = sql.ToString();
			}
			else
			{
				SqlConversionString = "";
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Searches a collection of parameters, looking for those that can be converted from deprecated data types.
		/// </summary>
		/// <param name="parameters">The collection of parameters to search.</param>
		/// <returns>A list of maxable parameters.</returns>
		public static List<MaxableParameter> FindMaxableParameters(StoredProcedureParameterCollection parameters)
		{
			var maxables = new List<MaxableParameter>();
			foreach (StoredProcedureParameter parameter in parameters)
				switch (parameter.DataType.SqlDataType)
				{
					case SqlDataType.Image:
					case SqlDataType.NText:
					case SqlDataType.Text:
						maxables.Add(new MaxableParameter(parameter));
						break;
				}
			return maxables;
		}

		/// <summary>
		/// Searches a collection of parameters, looking for those that can be converted from deprecated data types.
		/// </summary>
		/// <param name="parameters">The collection of parameters to search.</param>
		/// <returns>A list of maxable parameters.</returns>
		public static List<MaxableParameter> FindMaxableParameters(UserDefinedFunctionParameterCollection parameters)
		{
			var maxables = new List<MaxableParameter>();
			foreach (UserDefinedFunctionParameter parameter in parameters)
				switch (parameter.DataType.SqlDataType)
				{
					case SqlDataType.Image:
					case SqlDataType.NText:
					case SqlDataType.Text:
						maxables.Add(new MaxableParameter(parameter));
						break;
				}
			return maxables;
		}

		/// <summary>
		/// Does the user defined function return a maxable type?
		/// </summary>
		/// <param name="udf">The user defined function to check.</param>
		/// <returns>True if the return type for the user defined function is image, ntext, or text, false otherwise.</returns>
		public static bool HasMaxableReturnType(UserDefinedFunction udf)
		{
			if (udf.FunctionType != UserDefinedFunctionType.Scalar)
				return false;
			switch (udf.DataType.SqlDataType)
			{
				case SqlDataType.Image:
				case SqlDataType.NText:
				case SqlDataType.Text:
					return true;
			}
			return false;
		}
		#endregion
	}
}
