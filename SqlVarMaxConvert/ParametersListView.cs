﻿using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;
using System.Collections.Generic;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of maxable columns in the results pane.
	/// </summary>
	public class ParametersListView : MaxableListView
	{
		#region Protected Methods
		/// <summary>
		/// Re-synchronizes result nodes to parameters in scope node.
		/// </summary>
		private void Resync()
		{
			List<MaxableParameter> parameters;
			if (ScopeNode.Tag is DatabaseScan)
			{
				DatabaseScan scan = (DatabaseScan)ScopeNode.Tag;
				parameters = scan.MaxableParameters;
				DescriptionBarText = scan.Name + " parameters";
			}
			else if (ScopeNode.Tag is ServerScan)
			{
				ServerScan scan = (ServerScan)ScopeNode.Tag;
				parameters = scan.MaxableParameters;
				DescriptionBarText = scan.Name + " parameters";
			}
			else
			{
				parameters = new List<MaxableParameter>();
				DescriptionBarText = "No parameters";
			}
			ResultNodes.Clear();
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

		#region Event Handlers
		/// <summary>
        /// Defines the structure of the list view.
        /// </summary>
        /// <param name="status">Status for updating the console.</param>
		protected override void OnInitialize(AsyncStatus status)
		{
			base.OnInitialize(status);
			Mode = MmcListViewMode.Report;
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
			Resync();
		}
		#endregion
	}
}
