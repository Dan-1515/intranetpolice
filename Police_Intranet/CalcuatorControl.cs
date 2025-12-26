using Police_Intranet;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Police_Intranet
{
    public partial class CalculatorControl : UserControl
    {
        private readonly string[][] categoriesTexts = new string[][]
        {
            new string[] { "소음공해", "불법 주정차", "속도위반", "속도위반(고속도로)", "신호위반", "불법유턴", "역주행/차선위반", "인도 주행", "공공기물 파손", "스턴트", "차량파손", "뺑소니", "난폭운전", "폭주", "도주", "수배", "항공기 저공 비행", "보복운전", "미허가 항공기 운행" },
            new string[] { "명예훼손", "폭행", "영업 방해", "차량절도", "불법 물건 소지", "증거인멸", "불법 총기 소지", "불법 무기/물건 거래", "시민 살인" },
            new string[] { "공무원 차량 절도 시도", "공무원 명예훼손", "공무원 차량 파손", "공무원 사칭", "허위신고/공무집행방해/경관지시불이행", "국유지/사유지 침입", "공무원 폭행", "공무원 차량 절도", "공무원 살인" },
        };

        private readonly string[] categoriesNames = new string[]
        {
            "도로교통법",
            "형사 중범죄",
            "공무원법"
        };
        // private DB db;
        private Panel mainPanel;

        private FlowLayoutPanel flpButtonsPanel;

        private TextBox txtSelectedCrimes;
        private TextBox txtFine;
        private TextBox txtDetention;
        private TextBox txtBailFine;
        private TextBox txtBailDetention;

        private Button btnCopy;
        private Button btnClear;

        private CheckBox chkNewbie;

        // 선택된 죄목을 저장하는 HashSet (중복 방지 및 빠른 토글)
        // private System.Collections.Generic.HashSet<string> selectedCrimes = new System.Collections.Generic.HashSet<string>();
        private List<string> selectedCrimes = new List<string>();

        public CalculatorControl()
        {
            // InitializeComponent();
            SetupControls();
            // db = dbInstance;
        }

        private void SetupControls()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Dock = DockStyle.Fill;

            int panelWidth = 240;
            int panelHeight = 500;

            int xPos = this.Width - panelWidth - 30; // 오른쪽에서 30px 여백
            int yPos = (this.Height - panelHeight) / 2; // 세로 중앙 위치


            // mainPanel (전체 스크롤 가능 영역)
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,

                BackColor = Color.FromArgb(30, 30, 30),
            };
            this.Controls.Add(mainPanel);

            int yOffset = 10;

            int buttonWidth = 130;
            int buttonHeight = 40;
            int buttonMargin = 4;
            int buttonsPerRow = 6;

            // 카테고리별 버튼 영역 생성
            for (int catIndex = 0; catIndex < categoriesNames.Length; catIndex++)
            {
                Label lblCategory = new Label
                {
                    Text = categoriesNames[catIndex],
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
                    Size = new Size((buttonWidth + buttonMargin * 2) * buttonsPerRow, (buttonHeight + buttonMargin * 2) * (int)Math.Ceiling((double)categoriesTexts[catIndex].Length / buttonsPerRow)),
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = false,
                    Padding = new Padding(0),
                    Margin = new Padding(0),
                    BackColor = Color.Transparent,
                };
                mainPanel.Controls.Add(flpButtons);

                string[] btnTexts = categoriesTexts[catIndex];

                for (int i = 0; i < btnTexts.Length; i++)
                {
                    Button btn = new Button
                    {
                        Text = btnTexts[i],
                        Size = new Size(buttonWidth, buttonHeight),
                        Margin = new Padding(buttonMargin),
                        BackColor = Color.FromArgb(60, 60, 60),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                        Cursor = Cursors.Hand,
                        Tag = false // 선택 여부 저장용 (bool)
                    };
                    btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);

                    btn.Click += CrimeButton_Click;

                    flpButtons.Controls.Add(btn);
                }

                yOffset += flpButtons.Height + 25;
            }

            // 오른쪽에 선택된 죄목, 벌금, 구금 표시 영역을 위해 패널 생성
            Panel rightPanel = new Panel
            {
                Location = new Point(mainPanel.Width - 250, 10),
                BackColor = Color.FromArgb(30, 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(300, 600),
            };

            this.Controls.Add(rightPanel);
            rightPanel.BringToFront();

            // 선택한 죄목 라벨
            Label lblSelectedTitle = new Label
            {
                Text = "선택한 죄목",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblSelectedTitle);

            // 선택한 죄목 출력 텍스트박스
            txtSelectedCrimes = new TextBox
            {
                Location = new Point(10, 40),
                Size = new Size(280, 200),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                ScrollBars = ScrollBars.None
            };
            rightPanel.Controls.Add(txtSelectedCrimes);

            // 뉴비 체크박스
            chkNewbie = new CheckBox
            {
                Text = "뉴비",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(lblSelectedTitle.Width + 145, 15),
                AutoSize = true,
                Checked = false,
                BackColor = Color.Transparent
            };
            rightPanel.Controls.Add(chkNewbie);
            chkNewbie.CheckedChanged += (s, e) =>
            {
                UpdateFineAndDetention(); // 상태가 바뀌면 다시 계산
                UpdateSelectedCrimesDisplay();
            };

            // 벌금 라벨
            Label lblFine = new Label
            {
                Text = "벌금",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(10, 255),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblFine);

            // 벌금 텍스트박스
            txtFine = new TextBox
            {
                Location = new Point(10, 275),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            rightPanel.Controls.Add(txtFine);

            // 구금 라벨
            Label lblDetention = new Label
            {
                Text = "구금",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(10, 305),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblDetention);

            // 구금 텍스트박스
            txtDetention = new TextBox
            {
                Location = new Point(10, 325),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            rightPanel.Controls.Add(txtDetention);

            // 벌금(보석금포함) 라벨
            Label lblBailFine = new Label
            {
                Text = "벌금 (보석금 포함)",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(10, 355),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblBailFine);

            // 벌금(보석금포함) 텍스트박스
            txtBailFine = new TextBox
            {
                Location = new Point(10, 375),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            rightPanel.Controls.Add(txtBailFine);

            // 구금(보석금포함) 라벨
            Label lblBailDetention = new Label
            {
                Text = "구금 (보석금 포함)",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(10, 405),
                AutoSize = true
            };
            rightPanel.Controls.Add(lblBailDetention);

            // 구금(보석금포함) 텍스트박스
            txtBailDetention = new TextBox
            {
                Location = new Point(10, 425),
                Size = new Size(280, 25),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            rightPanel.Controls.Add(txtBailDetention);

            // 복사 버튼
            btnCopy = new Button
            {
                Text = "죄목 복사하기",
                Size = new Size(280, 30),
                Location = new Point(10, 465),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCopy.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnCopy.Click += BtnCopy_Click;
            rightPanel.Controls.Add(btnCopy);

            // 초기화 버튼
            btnClear = new Button
            {
                Text = "초기화",
                Size = new Size(280, 30),
                Location = new Point(10, 505),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnClear.Click += (s, e) =>
            {
                chkNewbie.Checked = false;
                // 모든 버튼 초기화
                foreach (Control ctrl in mainPanel.Controls)
                {
                    if (ctrl is FlowLayoutPanel flp)
                    {
                        foreach (Button btn in flp.Controls.OfType<Button>())
                        {
                            btn.BackColor = Color.FromArgb(60, 60, 60);
                            btn.Tag = false; // 선택 해제
                        }
                    }
                }
                selectedCrimes.Clear();
                UpdateSelectedCrimesDisplay();
                UpdateFineAndDetention();

            };
            rightPanel.Controls.Add(btnClear);

            // 폼 크기 및 위치 조절 시 오른쪽 패널이 계속 붙도록 리사이즈 이벤트 설정
            this.Resize += (s, e) =>
            {
                rightPanel.Location = new Point(this.Width - rightPanel.Width - 30, (this.Height - rightPanel.Height) / 2);
                mainPanel.Width = this.Width - rightPanel.Width - 40;
            };
        }

        private void CrimeButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                bool isSelected = btn.Tag is bool b && b;
                string crimeName = btn.Text;

                if (!isSelected)
                {
                    // 선택
                    btn.BackColor = Color.FromArgb(100, 140, 240);

                    // 클릭한 순서대로 저장
                    if (!selectedCrimes.Contains(crimeName))
                        selectedCrimes.Add(crimeName);

                    btn.Tag = true;
                }
                else
                {
                    // 선택 해제
                    btn.BackColor = Color.FromArgb(60, 60, 60);
                    selectedCrimes.Remove(crimeName);
                    btn.Tag = false;
                }

                UpdateSelectedCrimesDisplay();
                UpdateFineAndDetention();
            }
        }

        private void UpdateSelectedCrimesDisplay()
        {
            string result = string.Join(", ", selectedCrimes);

            if (chkNewbie.Checked && selectedCrimes.Count > 0)
                result += " (뉴비 감면)";

            txtSelectedCrimes.Text = result;
        }

        private void UpdateFineAndDetention()
        {
            // 벌금, 구금 계산
            long totalFine = 0;
            int totalDetention = 0;
            long totalBailFine = 0;
            int totalBailDetention = 10;

            foreach (var crime in selectedCrimes)
            {
                switch (crime)
                {
                    case "소음공해": totalFine += 5_000_000; break;
                    case "불법 주정차": totalFine += 5_000_000; break;
                    case "속도위반": totalFine += 5_000_000; break;
                    case "속도위반(고속도로)": totalFine += 10_000_000; break;
                    case "신호위반": totalFine += 5_000_000; break;
                    case "불법유턴": totalFine += 5_000_000; break;
                    case "역주행/차선위반": totalFine += 5_000_000; break;
                    case "인도 주행": totalFine += 5_000_000; break;
                    case "공공기물 파손": totalFine += 5_000_000; break;
                    case "스턴트": totalFine += 10_000_000; break;
                    case "차량파손": totalFine += 30_000_000; break;
                    case "뺑소니": totalFine += 50_000_000; totalDetention += 20; break;
                    case "난폭운전": totalFine += 30_000_000; totalDetention += 20; break;
                    case "폭주": totalFine += 50_000_000; totalDetention += 10; break;
                    case "도주": totalFine += 50_000_000; totalDetention += 30; break;
                    case "수배": totalFine += 30_000_000; totalDetention += 30; break; //미정
                    case "항공기 저공 비행": totalFine += 300_000_000; totalDetention += 60; break;
                    case "보복운전": totalFine += 100_000_000; totalDetention += 30; break;
                    case "미허가 항공기 운행": totalFine += 200_000_000; totalDetention += 60; break;


                    case "방조/공범죄": break;
                    case "명예훼손": totalFine += 10_000_000; totalDetention += 10; break;
                    case "폭행": totalFine += 50_000_000; totalDetention += 20; break;
                    case "영업 방해": totalFine += 30_000_000; totalDetention += 25; break;
                    case "차량절도": totalFine += 50_000_000; totalDetention += 30; break;
                    case "불법 물건 소지": totalFine += 50_000_000; totalDetention += 20; break;
                    case "증거인멸": totalFine += 100_000_000; totalDetention += 20; break;
                    case "불법 총기 소지": totalFine += 100_000_000; totalDetention += 20; break;
                    case "불법 무기/물건 거래": totalFine += 50_000_000; totalDetention += 20; break;
                    case "시민 살인": totalFine += 200_000_000; totalDetention += 30; break;

                    case "공무원 차량 절도 시도": totalFine += 50_000_000; break;
                    case "공무원 명예훼손": totalFine += 100_000_000; totalDetention += 10; break;
                    case "공무원 차량 파손": totalFine += 50_000_000; totalDetention += 20; break;
                    case "공무원 사칭": totalFine += 30_000_000; totalDetention += 20; break;
                    case "허위신고/공무집행 방해/경관 지시불이행": totalFine += 100_000_000; totalDetention += 30; break;
                    case "국유지/사유지 침입": totalFine += 50_000_000; totalDetention += 10; break;
                    case "공무원 폭행": totalFine += 200_000_000; totalDetention += 30; break;
                    case "공무원 차량 절도": totalFine += 300_000_000; totalDetention += 30; break;
                    case "공무원 살인": totalFine += 300_000_000; totalDetention += 40; break;

                        break;
                }
            }

            if (chkNewbie.Checked)
            {
                totalFine = Math.Max(0, totalFine / 2);
                totalBailFine = Math.Max(0, totalBailFine / 2);

                if (totalDetention > 120)
                {
                    // 원래 합계가 120분 초과 → 120에서 반 계산
                    totalDetention = 120 / 2;
                }
                else
                {
                    // 원래 합계가 120분 이하 → 합계에서 반 계산
                    totalDetention = Math.Max(0, totalDetention / 2);
                }
            }
            else
            {
                if (totalDetention > 120)
                    totalDetention = 120;
            }

            // 벌금은 원화 3자리 콤마 포맷 + "원"
            txtFine.Text = totalFine.ToString("N0") + "원";
            txtBailFine.Text = totalBailFine.ToString("N0") + "원"; // 보석금 포함 벌금

            // 구금 분 단위 표시
            txtDetention.Text = totalDetention.ToString() + "분";
            txtBailDetention.Text = totalBailDetention.ToString() + "분"; // 보석금 포함 구금

        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSelectedCrimes.Text))
            {
                MessageBox.Show("죄목을 선택하세요.", "복사 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Clipboard.SetText(txtSelectedCrimes.Text);

            MessageBox.Show("선택한 죄목이 복사되었습니다.", "복사 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
