
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
                Admin.ForceCheckoutEventBus.OnForceCheckout -= HandleForceCheckout;

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

        private void HandleForceCheckout(int obj)
        {
            throw new NotImplementedException();
        }


        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MypageControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            Name = "MypageControl";
            Size = new Size(1487, 756);
            ResumeLayout(false);
            // UserControl은 ClientSize와 Text 속성을 사용하지 않음
        }

        #endregion
    }
}
