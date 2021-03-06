﻿namespace DriveCrypt
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.chooseFolder = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.logout = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.emailInput = new System.Windows.Forms.TextBox();
            this.share = new System.Windows.Forms.Button();
            this.sharePublicKey = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.FolderList = new System.Windows.Forms.TreeView();
            this.FileListMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.shareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decodeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.DCListMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.encodeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.SharedWithMeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.decodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileListMenu.SuspendLayout();
            this.DCListMenu.SuspendLayout();
            this.SharedWithMeMenu.SuspendLayout();
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
            this.chooseFolder.Location = new System.Drawing.Point(194, 194);
            this.chooseFolder.Name = "chooseFolder";
            this.chooseFolder.Size = new System.Drawing.Size(75, 34);
            this.chooseFolder.TabIndex = 7;
            this.chooseFolder.Text = "Choose folder";
            this.chooseFolder.UseVisualStyleBackColor = true;
            this.chooseFolder.Click += new System.EventHandler(this.chooseFolder_Click);
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(275, 42);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(285, 20);
            this.textBox2.TabIndex = 9;
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
            // logout
            // 
            this.logout.Location = new System.Drawing.Point(12, 39);
            this.logout.Name = "logout";
            this.logout.Size = new System.Drawing.Size(75, 23);
            this.logout.TabIndex = 13;
            this.logout.Text = "Logout";
            this.logout.UseVisualStyleBackColor = true;
            this.logout.Click += new System.EventHandler(this.logout_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Email to share:";
            // 
            // emailInput
            // 
            this.emailInput.Location = new System.Drawing.Point(94, 107);
            this.emailInput.Name = "emailInput";
            this.emailInput.Size = new System.Drawing.Size(100, 20);
            this.emailInput.TabIndex = 15;
            // 
            // share
            // 
            this.share.Location = new System.Drawing.Point(109, 133);
            this.share.Name = "share";
            this.share.Size = new System.Drawing.Size(75, 23);
            this.share.TabIndex = 16;
            this.share.Text = "Share";
            this.share.UseVisualStyleBackColor = true;
            this.share.Click += new System.EventHandler(this.share_Click);
            // 
            // sharePublicKey
            // 
            this.sharePublicKey.Location = new System.Drawing.Point(94, 162);
            this.sharePublicKey.Name = "sharePublicKey";
            this.sharePublicKey.Size = new System.Drawing.Size(102, 23);
            this.sharePublicKey.TabIndex = 17;
            this.sharePublicKey.Text = "Share Public Key";
            this.sharePublicKey.UseVisualStyleBackColor = true;
            this.sharePublicKey.Click += new System.EventHandler(this.sharePublicKey_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(194, 234);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 34);
            this.button3.TabIndex = 18;
            this.button3.Text = "Sync folder";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // FolderList
            // 
            this.FolderList.Location = new System.Drawing.Point(275, 68);
            this.FolderList.Name = "FolderList";
            this.FolderList.Size = new System.Drawing.Size(285, 242);
            this.FolderList.TabIndex = 19;
            // 
            // FileListMenu
            // 
            this.FileListMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shareToolStripMenuItem,
            this.decodeToolStripMenuItem1});
            this.FileListMenu.Name = "FolderListMenu";
            this.FileListMenu.Size = new System.Drawing.Size(115, 48);
            // 
            // shareToolStripMenuItem
            // 
            this.shareToolStripMenuItem.Name = "shareToolStripMenuItem";
            this.shareToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.shareToolStripMenuItem.Text = "Share";
            this.shareToolStripMenuItem.Click += new System.EventHandler(this.shareToolStripMenuItem_Click);
            // 
            // decodeToolStripMenuItem1
            // 
            this.decodeToolStripMenuItem1.Name = "decodeToolStripMenuItem1";
            this.decodeToolStripMenuItem1.Size = new System.Drawing.Size(114, 22);
            this.decodeToolStripMenuItem1.Text = "Decode";
            this.decodeToolStripMenuItem1.Click += new System.EventHandler(this.decodeToolStripMenuItem_Click);
            // 
            // DCListMenu
            // 
            this.DCListMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.encodeToolStripMenuItem1});
            this.DCListMenu.Name = "DCListMenu";
            this.DCListMenu.Size = new System.Drawing.Size(114, 26);
            // 
            // encodeToolStripMenuItem1
            // 
            this.encodeToolStripMenuItem1.Name = "encodeToolStripMenuItem1";
            this.encodeToolStripMenuItem1.Size = new System.Drawing.Size(113, 22);
            this.encodeToolStripMenuItem1.Text = "Encode";
            this.encodeToolStripMenuItem1.Click += new System.EventHandler(this.encodeToolStripMenuItem1_Click);
            // 
            // SharedWithMeMenu
            // 
            this.SharedWithMeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decodeToolStripMenuItem});
            this.SharedWithMeMenu.Name = "SharedWithMeMenu";
            this.SharedWithMeMenu.Size = new System.Drawing.Size(153, 48);
            // 
            // decodeToolStripMenuItem
            // 
            this.decodeToolStripMenuItem.Name = "decodeToolStripMenuItem";
            this.decodeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.decodeToolStripMenuItem.Text = "Decode";
            this.decodeToolStripMenuItem.Click += new System.EventHandler(this.decodeToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 331);
            this.Controls.Add(this.FolderList);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.sharePublicKey);
            this.Controls.Add(this.share);
            this.Controls.Add(this.emailInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logout);
            this.Controls.Add(this.userNameLabel);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.chooseFolder);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Drive Crypt";
            this.FileListMenu.ResumeLayout(false);
            this.DCListMenu.ResumeLayout(false);
            this.SharedWithMeMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button chooseFolder;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label userNameLabel;
        private System.Windows.Forms.Button logout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox emailInput;
        private System.Windows.Forms.Button share;
        private System.Windows.Forms.Button sharePublicKey;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TreeView FolderList;
        private System.Windows.Forms.ContextMenuStrip FileListMenu;
        private System.Windows.Forms.ToolStripMenuItem shareToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DCListMenu;
        private System.Windows.Forms.ToolStripMenuItem encodeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem decodeToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip SharedWithMeMenu;
        private System.Windows.Forms.ToolStripMenuItem decodeToolStripMenuItem;
    }
}

