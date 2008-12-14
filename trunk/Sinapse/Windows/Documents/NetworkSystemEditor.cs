using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Sinapse.Windows.SystemEditor;

using Sinapse.Core;
using Sinapse.Core.Systems;

namespace Sinapse.Windows.Documents
{
    public partial class NetworkSystemEditor : DockContent, IWorkplaceDocument
    {
        private WorkplaceItem item;
        private ActivationNetworkSystem system;

        private InterfaceSpecification wndInterface;
        private NetworkSystemDesign wndNetworkDesign;



        public NetworkSystemEditor(ActivationNetworkSystem system)
        {
            this.system = system;

            InitializeComponent();
        }

        public ActivationNetworkSystem ActivationSystem
        {
            get { return system; }
            set { system = value; }
        }


        private void AdaptativeSystemEditor_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.wndInterface = new InterfaceSpecification();
            this.wndInterface.Inputs = this.system.Inputs;
            this.wndInterface.Outputs = this.system.Outputs;
            this.wndInterface.Show(this.dockPanel, DockState.Document);

            this.wndNetworkDesign = new NetworkSystemDesign();
            this.wndNetworkDesign.NetworkSystem = system;
            this.wndNetworkDesign.Show(this.dockPanel, DockState.Document);

            this.ResumeLayout(true);
        }








        #region IWorkplaceDocument Members

        public void Save()
        {
            if (system.FullPath != String.Empty)
                system.Save();
            else SaveAs();
        }

        public void SaveAs()
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                system.Save(saveFileDialog.FileName);
            }
        }

        public ToolStripMenuItem[] MenuItems
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public ToolStrip[] ToolStrips
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public WorkplaceItem Item
        {
            get { return item; }
            set { item = value; }
        }

        public bool HasChanges
        {
            get { return system.HasChanges; }
        }

        #endregion

    }
}
