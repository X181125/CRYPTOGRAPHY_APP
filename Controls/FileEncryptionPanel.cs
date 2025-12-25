using System.Drawing.Drawing2D;
using Cryptography_App.Ciphers;
using Cryptography_App.Helpers;

namespace Cryptography_App.Controls
{
    /// <summary>
    /// File Encryption Panel hỗ trợ mã hóa file bằng PlayFair và RSA
    /// </summary>
    public class FileEncryptionPanel : UserControl
    {
        private readonly PlayFairCipher _playFair = new PlayFairCipher();
        private readonly RSACipher _rsa = new RSACipher();
        
        // Colors
        private readonly Color darkBg = Color.FromArgb(30, 30, 46);
        private readonly Color darkBg2 = Color.FromArgb(45, 45, 65);
        private readonly Color accentBlue = Color.FromArgb(52, 152, 219);
        private readonly Color accentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color accentOrange = Color.FromArgb(230, 126, 34);
        private readonly Color accentRed = Color.FromArgb(231, 76, 60);
        private readonly Color accentPurple = Color.FromArgb(155, 89, 182);
        private readonly Color accentCyan = Color.FromArgb(26, 188, 156);
        private readonly Color textColor = Color.FromArgb(236, 240, 241);

        // Controls
        private FileDropPanel dropPanel = null!;
        private ModernGroupBox previewBox = null!;
        private Panel previewContent = null!;
        private PictureBox imagePreview = null!;
        private TextBox textPreview = null!;
        private HexViewer hexPreview = null!;
        private Label fileTypeLabel = null!;
        
        private ModernGroupBox settingsBox = null!;
        private ComboBox cmbCipherType = null!;
        private Panel playFairSettingsPanel = null!;
        private Panel rsaSettingsPanel = null!;
        private TextBox txtPlayFairKey = null!;
        private NumericUpDown numRsaKeySize = null!;
        private TextBox txtRsaP = null!;
        private TextBox txtRsaQ = null!;
        private TextBox txtRsaE = null!;
        private TextBox txtRsaKeyInfo = null!;
        
        private ModernProgressBar progressBar = null!;
        private AnimatedLabel statusLabel = null!;
        
        private IconButton btnEncrypt = null!;
        private IconButton btnDecrypt = null!;
        private IconButton btnClear = null!;
        private IconButton btnGenerateRsaKeys = null!;

        private string _currentFilePath = "";

