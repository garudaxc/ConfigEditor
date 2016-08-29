using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataConfig;
using System.Reflection;
using System.Collections;

namespace ConfigEditor
{

    public partial class MainForm : Form
    {
        TypeReflactor reflactor_;

        public MainForm()
        {
            InitializeComponent();
        }

        private void RefreshTreeView()
        {
            treeView.Nodes.Clear();
            FieldNode fn = reflactor_.Root;
            TreeNode root = treeView.Nodes.Add(fn.Name);

            AddTreeNode(root, fn);
            root.ExpandAll();
        }

        static void AddTreeNode(TreeNode tn, FieldNode fd)
        {
            tn.Tag = fd;
            if(fd.Type.IsArray) {
                return;
            }

            fd.Children.ForEach(node => {
                TreeNode newTn = tn.Nodes.Add(node.Name);
                AddTreeNode(newTn, node);
            });
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Log.Pipe = this.console;

            //Type type = typeof(Gift);

            reflactor_ = new TypeReflactor(typeof(Gift));

            RefreshTreeView();

            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill ;

        }


        private void FreshDataGrid(FieldNode field)
        {
            if(field.Type.IsArray)
            {
                Type elemType = field.Type.GetElementType();

                FieldInfo[] fields = elemType.GetFields(BindingFlags.NonPublic |
                                                   BindingFlags.Public |
                                                   BindingFlags.Instance);

                for (int i = 0; i < fields.Length; i++)
                {
                    dataGrid.Columns.Add(fields[i].Name, fields[i].Name);
                }

                dataGrid.RowHeadersVisible = true;

                for (int i = 0; i < field.Children.Count; i++)
                {
                    FieldNode node = field.Children[i];

                    DataGridViewRow row = new DataGridViewRow();

                    node.Children.ForEach(n => {
                        DataGridViewCell cell = new DataGridViewTextBoxCell();
                        cell.Value = n.Value;
                        cell.Tag = n;
                        row.Cells.Add(cell);
                    });

                    dataGrid.Rows.Add(row);
                }
            }
            else
            {
                dataGrid.Columns.Add("Name", "Name");
                dataGrid.Columns.Add("Value", "Value");
                dataGrid.RowHeadersVisible = false;
                for (int i = 0; i < field.Children.Count; i++)
                {
                    FieldNode node = field.Children[i];

                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewCell cell = new DataGridViewTextBoxCell();
                    row.Cells.Add(cell);
                    cell.Value = node.Name;
                    cell.ReadOnly = true;

                    cell = new DataGridViewTextBoxCell();
                    row.Cells.Add(cell);
                    cell.Value = node.Value == null ? "null" : node.Value;
                    cell.Tag = node;
                    if(!node.Type.IsValueType && !node.Type.Equals(typeof(string)))
                    {
                        cell.ReadOnly = true;
                    }

                    dataGrid.Rows.Add(row);

                }
            }

        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FieldNode field = (FieldNode)e.Node.Tag;

            this.lableInfo.Text = field.Type.FullName;

            dataGrid.Columns.Clear();

            if(field.Type.IsValueType || field.Type.Equals(typeof(string)))
            {
                btnEdit.Enabled = false;
                btnEdit.Tag = null;
                return;
            }

            if(field.Value == null)
            {
                btnEdit.Text = "Construct";
                btnEdit.Enabled = true;
                btnEdit.Tag = field;
            }
            else
            {
                btnEdit.Text = "Destroy";
                btnEdit.Enabled = true;
                btnEdit.Tag = field;

                FreshDataGrid(field);
            }

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            FieldNode node = (FieldNode)btnEdit.Tag;
            if (node.Value == null)
            {
                node.ConstructValue();
            }
            else
            {
                node.DestroyValue();
            }

            RefreshTreeView();
        }

        private void btnAddElem_Click(object sender, EventArgs e)
        {
            FieldNode node = (FieldNode)btnEdit.Tag;
            if (node.Value != null)
            {
                node.CreateArrayElement();
                RefreshTreeView();
            }
        }

        private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = this.dataGrid.
                               Rows[e.RowIndex].
                               Cells[e.ColumnIndex];

            FieldNode node = (FieldNode)cell.Tag;
            if(node.Type.IsValueType)
            {
                MethodInfo method = node.Type.GetMethod("Parse", new Type[] { typeof(string) });
                if(method == null)
                {
                    Log.Append("can not find method TryParse in value type {0}", node.Type.Name);
                }
                else
                {
                    try
                    {
                    	node.Value = method.Invoke(null, new Object[] { cell.Value });
                    }
                    catch (System.Exception ex)
                    {
                        Log.Append("invoke Parse exception : {0}", ex.ToString());
                    }
                    finally
                    {
                        cell.Value = node.Value.ToString();
                    }
                }
            }

            if(node.Type.Equals(typeof(string)))
            {
                node.Value = cell.Value;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reflactor_.Root.CollectInstanceValue();
            reflactor_.Root.SerializeTo(@"d:\test.xml");
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            Log.Append("resolve {0}", e.Name);
            Assembly ass = Assembly.ReflectionOnlyLoad(e.Name);
            return ass;
        }

        private void MenuItemLoadAssembly_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            openFileDialog.Filter = "程序集|*.dll;*.exe|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog.FileName;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.ResolveAssembly;
                try
                {
                    Assembly assembly = Assembly.ReflectionOnlyLoadFrom(fName);
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        Log.Append(type.FullName);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Append("打开程序集失败 {0}", ex.ToString());
                    return;
                }
                finally
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.ResolveAssembly;
                }

            }
        }

        private void MenuItemLoadInstance_Click(object sender, EventArgs e)
        {
            reflactor_.Root.SerializeFrom(@"d:\test.xml");
            reflactor_.Root.DispatchInstanceValue();

        }
    }

    class Log
    {
        public static TextBoxBase Pipe
        {
            set
            {
                pipe_ = value;
            }
        }

        private static TextBoxBase pipe_;
        
        public static void Append(string fmt, params Object[] values)
        {
            string str = string.Format(fmt + '\n', values);
            if (pipe_ != null)
            {
                pipe_.AppendText(str);
            }
        }

        public static void Clear()
        {
            if(pipe_ != null)
            {
                pipe_.Clear();
            }
        }
    }

}
