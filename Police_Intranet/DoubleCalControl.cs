using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class DoubleCalControl : UserControl
    {

        private const int NormalRemainMinutes = 10;
        private const int PeakRemainMinutes = 5;
        private const int BailPerMinute = 500_000;

        private readonly string[][] categoriesTexts =
        {
            new string[] { "ATM", "편의점", "빈집" },
            new string[] { "묻더 승", "묻더 패", "묻묻더 승", "묻묻더 패", "묻묻묻더 승", "묻묻묻더 패" }
        };

        private readonly string[] categoriesNames =
        {
            "스토리 RP",
            "묻더 선택"
        };

        private Panel mainPanel;
        private TextBox txtFine;
        private TextBox txtDetention;
        private TextBox txtBailFine;
        private TextBox txtBailDetention;
        private Button btnClear;
        private CheckBox chkPeak;

        // ===============================
        // 벌금 구조
        // ===============================
        private record Penalty(int Fine, int Detention);

        // ===============================
        // 벌금 테이블 (최종 기준 적용)
        // ===============================
        private readonly Dictionary<(string story, string mutder), Penalty> penaltyTable
            = new()
        {
            // ===== ATM =====
            { ("ATM", "묻더 승"),     new Penalty(200_000_000, 20) },
            { ("ATM", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("ATM", "묻묻더 승"),   new Penalty(300_000_000, 30) },
            { ("ATM", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("ATM", "묻묻묻더 승"), new Penalty(400_000_000, 40) },
            { ("ATM", "묻묻묻더 패"), new Penalty(300_000_000, 0) },

            // ===== 편의점 =====
            { ("편의점", "묻더 승"),     new Penalty(200_000_000, 30) },
            { ("편의점", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("편의점", "묻묻더 승"),   new Penalty(300_000_000, 45) },
            { ("편의점", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("편의점", "묻묻묻더 승"), new Penalty(400_000_000, 60) },
            { ("편의점", "묻묻묻더 패"), new Penalty(300_000_000, 0) },

            // ===== 빈집 =====
            { ("빈집", "묻더 승"),     new Penalty(200_000_000, 30) },
            { ("빈집", "묻더 패"),     new Penalty(100_000_000, 0) },
            { ("빈집", "묻묻더 승"),   new Penalty(300_000_000, 45) },
            { ("빈집", "묻묻더 패"),   new Penalty(200_000_000, 0) },
            { ("빈집", "묻묻묻더 승"), new Penalty(400_000_000, 60) },
            { ("빈집", "묻묻묻더 패"), new Penalty(300_000_000, 0) },
        };

        public DoubleCalControl()
        {
            InitializeComponent();
            SetupControls();
        }

        // ===============================
        // UI 구성 (변경 없음)
        // ===============================
        private void SetupControls()
        {
            BackColor = Color.FromArgb(30, 30, 30);
            Dock = DockStyle.Fill;

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                AutoScroll = true
            };
            Controls.Add(mainPanel);

            int yOffset = 150;
            int buttonWidth = 130;
            int buttonHeight = 40;
            int buttonMargin = 4;
            int buttonsPerRow = 5;

            for (int catIndex = 0; catIndex < categoriesNames.Length; catIndex++)
            {
                Label lblCategory = new Label
                {
                    Text = categoriesNames[catIndex],
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, yOffset),
                    AutoSize = true
                };
                mainPanel.Controls.Add(lblCategory);
                yOffset += lblCategory.Height + 5;

                FlowLayoutPanel flp = new FlowLayoutPanel
                {
                    Location = new Point(10, yOffset),
                    Size = new Size(
                        (buttonWidth + buttonMargin * 2) * buttonsPerRow,
                        (buttonHeight + buttonMargin * 2) *
                        (int)Math.Ceiling((double)categoriesTexts[catIndex].Length / buttonsPerRow)
                    ),
                    WrapContents = true,
                    BackColor = Color.Transparent
                };
                mainPanel.Controls.Add(flp);

                foreach (string text in categoriesTexts[catIndex])
                {
                    Button btn = new Button
                    {
                        Text = text,
                        Size = new Size(buttonWidth, buttonHeight),
                        Margin = new Padding(buttonMargin),
                        BackColor = Color.FromArgb(60, 60, 60),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 8.5F),
                        Cursor = Cursors.Hand,
                        Tag = false
                    };
                    btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                    btn.Click += CrimeButton_Click;
                    flp.Controls.Add(btn);
                }

                yOffset += flp.Height + 20;
            }

            Panel rightPanel = new Panel
            {
                Size = new Size(300, 450),
                BackColor = Color.FromArgb(30, 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Controls.Add(rightPanel);
            rightPanel.BringToFront();

            int startY = 140;

            rightPanel.Controls.Add(CreateLabel("벌금", 10, startY));
            txtFine = CreateTextBox(10, startY + 20);
            rightPanel.Controls.Add(txtFine);

            rightPanel.Controls.Add(CreateLabel("구금", 10, startY + 55));
            txtDetention = CreateTextBox(10, startY + 75);
            rightPanel.Controls.Add(txtDetention);

            rightPanel.Controls.Add(CreateLabel("벌금 (보석금 포함)", 10, startY + 110));
            txtBailFine = CreateTextBox(10, startY + 130);
            rightPanel.Controls.Add(txtBailFine);

            rightPanel.Controls.Add(CreateLabel("구금 (보석금 포함)", 10, startY + 165));
            txtBailDetention = CreateTextBox(10, startY + 185);
            rightPanel.Controls.Add(txtBailDetention);

            chkPeak = new CheckBox
            {
                Text = "피크 타임",
                ForeColor = Color.White,
                Location = new Point(10, startY + 215),
                AutoSize = true
            };
            chkPeak.CheckedChanged += (s, e) => UpdateFineAndDetention();
            rightPanel.Controls.Add(chkPeak);

            btnClear = new Button
            {
                Text = "초기화",
                Size = new Size(280, 30),
                Location = new Point(10, startY + 245),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnClear.Click += ClearAll;
            rightPanel.Controls.Add(btnClear);

            

            Resize += (s, e) =>
            {
                rightPanel.Location = new Point(Width - rightPanel.Width - 30, 30);
                mainPanel.Width = Width - rightPanel.Width - 40;
            };

        }

        private Label CreateLabel(string text, int x, int y) =>
            new Label { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", 10F), Location = new Point(x, y), AutoSize = true };

        private TextBox CreateTextBox(int x, int y) =>
            new TextBox { Location = new Point(x, y), Size = new Size(280, 25), ReadOnly = true, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };

        private void CrimeButton_Click(object sender, EventArgs e)
        {
            Button clickedBtn = sender as Button;
            FlowLayoutPanel parentPanel = clickedBtn.Parent as FlowLayoutPanel;

            foreach (Button btn in parentPanel.Controls.OfType<Button>())
            {
                btn.Tag = false;
                btn.BackColor = Color.FromArgb(60, 60, 60);
            }

            clickedBtn.Tag = true;
            clickedBtn.BackColor = Color.FromArgb(90, 120, 200);

            UpdateFineAndDetention();
        }

        private string GetSelectedText(int categoryIndex)
        {
            var flp = mainPanel.Controls.OfType<FlowLayoutPanel>().ElementAt(categoryIndex);
            return flp.Controls.OfType<Button>().FirstOrDefault(b => (bool)b.Tag)?.Text;
        }

        private void UpdateFineAndDetention()
        {
            string story = GetSelectedText(0);
            string mutder = GetSelectedText(1);

            if (story == null || mutder == null)
            {
                txtFine.Text = txtDetention.Text = txtBailFine.Text = txtBailDetention.Text = "-";
                return;
            }

            if (!penaltyTable.TryGetValue((story, mutder), out Penalty p))
            {
                txtFine.Text = txtDetention.Text = txtBailFine.Text = txtBailDetention.Text = "미정";
                return;
            }

            txtFine.Text = $"{p.Fine:N0}원";
            txtDetention.Text = $"{p.Detention}분";

            // === 보석금 계산 ===
            int remainMinutes = chkPeak.Checked ? PeakRemainMinutes : NormalRemainMinutes;

            if (p.Detention > remainMinutes)
            {
                int reducedMinutes = p.Detention - remainMinutes;
                int bailExtra = reducedMinutes * BailPerMinute;

                txtBailFine.Text = $"{(p.Fine + bailExtra):N0}원";
                txtBailDetention.Text = $"{remainMinutes}분";
            }
            else
            {
                txtBailFine.Text = "-";
                txtBailDetention.Text = "-";
            }

        }

        private void ClearAll(object sender, EventArgs e)
        {
            foreach (var flp in mainPanel.Controls.OfType<FlowLayoutPanel>())
            {
                foreach (Button btn in flp.Controls.OfType<Button>())
                {
                    btn.Tag = false;
                    btn.BackColor = Color.FromArgb(60, 60, 60);
                }
            }

            UpdateFineAndDetention();
        }
    }
}
