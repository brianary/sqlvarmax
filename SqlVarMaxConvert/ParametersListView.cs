using System.Collections.Generic;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;
using System.Text;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of maxable columns in the results pane.
	/// </summary>
	public class ParametersListView : MmcListView //TODO: inherit new MaxableListView
	{
		#region Private Methods
		/// <summary>
		/// Concatenates the SQL conversion strings for the selected maxable parameters.
		/// </summary>
		/// <returns>The SQL conversion script for the selected parameters.</returns>
		private string GetSqlString()
		{
			if (SelectedNodes.Count == 0) return "";
			var db = ((MaxableParameter)SelectedNodes[0].Tag).DatabaseName;
			var sql = new StringBuilder(String.Format("use [{0}]\nGO\n", db));
			foreach (ResultNode resultnode in SelectedNodes)
			{
				MaxableParameter maxparam = (MaxableParameter)resultnode.Tag;
				if (maxparam.DatabaseName != db)
				{
					db = maxparam.DatabaseName;
					sql.AppendFormat("use [{0}]\nGO\n", db);
				}
				sql.Append(maxparam.SqlConversionString);
			}
			return sql.ToString();
		}

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
        /// <param name="status"></param>
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

		/// <summary>
		/// Refreshes the list of columns.
		/// </summary>
		/// <param name="status">Status for updating the console.</param>
		protected override void OnRefresh(AsyncStatus status)
		{
			base.OnRefresh(status);
			if (ScopeNode is IRefreshableNode)
				((IRefreshableNode)ScopeNode).Refresh(status);
			Resync();
		}

		/// <summary>
		/// Update context menu when selected result nodes change.
		/// </summary>
		/// <param name="status">Status for updating the console.</param>
		protected override void OnSelectionChanged(SyncStatus status)
		{
			if (SelectedNodes.Count == 0)
			{
				SelectionData.Clear();
				return;
			}
			SelectionData.Update(GetSqlString(), SelectedNodes.Count > 1, null, null);
			SelectionData.EnabledStandardVerbs = StandardVerbs.Refresh;
			SelectionData.ActionsPaneItems.Clear();
			SelectionData.ActionsPaneItems.Add(new Action("Show SQL conversion script...",
				"Preview the SQL var*(max) datatype conversion script.", -1, "ShowSql"));
			SelectionData.ActionsPaneItems.Add(new Action("Save SQL conversion script...",
				"Export the SQL var*(max) datatype conversion script to a file.", -1, "SaveSql"));
			SelectionData.ActionsPaneItems.Add(new Action("Execute SQL conversion script...",
				"Convert the SQL var*(max) datatypes by running the script.", -1, "RunSql"));
		}

		/// <summary>
		/// React to selected result node(s) context menu items.
		/// </summary>
		/// <param name="action">The action that triggered the event.</param>
		/// <param name="status">Asynchronous status used to update the console.</param>
		protected override void OnSelectionAction(Action action, AsyncStatus status)
		{
			switch ((string)action.Tag)
			{
				case "ShowSql":
					string tempsql = Path.GetTempFileName();
					using (var writer = new StreamWriter(tempsql, false, Encoding.UTF8))
					{
						writer.Write(GetSqlString().Replace("\n", "\r\n"));
						writer.Close();
					}
					var editsql = new Process();
					editsql.StartInfo.FileName = "notepad.exe";
					editsql.StartInfo.Arguments = tempsql;
					editsql.Start();
					break;
				case "SaveSql":
					var saveas = new SaveFileDialog()
					{
						Title = "Save SQL Script",
						DefaultExt = ".sql",
						Filter = "SQL Scripts|*.sql|All Files|*.*",
						InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
					};
					if (saveas.ShowDialog() == DialogResult.Cancel) return;
					using (var writer = new StreamWriter(saveas.FileName, false, Encoding.UTF8))
					{
						writer.Write(GetSqlString().Replace("\n", "\r\n"));
						writer.Close();
					}
					break;
				case "RunSql":
					if (MessageBox.Show(
						"This could be a very dangerous operation.\n" +
						"** Back up your data before doing this! **\n" +
						"Are you sure you want to attempt this conversion?",
						"Confirm Conversion", MessageBoxButtons.YesNo,
						MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
						return;
					foreach (ResultNode resultnode in SelectedNodes)
						((MaxableColumn)resultnode.Tag).ExecuteConversion();
					OnRefresh(status);
					break;
			}
		}
		#endregion
	}
}
