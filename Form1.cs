using Cryptography_App.Ciphers;
using Cryptography_App.Controls;
using Cryptography_App.Helpers;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace Cryptography_App
{
    public partial class Form1 : Form
    {
        private PlayFairCipher _playFair = new PlayFairCipher();
        private RSACipher _rsa = new RSACipher();
        
        // Menu
        private MenuStrip menuStrip = new MenuStrip();
        
        // Image Lists for icons
        private ImageList menuImageList = null!;
        
        // Main Tab Control
        private TabControl mainTabControl = new TabControl();
        private TabPage playFairTab = new TabPage();
        private TabPage rsaTab = new TabPage();
        private TabPage fileEncryptTab = new TabPage();
        
        // PlayFair Controls
        private TextBox txtPlayFairKey = new TextBox();
        private TextBox txtPlayFairInput = new TextBox();
        private TextBox txtPlayFairOutput = new TextBox();
        private TextBox txtPlayFairMatrix = new TextBox();
        private DataGridView dgvPlayFairProcess = new DataGridView(); // DataGridView cho quá trình mã hóa
        private Label lblPlayFairProcessTitle = new Label();
        private Label lblPlayFairStatus = new Label();
        
        // RSA Controls
        private TextBox txtRsaInput = new TextBox();
        private TextBox txtRsaOutput = new TextBox();
        private TextBox txtRsaKeyInfo = new TextBox();
        private DataGridView dgvRsaProcess = new DataGridView(); // DataGridView cho quá trình mã hóa
        private Label lblRsaProcessTitle = new Label();
        private TextBox txtRsaP = new TextBox();
        private TextBox txtRsaQ = new TextBox();
        private TextBox txtRsaE = new TextBox();
        private Label lblRsaStatus = new Label();
        private NumericUpDown numKeySize = new NumericUpDown();
        
        // File Encryption Panel
        private FileEncryptionPanel fileEncryptionPanel = null!;
        
        // Progress
        private ProgressBar progressBar = new ProgressBar();

        // Colors - Modern Dark Theme
        private readonly Color darkBg = Color.FromArgb(25, 25, 40);
        private readonly Color darkBg2 = Color.FromArgb(40, 40, 60);
        private readonly Color darkBg3 = Color.FromArgb(55, 55, 75);
        private readonly Color accentBlue = Color.FromArgb(52, 152, 219);
        private readonly Color accentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color accentOrange = Color.FromArgb(230, 126, 34);
        private readonly Color accentRed = Color.FromArgb(231, 76, 60);
        private readonly Color accentPurple = Color.FromArgb(155, 89, 182);
        private readonly Color accentCyan = Color.FromArgb(26, 188, 156);
        private readonly Color accentGray = Color.FromArgb(52, 73, 94);
        private readonly Color textColor = Color.FromArgb(236, 240, 241);
        private readonly Color textMuted = Color.FromArgb(160, 160, 180);

        public Form1()
        {
            InitializeComponent();
            InitializeImageLists();
            SetupModernUI();
        }

        private void InitializeImageLists()
        {
            menuImageList = IconHelper.CreateMenuImageList();
        }

        private void SetupModernUI()
        {
            this.Text = "🔐 CryptoMaster Pro - Advanced Encryption Suite";
            this.Size = new Size(1500, 950);
            this.MinimumSize = new Size(1300, 850);
            this.BackColor = darkBg;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F);
            this.Icon = CreateAppIcon();
            this.DoubleBuffered = true;

            // Enable rounded corners for Windows 11
            EnableRoundedCorners();

            SetupMenu();
            SetupTabs();
            SetupStatusBar();
            UpdateRsaKeyInfo();
        }

        private void EnableRoundedCorners()
        {
            // Windows 11 rounded corners
            try
            {
                var preference = 2; // DWMWCP_ROUND
                DwmSetWindowAttribute(Handle, 33, ref preference, sizeof(int));
            }
            catch { }
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private Icon CreateAppIcon()
        {
            var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Gradient background
                using (var brush = new LinearGradientBrush(new Rectangle(0, 0, 32, 32), 
                    accentBlue, accentPurple, LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(brush, 1, 1, 30, 30);
                }
                
                // Lock icon
                using (var pen = new Pen(Color.White, 2.5f))
                {
                    g.DrawArc(pen, 10, 6, 12, 12, 180, 180);
                    g.DrawRectangle(pen, 8, 13, 16, 12);
                }
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private void SetupMenu()
        {
            menuStrip.BackColor = Color.FromArgb(45, 45, 65);
            menuStrip.ForeColor = textColor;
            menuStrip.Font = new Font("Segoe UI", 10F);
            menuStrip.Padding = new Padding(10, 5, 10, 5);
            menuStrip.Renderer = new ModernMenuRenderer();

            var fileMenu = CreateMenuWithIcon("File", IconManager.IconType.Folder);
            fileMenu.DropDownItems.Add(CreateMenuItem("Open Text File...", "open", OpenFile_Click, Keys.Control | Keys.O));
            fileMenu.DropDownItems.Add(CreateMenuItem("Save Output...", "save", SaveOutput_Click, Keys.Control | Keys.S));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateMenuItem("Export RSA Keys...", "export", ExportKeys_Click));
            fileMenu.DropDownItems.Add(CreateMenuItem("Import Public Key...", "import", ImportPublicKey_Click));
            fileMenu.DropDownItems.Add(CreateMenuItem("Import Private Key...", "import", ImportPrivateKey_Click));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateMenuItem("Exit", "exit", (s, e) => Application.Exit(), Keys.Alt | Keys.F4));

            var toolsMenu = CreateMenuWithIcon("Tools", IconManager.IconType.Settings);
            toolsMenu.DropDownItems.Add(CreateMenuItem("Generate New RSA Keys", "generate", GenerateNewKeys_Click));
            toolsMenu.DropDownItems.Add(CreateMenuItem("Clear All Fields", "clear", ClearAll_Click));
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());
            toolsMenu.DropDownItems.Add(CreateMenuItem("Copy Output to Clipboard", "copy", CopyOutput_Click));
            toolsMenu.DropDownItems.Add(CreateMenuItem("Paste from Clipboard", "paste", PasteInput_Click));

            var helpMenu = CreateMenuWithIcon("Help", IconManager.IconType.Help);
            helpMenu.DropDownItems.Add(CreateMenuItem("About PlayFair Cipher", "info", AboutPlayFair_Click));
            helpMenu.DropDownItems.Add(CreateMenuItem("About RSA Encryption", "info", AboutRSA_Click));
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add(CreateMenuItem("About CryptoMaster", "help", AboutApp_Click));

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private ToolStripMenuItem CreateMenuItem(string text, string imageKey, EventHandler onClick, Keys shortcut = Keys.None)
        {
            var item = new ToolStripMenuItem(text);
            item.ForeColor = Color.FromArgb(50, 50, 50); // Dark text for white background
            if (menuImageList.Images.ContainsKey(imageKey))
                item.Image = menuImageList.Images[imageKey];
            item.Click += onClick;
            if (shortcut != Keys.None)
                item.ShortcutKeys = shortcut;
            return item;
        }

        private ToolStripMenuItem CreateMenuWithIcon(string text, IconManager.IconType iconType)
        {
            var item = new ToolStripMenuItem(text);
            var icon = IconManager.GetIcon(iconType, 16);
            if (icon != null)
                item.Image = icon;
            return item;
        }

        private void SetupTabs()
        {
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            mainTabControl.Padding = new Point(25, 12);
            mainTabControl.ItemSize = new Size(180, 45);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabControl.DrawItem += MainTabControl_DrawItem;

            // PlayFair Tab
            playFairTab.Text = "PlayFair Cipher";
            playFairTab.BackColor = darkBg;
            SetupPlayFairTab();

            // RSA Tab  
            rsaTab.Text = "RSA Encryption";
            rsaTab.BackColor = darkBg;
            SetupRsaTab();

            // File Encryption Tab
            fileEncryptTab.Text = "File Encryption";
            fileEncryptTab.BackColor = darkBg;
            SetupFileEncryptTab();

            mainTabControl.TabPages.Add(playFairTab);
            mainTabControl.TabPages.Add(rsaTab);
            mainTabControl.TabPages.Add(fileEncryptTab);

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 55, 10, 10), // Increased top padding for tab header
                BackColor = darkBg
            };
            container.Controls.Add(mainTabControl);
            this.Controls.Add(container);
            
            // Đảm bảo menu luôn ở trên cùng
            menuStrip.BringToFront();
        }

        private void MainTabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            var tabPage = mainTabControl.TabPages[e.Index];
            var tabRect = mainTabControl.GetTabRect(e.Index);
            bool isSelected = (mainTabControl.SelectedIndex == e.Index);

            // Background
            Color bgColor = isSelected ? accentBlue : darkBg2;
            using (var path = GetRoundedRect(tabRect, 10))
            using (var brush = new LinearGradientBrush(tabRect, 
                isSelected ? ControlPaint.Light(bgColor, 0.1f) : bgColor, 
                bgColor, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // Bottom indicator for selected tab
            if (isSelected)
            {
                using (var pen = new Pen(accentCyan, 3))
                {
                    g.DrawLine(pen, tabRect.Left + 15, tabRect.Bottom - 2, tabRect.Right - 15, tabRect.Bottom - 2);
                }
            }

            // Text
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var brush = new SolidBrush(isSelected ? Color.White : textMuted))
            {
                g.DrawString(tabPage.Text, e.Font ?? mainTabControl.Font, brush, tabRect, sf);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
            path.CloseFigure();
            return path;
        }

        private void SetupPlayFairTab()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Key Panel
            var keyPanel = CreateStyledPanel("ENCRYPTION KEY");
            var keyLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 5, 10, 5),
                WrapContents = false
            };

            var lblKey = CreateLabel("Key:");
            lblKey.Margin = new Padding(0, 8, 15, 0);
            
            txtPlayFairKey = new TextBox
            {
                Size = new Size(400, 32),
                Font = new Font("Consolas", 12F),
                BackColor = darkBg2,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 3, 20, 0)
            };
            txtPlayFairKey.TextChanged += TxtPlayFairKey_TextChanged;

            keyLayout.Controls.AddRange(new Control[] { lblKey, txtPlayFairKey });
            keyPanel.Controls.Add(keyLayout);
            mainPanel.Controls.Add(keyPanel, 0, 0);
            mainPanel.SetColumnSpan(keyPanel, 3);

            // Input/Output Panel
            var ioPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            ioPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            ioPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var inputPanel = CreateStyledPanel("INPUT - Plain Text / Cipher Text");
            txtPlayFairInput = CreateStyledTextBox(true);
            inputPanel.Controls.Add(txtPlayFairInput);
            ioPanel.Controls.Add(inputPanel, 0, 0);

            var outputPanel = CreateStyledPanel("OUTPUT - Result");
            txtPlayFairOutput = CreateStyledTextBox(true);
            txtPlayFairOutput.ReadOnly = true;
            outputPanel.Controls.Add(txtPlayFairOutput);
            ioPanel.Controls.Add(outputPanel, 0, 1);

            mainPanel.Controls.Add(ioPanel, 0, 1);

            // Process Panel - Show detailed encryption process
            var processPanel = CreateStyledPanel("ENCRYPTION PROCESS");
            processPanel.BorderColor = accentCyan;
            
            var processContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            processContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            processContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            // Title label for process info
            lblPlayFairProcessTitle = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = accentCyan,
                Font = new Font("Consolas", 8.5F),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(5, 2, 5, 2),
                AutoSize = false
            };
            processContainer.Controls.Add(lblPlayFairProcessTitle, 0, 0);
            
            // DataGridView for encryption steps
            dgvPlayFairProcess = CreateProcessDataGridView();
            dgvPlayFairProcess.Columns.Add("Pair", "Pair");
            dgvPlayFairProcess.Columns.Add("Pos1", "Pos 1");
            dgvPlayFairProcess.Columns.Add("Pos2", "Pos 2");
            dgvPlayFairProcess.Columns.Add("Rule", "Rule");
            dgvPlayFairProcess.Columns.Add("Result", "Result");
            
            // Set column auto-size modes to fill available space
            dgvPlayFairProcess.Columns["Pair"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvPlayFairProcess.Columns["Pos1"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvPlayFairProcess.Columns["Pos2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvPlayFairProcess.Columns["Rule"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvPlayFairProcess.Columns["Result"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            
            processContainer.Controls.Add(dgvPlayFairProcess, 0, 1);
            processPanel.Controls.Add(processContainer);
            mainPanel.Controls.Add(processPanel, 1, 1);

            // Matrix Panel
            var matrixPanel = CreateStyledPanel("MATRIX 5x5");
            matrixPanel.BorderColor = accentPurple;
            txtPlayFairMatrix = CreateStyledTextBox(true);
            txtPlayFairMatrix.ReadOnly = true;
            txtPlayFairMatrix.Font = new Font("Consolas", 16F, FontStyle.Bold);
            txtPlayFairMatrix.TextAlign = HorizontalAlignment.Center;
            matrixPanel.Controls.Add(txtPlayFairMatrix);
            mainPanel.Controls.Add(matrixPanel, 2, 1);

            // Button Panel
            var btnPanel = CreateButtonPanel();
            
            var btnEncrypt = CreateIconButton("ENCRYPT", accentGreen, ButtonIconType.Lock, 110, 40);
            btnEncrypt.Click += BtnPlayFairEncrypt_Click;

            var btnDecrypt = CreateIconButton("DECRYPT", accentOrange, ButtonIconType.Unlock, 110, 40);
            btnDecrypt.Click += BtnPlayFairDecrypt_Click;

            var btnSwap = CreateIconButton("SWAP", accentCyan, ButtonIconType.Swap, 110, 40);
            btnSwap.Click += BtnPlayFairSwap_Click;

            var btnClear = CreateIconButton("CLEAR", accentRed, ButtonIconType.Trash, 110, 40);
            btnClear.Click += BtnPlayFairClear_Click;

            lblPlayFairStatus = CreateLabel("");
            lblPlayFairStatus.ForeColor = accentGreen;
            lblPlayFairStatus.AutoSize = true;
            lblPlayFairStatus.Margin = new Padding(20, 12, 0, 0);
            lblPlayFairStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnPanel.Controls.AddRange(new Control[] { btnEncrypt, btnDecrypt, btnSwap, btnClear, lblPlayFairStatus });
            mainPanel.Controls.Add(btnPanel, 0, 2);
            mainPanel.SetColumnSpan(btnPanel, 3);

            playFairTab.Controls.Add(mainPanel);
        }

        private DataGridView CreateProcessDataGridView()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = darkBg2,
                GridColor = darkBg3,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 35,
                RowTemplate = { Height = 28 },
                Font = new Font("Consolas", 10F),
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            
            // Header style
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = darkBg3,
                ForeColor = accentCyan,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = darkBg3,
                SelectionForeColor = accentCyan
            };
            
            // Cell style
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = darkBg2,
                ForeColor = textColor,
                SelectionBackColor = accentBlue,
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            
            // Alternating row style
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(35, 35, 55),
                ForeColor = textColor,
                SelectionBackColor = accentBlue,
                SelectionForeColor = Color.White
            };
            
            return dgv;
        }

        private void SetupRsaTab()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(15)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            // Key Settings Panel
            var keySettingsPanel = CreateStyledPanel("RSA KEY CONFIGURATION");
            keySettingsPanel.BorderColor = accentPurple;
            
            // Use a single FlowLayoutPanel for all controls
            var keySettingsLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(10, 5, 10, 5),
                AutoSize = false
            };

            // Row 1: Key Size + Generate
            var lblKeySize = CreateLabel("Key Size:");
            lblKeySize.Margin = new Padding(0, 8, 5, 5);
            keySettingsLayout.Controls.Add(lblKeySize);

            numKeySize = new NumericUpDown
            {
                Minimum = 256,
                Maximum = 2048,
                Value = 512,
                Increment = 256,
                Size = new Size(80, 28),
                BackColor = darkBg2,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 4, 15, 5)
            };
            keySettingsLayout.Controls.Add(numKeySize);

            var btnGenerate = CreateIconButton("Generate", accentBlue, ButtonIconType.Refresh, 110, 32);
            btnGenerate.Click += BtnRsaGenerate_Click;
            btnGenerate.Margin = new Padding(0, 2, 30, 5);
            keySettingsLayout.Controls.Add(btnGenerate);

            // Row 1 continued: P, Q, E + Set Manual
            var lblP = CreateLabel("P:");
            lblP.Margin = new Padding(0, 8, 5, 5);
            keySettingsLayout.Controls.Add(lblP);
            
            txtRsaP = new TextBox { Size = new Size(70, 26), BackColor = darkBg2, ForeColor = textColor, Margin = new Padding(0, 4, 15, 5), Font = new Font("Consolas", 9F) };
            keySettingsLayout.Controls.Add(txtRsaP);

            var lblQ = CreateLabel("Q:");
            lblQ.Margin = new Padding(0, 8, 5, 5);
            keySettingsLayout.Controls.Add(lblQ);
            
            txtRsaQ = new TextBox { Size = new Size(70, 26), BackColor = darkBg2, ForeColor = textColor, Margin = new Padding(0, 4, 15, 5), Font = new Font("Consolas", 9F) };
            keySettingsLayout.Controls.Add(txtRsaQ);

            var lblE = CreateLabel("E:");
            lblE.Margin = new Padding(0, 8, 5, 5);
            keySettingsLayout.Controls.Add(lblE);
            
            txtRsaE = new TextBox { Size = new Size(70, 26), BackColor = darkBg2, ForeColor = textColor, Text = "65537", Margin = new Padding(0, 4, 15, 5), Font = new Font("Consolas", 9F) };
            keySettingsLayout.Controls.Add(txtRsaE);

            var btnSetManual = CreateIconButton("Set", accentGreen, ButtonIconType.Key, 65, 32);
            btnSetManual.Click += BtnRsaSetManual_Click;
            btnSetManual.Margin = new Padding(0, 2, 0, 5);
            keySettingsLayout.Controls.Add(btnSetManual);

            keySettingsPanel.Controls.Add(keySettingsLayout);
            mainPanel.Controls.Add(keySettingsPanel, 0, 0);
            mainPanel.SetColumnSpan(keySettingsPanel, 3);

            // Input/Output Panel
            var ioPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            ioPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            ioPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var inputPanel = CreateStyledPanel("INPUT - Text or Numbers");
            txtRsaInput = CreateStyledTextBox(true);
            inputPanel.Controls.Add(txtRsaInput);
            ioPanel.Controls.Add(inputPanel, 0, 0);

            var outputPanel = CreateStyledPanel("OUTPUT - Encrypted / Decrypted");
            txtRsaOutput = CreateStyledTextBox(true);
            txtRsaOutput.ReadOnly = true;
            outputPanel.Controls.Add(txtRsaOutput);
            ioPanel.Controls.Add(outputPanel, 0, 1);

            mainPanel.Controls.Add(ioPanel, 0, 1);

            // Process Panel - Show detailed encryption process
            var processPanel = CreateStyledPanel("ENCRYPTION PROCESS");
            processPanel.BorderColor = accentOrange;
            
            var processContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            processContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            processContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            // Title label for RSA info
            lblRsaProcessTitle = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = accentOrange,
                Font = new Font("Consolas", 9F),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(5),
                AutoSize = false
            };
            processContainer.Controls.Add(lblRsaProcessTitle, 0, 0);
            
            // DataGridView for RSA encryption steps
            dgvRsaProcess = CreateProcessDataGridView();
            dgvRsaProcess.Columns.Add("Char", "Char");
            dgvRsaProcess.Columns.Add("ASCII", "ASCII");
            dgvRsaProcess.Columns.Add("M", "M");
            dgvRsaProcess.Columns.Add("C = M^E mod N", "C = M^E mod N");
            
            // Set column widths
            dgvRsaProcess.Columns["Char"].Width = 60;
            dgvRsaProcess.Columns["ASCII"].Width = 60;
            dgvRsaProcess.Columns["M"].Width = 60;
            dgvRsaProcess.Columns["C = M^E mod N"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            dgvRsaProcess.DefaultCellStyle.ForeColor = accentOrange;
            dgvRsaProcess.AlternatingRowsDefaultCellStyle.ForeColor = accentOrange;
            
            processContainer.Controls.Add(dgvRsaProcess, 0, 1);
            processPanel.Controls.Add(processContainer);
            mainPanel.Controls.Add(processPanel, 1, 1);

            // Key Info Panel
            var keyInfoPanel = CreateStyledPanel("RSA KEY INFO");
            keyInfoPanel.BorderColor = accentCyan;
            txtRsaKeyInfo = CreateStyledTextBox(true);
            txtRsaKeyInfo.ReadOnly = true;
            txtRsaKeyInfo.Font = new Font("Consolas", 9F);
            txtRsaKeyInfo.ForeColor = accentCyan;
            keyInfoPanel.Controls.Add(txtRsaKeyInfo);
            mainPanel.Controls.Add(keyInfoPanel, 2, 1);

            // Button Panel
            var btnPanel = CreateButtonPanel();

            var btnEncrypt = CreateIconButton("ENCRYPT", accentGreen, ButtonIconType.Lock, 110, 40);
            btnEncrypt.Click += BtnRsaEncrypt_Click;

            var btnDecrypt = CreateIconButton("DECRYPT", accentOrange, ButtonIconType.Unlock, 110, 40);
            btnDecrypt.Click += BtnRsaDecrypt_Click;

            var btnEncryptNum = CreateIconButton("ENC NUM", accentPurple, ButtonIconType.Lock, 110, 40);
            btnEncryptNum.Click += BtnRsaEncryptNum_Click;

            var btnDecryptNum = CreateIconButton("DEC NUM", accentGray, ButtonIconType.Unlock, 110, 40);
            btnDecryptNum.Click += BtnRsaDecryptNum_Click;

            var btnSwap = CreateIconButton("SWAP", accentCyan, ButtonIconType.Swap, 110, 40);
            btnSwap.Click += BtnRsaSwap_Click;

            var btnClear = CreateIconButton("CLEAR", accentRed, ButtonIconType.Trash, 110, 40);
            btnClear.Click += BtnRsaClear_Click;

            lblRsaStatus = CreateLabel("");
            lblRsaStatus.ForeColor = accentGreen;
            lblRsaStatus.AutoSize = true;
            lblRsaStatus.Margin = new Padding(10, 12, 0, 0);
            lblRsaStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            btnPanel.Controls.AddRange(new Control[] { btnEncrypt, btnDecrypt, btnEncryptNum, btnDecryptNum, btnSwap, btnClear, lblRsaStatus });
            mainPanel.Controls.Add(btnPanel, 0, 2);
            mainPanel.SetColumnSpan(btnPanel, 3);

            rsaTab.Controls.Add(mainPanel);
        }

        private void SetupFileEncryptTab()
        {
            fileEncryptionPanel = new FileEncryptionPanel
            {
                Dock = DockStyle.Fill
            };
            fileEncryptTab.Controls.Add(fileEncryptionPanel);
        }

        private void SetupStatusBar()
        {
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 4,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };
            this.Controls.Add(progressBar);
        }

        #region Helper Methods

        private ModernGroupBox CreateStyledPanel(string title)
        {
            return new ModernGroupBox
            {
                Title = title,
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Margin = new Padding(5),
                BorderColor = accentBlue
            };
        }

        private FlowLayoutPanel CreateButtonPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 0),
                WrapContents = false
            };
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = textColor,
                AutoSize = true,
                Font = new Font("Segoe UI", 11F)
            };
        }

        private TextBox CreateStyledTextBox(bool multiline = false)
        {
            return new TextBox
            {
                Multiline = multiline,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 11F),
                BackColor = darkBg2,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
            };
        }

        private enum ButtonIconType
        {
            None, Lock, Unlock, Swap, Trash, Matrix, Refresh, Key
        }

        private Button CreateIconButton(string text, Color bgColor, ButtonIconType iconType, int width, int height)
        {
            var btn = new IconButton
            {
                Text = text,
                Size = new Size(width, height),
                NormalColor = bgColor,
                HoverColor = ControlPaint.Light(bgColor, 0.2f),
                PressColor = ControlPaint.Dark(bgColor, 0.1f),
                IconType = null, // No icon - text only
                IconSize = 0,
                Margin = new Padding(0, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleCenter
            };

            return btn;
        }

        private void ShowStatus(Label lbl, string message, bool success = true)
        {
            lbl.ForeColor = success ? accentGreen : accentRed;
            lbl.Text = message;

            var timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                lbl.Text = "";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private async Task ShowProcessingAnimation(Action action)
        {
            progressBar.Visible = true;
            await Task.Run(action);
            progressBar.Visible = false;
        }

        #endregion

        #region PlayFair Events

        private void TxtPlayFairKey_TextChanged(object? sender, EventArgs e)
        {
            UpdatePlayFairMatrix();
        }

        private void UpdatePlayFairMatrix()
        {
            if (!string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
            {
                _playFair.Key = txtPlayFairKey.Text;
                txtPlayFairMatrix.Text = _playFair.GetMatrixDisplay();
            }
            else
            {
                txtPlayFairMatrix.Text = "";
            }
        }

        private async void BtnPlayFairEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
            {
                ShowStatus(lblPlayFairStatus, "⚠️ Please enter a key!", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPlayFairInput.Text))
            {
                ShowStatus(lblPlayFairStatus, "⚠️ Please enter text to encrypt!", false);
                return;
            }

            try
            {
                await ShowProcessingAnimation(() =>
                {
                    _playFair.Key = txtPlayFairKey.Text;
                    var (result, processData) = _playFair.EncryptWithProcessData(txtPlayFairInput.Text);
                    this.Invoke(() => 
                    {
                        txtPlayFairOutput.Text = result;
                        UpdatePlayFairProcessGrid(processData, true);
                    });
                });
                ShowStatus(lblPlayFairStatus, "✓ Encryption successful!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblPlayFairStatus, $"✗ {ex.Message}", false);
            }
        }

        private async void BtnPlayFairDecrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
            {
                ShowStatus(lblPlayFairStatus, "⚠️ Please enter a key!", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPlayFairInput.Text))
            {
                ShowStatus(lblPlayFairStatus, "⚠️ Please enter text to decrypt!", false);
                return;
            }

            try
            {
                await ShowProcessingAnimation(() =>
                {
                    _playFair.Key = txtPlayFairKey.Text;
                    var (result, processData) = _playFair.DecryptWithProcessData(txtPlayFairInput.Text);
                    this.Invoke(() => 
                    {
                        txtPlayFairOutput.Text = result;
                        UpdatePlayFairProcessGrid(processData, false);
                    });
                });
                ShowStatus(lblPlayFairStatus, "✓ Decryption successful!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblPlayFairStatus, $"✗ {ex.Message}", false);
            }
        }

        private void UpdatePlayFairProcessGrid(List<PlayFairCipher.EncryptionStep> steps, bool isEncrypt)
        {
            dgvPlayFairProcess.Rows.Clear();
            lblPlayFairProcessTitle.Text = $"Key: {txtPlayFairKey.Text.ToUpper()}\n" +
                                           $"Input: {txtPlayFairInput.Text}\n" +
                                           $"Mode: {(isEncrypt ? "ENCRYPT" : "DECRYPT")}";
            
            foreach (var step in steps)
            {
                dgvPlayFairProcess.Rows.Add(
                    step.Pair,
                    step.Pos1,
                    step.Pos2,
                    step.Rule,
                    step.Result
                );
            }
        }

        private void BtnPlayFairSwap_Click(object? sender, EventArgs e)
        {
            (txtPlayFairInput.Text, txtPlayFairOutput.Text) = (txtPlayFairOutput.Text, txtPlayFairInput.Text);
            ShowStatus(lblPlayFairStatus, "✓ Input/Output swapped!");
        }

        private void BtnPlayFairClear_Click(object? sender, EventArgs e)
        {
            txtPlayFairKey.Clear();
            txtPlayFairInput.Clear();
            txtPlayFairOutput.Clear();
            txtPlayFairMatrix.Clear();
            dgvPlayFairProcess.Rows.Clear();
            lblPlayFairProcessTitle.Text = "";
            ShowStatus(lblPlayFairStatus, "✓ Cleared!");
        }

        #endregion

        #region RSA Events

        private void UpdateRsaKeyInfo()
        {
            txtRsaKeyInfo.Text = _rsa.GetKeyInfo();
        }

        private async void BtnRsaGenerate_Click(object? sender, EventArgs e)
        {
            try
            {
                lblRsaStatus.Text = "⏳ Generating keys...";
                lblRsaStatus.ForeColor = accentBlue;
                await ShowProcessingAnimation(() =>
                {
                    _rsa.GenerateKeys((int)numKeySize.Value);
                    this.Invoke(UpdateRsaKeyInfo);
                });
                ShowStatus(lblRsaStatus, $"✓ {numKeySize.Value}-bit keys generated!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private void BtnRsaSetManual_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!BigInteger.TryParse(txtRsaP.Text, out BigInteger p) ||
                    !BigInteger.TryParse(txtRsaQ.Text, out BigInteger q) ||
                    !BigInteger.TryParse(txtRsaE.Text, out BigInteger eVal))
                {
                    ShowStatus(lblRsaStatus, "⚠️ Please enter valid numbers!", false);
                    return;
                }

                _rsa.SetManualKeys(p, q, eVal);
                UpdateRsaKeyInfo();
                ShowStatus(lblRsaStatus, "✓ Manual keys set!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private async void BtnRsaEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRsaInput.Text))
            {
                ShowStatus(lblRsaStatus, "⚠️ Please enter text!", false);
                return;
            }

            try
            {
                await ShowProcessingAnimation(() =>
                {
                    var (result, processData) = _rsa.EncryptWithProcessData(txtRsaInput.Text);
                    this.Invoke(() => 
                    {
                        txtRsaOutput.Text = result;
                        UpdateRsaProcessGrid(processData, true);
                    });
                });
                ShowStatus(lblRsaStatus, "✓ Text encrypted!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private async void BtnRsaDecrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRsaInput.Text))
            {
                ShowStatus(lblRsaStatus, "⚠️ Please enter ciphertext!", false);
                return;
            }

            try
            {
                await ShowProcessingAnimation(() =>
                {
                    var (result, processData) = _rsa.DecryptWithProcessData(txtRsaInput.Text);
                    this.Invoke(() => 
                    {
                        txtRsaOutput.Text = result;
                        UpdateRsaProcessGrid(processData, false);
                    });
                });
                ShowStatus(lblRsaStatus, "✓ Text decrypted!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private void UpdateRsaProcessGrid(List<RSACipher.EncryptionStep> steps, bool isEncrypt)
        {
            dgvRsaProcess.Rows.Clear();
            
            // Update column headers based on mode
            if (isEncrypt)
            {
                // Encrypt: Char -> ASCII -> M -> C
                dgvRsaProcess.Columns[0].HeaderText = "Char";
                dgvRsaProcess.Columns[1].HeaderText = "ASCII";
                dgvRsaProcess.Columns[2].HeaderText = "M";
                dgvRsaProcess.Columns[3].HeaderText = "C = M^E mod N";
            }
            else
            {
                // Decrypt: C -> M -> ASCII -> Char (reverse order)
                dgvRsaProcess.Columns[0].HeaderText = "C";
                dgvRsaProcess.Columns[1].HeaderText = "M = C^D mod N";
                dgvRsaProcess.Columns[2].HeaderText = "ASCII";
                dgvRsaProcess.Columns[3].HeaderText = "Char";
            }
            
            if (isEncrypt)
            {
                lblRsaProcessTitle.Text = $"Public Key (E, N): E = {_rsa.E}\n" +
                                          $"N = {TruncateForDisplay(_rsa.N)}\n" +
                                          $"Mode: ENCRYPT";
            }
            else
            {
                lblRsaProcessTitle.Text = $"Private Key (D, N): D = {TruncateForDisplay(_rsa.D)}\n" +
                                          $"N = {TruncateForDisplay(_rsa.N)}\n" +
                                          $"Mode: DECRYPT";
            }
            
            foreach (var step in steps)
            {
                if (isEncrypt)
                {
                    // Encrypt order: Char, ASCII, M, C
                    dgvRsaProcess.Rows.Add(
                        step.Character,
                        step.Ascii,
                        step.M,
                        step.Result
                    );
                }
                else
                {
                    // Decrypt order: C, M, ASCII, Char (reversed)
                    dgvRsaProcess.Rows.Add(
                        step.M,        // C (cipher)
                        step.Result,   // M = C^D mod N
                        step.Ascii,    // ASCII
                        step.Character // Char
                    );
                }
            }
        }

        private string TruncateForDisplay(BigInteger num)
        {
            string s = num.ToString();
            return s.Length > 30 ? s.Substring(0, 15) + "..." + s.Substring(s.Length - 15) : s;
        }

        private void BtnRsaEncryptNum_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!BigInteger.TryParse(txtRsaInput.Text.Trim(), out BigInteger m))
                {
                    ShowStatus(lblRsaStatus, "⚠️ Enter a valid number!", false);
                    return;
                }

                var result = _rsa.EncryptNumber(m);
                txtRsaOutput.Text = result.ToString();
                
                // Clear and update grid for number encryption
                dgvRsaProcess.Rows.Clear();
                dgvRsaProcess.Columns["M"].HeaderText = "M";
                dgvRsaProcess.Columns["C = M^E mod N"].HeaderText = "C = M^E mod N";
                lblRsaProcessTitle.Text = $"MÃ HÓA SỐ\n" +
                                          $"M = {TruncateForDisplay(m)}\n" +
                                          $"C = M^E mod N = {TruncateForDisplay(result)}";
                dgvRsaProcess.Rows.Add("Số", "-", TruncateForDisplay(m), TruncateForDisplay(result));
                
                ShowStatus(lblRsaStatus, "✓ Number encrypted!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private void BtnRsaDecryptNum_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!BigInteger.TryParse(txtRsaInput.Text.Trim(), out BigInteger c))
                {
                    ShowStatus(lblRsaStatus, "⚠️ Enter a valid number!", false);
                    return;
                }

                var result = _rsa.DecryptNumber(c);
                txtRsaOutput.Text = result.ToString();
                
                // Clear and update grid for number decryption
                dgvRsaProcess.Rows.Clear();
                dgvRsaProcess.Columns["M"].HeaderText = "C";
                dgvRsaProcess.Columns["C = M^E mod N"].HeaderText = "M = C^D mod N";
                lblRsaProcessTitle.Text = $"GIẢI MÃ SỐ\n" +
                                          $"C = {TruncateForDisplay(c)}\n" +
                                          $"M = C^D mod N = {TruncateForDisplay(result)}";
                dgvRsaProcess.Rows.Add("Số", "-", TruncateForDisplay(c), TruncateForDisplay(result));
                
                ShowStatus(lblRsaStatus, "✓ Number decrypted!");
            }
            catch (Exception ex)
            {
                ShowStatus(lblRsaStatus, $"✗ {ex.Message}", false);
            }
        }

        private void BtnRsaSwap_Click(object? sender, EventArgs e)
        {
            (txtRsaInput.Text, txtRsaOutput.Text) = (txtRsaOutput.Text, txtRsaInput.Text);
            ShowStatus(lblRsaStatus, "✓ Input/Output swapped!");
        }

        private void BtnRsaClear_Click(object? sender, EventArgs e)
        {
            txtRsaInput.Clear();
            txtRsaOutput.Clear();
            txtRsaP.Clear();
            txtRsaQ.Clear();
            dgvRsaProcess.Rows.Clear();
            lblRsaProcessTitle.Text = "";
            ShowStatus(lblRsaStatus, "✓ Cleared!");
        }

        #endregion

        #region Menu Events

        private void OpenFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open Text File"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string content = File.ReadAllText(ofd.FileName);
                if (mainTabControl.SelectedTab == playFairTab)
                    txtPlayFairInput.Text = content;
                else if (mainTabControl.SelectedTab == rsaTab)
                    txtRsaInput.Text = content;

                MessageBox.Show("✓ File loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveOutput_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save Output"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string content = mainTabControl.SelectedTab == playFairTab ? txtPlayFairOutput.Text : txtRsaOutput.Text;
                File.WriteAllText(sfd.FileName, content);
                MessageBox.Show("✓ File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportKeys_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = "Select folder to save RSA keys" };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string pubPath = Path.Combine(fbd.SelectedPath, "rsa_public.key");
                string privPath = Path.Combine(fbd.SelectedPath, "rsa_private.key");
                _rsa.ExportKeys(pubPath, privPath);
                MessageBox.Show($"Keys exported successfully:\n{pubPath}\n{privPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ImportPublicKey_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*",
                Title = "Import Public Key"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _rsa.ImportPublicKey(ofd.FileName);
                    UpdateRsaKeyInfo();
                    MessageBox.Show("✓ Public key imported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"✗ Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportPrivateKey_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*",
                Title = "Import Private Key"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _rsa.ImportPrivateKey(ofd.FileName);
                    UpdateRsaKeyInfo();
                    MessageBox.Show("✓ Private key imported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"✗ Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerateNewKeys_Click(object? sender, EventArgs e)
        {
            BtnRsaGenerate_Click(sender, e);
            mainTabControl.SelectedTab = rsaTab;
        }

        private void ClearAll_Click(object? sender, EventArgs e)
        {
            BtnPlayFairClear_Click(sender, e);
            BtnRsaClear_Click(sender, e);
            MessageBox.Show("✓ All fields cleared!", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyOutput_Click(object? sender, EventArgs e)
        {
            string output = mainTabControl.SelectedTab == playFairTab ? txtPlayFairOutput.Text : txtRsaOutput.Text;
            if (!string.IsNullOrEmpty(output))
            {
                Clipboard.SetText(output);
                MessageBox.Show("✓ Copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PasteInput_Click(object? sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                if (mainTabControl.SelectedTab == playFairTab)
                    txtPlayFairInput.Text = Clipboard.GetText();
                else if (mainTabControl.SelectedTab == rsaTab)
                    txtRsaInput.Text = Clipboard.GetText();
            }
        }

        private void AboutPlayFair_Click(object? sender, EventArgs e)
        {
            string info = @"🔤 PLAYFAIR CIPHER
═══════════════════════════════

The Playfair cipher was invented by Charles Wheatstone in 1854, 
but bears the name of Lord Playfair who promoted its use.

📖 HOW IT WORKS:
1. Create a 5x5 matrix using the key (J is merged with I)
2. Split plaintext into pairs of letters
3. Apply rules based on letter positions:
   • Same row → replace with letters to the right
   • Same column → replace with letters below
   • Rectangle → swap column positions

✨ FEATURES:
✓ More secure than simple substitution ciphers
✓ Encrypts pairs of letters (digraphs)
✓ Harder to break with frequency analysis";

            MessageBox.Show(info, "About PlayFair Cipher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AboutRSA_Click(object? sender, EventArgs e)
        {
            string info = @"🔑 RSA ENCRYPTION
═══════════════════════════════

RSA (Rivest-Shamir-Adleman) is one of the first public-key 
cryptosystems, invented in 1977.

📖 HOW IT WORKS:
1. Choose two large prime numbers P and Q
2. Calculate N = P × Q and φ(N) = (P-1)(Q-1)
3. Choose E (typically 65537) coprime with φ(N)
4. Calculate D such that D × E ≡ 1 (mod φ(N))

🔐 KEYS:
• Public Key: (E, N) - used for encryption
• Private Key: (D, N) - used for decryption

📝 FORMULAS:
• Encryption: C = M^E mod N
• Decryption: M = C^D mod N

🛡️ SECURITY:
The security relies on the difficulty of factoring 
the product of two large prime numbers.";

            MessageBox.Show(info, "About RSA Encryption", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AboutApp_Click(object? sender, EventArgs e)
        {
            string info = @"🔐 CRYPTOMASTER PRO
═══════════════════════════════

Version: 2.0
Ứng dụng mã hóa hiện đại với các tính năng:

🔤 PlayFair Cipher
   • Hiển thị ma trận 5x5 trực quan
   • Tạo key theo thời gian thực
   • Mã hóa/Giải mã văn bản
   • Mã hóa file (NEW!)

🔑 RSA Encryption
   • Tùy chỉnh kích thước key (256-2048 bits)
   • Nhập key thủ công (P, Q, E)
   • Mã hóa văn bản và số
   • Import/Export key
   • Mã hóa file (NEW!)

📁 File Encryption
   • Hỗ trợ PlayFair và RSA
   • Mã hóa MỌI loại file
   • Kéo & Thả (Drag & Drop)
   • Xem trước file (ảnh, text, hex)
   • Theo dõi tiến trình

🎨 Giao diện hiện đại
   • Theme tối với bo góc
   • Animation mượt mà
   • Thiết kế đáp ứng

Phát triển với .NET 8 và Windows Forms
© 2025 CryptoMaster";

            MessageBox.Show(info, "About CryptoMaster Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initial setup complete
        }
    }

    /// <summary>
    /// Custom menu renderer for modern dark theme
    /// </summary>
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernMenuColors()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using var brush = new SolidBrush(Color.FromArgb(52, 152, 219));
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    public class ModernMenuColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(52, 152, 219);
        public override Color MenuItemBorder => Color.FromArgb(52, 152, 219);
        public override Color MenuBorder => Color.FromArgb(200, 200, 200);
        public override Color ToolStripDropDownBackground => Color.FromArgb(250, 250, 250);
        public override Color ImageMarginGradientBegin => Color.FromArgb(245, 245, 245);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(245, 245, 245);
        public override Color ImageMarginGradientEnd => Color.FromArgb(245, 245, 245);
        public override Color SeparatorDark => Color.FromArgb(200, 200, 200);
        public override Color SeparatorLight => Color.FromArgb(230, 230, 230);
    }
}
