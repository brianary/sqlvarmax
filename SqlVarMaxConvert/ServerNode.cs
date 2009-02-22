using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// A server node of the snap-in.
    /// </summary>
    public class ServerNode : ScanNode
    {
        #region Private Methods
        /// <summary>
        /// Performs the server scan, and builds the child nodes.
        /// </summary>
        protected override void Refresh()
        {
            ServerScan serverscan = (ServerScan)Tag;
            Status.Title = "Scanning " + serverscan.Name; //TODO: fix (can't do this here)
            serverscan.PerformScan();
            Children.Clear();
            foreach (var databasescan in serverscan.DatabaseScans)
                Children.Add(new DatabaseNode(databasescan));
            Status.Complete("Scan complete.", true);
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Constructs the ServerNode, given the server name.
        /// </summary>
        /// <param name="server">The server name.</param>
        public ServerNode(string servername, SyncStatus status)
        {
            DisplayName = servername;
            ImageIndex = 2;
            Status = status;
            var serverscan = new ServerScan(servername);
            Tag = serverscan;
            serverscan.Scanning += new ServerScan.ScanProgressHandler(Scanning);
            ViewDescriptions.AddRange(new MmcListViewDescription[] { 
				new MmcListViewDescription()
                { DisplayName= "All Database Columns", ViewType= typeof(ColumnsListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
				new MmcListViewDescription()
                { DisplayName= "All Database Parameters", ViewType= typeof(ParametersListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
				new MmcListViewDescription()
                { DisplayName= "All Database Subroutines", ViewType= typeof(SubroutinesListView), 
                    Options= MmcListViewOptions.ExcludeScopeNodes },
			});
            ViewDescriptions.DefaultIndex = 0;
            EnabledStandardVerbs = StandardVerbs.Refresh;
            Refresh();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Rescans the server, and rebuilds the child nodes.
        /// </summary>
        /// <param name="status">Status for updating the console.</param>
        public void Refresh(AsyncStatus status)
        {
            OnRefresh(status);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Rescans the server, and rebuilds the child nodes.
        /// </summary>
        /// <param name="status">Status for updating the console.</param>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            Status = status;
            Status.Title = "Refresh " + ((ServerScan)Tag).Name;
            Refresh();
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
