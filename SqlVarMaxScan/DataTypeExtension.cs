using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	public static class DataTypeExtension
	{
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
	}
}
