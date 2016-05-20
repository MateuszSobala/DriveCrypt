namespace DriveCrypt
{
    partial class AuthorizationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthorizationForm));
            this.password = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.login = new System.Windows.Forms.Button();
            this.exportRsaKeys = new System.Windows.Forms.Button();
            this.importRsaKeys = new System.Windows.Forms.Button();
            this.userNameLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.newPassword = new System.Windows.Forms.TextBox();
            this.changePassword = new System.Windows.Forms.Button();
            this.register = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.confirmPassword = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(103, 12);
            this.password.Name = "password";
            this.password.PasswordChar = '•';
            this.password.Size = new System.Drawing.Size(106, 20);
            this.password.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "User password:";
            // 
            // login
            // 
            this.login.Location = new System.Drawing.Point(103, 41);
            this.login.Name = "login";
            this.login.Size = new System.Drawing.Size(100, 35);
            this.login.TabIndex = 5;
            this.login.Text = "Load and Decrypt user key";
            this.login.UseVisualStyleBackColor = true;
            this.login.Click += new System.EventHandler(this.login_Click);
            // 
            // exportRsaKeys
            // 
            this.exportRsaKeys.Location = new System.Drawing.Point(103, 30);
            this.exportRsaKeys.Name = "exportRsaKeys";
            this.exportRsaKeys.Size = new System.Drawing.Size(75, 34);
            this.exportRsaKeys.TabIndex = 10;
            this.exportRsaKeys.Text = "Export RSA keys";
            this.exportRsaKeys.UseVisualStyleBackColor = true;
            this.exportRsaKeys.Click += new System.EventHandler(this.exportRsaKeys_Click);
            // 
            // importRsaKeys
            // 
            this.importRsaKeys.Location = new System.Drawing.Point(9, 30);
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Confirm password:";
            // 
            // confirmNewPassword
            // 
            this.confirmNewPassword.Location = new System.Drawing.Point(110, 51);
            this.confirmNewPassword.Name = "confirmNewPassword";
            this.confirmNewPassword.PasswordChar = '•';
            this.confirmNewPassword.Size = new System.Drawing.Size(109, 20);
            this.confirmNewPassword.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "New password:";
            // 
            // newPassword
            // 
            this.newPassword.Location = new System.Drawing.Point(110, 17);
            this.newPassword.Name = "newPassword";
            this.newPassword.PasswordChar = '•';
            this.newPassword.Size = new System.Drawing.Size(109, 20);
            this.newPassword.TabIndex = 15;
            // 
            // changePassword
            // 
            this.changePassword.Location = new System.Drawing.Point(110, 81);
            this.changePassword.Name = "changePassword";
            this.changePassword.Size = new System.Drawing.Size(109, 40);
            this.changePassword.TabIndex = 17;
            this.changePassword.Text = "Change encryption password";
            this.changePassword.UseVisualStyleBackColor = true;
            this.changePassword.Click += new System.EventHandler(this.changePassword_Click);
            // 
            // register
            // 
            this.register.Location = new System.Drawing.Point(103, 38);
            this.register.Name = "register";
            this.register.Size = new System.Drawing.Size(94, 38);
            this.register.TabIndex = 18;
            this.register.Text = "Create and Encrypt user key";
            this.register.UseVisualStyleBackColor = true;
            this.register.Click += new System.EventHandler(this.register_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.login);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.password);
            this.groupBox1.Location = new System.Drawing.Point(8, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(227, 82);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Login";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.changePassword);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.newPassword);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.confirmNewPassword);
            this.groupBox2.Location = new System.Drawing.Point(260, 45);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(235, 127);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Change password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Confirm password:";
            // 
            // confirmPassword
            // 
            this.confirmPassword.Location = new System.Drawing.Point(103, 12);
            this.confirmPassword.Name = "confirmPassword";
            this.confirmPassword.PasswordChar = '•';
            this.confirmPassword.Size = new System.Drawing.Size(106, 20);
            this.confirmPassword.TabIndex = 3;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.confirmPassword);
            this.groupBox3.Controls.Add(this.register);
            this.groupBox3.Location = new System.Drawing.Point(8, 142);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(227, 82);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Register";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.exportRsaKeys);
            this.groupBox4.Controls.Add(this.importRsaKeys);
            this.groupBox4.Location = new System.Drawing.Point(151, 240);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 84);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Manage user keys";
            // 
            // AuthorizationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 336);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.userNameLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AuthorizationForm";
            this.Text = "Drive Crypt";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button login;
        private System.Windows.Forms.Button exportRsaKeys;
        private System.Windows.Forms.Button importRsaKeys;
        private System.Windows.Forms.Label userNameLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox confirmNewPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox newPassword;
        private System.Windows.Forms.Button changePassword;
        private System.Windows.Forms.Button register;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox confirmPassword;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}

