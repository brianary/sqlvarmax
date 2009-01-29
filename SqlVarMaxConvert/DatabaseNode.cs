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
    public class DatabaseNode : ScopeNode, IRefreshableNode //TODO: scrap IRefreshableNode, inherit from new ScanNode
    {
		#region Private Fields
		/// <summary>
		/// Used to report the status of the long operation to the user.
		/// </summary>
		private Status Status;
		#endregion

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
			});
			ViewDescriptions.DefaultIndex = 0;
			EnabledStandardVerbs = StandardVerbs.Refresh;
		}
        #endregion

		#region Public Methods
		/// <summary>
		/// Rescans the database.
		/// </summary>
		/// <param name="status">Status for updating the console.</param>
		public void Refresh(AsyncStatus status)
		{
			OnRefresh(status);
		}
		#endregion

		#region Event Handlers
		/// <summary>
		/// Rescans the database.
		/// </summary>
		/// <param name="status">Status for updating the console.</param>
		protected override void OnRefresh(AsyncStatus status)
		{
			base.OnRefresh(status);
			Status = status;
			DatabaseScan scan = (DatabaseScan)Tag;
			Status.Title = "Refresh " + scan.Name;
			scan.Scanning += new DatabaseScan.ScanProgressHandler(Scanning);
			scan.PerformScan();
			Status.Complete("Scan complete.", true);
		}

		/// <summary>
		/// Called when the progress of the scan changes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The details of the event.</param>
		protected void Scanning(object sender, ScanProgressEventArgs e)
		{
			if (Status is SyncStatus && ((SyncStatus)Status).IsCancelSignaled)
				Status.Complete("The scan has been cancelled.", false);
			Status.ReportProgress(e.WorkComplete, e.TotalWork, e.StatusMessage);
		}
		#endregion
	}
}
