using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigableData;

namespace ConfigEditor
{
    public partial class FormSelectType : Form
    {
        public Type SelectedType
        {
            get
            {
                return selectType_;
            }
        }

        private Type selectType_;

        public FormSelectType()
        {
            InitializeComponent();
        }

        private void FormSelectType_Load(object sender, EventArgs e)
        {
            dataGridViewType.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewType.MultiSelect = false;

            this.dataGridViewType.Columns.Add("type", "类型");
            this.dataGridViewType.Columns.Add("info", "");
            //this.dataGridViewType.Rows.Add()
            foreach(LoadedAssembly a in MainForm.Assemblys)
            {
                foreach(Type t in a.LoadedTypes)
                {
                    ConfigDataAttribute attri = t.GetCustomAttribute<ConfigDataAttribute>(false);
                    dataGridViewType.Rows.Add(new Object[] { t, attri.Comment });
                }
            }
        }

        private void dataGridViewType_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridViewType.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewType.SelectedRows[0];
                selectType_ = (Type)row.Cells[0].Value;
            }
            else
            {
                selectType_ = null;
            }
        }
    }
}
