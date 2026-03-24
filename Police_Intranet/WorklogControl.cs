using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class WorklogControl : UserControl
    {
        private TextBox txtLogs;
        private Button btnCalculate;
        private Button btnClear;

        public WorklogControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(30, 30, 30);

            txtLogs = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 11),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 45),
                BorderStyle = BorderStyle.FixedSingle,

                Dock = DockStyle.None,      // ❌ Dock 해제
                Width = 350,                // ⬅ 너비 줄이기
                Height = 400,               // 기존보다 살짝 줄임
                Location = new Point(100, 100)};

            btnCalculate = new Button
            {
                Text = "근무시간 계산",
                Width = 130,                // ⬅ 버튼 너비 줄임
                Height = 40,
                BackColor = Color.FromArgb(100, 140, 240),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),

                Dock = DockStyle.None,      // ❌ Dock 해제
                Location = new Point(120, txtLogs.Bottom + 12)
            };

            btnCalculate.Click += BtnCalculate_Click;

            btnClear = new Button
            {
                Text = "로그 초기화",
                Width = 130,
                Height = 40,
                BackColor = Color.FromArgb(160, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.None,
                Location = new Point(btnCalculate.Right + 20, txtLogs.Bottom + 12)
            };
            btnClear.Click += BtnClear_Click;

            Controls.Add(btnCalculate);
            Controls.Add(btnClear);
            Controls.Add(txtLogs);
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogs.Text))
            {
                MessageBox.Show("출근/퇴근 로그를 넣어 주세요.");
                return;
            }

            CalculateAndShow(txtLogs.Text);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "로그를 모두 삭제하시겠습니까?",
                "확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                txtLogs.Clear();
            }

        }

        // =========================
        // 핵심 계산 로직
        // =========================
        private void CalculateAndShow(string raw)
        {
            var events = ParseLogs(raw);

            if (events.Count == 0)
            {
                MessageBox.Show("유효한 출근/퇴근 로그를 찾을 수 없습니다.");
                return;
            }

            Dictionary<int, DateTime> active = new();
            TimeSpan total = TimeSpan.Zero;

            foreach (var e in events.OrderBy(e => e.Time))
            {
                if (e.IsCheckIn)
                {
                    if (!active.ContainsKey(e.UserId))
                        active[e.UserId] = e.Time;
                }
                else
                {
                    if (active.TryGetValue(e.UserId, out var start))
                    {
                        total += (e.Time - start);
                        active.Remove(e.UserId);
                    }
                }
            }

            if (active.Count > 0)
            {
                MessageBox.Show(
                    "퇴근 로그가 없는 출근 기록이 있습니다.\n해당 시간은 계산에서 제외되었습니다.",
                    "주의",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            int hours = (int)total.TotalHours;
            int minutes = total.Minutes;
            int seconds = total.Seconds;

            MessageBox.Show(
                $"총 근무시간: {hours}시간 {minutes}분 {seconds}초",
                "근무시간 계산 결과",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // =========================
        // 로그 파싱
        // =========================
        private List<WorkEvent> ParseLogs(string raw)
        {
            var lines = raw.Split('\n');
            var result = new List<WorkEvent>();

            bool? isCheckIn = null;
            int? userId = null;

            foreach (var line in lines)
            {
                if (line.Contains("출근")) isCheckIn = true;
                else if (line.Contains("퇴근")) isCheckIn = false;

                // 👮단 (17417)
                var userMatch = Regex.Match(line, @"\((\d+)\)");
                if (userMatch.Success)
                {
                    userId = int.Parse(userMatch.Groups[1].Value);
                }

                // 2026-02-27 01:33:27
                if (DateTime.TryParse(line.Trim(), out DateTime time))
                {
                    if (isCheckIn != null && userId != null)
                    {
                        result.Add(new WorkEvent
                        {
                            UserId = userId.Value,
                            Time = time,
                            IsCheckIn = isCheckIn.Value
                        });

                        // 초기화
                        isCheckIn = null;
                        userId = null;
                    }
                }
            }

            return result;
        }

        private class WorkEvent
        {
            public int UserId;
            public DateTime Time;
            public bool IsCheckIn;
        }
    }
}