using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Data;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	public class ServerScan
	{
		Server Server;
		public List<DatabaseScan> DatabaseScans = new List<DatabaseScan>();
		public List<MaxableColumn> MaxableColumns = new List<MaxableColumn>();
		public delegate void ScanProgressHandler(object sender, ScanProgressEventArgs e);
		public event ScanProgressHandler Scanning;

		public bool HasMaxables
		{
			get { return DatabaseScans.Count > 0; }
		}

		public ServerScan(string server)
		{
			Server = new Server(server);
		}

		public void PerformScan()
		{
			int db = 0, dbcount = Server.Databases.Count;
			string statusmessage = "Scanning " + Server.Name + ": ";
			foreach (Database database in Server.Databases)
			{
				var dbscan = new DatabaseScan(database);
				Scanning(this, new ScanProgressEventArgs(statusmessage + database.Name, db++, dbcount));
				dbscan.PerformScan();
				if (dbscan.HasMaxables)
				{
					DatabaseScans.Add(dbscan);
					MaxableColumns.AddRange(dbscan.MaxableColumns);
				}
			}
		}

		public string GetSqlConversionString()
		{
			var sql = new StringBuilder();
			sql.AppendFormat("-- SqlVarMax conversion script for {0}\\{1}\n", Server.Name, Server.InstanceName);
			return sql.ToString();
		}

		public static IEnumerable<string> EnumAvailableSqlServers()
		{
			var serverlist = new List<string>();
			using (var servers = SmoApplication.EnumAvailableSqlServers())
				foreach (DataRow found in servers.Rows)
					serverlist.Add((string)found["Name"]);
			return serverlist;
		}
	}
}
