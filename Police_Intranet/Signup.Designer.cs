using System;
using System.Drawing;
using System.Windows.Forms;

namespace Police_Intranet
{
    partial class Signup
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtUserid;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnRegister;
        private Button btnSignin;
        private Panel pnlContainer;
        private PictureBox picLogo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Signup));
            pnlContainer = new Panel();
            picLogo = new PictureBox();
            txtUserid = new TextBox();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnRegister = new Button();
            btnSignin = new Button();
            pnlContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // pnlContainer
            // 
            pnlContainer.Controls.Add(txtUserid);
            pnlContainer.Controls.Add(txtUsername);
            pnlContainer.Controls.Add(txtPassword);
            pnlContainer.Controls.Add(btnRegister);
            pnlContainer.Controls.Add(btnSignin);
            pnlContainer.Location = new Point(0, 0);
            pnlContainer.Name = "pnlContainer";
            pnlContainer.Size = new Size(330, 300);
            pnlContainer.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.Image = Properties.Resource1.policee;
            picLogo.Location = new Point(200, 80);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(390, 90);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // txtUserid
            // 
            txtUserid.BackColor = Color.FromArgb(45, 45, 45);
            txtUserid.BorderStyle = BorderStyle.FixedSingle;
            txtUserid.ForeColor = Color.White;
            txtUserid.Location = new Point(50, 120);
            txtUserid.Name = "txtUserid";
            txtUserid.PlaceholderText = "고유번호";
            txtUserid.Size = new Size(230, 23);
            txtUserid.TabIndex = 1;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(45, 45, 45);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.ForeColor = Color.White;
            txtUsername.Location = new Point(50, 150);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "닉네임";
            txtUsername.Size = new Size(230, 23);
            txtUsername.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(45, 45, 45);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.ForeColor = Color.White;
            txtPassword.Location = new Point(50, 180);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "비밀번호";
            txtPassword.Size = new Size(230, 23);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(70, 70, 70);
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(50, 210);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(230, 30);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "회원가입";
            btnRegister.UseVisualStyleBackColor = false;
            // 
            // btnSignin
            // 
            btnSignin.BackColor = Color.FromArgb(70, 70, 70);
            btnSignin.FlatAppearance.BorderSize = 0;
            btnSignin.FlatStyle = FlatStyle.Flat;
            btnSignin.ForeColor = Color.White;
            btnSignin.Location = new Point(50, 250);
            btnSignin.Name = "btnSignin";
            btnSignin.Size = new Size(230, 30);
            btnSignin.TabIndex = 5;
            btnSignin.Text = "로그인 하러가기";
            btnSignin.UseVisualStyleBackColor = false;
            // 
            // Signup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 45);
            ClientSize = new Size(800, 450);
            Controls.Add(picLogo);
            Controls.Add(pnlContainer);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Signup";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "회원가입";
            Load += Signup_Load;
            pnlContainer.ResumeLayout(false);
            pnlContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }
    }
}
