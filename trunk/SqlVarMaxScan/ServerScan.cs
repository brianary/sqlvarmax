using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Data;

namespace Webcoder.SqlServer.SqlVarMaxScan
{
	/// <summary>
	/// Scans a server for use of deprecated data types.
	/// </summary>
	public class ServerScan
	{
		#region Private Fields
		/// <summary>
		/// The server to scan.
		/// </summary>
		Server Server;
		#endregion

		#region Public Fields
		/// <summary>
		/// The list of databases on the server that contain maxable objects.
		/// </summary>
		public List<DatabaseScan> DatabaseScans = new List<DatabaseScan>();

		/// <summary>
		/// The list of columns on the server, for all databases, that use deprecated data types.
		/// </summary>
		public List<MaxableColumn> MaxableColumns = new List<MaxableColumn>();

		/// <summary>
		/// The list of parameters on the server, for all databases, that use deprecated data types.
		/// </summary>
		public List<MaxableParameter> MaxableParameters = new List<MaxableParameter>();
		#endregion

		#region Public Properties
		/// <summary>
		/// Does this server contain objects that use deprecated data types?
		/// </summary>
		public bool HasMaxables
		{
			get { return DatabaseScans.Count > 0; }
		}
		#endregion

		#region Public Constructors
		/// <summary>
		/// Creates a ServerScan object, given a server address.
		/// </summary>
		/// <param name="server">The server name or name\instance.</param>
		public ServerScan(string server)
		{
			Server = new Server(server);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Scans the server for deprecated data types.
		/// </summary>
		public void PerformScan()
		{
			int done = 0, total = Server.Databases.Count;
			string statusmessage = "Scanning ";
			foreach (Database database in Server.Databases) if(!database.IsSystemObject)
			{
				var dbscan = new DatabaseScan(database);
				Scanning(this, new ScanProgressEventArgs(statusmessage + database.Name, done++, total));
				dbscan.Scanning+= new DatabaseScan.ScanProgressHandler(DatabaseScanning);
				dbscan.PerformScan();
				if (dbscan.HasMaxables)
				{
					DatabaseScans.Add(dbscan);
					MaxableColumns.AddRange(dbscan.MaxableColumns);
					MaxableParameters.AddRange(dbscan.MaxableParameters);
				}
			}
		}

		/// <summary>
		/// Concatenates the SQL conversion strings from all of the scanned databases.
		/// </summary>
		/// <returns></returns>
		public string GetSqlConversionString()
		{
			var sql = new StringBuilder();
			sql.AppendFormat("-- SqlVarMax conversion script for {0}\\{1}\n", Server.Name, Server.InstanceName);
			foreach (var database in DatabaseScans)
				sql.Append(database.GetSqlConversionString());
			return sql.ToString();
		}

		/// <summary>
		/// Returns a list of SQL Servers on the network.
		/// </summary>
		/// <returns>A collection of names of SQL Servers on the network.</returns>
		public static IEnumerable<string> EnumAvailableSqlServers()
		{
			var serverlist = new List<string>();
			using (var servers = SmoApplication.EnumAvailableSqlServers())
				foreach (DataRow found in servers.Rows)
					serverlist.Add((string)found["Name"]);
			return serverlist;
		}
		#endregion

		#region Event Handlers
		/// <summary>
		/// Called when the progress of the scan changes.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The details of the event.</param>
		public void DatabaseScanning(object sender, ScanProgressEventArgs e)
		{
			Scanning(sender, e);
		}
		#endregion

		#region Public Delegates
		/// <summary>
		/// A handler for a scanning progress update event.
		/// </summary>
		/// <param name="sender">The object that caused the event.</param>
		/// <param name="e">The event details.</param>
		public delegate void ScanProgressHandler(object sender, ScanProgressEventArgs e);
		#endregion

		#region Events
		/// <summary>
		/// Fired when the scanning progress is updated.
		/// </summary>
		public event ScanProgressHandler Scanning;
		#endregion
	}
}
