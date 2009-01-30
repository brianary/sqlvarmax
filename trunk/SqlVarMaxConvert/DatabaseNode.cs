using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// A database node of the snap-in.
    /// </summary>
    public class DatabaseNode : ScanNode
    {
        #region Public Constructors
        /// <summary>
        /// Constructs a database node, given the database.
        /// </summary>
        /// <param name="db">The database this node represents.</param>
        public DatabaseNode(DatabaseScan databasescan)
        {
            Tag = databasescan;
            DisplayName = databasescan.Name;
            ViewDescriptions.AddRange(new MmcListViewDescription[] { 
				new MmcListViewDescription()
                { DisplayName= "Database Columns", ViewType= typeof(ColumnsListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
				new MmcListViewDescription()
                { DisplayName= "Database Parameters", ViewType= typeof(ParametersListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
				new MmcListViewDescription()
                { DisplayName= "Database Subroutines", ViewType= typeof(SubroutinesListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
			});
			ViewDescriptions.DefaultIndex = 0;
			EnabledStandardVerbs = StandardVerbs.Refresh;
		}
        #endregion
	}
}
