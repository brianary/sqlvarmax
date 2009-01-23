using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ManagementConsole;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
	/// <summary>
	/// Displays a list of maxable columns in the results pane.
	/// </summary>
    public class ColumnsListView : MmcListView
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
			List<MaxableColumn> columns;
			if (ScopeNode.Tag is DatabaseScan)
				columns = (ScopeNode.Tag as DatabaseScan).MaxableColumns;
			else if (ScopeNode.Tag is ServerScan)
				columns = (ScopeNode.Tag as ServerScan).MaxableColumns;
			else
				columns = new List<MaxableColumn>();
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
			foreach (var maxcol in columns)
			{
				var colnode = new ResultNode() { DisplayName = maxcol.ColumnName, Tag = maxcol };
				if (ScopeNode.Tag is ServerScan)
					colnode.SubItemDisplayNames.Add(maxcol.DatabaseName);
				colnode.SubItemDisplayNames.AddRange(new string[] {
					maxcol.TableSchema, maxcol.TableName, maxcol.CurrentDataTypeName, maxcol.MaxDataTypeName,
					maxcol.RowCount.ToString("N0"), maxcol.RowsUnder8000BytesCount.ToString("N0"),
					maxcol.MinDataLength.ToString("N0"), maxcol.AvgDataLength.ToString("N0"), maxcol.MaxDataLength.ToString("N0"),
				});
				ResultNodes.Add(colnode);
			}
        }
		#endregion
    }
}
