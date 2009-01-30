using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of subroutines that use deprecated datatypes internally, in the results pane.
	/// </summary>
	public class SubroutinesListView : MaxableListView
	{
		#region Protected Methods
		/// <summary>
		/// Re-synchronizes result nodes to parameters in scope node.
		/// </summary>
		private void Resync()
		{
			List<MaxableSubroutine> subroutines;
			if (ScopeNode.Tag is DatabaseScan)
			{
				DatabaseScan scan = (DatabaseScan)ScopeNode.Tag;
				subroutines = scan.MaxableSubroutines;
				DescriptionBarText = scan.Name + " stored procedures or user-defined functions";
			}
			else if (ScopeNode.Tag is ServerScan)
			{
				ServerScan scan = (ServerScan)ScopeNode.Tag;
				subroutines = scan.MaxableSubroutines;
				DescriptionBarText = scan.Name + " stored procedures or user-defined functions";
			}
			else
			{
				subroutines = new List<MaxableSubroutine>();
				DescriptionBarText = "No stored procedures or user-defined functions";
			}
			ResultNodes.Clear();
			foreach (var subroutine in subroutines)
			{
				var paramnode = new ResultNode() { DisplayName = subroutine.SubroutineName, Tag = subroutine };
				if (ScopeNode.Tag is ServerScan)
					paramnode.SubItemDisplayNames.Add(subroutine.DatabaseName);
				paramnode.SubItemDisplayNames.AddRange(new string[] {
					subroutine.SchemaName, subroutine.SubroutineSpecies, 
					subroutine.CurrentDataTypeName, subroutine.MaxDataTypeName, subroutine.SubroutineSymptom,
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
			Columns[0].Title = "Subroutine";
			Columns[0].SetWidth(150);
			if (ScopeNode.Tag is ServerScan)
				Columns.Add(new MmcListViewColumn("Database", 120));
			Columns.AddRange(new MmcListViewColumn[] {
                new MmcListViewColumn("Schema", 70),
                new MmcListViewColumn("Species", 150),
                new MmcListViewColumn("Current Type", 100),
                new MmcListViewColumn("Max Type", 100),
				new MmcListViewColumn("Symptom", 200),
            });
			Resync();
		}
		#endregion
	}
}
