namespace DriveCrypt
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.chooseFolder = new System.Windows.Forms.Button();
            this.FolderList = new System.Windows.Forms.ListBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.exportRsaKeys = new System.Windows.Forms.Button();
            this.importRsaKeys = new System.Windows.Forms.Button();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 194);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Encode file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(9, 223);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Decode file";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(95, 39);
            this.textBox1.Name = "textBox1";
            this.textBox1.PasswordChar = '•';
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "User password:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(95, 68);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(94, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Use password";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(9, 287);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Send file";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // chooseFolder
            // 
            this.chooseFolder.Location = new System.Drawing.Point(194, 139);
            this.chooseFolder.Name = "chooseFolder";
            this.chooseFolder.Size = new System.Drawing.Size(75, 23);
            this.chooseFolder.TabIndex = 7;
            this.chooseFolder.Text = "Choose folder";
            this.chooseFolder.UseVisualStyleBackColor = true;
            this.chooseFolder.Click += new System.EventHandler(this.chooseFolder_Click);
            // 
            // FolderList
            // 
            this.FolderList.FormattingEnabled = true;
            this.FolderList.HorizontalScrollbar = true;
            this.FolderList.Location = new System.Drawing.Point(275, 68);
            this.FolderList.Name = "FolderList";
            this.FolderList.Size = new System.Drawing.Size(285, 238);
            this.FolderList.TabIndex = 8;
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(275, 42);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(285, 20);
            this.textBox2.TabIndex = 9;
            // 
            // exportRsaKeys
            // 
            this.exportRsaKeys.Location = new System.Drawing.Point(9, 108);
            this.exportRsaKeys.Name = "exportRsaKeys";
            this.exportRsaKeys.Size = new System.Drawing.Size(75, 34);
            this.exportRsaKeys.TabIndex = 10;
            this.exportRsaKeys.Text = "Export RSA keys";
            this.exportRsaKeys.UseVisualStyleBackColor = true;
            this.exportRsaKeys.Click += new System.EventHandler(this.exportRsaKeys_Click);
            // 
            // importRsaKeys
            // 
            this.importRsaKeys.Location = new System.Drawing.Point(95, 108);
            this.importRsaKeys.Name = "importRsaKeys";
            this.importRsaKeys.Size = new System.Drawing.Size(75, 34);
            this.importRsaKeys.TabIndex = 11;
            this.importRsaKeys.Text = "Import RSA keys";
            this.importRsaKeys.UseVisualStyleBackColor = true;
            this.importRsaKeys.Click += new System.EventHandler(this.importRsaKeys_Click);
            // 
            // userNameLabel
            // 
            this.userNameLabel.AutoSize = true;
            this.userNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.userNameLabel.Location = new System.Drawing.Point(9, 9);
            this.userNameLabel.Name = "userNameLabel";
            this.userNameLabel.Size = new System.Drawing.Size(94, 13);
            this.userNameLabel.TabIndex = 12;
            this.userNameLabel.Text = "userNameLabel";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 331);
            this.Controls.Add(this.userNameLabel);
            this.Controls.Add(this.importRsaKeys);
            this.Controls.Add(this.exportRsaKeys);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.FolderList);
            this.Controls.Add(this.chooseFolder);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Drive Crypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button chooseFolder;
        private System.Windows.Forms.ListBox FolderList;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button exportRsaKeys;
        private System.Windows.Forms.Button importRsaKeys;
        private System.Windows.Forms.Label userNameLabel;
    }
}

