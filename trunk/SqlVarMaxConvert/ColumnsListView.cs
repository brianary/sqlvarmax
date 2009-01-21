using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ManagementConsole;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    public class ColumnsListView : MmcListView
    {
        public ColumnsListView()
        {
        }

        /// <summary>
        /// Defines the structure of the list view.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            base.OnInitialize(status);
            ResultNodes.Clear();
            Columns.Clear();
            Columns[0].Title = "Column";
            Columns[0].SetWidth(150);
            Columns.AddRange(new MmcListViewColumn[] {
                new MmcListViewColumn("Schema", 70),
                new MmcListViewColumn("Table", 150),
                new MmcListViewColumn("Current Type", 100),
                new MmcListViewColumn("Max Type", 100),
            }); //TODO: Data, Count, < 8000, MinLen, AveLen, MaxLen
            Mode = MmcListViewMode.Report;
            var databasescan = ((DatabaseNode)ScopeNode).DatabaseScan;
			foreach (var maxcol in databasescan.MaxableColumns)
			{
				var colnode = new ResultNode() { DisplayName = maxcol.ColumnName, Tag = maxcol };
				colnode.SubItemDisplayNames.AddRange(new string[] {
					maxcol.TableSchema, maxcol.TableName, maxcol.CurrentDataTypeName, maxcol.MaxDataTypeName
				});
				ResultNodes.Add(colnode);
			}
        }

    }
}
