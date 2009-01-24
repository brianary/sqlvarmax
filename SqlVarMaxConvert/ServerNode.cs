using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// A server node of the snap-in.
    /// </summary>
    public class ServerNode : ScopeNode
	{
		#region Private Fields
		/// <summary>
		/// Used to report the status of the long operation to the user.
		/// </summary>
		SyncStatus Status;
		#endregion

        #region Public Constructors
        /// <summary>
        /// Constructs the ServerNode, given the server name.
        /// </summary>
        /// <param name="server">The server name.</param>
        public ServerNode(string servername, SyncStatus status)
        {
            DisplayName = servername;
			Status = status;
            var serverscan = new ServerScan(servername);
			Tag = serverscan;
			serverscan.Scanning += new ServerScan.ScanProgressHandler(Scanning);
			serverscan.PerformScan();
			Status.Complete("Scan complete.", true);
            foreach (var databasescan in serverscan.DatabaseScans)
				Children.Add(new DatabaseNode(databasescan));
			ViewDescriptions.AddRange(new MmcListViewDescription[] { 
				new MmcListViewDescription()
                { DisplayName= "All Database Columns", ViewType= typeof(ColumnsListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
				new MmcListViewDescription()
                { DisplayName= "All Database Parameters", ViewType= typeof(ParametersListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
			});
			ViewDescriptions.DefaultIndex = 0;
		}
        #endregion

		#region Event Handlers
		/// <summary>
		/// Called when the progress of the scan changes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The details of the event.</param>
		public void Scanning(object sender, ScanProgressEventArgs e)
		{
			if (Status.IsCancelSignaled)
				Status.Complete("The scan has been cancelled.", false);
			Status.ReportProgress(e.WorkComplete, e.TotalWork, e.StatusMessage);
		}
		#endregion
    }
}
