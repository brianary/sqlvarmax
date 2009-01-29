using Microsoft.ManagementConsole;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// A node that provides a Refresh method.
	/// </summary>
	interface IRefreshableNode
	{
		/// <summary>
		/// Rescans and rebuilds any child nodes.
		/// </summary>
		/// <param name="status">Status for updating the console.</param>
		void Refresh(AsyncStatus status);
	}
}
