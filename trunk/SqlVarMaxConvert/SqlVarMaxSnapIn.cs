﻿using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;

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
            this.RootNode = new RootNode();
        }
        #endregion
    }
}
