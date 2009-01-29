using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of maxable columns in the results pane.
	/// </summary>
	public class ColumnsListView : MmcListView //TODO: inherit new MaxableListView
	{
		#region Private Methods
		/// <summary>
		/// Concatenates the SQL conversion strings for the selected maxable columns.
		/// </summary>
		/// <returns>The SQL conversion script for the selected columns.</returns>
		private string GetSqlString()
		{
			if (SelectedNodes.Count == 0) return "";
			var db = ((MaxableColumn)SelectedNodes[0].Tag).DatabaseName;
			var sql = new StringBuilder(String.Format("use [{0}]\nGO\n",db));
			foreach (ResultNode resultnode in SelectedNodes)
			{
				MaxableColumn maxcol= (MaxableColumn)resultnode.Tag;
				if (maxcol.DatabaseName != db)
				{
					db = maxcol.DatabaseName;
					sql.AppendFormat("use [{0}]\nGO\n", db);
				}
				sql.Append(maxcol.SqlConversionString);
			}
			return sql.ToString();
		}

		/// <summary>
		/// Re-synchronizes result nodes to columns in scope node.
		/// </summary>
		private void Resync()
		{
			List<MaxableColumn> columns;
			if (ScopeNode.Tag is DatabaseScan)
			{
				DatabaseScan scan = (DatabaseScan)ScopeNode.Tag;
				columns = scan.MaxableColumns;
				DescriptionBarText = scan.Name + " columns";
			}
			else if (ScopeNode.Tag is ServerScan)
			{
				ServerScan scan = (ServerScan)ScopeNode.Tag;
				columns = scan.MaxableColumns;
				DescriptionBarText = scan.Name + " columns";
			}
			else
			{
				columns = new List<MaxableColumn>();
				DescriptionBarText = "No columns";
			}
			ResultNodes.Clear();
			foreach (var maxcol in columns)
			{
				var colnode = new ResultNode() { DisplayName = maxcol.ColumnName, Tag = maxcol };
				if (ScopeNode.Tag is ServerScan)
					colnode.SubItemDisplayNames.Add(maxcol.DatabaseName);
				colnode.SubItemDisplayNames.AddRange(new string[] {
					maxcol.SchemaName, maxcol.TableName, maxcol.CurrentDataTypeName, maxcol.MaxDataTypeName,
					maxcol.RowCount.ToString("N0"), maxcol.RowsUnder8000BytesCount.ToString("N0"),
					maxcol.MinDataLength.ToString("N0"), maxcol.AvgDataLength.ToString("N0"), maxcol.MaxDataLength.ToString("N0"),
				});
				ResultNodes.Add(colnode);
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
			Columns[0].Title = "Column";
			Columns[0].SetWidth(150);
			if (ScopeNode.Tag is ServerScan)
				Columns.Add(new MmcListViewColumn("Database", 120));
			Columns.AddRange(new MmcListViewColumn[] {
                new MmcListViewColumn("Schema", 70),
                new MmcListViewColumn("Table", 150),
                new MmcListViewColumn("Current Type", 100),
                new MmcListViewColumn("Max Type", 100),
				new MmcListViewColumn("Rows", 70, MmcListViewColumnFormat.Right),
				new MmcListViewColumn("< 8000", 70, MmcListViewColumnFormat.Right),
				new MmcListViewColumn("Min Length", 70, MmcListViewColumnFormat.Right),
				new MmcListViewColumn("Avg Length", 70, MmcListViewColumnFormat.Right),
				new MmcListViewColumn("Max Length", 70, MmcListViewColumnFormat.Right),
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
			if(ScopeNode is IRefreshableNode)
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
