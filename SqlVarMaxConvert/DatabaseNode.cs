using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// A database node of the snap-in.
    /// </summary>
    public class DatabaseNode : ScopeNode
    {
        #region Public Properties
        /// <summary>
        /// The database represented by this node.
        /// </summary>
        public DatabaseScan DatabaseScan { get; set; }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Constructs a database node, given the database.
        /// </summary>
        /// <param name="db">The database this node represents.</param>
        public DatabaseNode(DatabaseScan databasescan)
        {
            DatabaseScan = databasescan;
            DisplayName = databasescan.Name;
            ViewDescriptions.Add(new MmcListViewDescription()
                { DisplayName= "Database Columns", ViewType= typeof(ColumnsListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes });
        }
        #endregion
    }
}
