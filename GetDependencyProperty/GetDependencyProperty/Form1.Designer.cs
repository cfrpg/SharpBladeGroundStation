namespace GetDependencyProperty
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.ctrltext = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.typetext = new System.Windows.Forms.TextBox();
            this.nametext = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.restext = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Control";
            // 
            // ctrltext
            // 
            this.ctrltext.Location = new System.Drawing.Point(65, 22);
            this.ctrltext.Name = "ctrltext";
            this.ctrltext.Size = new System.Drawing.Size(100, 21);
            this.ctrltext.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Type";
            // 
            // typetext
            // 
            this.typetext.Location = new System.Drawing.Point(65, 49);
            this.typetext.Name = "typetext";
            this.typetext.Size = new System.Drawing.Size(100, 21);
            this.typetext.TabIndex = 3;
            // 
            // nametext
            // 
            this.nametext.Location = new System.Drawing.Point(65, 76);
            this.nametext.Name = "nametext";
            this.nametext.Size = new System.Drawing.Size(100, 21);
            this.nametext.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "Name";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(202, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // restext
            // 
            this.restext.Location = new System.Drawing.Point(65, 103);
            this.restext.Multiline = true;
            this.restext.Name = "restext";
            this.restext.Size = new System.Drawing.Size(1070, 276);
            this.restext.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1147, 391);
            this.Controls.Add(this.restext);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nametext);
            this.Controls.Add(this.typetext);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ctrltext);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ctrltext;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox typetext;
        private System.Windows.Forms.TextBox nametext;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox restext;
    }
}

