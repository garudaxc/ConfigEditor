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
            tn.Nodes.Clear();
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

            Type type = typeof(Gift);
            ConfigDataAttribute attri = type.GetCustomAttribute<ConfigDataAttribute>();
            if(attri != null)
            {
                labelComment.Text = attri.Comment;
            }

            reflactor_ = new TypeReflactor(typeof(Gift));

            RefreshTreeView();

            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill ;            
            //Assembly assembly = Assembly.LoadFrom(@"D:\Program Files\Unity\Editor\Data\Managed\UnityEngine.dll");
        }

        private DataGridViewCell CreateGridViewCell(FieldNode node)
        {
            DataGridViewCell cell;
            if (node.Type.IsEnum)
            {
                DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                combo.DataSource = node.Type.GetEnumNames();
                cell = combo;

                cell.Value = Enum.GetName(node.Type, node.Value);

                Log.Append("value type " + combo.ValueType.Name);
            }
            else 
            if (node.Type == typeof(Boolean))
            {
                cell = new DataGridViewCheckBoxCell();
                cell.Value = node.Value;
            }
            else
            {
                cell = new DataGridViewTextBoxCell();
                cell.Value = (node.Value == null) ? "null" : node.Value;
            }

            cell.Tag = node;
            return cell;
        }


        private void FreshDataGrid(FieldNode field)
        {
            dataGrid.Columns.Clear();
            if (field == null)
            {
                return;
            }

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
                dataGrid.Columns.Add("Name", "名称");
                dataGrid.Columns.Add("Value", "值");
                dataGrid.Columns.Add("Type", "类型");
                dataGrid.RowHeadersVisible = false;
                for (int i = 0; i < field.Children.Count; i++)
                {
                    FieldNode node = field.Children[i];

                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewCell cell = new DataGridViewTextBoxCell();
                    row.Cells.Add(cell);
                    cell.Value = node.Name;
                    cell.ReadOnly = true;

                    cell = CreateGridViewCell(node);
                    row.Cells.Add(cell);
                    if (!node.Type.IsValueType && !node.Type.Equals(typeof(string)))
                    {
                        cell.ReadOnly = true;
                    }

                    cell = new DataGridViewTextBoxCell();
                    row.Cells.Add(cell);
                    string v = string.Empty;
                    if(node.Type.IsClass && node.Type != typeof(string))
                    {
                        v += "类:";
                    }
                    if (node.Type.IsEnum)
                    {
                        v += "枚举:";
                    }
                    cell.Value = v + node.Type.Name;
                    cell.ReadOnly = true;

                    dataGrid.Rows.Add(row);
                }
            }

            treeView.Focus();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FieldNode field = (FieldNode)e.Node.Tag;

            this.lableInfo.Text = field.Type.FullName;
            btnEdit.Tag = e.Node;

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
            }
            else
            {
                btnEdit.Text = "Destroy";
                btnEdit.Enabled = true;
            }

            if(field.Type.IsArray && field.Value != null)
            {
                btnAddElem.Enabled = true;
            }
            else
            {
                btnAddElem.Enabled = false;
            }

            FreshDataGrid(field);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            TreeNode tn = (TreeNode)btnEdit.Tag;
            FieldNode fn = (FieldNode)tn.Tag;

            if (fn.Value == null)
            {
                fn.ConstructValue();
            }
            else
            {
                fn.DestroyValue();
            }

            AddTreeNode(tn, fn);
            tn.ExpandAll();
            treeView.SelectedNode = null;
            treeView.SelectedNode = tn;
        }

        private void btnAddElem_Click(object sender, EventArgs e)
        {
            TreeNode tn = (TreeNode)btnEdit.Tag;
            FieldNode fn = (FieldNode)tn.Tag;

            if (fn.Value != null)
            {
                fn.CreateArrayElement();

                AddTreeNode(tn, fn);
                tn.ExpandAll();
                treeView.SelectedNode = null;
                treeView.SelectedNode = tn;
            }
        }

        private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = this.dataGrid.
                               Rows[e.RowIndex].
                               Cells[e.ColumnIndex];

            FieldNode node = (FieldNode)cell.Tag;

            if (node.Type == typeof(bool) ||
                node.Type == typeof(string))
            {
                node.Value = cell.Value;
            }
            else if (node.Type.IsEnum)
            {
                node.Value = Enum.Parse(node.Type, (string)cell.Value);
            }
            else if (node.Type.IsValueType)
            {
                MethodInfo method = node.Type.GetMethod("Parse", new Type[] { typeof(string) });
                if (method == null)
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
                // 预加载程序集？
                //AppDomain.CurrentDomain.AssemblyResolve
                //AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.ResolveAssembly;
                try
                {
                    Assembly assembly = Assembly.LoadFrom(fName);
                    Type attri = assembly.GetType("ConfigDataAttribute");

                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsDefined(attri, false))
                        {
                            Log.Append(type.FullName);
                            
                            reflactor_ = new TypeReflactor(type);
                            RefreshTreeView();
                            break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Append("打开程序集失败 {0}", ex.ToString());
                    return;
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
