using System;
using System.Data;
using System.Windows.Forms;
using Webcoder.SqlServer.SqlVarMaxScan;

namespace Webcoder.SqlServer.SqlVarMaxConvert
{
    /// <summary>
    /// The server connect dialog.
    /// </summary>
    public partial class ConnectServerDialog : Form
	{
		#region Private Fields
		/// <summary>
		/// Keeps track of whether the servers listed in the dropdown have been enumerated.
		/// </summary>
		private bool ServersEnumerated = false;
		#endregion

		#region Public Properties
		/// <summary>
        /// The server that was selected by the user, null if no selection was made.
        /// </summary>
        public string ServerSelected { get; set; }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Constructs the ConnectServerDialog, given the root node to attach the server node to.
        /// </summary>
        public ConnectServerDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// The list of potential servers to populate the dropdown with.
        /// </summary>
        /// <param name="sender">The ConnectServerDialog object.</param>
        /// <param name="e">Not used.</param>
        private void ConnectServerDialog_Load(object sender, EventArgs e)
        {
			Activate();
        }

        /// <summary>
        /// The OK button should only be enabled when the server selection is not blank.
        /// </summary>
        /// <param name="sender">The combobox object.</param>
        /// <param name="e">Not used.</param>
        private void ServerSelection_TextChanged(object sender, EventArgs e)
        {
            OkButton.Enabled = !String.IsNullOrEmpty(ServerSelection.Text);
        }

		/// <summary>
		/// Populates the dropdown list, the first time it is opened, with the list of potential servers.
		/// </summary>
		/// <param name="sender">The combobox object.</param>
		/// <param name="e">Not used.</param>
		private void ServerSelection_DropDown(object sender, EventArgs e)
		{
			if (!ServersEnumerated)
			{
				foreach(var server in ServerScan.EnumAvailableSqlServers())
					ServerSelection.Items.Add(server);
				ServersEnumerated = true;
				Activate();
			}
		}

        /// <summary>
        /// The OK button was pressed.
        /// </summary>
        /// <param name="sender">The OK button.</param>
        /// <param name="e">The event details.</param>
        private void OkButton_Click(object sender, EventArgs e)
        {
			ServerSelected = ServerSelection.Text.ToUpper();
            ServerSelection.Text = "";
            DialogResult = DialogResult.OK;
            Hide();
        }

        /// <summary>
        /// The Cancel button was pressed.
        /// </summary>
        /// <param name="sender">The Cancel button.</param>
        /// <param name="e">The event details.</param>
        private void TheCancelButton_Click(object sender, EventArgs e)
        {
            ServerSelected = null;
            ServerSelection.Text = "";
            DialogResult = DialogResult.Cancel;
            Hide();
        }
        #endregion
    }
}
