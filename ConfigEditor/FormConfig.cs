using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigEditor
{
    public partial class FormConfig : Form
    {
        public FormConfig()
        {
            InitializeComponent();
        }
        
        private void FormConfig_Load(object sender, EventArgs e)
        {
            this.listBoxAssembly.DataSource = Properties.Settings.Default.AssemblyPath;
            this.listBoxDependPath.DataSource = Properties.Settings.Default.DependPath;
        }

        private void buttonAddAssembly_Click(object sender, EventArgs e)
        {            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "程序集|*.dll;*.exe|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.AssemblyPath.Add(openFileDialog.FileName);
                this.listBoxAssembly.DataSource = null;
                this.listBoxAssembly.DataSource = Properties.Settings.Default.AssemblyPath;
                Properties.Settings.Default.Save();
            }
        }

        private void buttonDelAssebly_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AssemblyPath.RemoveAt(listBoxAssembly.SelectedIndex);
            this.listBoxAssembly.DataSource = null;            
            this.listBoxAssembly.DataSource = Properties.Settings.Default.AssemblyPath;
        }

        private void buttonAddPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = false;
;
            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.DependPath.Add(folderDlg.SelectedPath);
                this.listBoxDependPath.DataSource = null;
                this.listBoxDependPath.DataSource = Properties.Settings.Default.DependPath;
                Properties.Settings.Default.Save();
            }
        }

        private void buttonDelPath_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DependPath.RemoveAt(listBoxDependPath.SelectedIndex);
            this.listBoxDependPath.DataSource = null;
            this.listBoxDependPath.DataSource = Properties.Settings.Default.DependPath;
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {            
            buttonDelAssebly.Enabled = (listBoxAssembly.SelectedIndex != -1);
            buttonDelPath.Enabled = (listBoxDependPath.SelectedIndex != -1);
        }
    }
}
