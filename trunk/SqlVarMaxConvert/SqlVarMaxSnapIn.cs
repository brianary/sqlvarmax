using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;
using System.Drawing;
using System.Resources;
using System.Windows;
using System.Collections;

[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Unrestricted = true)]

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// The main entry point for the creation of the snap-in.
    /// </summary>
    [SnapInSettings("{A8EC8985-8E7B-41E6-BBBA-88BAE8D6BF64}",
         DisplayName = "SQL VarMax Convert",
         Description = "Scans SQL Server for the deprecated data types ntext, text, and image.")]
    public class SqlVarMaxSnapIn : SnapIn
    {
        #region Public Constructors
        /// <summary>
        /// The constructor.
        /// </summary>
        public SqlVarMaxSnapIn()
        {
            RootNode = new RootNode();
            var resx = new ResXResourceReader("SqlVarMaxIcons.resx");
            foreach (DictionaryEntry r in resx)
                SmallImages.Add((Icon)r.Value);
        }
        #endregion
    }
}
