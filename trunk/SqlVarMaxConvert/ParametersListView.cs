using System.Collections.Generic;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of maxable columns in the results pane.
	/// </summary>
	public class ParametersListView : MmcListView
	{
		#region Event Handlers
		/// <summary>
        /// Defines the structure of the list view.
        /// </summary>
        /// <param name="status"></param>
		protected override void OnInitialize(AsyncStatus status)
		{
			base.OnInitialize(status);
			Mode = MmcListViewMode.Report;
			ResultNodes.Clear();
			List<MaxableParameter> parameters;
			if (ScopeNode.Tag is DatabaseScan)
				parameters = (ScopeNode.Tag as DatabaseScan).MaxableParameters;
			else if (ScopeNode.Tag is ServerScan)
				parameters = (ScopeNode.Tag as ServerScan).MaxableParameters;
			else
				parameters = new List<MaxableParameter>();
			Columns.Clear();
			Columns[0].Title = "Parameter";
			Columns[0].SetWidth(150);
			if (ScopeNode.Tag is ServerScan)
				Columns.Add(new MmcListViewColumn("Database", 120));
			Columns.AddRange(new MmcListViewColumn[] {
                new MmcListViewColumn("Schema", 70),
                new MmcListViewColumn("Subroutine", 150),
                new MmcListViewColumn("Species", 150),
                new MmcListViewColumn("Current Type", 100),
                new MmcListViewColumn("Max Type", 100),
				new MmcListViewColumn("Direction", 60),
            });
			foreach (var maxparam in parameters)
			{
				var paramnode = new ResultNode() { DisplayName = maxparam.ParameterName, Tag = maxparam };
				if (ScopeNode.Tag is ServerScan)
					paramnode.SubItemDisplayNames.Add(maxparam.DatabaseName);
				paramnode.SubItemDisplayNames.AddRange(new string[] {
					maxparam.SchemaName, maxparam.SubroutineName, maxparam.SubroutineSpecies, 
					maxparam.CurrentDataTypeName, maxparam.MaxDataTypeName, maxparam.ParameterDirectionName,
				});
				ResultNodes.Add(paramnode);
			}
		}
		#endregion
	}
}
