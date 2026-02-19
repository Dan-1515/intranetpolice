namespace Police_Intranet
{
    partial class MypageControl
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
            if (disposing)
            {
                // 🔥 이벤트 해제 (가장 중요)
                ForceCheckoutEventBus.OnForceCheckout -= HandleForceCheckout;

                // 🔥 타이머 정리
                if (workTimer != null)
                {
                    workTimer.Stop();
                    workTimer.Dispose();
                    workTimer = null;
                }

                // 디자이너 컴포넌트
                components?.Dispose();
            }
            base.Dispose(disposing);
        }


        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            // UserControl은 ClientSize와 Text 속성을 사용하지 않음
        }

        #endregion
    }
}
