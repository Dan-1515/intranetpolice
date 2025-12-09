namespace Police_Intranet
{
    partial class CalculatorControl
    {
        /// <summary>필수 디자이너 변수입니다.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>사용 중인 모든 리소스를 정리합니다.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CalculatorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Name = "CalculatorControl";
            this.Size = new System.Drawing.Size(900, 600);
            this.ResumeLayout(false);
        }
        #endregion
    }
}
