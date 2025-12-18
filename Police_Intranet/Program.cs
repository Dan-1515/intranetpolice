using System;
using System.Linq;
using System.Windows.Forms;
using Police_Intranet.Models;
using Police_Intranet.Properties;
using Supabase;

namespace Police_Intranet
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ===============================
            // Supabase 초기화 (단 한 번)
            // ===============================
            Client client;
            try
            {
                SupabaseClient.Initialize();
                client = SupabaseClient.Instance;

                if (client == null)
                {
                    MessageBox.Show(
                        "Supabase 클라이언트 초기화 실패",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Supabase 초기화 중 오류: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // ===============================
            // 로그인 처리
            // ===============================
            User loggedInUser = null;

            // 🔹 자동 로그인 시도
            if (Settings.Default.AutoLogin &&
                !string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
            {
                try
                {
                    var result = client.From<User>()
                                       .Where(u => u.Username == Settings.Default.SavedUsername)
                                       .Get()
                                       .GetAwaiter()
                                       .GetResult();

                    var user = result?.Models?.FirstOrDefault();

                    // 승인된 계정만 자동 로그인 허용
                    if (user != null && user.IsApproved == true)
                    {
                        loggedInUser = user;
                    }
                }
                catch
                {
                    loggedInUser = null;
                }
            }

            // 🔹 자동 로그인 실패 → 로그인 폼 표시
            if (loggedInUser == null)
            {
                using (var loginForm = new Login())
                {
                    var result = loginForm.ShowDialog();

                    // 로그인창 닫힘(X) → 프로그램 종료
                    if (result != DialogResult.OK || loginForm.LoggedInUser == null)
                        return;

                    loggedInUser = loginForm.LoggedInUser;
                }
            }

            // ===============================
            // 메인 폼 실행
            // ===============================
            Application.Run(new Main(loggedInUser, client));
        }
    }
}
