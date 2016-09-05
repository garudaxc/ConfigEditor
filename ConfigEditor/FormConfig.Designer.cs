namespace ConfigEditor
{
    partial class FormConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Button buttonAddAssembly;
            System.Windows.Forms.Button buttonAddPath;
            this.listBoxAssembly = new System.Windows.Forms.ListBox();
            this.listBoxDependPath = new System.Windows.Forms.ListBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonDelAssebly = new System.Windows.Forms.Button();
            this.buttonDelPath = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            buttonAddAssembly = new System.Windows.Forms.Button();
            buttonAddPath = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 25);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(41, 12);
            label1.TabIndex = 1;
            label1.Text = "程序集";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 174);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(77, 12);
            label2.TabIndex = 2;
            label2.Text = "依赖加载路径";
            // 
            // buttonAddAssembly
            // 
            buttonAddAssembly.Anchor = System.Windows.Forms.AnchorStyles.Right;
            buttonAddAssembly.Location = new System.Drawing.Point(455, 40);
            buttonAddAssembly.Name = "buttonAddAssembly";
            buttonAddAssembly.Size = new System.Drawing.Size(40, 35);
            buttonAddAssembly.TabIndex = 5;
            buttonAddAssembly.Text = "Add";
            buttonAddAssembly.UseVisualStyleBackColor = true;
            buttonAddAssembly.Click += new System.EventHandler(this.buttonAddAssembly_Click);
            // 
            // buttonAddPath
            // 
            buttonAddPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            buttonAddPath.Location = new System.Drawing.Point(453, 189);
            buttonAddPath.Name = "buttonAddPath";
            buttonAddPath.Size = new System.Drawing.Size(40, 35);
            buttonAddPath.TabIndex = 7;
            buttonAddPath.Text = "Add";
            buttonAddPath.UseVisualStyleBackColor = true;
            buttonAddPath.Click += new System.EventHandler(this.buttonAddPath_Click);
            // 
            // listBoxAssembly
            // 
            this.listBoxAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxAssembly.FormattingEnabled = true;
            this.listBoxAssembly.ItemHeight = 12;
            this.listBoxAssembly.Location = new System.Drawing.Point(15, 40);
            this.listBoxAssembly.Name = "listBoxAssembly";
            this.listBoxAssembly.Size = new System.Drawing.Size(434, 112);
            this.listBoxAssembly.TabIndex = 0;
            this.listBoxAssembly.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // listBoxDependPath
            // 
            this.listBoxDependPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxDependPath.FormattingEnabled = true;
            this.listBoxDependPath.ItemHeight = 12;
            this.listBoxDependPath.Location = new System.Drawing.Point(12, 189);
            this.listBoxDependPath.Name = "listBoxDependPath";
            this.listBoxDependPath.Size = new System.Drawing.Size(436, 112);
            this.listBoxDependPath.TabIndex = 3;
            this.listBoxDependPath.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(172, 307);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(144, 37);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonDelAssebly
            // 
            this.buttonDelAssebly.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonDelAssebly.Location = new System.Drawing.Point(455, 80);
            this.buttonDelAssebly.Name = "buttonDelAssebly";
            this.buttonDelAssebly.Size = new System.Drawing.Size(40, 35);
            this.buttonDelAssebly.TabIndex = 5;
            this.buttonDelAssebly.Text = "Del";
            this.buttonDelAssebly.UseVisualStyleBackColor = true;
            this.buttonDelAssebly.Click += new System.EventHandler(this.buttonDelAssebly_Click);
            // 
            // buttonDelPath
            // 
            this.buttonDelPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonDelPath.Location = new System.Drawing.Point(453, 230);
            this.buttonDelPath.Name = "buttonDelPath";
            this.buttonDelPath.Size = new System.Drawing.Size(40, 35);
            this.buttonDelPath.TabIndex = 6;
            this.buttonDelPath.Text = "Del";
            this.buttonDelPath.UseVisualStyleBackColor = true;
            this.buttonDelPath.Click += new System.EventHandler(this.buttonDelPath_Click);
            // 
            // FormConfig
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 366);
            this.Controls.Add(this.buttonDelPath);
            this.Controls.Add(buttonAddPath);
            this.Controls.Add(this.buttonDelAssebly);
            this.Controls.Add(buttonAddAssembly);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listBoxDependPath);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this.listBoxAssembly);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfig";
            this.Text = "设置";
            this.Load += new System.EventHandler(this.FormConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxAssembly;
        private System.Windows.Forms.ListBox listBoxDependPath;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonDelAssebly;
        private System.Windows.Forms.Button buttonDelPath;
    }
}