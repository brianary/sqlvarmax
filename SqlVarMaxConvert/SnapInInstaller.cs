using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Unrestricted = true)]

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	#region Public Constructors
	/// <summary>
    /// The RunInstaller attribute allows the .Net framework to install the assembly.
    /// </summary>
    [RunInstaller(true)]
    public class InstallUtilSupport : SnapInInstaller
    {
    }
	#endregion
}
