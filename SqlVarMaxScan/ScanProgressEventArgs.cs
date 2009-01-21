using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	public class ScanProgressEventArgs : EventArgs
	{
		public string StatusMessage;
		public int WorkComplete;
		public int TotalWork;

		public ScanProgressEventArgs(string statusmessage, int workcomplete, int totalwork)
		{
			StatusMessage = statusmessage;
			WorkComplete = workcomplete;
			TotalWork = totalwork;
		}
	}
}
