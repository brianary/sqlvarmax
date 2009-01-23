using System;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// Extension methods for the DataType object.
	/// </summary>
	public static class DataTypeExtension
	{
		#region Public Methods
		/// <summary>
		/// Converts the DataType object into the equivalent SQL syntax.
		/// </summary>
		/// <param name="datatype">The DataType object (extended).</param>
		/// <returns>A SQL datatype string.</returns>
		public static string ToSqlString(this DataType datatype)
		{
			var sqltype = datatype.Name;
			switch (datatype.SqlDataType)
			{
				case SqlDataType.Image:
				case SqlDataType.NText:
				case SqlDataType.Text:
					break;
				case SqlDataType.NVarCharMax:
				case SqlDataType.VarBinaryMax:
				case SqlDataType.VarCharMax:
					sqltype += "(max)";
					break;
				default:
					if (!String.IsNullOrEmpty(datatype.Schema))
						sqltype = datatype.Schema + "." + sqltype;
					else if(datatype.MaximumLength > 0)
						sqltype += "(" + datatype.MaximumLength + ")";
					else if(datatype.NumericPrecision > 0 || datatype.NumericScale > 0)
						sqltype += "(" + datatype.NumericPrecision + "," + datatype.NumericScale + ")";
					break;
			}
			return sqltype;
		}
		#endregion
	}
}
