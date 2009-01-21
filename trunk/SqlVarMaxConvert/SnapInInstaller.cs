using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Unrestricted = true)]

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// The RunInstaller attribute allows the .Net framework to install the assembly.
    /// </summary>
    [RunInstaller(true)]
    public class InstallUtilSupport : SnapInInstaller
    {
    }
}
