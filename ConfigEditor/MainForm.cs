using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using ConfigableData;
using System.IO;

namespace ConfigEditor
{
    // 验证可序列化
    // 初始化时自动添加unity路径
    // 泛型list支持
    
    public partial class MainForm : Form
    {
        FieldNode root_;

        public static List<LoadedAssembly> Assemblys = new List<LoadedAssembly>();

        public MainForm()
        {
            InitializeComponent();
        }
        
        private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            Log.Append("resolve {0}", e.Name);
            AssemblyName resName = new AssemblyName(e.Name);
            foreach (string path in Properties.Settings.Default.DependPath)
            {
                string fullpath = string.Format("{0}\\{1}.dll", path, resName.Name);
                if (File.Exists(fullpath))
                {
                    Log.Append("load {0}", fullpath);
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(fullpath);
                        return assembly;
                    }
                    catch (System.Exception ex)
                    {
                        Log.Append("找到{0}，但加载失败 {1}", fullpath, ex.ToString());
                    }
                }
            }

            return null ;
        }

        private void LoadClasses()
        {
            Assemblys.Clear();

            AppDomain.CurrentDomain.AssemblyResolve += this.ResolveAssembly;
            foreach (string fName in Properties.Settings.Default.AssemblyPath)
            {
                Assembly assembly;
                LoadedAssembly loaded;
                try
                {
                    assembly = Assembly.LoadFrom(fName);
                    if (assembly == null)
                    {
                        Log.Append("assembly is null {0}", fName);
                    }

                    loaded = new LoadedAssembly();
                    loaded.assembly = assembly;

                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsDefined(typeof(ConfigableData.ConfigDataAttribute), false))
                        {
                            loaded.LoadedTypes.Add(type);
                            Log.Append(type.FullName);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Append("加载程序集 {0} 失败：{1}", fName, ex.ToString());
                    continue;
                }

                Assemblys.Add(loaded);
            }

            AppDomain.CurrentDomain.AssemblyResolve -= this.ResolveAssembly;
        }

        private void RefreshTreeView()
        {
            treeView.Nodes.Clear();
            FieldNode fn = root_;
            TreeNode root = treeView.Nodes.Add(fn.Name);

            AddTreeNode(root, fn);
            root.ExpandAll();
        }

        static void AddTreeNode(TreeNode tn, FieldNode fd)
        {
            tn.Nodes.Clear();
            tn.Tag = fd;
            tn.ToolTipText = fd.Comment;
            //if(fd.Type.IsArray) {
            //    return;
            //}

            fd.Children.ForEach(node => {
                if (!node.Type.IsValueType && node.Type != typeof(string))
                {
                    TreeNode newTn = tn.Nodes.Add(node.Name);
                    AddTreeNode(newTn, node);
                }
            });
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Log.Pipe = this.console;

            foreach (string path in Properties.Settings.Default.DependPath)
            { }

            LoadClasses();

            //RefreshTreeView();

            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill ;
        }

        private DataGridViewCell CreateGridViewCell(FieldNode node, DataGridViewCellCollection cells)
        {
            DataGridViewCell cell;
            if (node.Type.IsEnum)
            {
                DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                cells.Add(combo);
                combo.DataSource = node.Type.GetEnumNames();
                cell = combo;

                cell.Value = Enum.GetName(node.Type, node.Value);

                Log.Append("value type " + combo.ValueType.Name);
            }
            else if (node.Type == typeof(Boolean))
            {
                cell = new DataGridViewCheckBoxCell();
                cells.Add(cell);
                cell.Value = node.Value;
            }
            else
            {
                cell = new DataGridViewTextBoxCell();
                cells.Add(cell);
                cell.Value = (node.Value == null) ? "null" : node.Value;
            }

            cell.Tag = node;
            return cell;
        }

        private DataGridViewColumn CreateColumn(Type type)
        {

            DataGridViewColumn column;
            if (type.IsEnum)
            {
                DataGridViewComboBoxColumn c = new DataGridViewComboBoxColumn();
                c.DataSource = type.GetEnumNames();
                column = c;
                //DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                //cells.Add(combo);
                //combo.DataSource = node.Type.GetEnumNames();
                //cell = combo;

                //cell.Value = Enum.GetName(node.Type, node.Value);

                //Log.Append("value type " + combo.ValueType.Name);
            }
            else if (type == typeof(Boolean))
            {
                DataGridViewCheckBoxColumn c = new DataGridViewCheckBoxColumn();
                column = c;
            }
            else
            {
                DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                column = c;
            }

            return column;
        }

        private void RefreshDataGrid(FieldNode field)
        {
            dataGrid.Columns.Clear();
            if (field == null)
            {
                return;
            }

            if(field.Type.IsArray)
            {
                Type elemType = field.Type.GetElementType();
                
                if (elemType.IsValueType || elemType == typeof(string))
                {
                    dataGrid.Columns.Add(CreateColumn(elemType));
                }
                else
                {
                    FieldInfo[] fields = elemType.GetFields(BindingFlags.NonPublic |
                                                       BindingFlags.Public |
                                                       BindingFlags.Instance);

                    for (int i = 0; i < fields.Length; i++)
                    {
                        DataGridViewColumn column = CreateColumn(fields[i].FieldType);
                        column.HeaderText = fields[i].Name;
                        column.ToolTipText = fields[i].Comment();
                        dataGrid.Columns.Add(column);
                    }
                }

                dataGrid.RowHeadersVisible = true;

                if (field.Children.Count > 0)
                {
                    dataGrid.Rows.Add(field.Children.Count);

                    for (int i = 0; i < field.Children.Count; i++)
                    {
                        FieldNode node = field.Children[i];
                        for (int j = 0; j < node.Children.Count; j++)
                        {
                            FieldNode n = node.Children[j];
                            dataGrid.Rows[i].Cells[j].Tag = n;
                            if (node.Children[j].Type.IsEnum)
                            {
                                dataGrid.Rows[i].Cells[j].Value = Enum.GetName(n.Type, n.Value);
                            }
                            else
                            {
                                dataGrid.Rows[i].Cells[j].Value = n.Value;
                            }
                        }
                    }
                }


                //for (int i = 0; i < field.Children.Count; i++)
                //{
                //    FieldNode node = field.Children[i];

                //    DataGridViewRow row = new DataGridViewRow();

                //    node.Children.ForEach(n => {
                //        DataGridViewCell cell = CreateGridViewCell(n, row.Cells);
                        
                //        //row.Cells.Add(cell);
                //    });
                //    dataGrid.Rows.Add(row);
                //}
            }
            else
            {
                dataGrid.Columns.Add("Name", "名称");
                dataGrid.Columns.Add("Value", "值");
                dataGrid.Columns.Add("Type", "类型");
                dataGrid.Columns.Add("Comment", "");
                dataGrid.RowHeadersVisible = false;
                for (int i = 0; i < field.Children.Count; i++)
                {
                    FieldNode node = field.Children[i];

                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewCell cell = new DataGridViewTextBoxCell();
                    row.Cells.Add(cell);
                    cell.Value = node.Name;
                    cell.ReadOnly = true;

                    cell = CreateGridViewCell(node, row.Cells);
                    //row.Cells.Add(cell);
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

                    cell = new DataGridViewTextBoxCell();
                    cell.Value = node.Comment;
                    row.Cells.Add(cell);

                    dataGrid.Rows.Add(row);
                }
            }

            treeView.Focus();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            FieldNode field = (FieldNode)e.Node.Tag;

            //this.lableInfo.Text = field.Type.FullName;
            toolStripButtonCreate.Tag = e.Node;

            if(field.Type.IsValueType || field.Type.Equals(typeof(string)))
            {
                toolStripButtonCreate.Enabled = false;
                toolStripButtonCreate.Tag = null;
                return;
            }

            if(field.Value == null)
            {
                toolStripButtonCreate.Text = "Construct";
                toolStripButtonCreate.Enabled = true;
            }
            else
            {
                toolStripButtonCreate.Text = "Destroy";
                toolStripButtonCreate.Enabled = true;
            }

            if(field.Type.IsArray && field.Value != null)
            {
                this.toolStripButtonAddElem.Enabled = true;
            }
            else
            {
                toolStripButtonAddElem.Enabled = false;
            }

            RefreshDataGrid(field);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            TreeNode tn = (TreeNode)this.toolStripButtonCreate.Tag;
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
            TreeNode tn = (TreeNode)toolStripButtonCreate.Tag;
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
            FormSelectType form = new FormSelectType();
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (form.SelectedType != null)
                {
                    root_ = new FieldNode(form.SelectedType);
                    root_.ConstructValue();
                    RefreshTreeView();
                }
            }
            form.Dispose();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (root_ != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "xml文件|*.xml|所有文件|*.*";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false;
                openFileDialog.FilterIndex = 1;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    root_.CollectInstanceValue();
                    root_.SerializeTo(@"d:\test.xml");
                }
                openFileDialog.Dispose();
            }
        }
                
        private void MenuItemLoadInstance_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml文件|*.xml|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                root_ = FieldNode.SerializeFrom(@"d:\test.xml");
                root_.DispatchField();
                //root_.DispatchValue();
                RefreshTreeView();
            }

            openFileDialog.Dispose();
        }

        private void ToolStripMenuItemConfig_Click(object sender, EventArgs e)
        {
            FormConfig form = new FormConfig();
            DialogResult result = form.ShowDialog();
            LoadClasses();
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
    
    public class LoadedAssembly
    {
        public Assembly assembly;
        public List<Type> LoadedTypes = new List<Type>();
    }

}
