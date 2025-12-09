using System;
using System.Drawing;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class Login : Form
    {
        private System.ComponentModel.IContainer components = null;

        private PictureBox picLogo;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Panel pnlContainer;
        private CheckBox chkAutoLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            pnlContainer = new Panel();
            picLogo = new PictureBox();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            chkAutoLogin = new CheckBox();
            btnLogin = new Button();
            btnRegister = new Button();

            pnlContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(picLogo)).BeginInit();
            SuspendLayout();

            // 
            // pnlContainer
            // 
            pnlContainer.Controls.Add(txtUsername);
            pnlContainer.Controls.Add(txtPassword);
            pnlContainer.Controls.Add(chkAutoLogin);
            pnlContainer.Controls.Add(btnLogin);
            pnlContainer.Controls.Add(btnRegister);
            pnlContainer.Location = new Point(0, 0);
            pnlContainer.Name = "pnlContainer";
            pnlContainer.Size = new Size(330, 300);
            pnlContainer.TabIndex = 0;

            // 
            // picLogo
            // 
            picLogo.Image = Properties.Resource1.policee; // 리소스 로고 이미지
            picLogo.Location = new Point(200, 80);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(390, 90);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            this.Controls.Add(picLogo);
            picLogo.BringToFront();

            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(45, 45, 45);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.ForeColor = Color.White;
            txtUsername.Location = new Point(50, 120);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "닉네임";
            txtUsername.Size = new Size(230, 23);
            txtUsername.TabIndex = 1;

            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(45, 45, 45);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.ForeColor = Color.White;
            txtPassword.Location = new Point(50, 155);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "비밀번호";
            txtPassword.Size = new Size(230, 23);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;

            // 
            // chkAutoLogin
            // 
            chkAutoLogin.AutoSize = true;
            chkAutoLogin.ForeColor = Color.White;
            chkAutoLogin.Location = new Point(50, 182);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Size = new Size(90, 19);
            chkAutoLogin.TabIndex = 5;
            chkAutoLogin.Text = "자동 로그인";
            chkAutoLogin.UseVisualStyleBackColor = true;

            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(70, 70, 70);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(50, 210);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(230, 30);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "로그인";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;

            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(70, 70, 70);
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(50, 250);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(230, 30);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "회원가입 하러가기";
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Click += BtnRegister_Click;

            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 45);
            ClientSize = new Size(800, 450);
            Controls.Add(pnlContainer);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "로그인";
            Load += Login_Load;

            pnlContainer.ResumeLayout(false);
            pnlContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(picLogo)).EndInit();
            ResumeLayout(false);
        }
    }
}
