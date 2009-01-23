using System;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// Event details for a change in the scanning progress.
	/// </summary>
	public class ScanProgressEventArgs : EventArgs
	{
		#region Public Fields
		/// <summary>
		/// The status message to display; information about the current scanning activity.
		/// </summary>
		public string StatusMessage;

		/// <summary>
		/// The amount of items that have been completed in the scanning operation.
		/// </summary>
		public int WorkComplete;

		/// <summary>
		/// The total number of items to complete in the scanning operation.
		/// </summary>
		public int TotalWork;
		#endregion

		#region Public Constructors
		/// <summary>
		/// The constructor.
		/// </summary>
		/// <param name="statusmessage">The status message to display; information about the current scanning activity.</param>
		/// <param name="workcomplete">The amount of items that have been completed in the scanning operation.</param>
		/// <param name="totalwork">The total number of items to complete in the scanning operation.</param>
		public ScanProgressEventArgs(string statusmessage, int workcomplete, int totalwork)
		{
			StatusMessage = statusmessage;
			WorkComplete = workcomplete;
			TotalWork = totalwork;
		}
		#endregion
	}
}