        public FileEncryptionPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = darkBg;
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            Padding = new Padding(15);

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));

            // === Row 1: Drop Panel + Settings ===
            CreateDropPanel();
            mainLayout.Controls.Add(dropPanel, 0, 0);

            CreateSettingsPanel();
            mainLayout.Controls.Add(settingsBox, 1, 0);

            // === Row 2: Preview ===
            CreatePreviewPanel();
            mainLayout.Controls.Add(previewBox, 0, 1);
            mainLayout.SetColumnSpan(previewBox, 2);

            // === Row 3: Buttons + Progress ===
            var bottomPanel = CreateBottomPanel();
            mainLayout.Controls.Add(bottomPanel, 0, 2);
            mainLayout.SetColumnSpan(bottomPanel, 2);

            Controls.Add(mainLayout);
            
            // Default cipher
            UpdateCipherSettings();
        }

        private void CreateDropPanel()
        {
            dropPanel = new FileDropPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 10),
                DropText = "Drag & Drop file here\nor click to browse\n\nSupports all file types"
            };
            dropPanel.FileDropped += DropPanel_FileDropped;
        }

        private void CreateSettingsPanel()
        {
            settingsBox = new ModernGroupBox
            {
                Title = "ENCRYPTION SETTINGS",
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Margin = new Padding(10, 0, 0, 10),
                BorderColor = accentPurple
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(5)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 5));

            // Cipher Type Selection
            var lblCipherType = CreateLabel("Algorithm:");
            layout.Controls.Add(lblCipherType, 0, 0);

            cmbCipherType = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11F),
                BackColor = darkBg2,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 0, 5)
            };
            cmbCipherType.Items.AddRange(new object[] { "PlayFair Cipher", "RSA Encryption" });
            cmbCipherType.SelectedIndex = 0;
            cmbCipherType.SelectedIndexChanged += CmbCipherType_SelectedIndexChanged;
            layout.Controls.Add(cmbCipherType, 1, 0);

            // Settings Panel Container
            var settingsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            // PlayFair Settings
            CreatePlayFairSettings();
            settingsContainer.Controls.Add(playFairSettingsPanel);

            // RSA Settings
            CreateRsaSettings();
            settingsContainer.Controls.Add(rsaSettingsPanel);

            layout.Controls.Add(settingsContainer, 0, 1);
            layout.SetColumnSpan(settingsContainer, 2);

            settingsBox.Controls.Add(layout);
        }

        private void CreatePlayFairSettings()
        {
            playFairSettingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = true
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblKey = CreateLabel("Key:");
            layout.Controls.Add(lblKey, 0, 0);

            txtPlayFairKey = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 12F),
                BackColor = darkBg2,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 0, 5),
                Text = "SECURITY"
            };
            txtPlayFairKey.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
                    _playFair.Key = txtPlayFairKey.Text;
            };
            layout.Controls.Add(txtPlayFairKey, 1, 0);

            // Info label
            var infoLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Segoe UI", 9F),
                Text = "PlayFair uses 5x5 matrix.\nJ is replaced by I.\nEncrypts only A-Z letters.",
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 5, 0, 0)
            };
            layout.Controls.Add(infoLabel, 0, 1);
            layout.SetColumnSpan(infoLabel, 2);

            playFairSettingsPanel.Controls.Add(layout);
            _playFair.Key = "SECURITY";
        }

        private void CreateRsaSettings()
        {
            rsaSettingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Row 1: Key Size + Generate Button
            var lblKeySize = CreateLabel("Key:");
            layout.Controls.Add(lblKeySize, 0, 0);

            numRsaKeySize = new NumericUpDown
            {
                Minimum = 256,
                Maximum = 1024,
                Value = 512,
                Increment = 256,
                Dock = DockStyle.Fill,
                BackColor = darkBg2,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(0, 5, 10, 5)
            };
            layout.Controls.Add(numRsaKeySize, 1, 0);

            btnGenerateRsaKeys = CreateIconButton("Generate Keys", accentBlue, IconManager.IconType.Run, 140, 30);
            btnGenerateRsaKeys.Click += BtnGenerateRsaKeys_Click;
            btnGenerateRsaKeys.Margin = new Padding(0, 3, 0, 3);
            layout.Controls.Add(btnGenerateRsaKeys, 2, 0);
            layout.SetColumnSpan(btnGenerateRsaKeys, 2);

            // Row 2: Manual P, Q, E
            var pqePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };

            var lblP = CreateSmallLabel("P:");
            txtRsaP = CreateSmallTextBox(60);
            var lblQ = CreateSmallLabel("Q:");
            txtRsaQ = CreateSmallTextBox(60);
            var lblE = CreateSmallLabel("E:");
            txtRsaE = CreateSmallTextBox(60);
            txtRsaE.Text = "65537";

            var btnSetManual = CreateIconButton("Set", accentGreen, IconManager.IconType.Lock, 55, 28);
            btnSetManual.Click += BtnSetManualRsa_Click;
            btnSetManual.Margin = new Padding(5, 4, 0, 0);

            pqePanel.Controls.AddRange(new Control[] { lblP, txtRsaP, lblQ, txtRsaQ, lblE, txtRsaE, btnSetManual });
            layout.Controls.Add(pqePanel, 0, 1);
            layout.SetColumnSpan(pqePanel, 4);

            // Row 3: Key Info
            txtRsaKeyInfo = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 8F),
                BackColor = darkBg2,
                ForeColor = accentCyan,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                Margin = new Padding(0, 3, 0, 0)
            };
            layout.Controls.Add(txtRsaKeyInfo, 0, 2);
            layout.SetColumnSpan(txtRsaKeyInfo, 4);

            rsaSettingsPanel.Controls.Add(layout);
            UpdateRsaKeyInfo();
        }

        private Label CreateSmallLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = textColor,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 8, 3, 0)
            };
        }

        private TextBox CreateSmallTextBox(int width)
        {
            return new TextBox
            {
                Size = new Size(width, 26),
                BackColor = darkBg2,
                ForeColor = textColor,
                Font = new Font("Consolas", 9F),
                Margin = new Padding(0, 4, 8, 0),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void CreatePreviewPanel()
        {
            previewBox = new ModernGroupBox
            {
                Title = "FILE PREVIEW",
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Margin = new Padding(0, 10, 0, 10),
                BorderColor = accentBlue
            };

            previewContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // File type label
            fileTypeLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = accentBlue,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            // Image preview
            imagePreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Visible = false
            };

            // Text preview
            textPreview = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10F),
                BackColor = darkBg2,
                ForeColor = textColor,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Visible = false,
                WordWrap = false
            };

            // Hex preview
            hexPreview = new HexViewer
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // Empty state
            var emptyLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Select a file to preview\n\nSupported:\n- Images: JPG, PNG, GIF, BMP\n- Text: TXT, JSON, XML, CSV\n- Binary: All files (Hex view)",
                ForeColor = Color.FromArgb(120, 255, 255, 255),
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter
            };

            previewContent.Controls.Add(imagePreview);
            previewContent.Controls.Add(textPreview);
            previewContent.Controls.Add(hexPreview);
            previewContent.Controls.Add(emptyLabel);
            previewContent.Controls.Add(fileTypeLabel);

            previewBox.Controls.Add(previewContent);
        }

        private Panel CreateBottomPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 5, 0, 0)
            };

            // Progress bar
            progressBar = new ModernProgressBar
            {
                Dock = DockStyle.Top,
                Height = 28,
                ProgressColor = accentGreen,
                Visible = false
            };

            // Buttons panel
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0)
            };

            btnEncrypt = CreateIconButton("ENCRYPT", accentGreen, IconManager.IconType.Lock, 110, 40);
            btnEncrypt.Click += BtnEncrypt_Click;

            btnDecrypt = CreateIconButton("DECRYPT", accentOrange, IconManager.IconType.Security, 110, 40);
            btnDecrypt.Click += BtnDecrypt_Click;

            btnClear = CreateIconButton("CLEAR", accentRed, IconManager.IconType.Trash, 110, 40);
            btnClear.Click += BtnClear_Click;

            statusLabel = new AnimatedLabel
            {
                Margin = new Padding(20, 12, 0, 0)
            };

            btnPanel.Controls.Add(btnEncrypt);
            btnPanel.Controls.Add(btnDecrypt);
            btnPanel.Controls.Add(btnClear);
            btnPanel.Controls.Add(statusLabel);

            panel.Controls.Add(progressBar);
            panel.Controls.Add(btnPanel);

            return panel;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = textColor,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private IconButton CreateIconButton(string text, Color bgColor, IconManager.IconType iconType, int width, int height)
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

        private void CmbCipherType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateCipherSettings();
        }

        private void UpdateCipherSettings()
        {
            bool isPlayFair = cmbCipherType.SelectedIndex == 0;
            playFairSettingsPanel.Visible = isPlayFair;
            rsaSettingsPanel.Visible = !isPlayFair;
        }

        private void BtnGenerateRsaKeys_Click(object? sender, EventArgs e)
        {
            try
            {
                statusLabel.StartAnimation("Generating keys");
                _rsa.GenerateKeys((int)numRsaKeySize.Value);
                UpdateRsaKeyInfo();
                statusLabel.StopAnimation();
                statusLabel.ShowSuccess($"Generated RSA {numRsaKeySize.Value}-bit keys!");
            }
            catch (Exception ex)
            {
                statusLabel.StopAnimation();
                statusLabel.ShowError($"Error: {ex.Message}");
            }
        }

        private void BtnSetManualRsa_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!System.Numerics.BigInteger.TryParse(txtRsaP.Text, out var p) ||
                    !System.Numerics.BigInteger.TryParse(txtRsaQ.Text, out var q) ||
                    !System.Numerics.BigInteger.TryParse(txtRsaE.Text, out var eVal))
                {
                    statusLabel.ShowError("Please enter valid numbers for P, Q, E!");
                    return;
                }

                _rsa.SetManualKeys(p, q, eVal);
                UpdateRsaKeyInfo();
                statusLabel.ShowSuccess("Manual keys set successfully!");
            }
            catch (Exception ex)
            {
                statusLabel.ShowError($"Error: {ex.Message}");
            }
        }

        private void UpdateRsaKeyInfo()
        {
            txtRsaKeyInfo.Text = $"E = {_rsa.E}\r\nD = {TruncateNumber(_rsa.D)}\r\nN = {TruncateNumber(_rsa.N)}";
        }

        private string TruncateNumber(System.Numerics.BigInteger num)
        {
            string s = num.ToString();
            if (s.Length > 30)
                return s.Substring(0, 15) + "..." + s.Substring(s.Length - 10);
            return s;
        }

        private void DropPanel_FileDropped(object? sender, string filePath)
        {
            _currentFilePath = filePath;
            LoadFilePreview(filePath);
        }

        private void LoadFilePreview(string filePath)
        {
            // Reset all previews
            imagePreview.Visible = false;
            textPreview.Visible = false;
            hexPreview.Visible = false;
            imagePreview.Image?.Dispose();
            imagePreview.Image = null;

            var fileType = FilePreviewHelper.GetFileType(filePath);
            var fi = new FileInfo(filePath);
            
            fileTypeLabel.Text = $"{fi.Name} | {FilePreviewHelper.FormatFileSize(fi.Length)} | Type: {fileType}";

            try
            {
                switch (fileType)
                {
                    case FilePreviewHelper.FileType.Image:
                        var img = FilePreviewHelper.GetImagePreview(filePath, 800, 500);
                        if (img != null)
                        {
                            imagePreview.Image = img;
                            imagePreview.Visible = true;
                        }
                        break;

                    case FilePreviewHelper.FileType.Text:
                        textPreview.Text = FilePreviewHelper.GetTextPreview(filePath, 50000);
                        textPreview.Visible = true;
                        break;

                    default:
                        // Show hex view for binary/unknown files
                        var bytes = FilePreviewHelper.GetBinaryPreview(filePath, 4096);
                        hexPreview.Data = bytes;
                        hexPreview.Visible = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                fileTypeLabel.Text = $"⚠️ Error: {ex.Message}";
            }
        }

        private async void BtnEncrypt_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                statusLabel.ShowError("Please select a file first!");
                return;
            }

            bool isPlayFair = cmbCipherType.SelectedIndex == 0;

            if (isPlayFair && string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
            {
                statusLabel.ShowError("Please enter PlayFair key!");
                return;
            }

            string fileExtension = isPlayFair ? ".playfair" : ".rsa";
            using var sfd = new SaveFileDialog
            {
                Filter = isPlayFair 
                    ? "PlayFair Encrypted (*.playfair)|*.playfair|All Files (*.*)|*.*"
                    : "RSA Encrypted (*.rsa)|*.rsa|All Files (*.*)|*.*",
                Title = "Save Encrypted File",
                FileName = Path.GetFileName(_currentFilePath) + fileExtension
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            await EncryptFileAsync(_currentFilePath, sfd.FileName, isPlayFair);
        }

        private async void BtnDecrypt_Click(object? sender, EventArgs e)
        {
            bool isPlayFair = cmbCipherType.SelectedIndex == 0;

            if (isPlayFair && string.IsNullOrWhiteSpace(txtPlayFairKey.Text))
            {
                statusLabel.ShowError("Please enter PlayFair key!");
                return;
            }

            using var ofd = new OpenFileDialog
            {
                Filter = isPlayFair 
                    ? "PlayFair Encrypted (*.playfair)|*.playfair|All Files (*.*)|*.*"
                    : "RSA Encrypted (*.rsa)|*.rsa|All Files (*.*)|*.*",
                Title = "Select Encrypted File"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            // Determine original filename
            string originalName = Path.GetFileNameWithoutExtension(ofd.FileName);

            using var sfd = new SaveFileDialog
            {
                Title = "Save Decrypted File",
                FileName = originalName
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            await DecryptFileAsync(ofd.FileName, sfd.FileName, isPlayFair);
        }

        private async Task EncryptFileAsync(string inputPath, string outputPath, bool usePlayFair)
        {
            SetProcessingState(true);
            string cipherName = usePlayFair ? "PlayFair" : "RSA";
            statusLabel.StartAnimation($"Encrypting ({cipherName})");

            var progress = new Progress<int>(p =>
            {
                progressBar.Value = p;
            });

            try
            {
                await Task.Run(() =>
                {
                    if (usePlayFair)
                    {
                        _playFair.Key = txtPlayFairKey.Text;
                        _playFair.EncryptFile(inputPath, outputPath, progress);
                    }
                    else
                    {
                        _rsa.EncryptFile(inputPath, outputPath, progress);
                    }
                });
                
                statusLabel.StopAnimation();
                statusLabel.ShowSuccess($"Encryption successful!");
                
                MessageBox.Show(
                    $"✅ File encrypted successfully!\n\n" +
                    $"📁 Output: {Path.GetFileName(outputPath)}\n" +
                    $"📊 Size: {FilePreviewHelper.FormatFileSize(new FileInfo(outputPath).Length)}\n" +
                    $"🔐 Algorithm: {cipherName}\n\n" +
                    (usePlayFair ? "⚠️ Remember your key for decryption!" : "⚠️ Use correct RSA key for decryption!"),
                    "Encryption Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                statusLabel.StopAnimation();
                statusLabel.ShowError($"Encryption error: {ex.Message}");
                MessageBox.Show($"❌ Encryption failed!\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetProcessingState(false);
            }
        }

        private async Task DecryptFileAsync(string inputPath, string outputPath, bool usePlayFair)
        {
            SetProcessingState(true);
            string cipherName = usePlayFair ? "PlayFair" : "RSA";
            statusLabel.StartAnimation($"Decrypting ({cipherName})");

            var progress = new Progress<int>(p =>
            {
                progressBar.Value = p;
            });

            try
            {
                await Task.Run(() =>
                {
                    if (usePlayFair)
                    {
                        _playFair.Key = txtPlayFairKey.Text;
                        _playFair.DecryptFile(inputPath, outputPath, progress);
                    }
                    else
                    {
                        _rsa.DecryptFile(inputPath, outputPath, progress);
                    }
                });
                
                statusLabel.StopAnimation();
                statusLabel.ShowSuccess($"Decryption successful!");

                // Load preview of decrypted file
                dropPanel.SetFile(outputPath);
                
                MessageBox.Show(
                    $"✅ File decrypted successfully!\n\n" +
                    $"📁 Output: {Path.GetFileName(outputPath)}\n" +
                    $"📊 Size: {FilePreviewHelper.FormatFileSize(new FileInfo(outputPath).Length)}",
                    "Decryption Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (InvalidDataException ex)
            {
                statusLabel.StopAnimation();
                statusLabel.ShowError("Wrong key or corrupted file!");
                MessageBox.Show($"❌ Decryption failed!\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                statusLabel.StopAnimation();
                statusLabel.ShowError($"Decryption error: {ex.Message}");
                MessageBox.Show($"❌ Decryption failed!\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetProcessingState(false);
            }
        }

        private void SetProcessingState(bool processing)
        {
            progressBar.Visible = processing;
            progressBar.Value = 0;
            btnEncrypt.Enabled = !processing;
            btnDecrypt.Enabled = !processing;
            dropPanel.Enabled = !processing;
            cmbCipherType.Enabled = !processing;
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _currentFilePath = "";
            dropPanel.ClearFile();
            txtPlayFairKey.Text = "SECURITY";
            
            imagePreview.Image?.Dispose();
            imagePreview.Image = null;
            imagePreview.Visible = false;
            textPreview.Visible = false;
            hexPreview.Visible = false;
            fileTypeLabel.Text = "";

            statusLabel.ShowSuccess("Cleared!");
        }
    }
}
