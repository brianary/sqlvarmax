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
        ServerScan ServerScan { get; set; }
		SyncStatus Status;

        #region Public Constructors
        /// <summary>
        /// Constructs the ServerNode, given the server name.
        /// </summary>
        /// <param name="server">The server name.</param>
        public ServerNode(string servername, SyncStatus status)
        {
            DisplayName = servername;
			Status = status;
            ServerScan = new ServerScan(servername);
			ServerScan.Scanning += new ServerScan.ScanProgressHandler(Scanning);
			ServerScan.PerformScan();
            foreach (var databasescan in ServerScan.DatabaseScans)
				Children.Add(new DatabaseNode(databasescan));
        }
        #endregion

		public void Scanning(object sender, ScanProgressEventArgs e)
		{
			Status.ReportProgress(e.WorkComplete, e.TotalWork, e.StatusMessage);
		}
    }
}
