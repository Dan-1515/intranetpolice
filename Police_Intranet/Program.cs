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
            try
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
                        throw new Exception("Supabase Client 인스턴스가 null입니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Supabase 초기화 실패\n\n" + ex,
                        "치명적 오류",
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
                        var result = client
                            .From<User>()
                            .Where(u => u.Username == Settings.Default.SavedUsername)
                            .Get()
                            .GetAwaiter()
                            .GetResult();

                        var user = result?.Models?.FirstOrDefault();

                        if (user != null && user.IsApproved == true)
                            loggedInUser = user;
                    }
                    catch
                    {
                        loggedInUser = null;
                    }
                }

                // 🔹 자동 로그인 실패 → 로그인 폼
                if (loggedInUser == null)
                {
                    using (var loginForm = new Login())
                    {
                        var result = loginForm.ShowDialog();

                        if (result != DialogResult.OK)
                            return;

                        if (loginForm.LoggedInUser == null)
                        {
                            MessageBox.Show(
                                "로그인 정보가 전달되지 않았습니다.",
                                "오류",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return;
                        }

                        loggedInUser = loginForm.LoggedInUser;
                    }
                }

                // ===============================
                // 메인 폼 실행
                // ===============================
                Application.Run(new Main(loggedInUser, client));
            }
            catch (Exception ex)
            {
                // 🔥 여기 걸리면 exe 실행 즉시 죽던 원인임
                MessageBox.Show(
                    "프로그램 시작 중 치명적인 오류 발생\n\n" + ex,
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
