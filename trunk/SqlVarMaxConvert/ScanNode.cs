using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// A node of the snap-in representing a scannable container.
	/// </summary>
	public abstract class ScanNode : ScopeNode
	{
		#region Protected Fields
		/// <summary>
		/// Used to report the status of the long operation to the user.
		/// </summary>
		protected Status Status;
		#endregion

		#region Protected Methods
		/// <summary>
		/// Performs the server scan, and builds any child nodes.
		/// </summary>
		protected virtual void Refresh() { }
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
            ImageIndex = this is ServerNode ? 1 : 2;
			Status = status;
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
