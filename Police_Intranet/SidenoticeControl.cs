using Police_Intranet;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class SideNoticeControl : UserControl
    {
        private void InitializeComponent()
        {

        }

        public SideNoticeControl()
        {
            InitializeComponent();
            SetupControls();

            // DB를 사용해서 초기화 등
            this.Padding = new Padding(0, 60, 0, 0); // 전체 UI를 아래로 60px 내림
        }

        private readonly (string Text, string Message)[][] categoriesData = new (string, string)[][]
        {
            new (string, string)[] { ("출석요구", "까지 경찰청으로 출석바랍니다."), ("수배", "까지 수배RP를 진행합니다. 시민 여러분들께서는 협조 부탁드립니다.") },
            new (string, string)[] { ("운전 면접", "/팩션공지 치즈 경찰청에서 안내 드립니다. 운전 시험 진행 중으로 사이렌 소리가 들리면 시민 여러분들은 갓길로 피양하여 주시길 부탁 드립니다.") },
            new (string, string)[] { ("이륙 공지", "/팩션공지 치즈 경찰청에서 안내 드립니다. 경찰청 헬기 이륙합니다."), ("항공 순찰", "/팩션공지 경찰청에서 안내 드립니다. 헬기 순찰을 위해 경찰청 헬기 이륙합니다."), ("면접&&시험", "/팩션공지 치즈 경찰청에서 안내 드립니다. 헬기 면접&시험 진행을 위해 경찰청 헬기 이륙합니다."), ("헬기 연습", "/팩션공지 경찰청에서 안내 드립니다. 경찰청 헬기 이륙합니다.") },
            new (string, string)[] {
                ("206", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 206번지 ATM ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("333", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 333번지 편의점 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("449", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 449번지 편의점 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("574", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 574번지 편의점 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("575", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 575번지 남부은행 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("584 ", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 584번지 ATM ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("692", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 692번지 빈집 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("697", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 697번지 보석상 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("경털1차", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 남부 경찰청 서버실 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
                ("경털2차", "/팩션공지 치즈 경찰청에서 안내 드립니다. 현 시간부로 [ 217번지 경찰서 ]에 강도가 침입했습니다. 주위 한 블럭 이내로 접근시 공범으로 간주되어 사살 될 수 있습니다. 시민 여러분들의 협조를 부탁 드립니다."),
            }
        };

        private readonly string[] categoriesNames = new string[]
        {
            "출석 && 수배",
            "면접 공지",
            "헬기 공지",
            "RP 공지"
        };

        private Panel mainPanel;
        private FlowLayoutPanel[] flpCategories;

        private TextBox txtId, txtName, txtHour, txtMinute;
        private TextBox txtSelectedItems;
        private Button btnClear;
        private Button btnCopy;

        private void SetupControls()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Dock = DockStyle.Fill;

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.FromArgb(30, 30, 30),
                Location = new Point(0, 500)
            };
            this.Controls.Add(mainPanel);

            int yOffset = 20;
            int buttonWidth = 120;
            int buttonHeight = 40;
            int buttonMargin = 4;
            int buttonsPerRow = 5;

            // 입력창 영역
            Label lblId = new Label
            {
                Text = "고유번호 :",
                ForeColor = Color.White,
                Location = new Point(10, yOffset + 3),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblId);

            txtId = new TextBox
            {
                Location = new Point(80, yOffset),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            mainPanel.Controls.Add(txtId);

            Label lblName = new Label
            {
                Text = "닉네임 :",
                ForeColor = Color.White,
                Location = new Point(190, yOffset + 3),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblName);

            txtName = new TextBox
            {
                Location = new Point(250, yOffset),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            mainPanel.Controls.Add(txtName);

            Label lblTime = new Label
            {
                Text = "현재시각(시) :",
                ForeColor = Color.White,
                Location = new Point(370, yOffset + 3),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblTime);

            txtHour = new TextBox
            {
                Location = new Point(460, yOffset),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            mainPanel.Controls.Add(txtHour);

            Label lblColon = new Label
            {
                Text = "현재시각(분) :",
                ForeColor = Color.White,
                Location = new Point(570, yOffset + 3),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblColon);

            txtMinute = new TextBox
            {
                Location = new Point(660, yOffset),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            mainPanel.Controls.Add(txtMinute);

            yOffset += 40;

            // 버튼 카테고리
            flpCategories = new FlowLayoutPanel[categoriesNames.Length];

            for (int i = 0; i < categoriesNames.Length; i++)
            {
                Label lblCategory = new Label
                {
                    Text = categoriesNames[i],
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(10, yOffset)
                };
                mainPanel.Controls.Add(lblCategory);
                yOffset += lblCategory.Height + 8;

                FlowLayoutPanel flpButtons = new FlowLayoutPanel
                {
                    Location = new Point(10, yOffset),
                    Size = new Size((buttonWidth + buttonMargin * 2) * buttonsPerRow,
                        (buttonHeight + buttonMargin * 2) * (int)Math.Ceiling((double)categoriesData[i].Length / buttonsPerRow)),
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = false,
                    Padding = new Padding(0),
                    Margin = new Padding(0),
                    BackColor = Color.Transparent,
                };
                mainPanel.Controls.Add(flpButtons);
                flpCategories[i] = flpButtons;

                foreach (var (text, message) in categoriesData[i])
                {
                    Button btn = new Button
                    {
                        Text = text,
                        Tag = message, // 버튼 메시지 저장
                        Size = new Size(buttonWidth, buttonHeight),
                        Margin = new Padding(buttonMargin),
                        BackColor = Color.FromArgb(60, 60, 60),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                        Cursor = Cursors.Hand
                    };
                    btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                    btn.Click += CategoryButton_Click;
                    flpButtons.Controls.Add(btn);
                }

                yOffset += flpButtons.Height + 20;
            }

            // 오른쪽 출력 패널
            Panel rightPanel = new Panel
            {
                Location = new Point(mainPanel.Width - 250, 10),
                BackColor = Color.FromArgb(30, 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(300, 600),
            };
            this.Controls.Add(rightPanel);
            rightPanel.BringToFront();

            Label lblSelectedTitle = new Label
            {
                Text = "사이드 공지",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblSelectedTitle);

            txtSelectedItems = new TextBox
            {
                Location = new Point(10, 40),
                Size = new Size(280, 200),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                ScrollBars = ScrollBars.None,
            };
            rightPanel.Controls.Add(txtSelectedItems);

            btnCopy = new Button
            {
                Text = "복사하기",
                Size = new Size(280, 30),
                Location = new Point(10, 275),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            rightPanel.Controls.Add(btnCopy);
            btnCopy.Click += btnCopy_Click;

            btnClear = new Button
            {
                Text = "초기화",
                Size = new Size(280, 30),
                Location = new Point(10, 315),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnClear.Click += (s, e) =>
            {
                txtSelectedItems.Clear();
                txtId.Clear();
                txtName.Clear();
                txtHour.Clear();
                txtMinute.Clear();

                foreach (Control ctrl in mainPanel.Controls)
                {
                    if (ctrl is FlowLayoutPanel flp)
                    {
                        foreach (Button btn in flp.Controls.OfType<Button>())
                        {
                            btn.BackColor = Color.FromArgb(60, 60, 60);
                        }
                    }
                }
            };
            rightPanel.Controls.Add(btnClear);

            this.Resize += (s, e) =>
            {
                rightPanel.Location = new Point(this.Width - rightPanel.Width - 30, (this.Height - rightPanel.Height) / 2);
                mainPanel.Width = this.Width - rightPanel.Width - 40;
            };
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                string finalText = "";

                // 입력값 조합 (id, name, 시간)
                string id = txtId.Text.Trim();
                string name = txtName.Text.Trim();
                string hourText = txtHour.Text.Trim();
                string minuteText = txtMinute.Text.Trim();

                int hour = 0;
                int minute = 0;
                bool validTime = int.TryParse(hourText, out hour) && int.TryParse(minuteText, out minute);

                if (!validTime)
                {
                    hour = 0;
                    minute = 0;
                }

                // 출석요구는 +5분, 수배는 +30분 시간 계산
                int addMinutes = 0;
                if (btn.Text == "출석요구") addMinutes = 5;
                else if (btn.Text == "수배") addMinutes = 30;

                minute += addMinutes;
                if (minute >= 60)
                {
                    hour += minute / 60;
                    minute %= 60;
                }
                hour %= 24;

                string timePart = validTime ? $"{hour:D2}:{minute:D2}" : "";

                string infoPart = $"[{id} / {name}]";
                if (!string.IsNullOrWhiteSpace(timePart))
                    infoPart += $" / [{timePart}]";

                if (btn.Text == "출석요구" || btn.Text == "수배")
                {
                    // 메시지 텍스트에서 앞/뒤 문장 분리
                    string message = btn.Tag.ToString();

                    finalText = $"/팩션공지 치즈 경찰청에서 안내 드립니다. {infoPart} {message}";
                }
                else
                {
                    // 나머지 버튼들은 저장된 메시지만 그대로
                    finalText = btn.Tag.ToString();
                }

                // 버튼 색상 초기화
                foreach (var flp in flpCategories)
                {
                    foreach (Button b in flp.Controls.OfType<Button>())
                    {
                        b.BackColor = Color.FromArgb(60, 60, 60);
                    }
                }

                btn.BackColor = Color.FromArgb(100, 149, 237);
                txtSelectedItems.Text = finalText;
            }
        }


        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSelectedItems.Text))
            {
                MessageBox.Show("사이드 공지를 선택하세요.", "복사 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Clipboard.SetText(txtSelectedItems.Text);

            MessageBox.Show("사이드 공지가 복사되었습니다.", "복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
