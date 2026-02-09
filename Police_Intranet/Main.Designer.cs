using System.Resources;

namespace Police_Intranet
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel leftSidebarPanel;
        private System.Windows.Forms.Panel leftSidebarContentPanel;
        private System.Windows.Forms.Panel logoPanel;
        private System.Windows.Forms.Panel buttonsPanel;

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Button btnMypage;
        private System.Windows.Forms.Button btnCalculator;
        private System.Windows.Forms.Button btnDoubleCal;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.Button btnSideNotice;
        private System.Windows.Forms.Button btnAdmin;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.PictureBox picLogo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.leftSidebarPanel = new System.Windows.Forms.Panel();
            this.leftSidebarContentPanel = new System.Windows.Forms.Panel();
            this.logoPanel = new System.Windows.Forms.Panel();
            this.buttonsPanel = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();

            this.btnMypage = new System.Windows.Forms.Button();
            this.btnCalculator = new System.Windows.Forms.Button();
            this.btnDoubleCal = new System.Windows.Forms.Button();
            this.btnSideNotice = new System.Windows.Forms.Button();
            this.btnReport = new System.Windows.Forms.Button();
            this.btnAdmin = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();

            this.mainPanel = new System.Windows.Forms.Panel();

            this.leftSidebarPanel.SuspendLayout();
            this.leftSidebarContentPanel.SuspendLayout();
            this.logoPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();

            // leftSidebarPanel
            this.leftSidebarPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.leftSidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftSidebarPanel.Width = 180;

            // leftSidebarContentPanel
            this.leftSidebarContentPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.leftSidebarContentPanel.Width = 180;
            this.leftSidebarContentPanel.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.leftSidebarContentPanel.AutoSize = true;
            this.leftSidebarContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            // logoPanel
            this.logoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.logoPanel.Height = 100;

            // picLogo
            this.picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            this.logoPanel.Controls.Add(this.picLogo);

            // buttonsPanel
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonsPanel.Width = 180;
            this.buttonsPanel.AutoSize = true;
            this.buttonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            // 버튼 공통 속성 설정
            void SetupButton(System.Windows.Forms.Button btn, string text, System.EventHandler handler)
            {
                btn.Dock = System.Windows.Forms.DockStyle.Top;
                btn.Height = 60;
                btn.Text = text;
                btn.ForeColor = System.Drawing.Color.White;
                btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += handler;
            }

            SetupButton(this.btnAdmin, "🛠 관리자", this.btnAdmin_Click);
            SetupButton(this.btnReport, "📑 보고서&&맥비", this.btnReport_Click);
            SetupButton(this.btnSideNotice, "🚨 사이드 공지", this.btnSideNotice_Click);
            SetupButton(this.btnDoubleCal, "⚖︎ 묻더 계산기", this.btnDoubleCal_Click);
            SetupButton(this.btnCalculator, "⚖︎ 법률 계산기", this.btnCalculator_Click);
            SetupButton(this.btnMypage, "👤 마이페이지", this.btnMypage_Click);
            SetupButton(this.btnLogout, "↩⏻ 로그아웃", this.BtnLogout_Click);

            // 버튼 역순 추가
            this.buttonsPanel.Controls.Add(this.btnAdmin);
            this.buttonsPanel.Controls.Add(this.btnReport);
            this.buttonsPanel.Controls.Add(this.btnSideNotice);
            this.buttonsPanel.Controls.Add(this.btnDoubleCal);
            this.buttonsPanel.Controls.Add(this.btnCalculator);
            this.buttonsPanel.Controls.Add(this.btnMypage);

            // leftSidebarContentPanel에 logoPanel과 buttonsPanel 추가
            this.leftSidebarContentPanel.Controls.Add(this.buttonsPanel);
            this.leftSidebarContentPanel.Controls.Add(this.logoPanel);

            // leftSidebarPanel에 leftSidebarContentPanel 추가
            this.leftSidebarPanel.Controls.Add(this.leftSidebarContentPanel);

            // logout 버튼 위치 조정
            this.btnLogout.Height = 60;
            this.btnLogout.Width = 180;
            this.btnLogout.Text = "⏻ 로그아웃";
            this.btnLogout.ForeColor = Color.White;
            this.btnLogout.BackColor = Color.FromArgb(30, 30, 30);
            this.btnLogout.FlatStyle = FlatStyle.Flat;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.Dock = DockStyle.None;
            this.btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnLogout.Location = new System.Drawing.Point(0, leftSidebarPanel.Height - 170);
            this.leftSidebarPanel.Controls.Add(this.btnLogout);

            // mainPanel
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            // Main Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 800);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.leftSidebarPanel);
            this.Name = "Main";
            this.Text = "치즈 경찰청 인트라넷";

            this.leftSidebarPanel.ResumeLayout(false);
            this.leftSidebarContentPanel.ResumeLayout(false);
            this.leftSidebarContentPanel.PerformLayout();
            this.logoPanel.ResumeLayout(false);
            this.buttonsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);

            // 버튼 초기 표시 상태
            btnMypage.Visible = true;
            btnDoubleCal.Visible = true;
            btnReport.Visible = true;
            btnAdmin.Visible = true;
            btnLogout.Visible = true;
        }

        #endregion
    }
}
