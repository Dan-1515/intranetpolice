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
            new string[] { "속도위반", "신호위반", "불법 주정차", "스턴트", "차선위반", "중앙선 침범", "역주행", "불법유턴", "보복운전", "승차 방법 제한", "폭주", "인도주행", "공공기물 파손", "음주운전", "비정상도로 진입", "도로 외 공간 주행", "운전 중 휴대폰 사용" },
            new string[] { "시민 살인", "살인 미수", "명예훼손", "납치", "감금", "시민 폭행", "시민 폭행(2인 이상)", "시민 폭행\n(불법 무기/물건)", "차량/헬기 절도", "차량/헬기 절도 미수", "도난 차량 압류", "재물손괴(일부)", "재물손괴(전체)", "재물손괴 후 도주", "스토킹", "횡령/사기(5억미만)", "횡령/사기(5억이상)", "영업방해", "불법물건 소지", "불법물건 언급", "불법물건 유통", "근접무기 사용", "사유지 무단침입", "증거인멸", "거짓 진술" },
            new string[] { "저공비행", "미허가 헬기 운행" },
            new string[] { "공무원 살인", "공무원 살인 미수", "공무원 명예훼손", "공무원 폭행", "공무원 폭행(2인 이상)", "공무원 폭행\n(불법 무기/물건)", "공무집행 방해", "공무집행 방해(상해,사망)", "공무원 지시 불이행", "공무원 지시 불이행\n(3회 이상)", "수갑 미착용 도주", "차량 재탑승 도주", "공무원 사칭", "공무원 차량/헬기 절도", "공무원 차량/헬기 절도 미수", "공무원 도난 차량 압류", "국유지 침입", "공무원 뇌물 수수", "공무원 재물손괴", "공무원 재물손괴 후 도주" },
        };

        private readonly string[] categoriesNames = new string[]
        {
            "도로교통법",
            "형사 중범죄",
            "항공법",
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
            int buttonsPerRow = 5;

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

                yOffset += lblCategory.Height;

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
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                        Cursor = Cursors.Hand,
                        Tag = false // 선택 여부 저장용 (bool)
                    };
                    btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);

                    btn.Click += CrimeButton_Click;

                    flpButtons.Controls.Add(btn);
                }

                yOffset += flpButtons.Height + 15;
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
                Text = "피크",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(lblSelectedTitle.Width + 145, 15),
                AutoSize = true,
                Checked = false,
                BackColor = Color.Transparent
            };
            // rightPanel.Controls.Add(chkNewbie);
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

            txtSelectedCrimes.Text = result;
        }

        private void UpdateFineAndDetention()
        {

            // 벌금, 구금 계산
            long totalFine = 0;
            
            int totalDetention = 0;
            int totalBailDetention = 10;

            const int MAX_DETENTION = 150;
            const int BAIL_PER_MINUTE = 3_000_000;
            const int BAIL_BASE_DETENTION = 10;

            bool isPeak = chkNewbie.Checked;      // 피크 체크
            int bailDetentionBase = isPeak ? 5 : 10;
            int BailPerMinute = 3_000_000;



            foreach (var crime in selectedCrimes)
            {
                switch (crime)
                {
                    // 도로교통법
                    case "속도위반": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "신호위반": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "불법 주정차": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "스턴트": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "차선위반": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "중앙선 침범": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "역주행": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "불법유턴": totalFine += 12_000_000; totalBailDetention += 0; break;
                    case "보복운전": totalFine += 15_000_000; totalBailDetention += 0; break;
                    case "승차 방법 제한": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "폭주": totalFine += 30_000_000; totalDetention += 20; break;
                    case "인도주행": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "공공기물 파손": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "음주운전": totalFine += 17_000_000; totalDetention += 10; break;
                    case "비정상도로 진입": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "도로 외 공간 주행": totalFine += 10_000_000; totalBailDetention += 0; break;
                    case "운전 중 휴대폰 사용": totalFine += 10_000_000; totalBailDetention += 0; break;

                    // 형사 중범죄
                    case "시민 살인": totalFine += 50_000_000; totalDetention += 20; break;
                    case "살인 미수": totalFine += 30_000_000; totalDetention += 20; break;
                    case "명예훼손": totalFine += 20_000_000; totalDetention += 10; break;
                    case "납치": totalFine += 60_000_000; totalDetention += 10; break;
                    case "감금": totalFine += 90_000_000; totalDetention += 15; break;
                    case "시민 폭행": totalFine += 20_000_000; totalDetention += 15; break;
                    case "시민 폭행(2인 이상)": totalFine += 25_000_000; totalDetention += 15; break;
                    case "시민 폭행\n(불법 무기/물건)": totalFine += 25_000_000; totalDetention += 15; break;
                    case "차량/헬기 절도": totalFine += 40_000_000; totalDetention += 20; break;
                    case "차량/헬기 절도 미수": totalFine += 15_000_000; break;
                    case "도난 차량 압류": totalFine += 25_000_000; totalDetention += 20; break;
                    case "재물손괴(일부)": totalFine += 15_000_000; break;
                    case "재물손괴(전체)": totalFine += 30_000_000; break;
                    case "재물손괴 후 도주": totalFine += 20_000_000; totalDetention += 10; break;
                    case "스토킹": totalFine += 20_000_000; totalDetention += 10; break;
                    case "횡령/사기(5억미만)": totalFine += 15_000_000; totalDetention += 10; break;
                    case "횡령/사기(5억이상)": totalFine += 60_000_000; totalDetention += 20; break;
                    case "영업방해": totalFine += 30_000_000; totalDetention += 10; break;
                    case "불법물건 소지": totalFine += 30_000_000; totalDetention += 30; break;
                    case "불법물건 언급": totalFine += 20_000_000; totalDetention += 15; break;
                    case "불법물건 유통": totalFine += 20_000_000; totalDetention += 20; break;
                    case "근접무기 사용": totalFine += 20_000_000; totalDetention += 20; break;
                    case "사유지 무단침입": totalFine += 20_000_000; totalDetention += 10; break;
                    case "증거인멸": totalFine += 20_000_000; totalDetention += 20; break;
                    case "거짓 진술": totalFine += 30_000_000; totalDetention += 20; break;

                    // 항공법
                    case "저공비행": totalFine += 50_000_000; totalDetention += 40; break;
                    case "미허가 헬기 운행": totalFine += 60_000_000; totalDetention += 60; break;

                    // 공무원법
                    case "공무원 살인": totalFine += 70_000_000; totalDetention += 30; break;
                    case "공무원 살인 미수": totalFine += 50_000_000; totalDetention += 30; break;
                    case "공무원 명예훼손": totalFine += 40_000_000; totalDetention += 20; break;
                    case "공무원 폭행": totalFine += 40_000_000; totalDetention += 20; break;
                    case "공무원 폭행(2인 이상)": totalFine += 60_000_000; totalDetention += 25; break;
                    case "공무원 폭행\n(불법 무기/물건)": totalFine += 60_000_000; totalDetention += 25; break;
                    case "공무집행 방해": totalFine += 30_000_000; totalDetention += 25; break;
                    case "공무집행 방해(상해,사망)": totalFine += 65_000_000; totalDetention += 20; break;
                    case "공무원 지시 불이행": totalFine += 40_000_000; totalDetention += 10; break;
                    case "공무원 지시 불이행\n(3회 이상)": totalFine += 70_000_000; totalDetention += 20; break;
                    case "수갑 미착용 도주": totalFine += 40_000_000; totalDetention += 10; break;
                    case "차량 재탑승 도주": totalFine += 60_000_000; totalDetention += 20; break;
                    case "공무원 사칭": totalFine += 40_000_000; totalDetention += 25; break;
                    case "공무원 차량/헬기 절도": totalFine += 40_000_000; totalDetention += 30; break;
                    case "공무원 차량/헬기 절도 미수": totalFine += 30_000_000; break;
                    case "공무원 도난 차량 압류": totalFine += 50_000_000; totalDetention += 30; break;
                    case "국유지 침입": totalFine += 50_000_000; totalDetention += 15; break;
                    case "공무원 뇌물 수수": totalFine += 60_000_000; totalDetention += 40; break;
                    case "공무원 재물손괴": totalFine += 30_000_000; totalDetention += 10; break;
                    case "공무원 재물손괴 후 도주": totalFine += 50_000_000; totalDetention += 20; break;
                }
            }

            if (totalDetention > MAX_DETENTION)
                totalDetention = MAX_DETENTION;
            
            int reducedMinutes = totalDetention > BAIL_BASE_DETENTION
                ? totalDetention - BAIL_BASE_DETENTION
                : 0;

            long totalBailFine = totalFine + (long)reducedMinutes * BAIL_PER_MINUTE;
            if (chkNewbie.Checked)
            {
                totalBailFine += 45_000_000;
            }

            // 벌금은 원화 3자리 콤마 포맷 + "원"
            txtFine.Text = totalFine.ToString("N0") + "원";
            txtBailFine.Text = totalBailFine.ToString("N0") + "원";

            // 구금 분 단위 표시
            txtDetention.Text = totalDetention.ToString() + "분";
            txtBailDetention.Text = bailDetentionBase.ToString() + "분";

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
