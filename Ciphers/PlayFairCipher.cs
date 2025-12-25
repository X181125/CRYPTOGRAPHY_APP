using System.Text;

namespace Cryptography_App.Ciphers
{
    public class PlayFairCipher
    {
        private char[,] _matrix = new char[5, 5];
        private string _key = "";

        // Class để lưu thông tin từng bước mã hóa cho DataGridView
        public class EncryptionStep
        {
            public string Pair { get; set; } = "";
            public string Pos1 { get; set; } = "";
            public string Pos2 { get; set; } = "";
            public string Rule { get; set; } = "";
            public string Result { get; set; } = "";
        }

        public string Key
        {
            get => _key;
            set
            {
                _key = value.ToUpper().Replace("J", "I");
                GenerateMatrix();
            }
        }

        public char[,] Matrix => _matrix;

        public PlayFairCipher(string key = "")
        {
            Key = key;
        }

        private void GenerateMatrix()
        {
            string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ"; // No J
            string keyString = _key + alphabet;
            
            HashSet<char> used = new HashSet<char>();
            StringBuilder matrixString = new StringBuilder();

            foreach (char c in keyString)
            {
                if (char.IsLetter(c))
                {
                    char upper = char.ToUpper(c);
                    if (upper == 'J') upper = 'I';
                    
                    if (!used.Contains(upper))
                    {
                        used.Add(upper);
                        matrixString.Append(upper);
                    }
                }
            }

            int index = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    _matrix[i, j] = matrixString[index++];
                }
            }
        }

        private (int row, int col) FindPosition(char c)
        {
            c = char.ToUpper(c);
            if (c == 'J') c = 'I';

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (_matrix[i, j] == c)
                        return (i, j);
                }
            }
            return (-1, -1);
        }

        private string PrepareText(string text)
        {
            StringBuilder result = new StringBuilder();
            text = text.ToUpper().Replace("J", "I");

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                    result.Append(c);
            }

            // Insert X between duplicate letters in a pair
            StringBuilder prepared = new StringBuilder();
            int i = 0;
            while (i < result.Length)
            {
                prepared.Append(result[i]);
                
                if (i + 1 < result.Length)
                {
                    if (result[i] == result[i + 1])
                    {
                        prepared.Append('X');
                    }
                    else
                    {
                        prepared.Append(result[i + 1]);
                        i++;
                    }
                }
                i++;
            }

            // Add X if odd length
            if (prepared.Length % 2 != 0)
                prepared.Append('X');

            return prepared.ToString();
        }

        public string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            string prepared = PrepareText(plaintext);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < prepared.Length; i += 2)
            {
                char a = prepared[i];
                char b = prepared[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == row2)
                {
                    // Same row - shift right
                    result.Append(_matrix[row1, (col1 + 1) % 5]);
                    result.Append(_matrix[row2, (col2 + 1) % 5]);
                }
                else if (col1 == col2)
                {
                    // Same column - shift down
                    result.Append(_matrix[(row1 + 1) % 5, col1]);
                    result.Append(_matrix[(row2 + 1) % 5, col2]);
                }
                else
                {
                    // Rectangle - swap columns
                    result.Append(_matrix[row1, col2]);
                    result.Append(_matrix[row2, col1]);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Mã hóa và trả về quá trình mã hóa chi tiết
        /// </summary>
        public (string result, string process) EncryptWithProcess(string plaintext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            string prepared = PrepareText(plaintext);
            StringBuilder result = new StringBuilder();
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔══════════════════════════════════════════════════╗");
            process.AppendLine("║       QUÁ TRÌNH MÃ HÓA PLAYFAIR CHI TIẾT         ║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine($"║ Key: {_key.PadRight(43)}║");
            process.AppendLine($"║ Input gốc: {plaintext.PadRight(37)}║");
            process.AppendLine($"║ Sau chuẩn bị: {prepared.PadRight(34)}║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine("║  Cặp  │ Vị trí 1  │ Vị trí 2  │ Quy tắc   │ KQ   ║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");

            for (int i = 0; i < prepared.Length; i += 2)
            {
                char a = prepared[i];
                char b = prepared[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                char c1, c2;
                string rule;

                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 1) % 5];
                    c2 = _matrix[row2, (col2 + 1) % 5];
                    rule = "Same Row ";
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 1) % 5, col1];
                    c2 = _matrix[(row2 + 1) % 5, col2];
                    rule = "Same Col ";
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                    rule = "Rectangle";
                }

                result.Append(c1);
                result.Append(c2);

                process.AppendLine($"║  {a}{b}   │ ({row1},{col1})      │ ({row2},{col2})      │ {rule} │ {c1}{c2}   ║");
            }

            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine($"║ RESULT: {result.ToString().PadRight(39)}║");
            process.AppendLine("╚══════════════════════════════════════════════════╝");

            return (result.ToString(), process.ToString());
        }

        /// <summary>
        /// Mã hóa và trả về dữ liệu dạng List cho DataGridView
        /// </summary>
        public (string result, List<EncryptionStep> steps) EncryptWithProcessData(string plaintext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            string prepared = PrepareText(plaintext);
            StringBuilder result = new StringBuilder();
            var steps = new List<EncryptionStep>();

            for (int i = 0; i < prepared.Length; i += 2)
            {
                char a = prepared[i];
                char b = prepared[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                char c1, c2;
                string rule;

                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 1) % 5];
                    c2 = _matrix[row2, (col2 + 1) % 5];
                    rule = "Same Row";
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 1) % 5, col1];
                    c2 = _matrix[(row2 + 1) % 5, col2];
                    rule = "Same Col";
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                    rule = "Rectangle";
                }

                result.Append(c1);
                result.Append(c2);

                steps.Add(new EncryptionStep
                {
                    Pair = $"{a}{b}",
                    Pos1 = $"({row1},{col1})",
                    Pos2 = $"({row2},{col2})",
                    Rule = rule,
                    Result = $"{c1}{c2}"
                });
            }

            return (result.ToString(), steps);
        }

        /// <summary>
        /// Decrypt and return detailed decryption process
        /// </summary>
        public (string result, string process) DecryptWithProcess(string ciphertext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            ciphertext = ciphertext.ToUpper().Replace(" ", "").Replace("J", "I");
            StringBuilder result = new StringBuilder();
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔══════════════════════════════════════════════════╗");
            process.AppendLine("║       QUÁ TRÌNH GIẢI MÃ PLAYFAIR CHI TIẾT        ║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine($"║ Key: {_key.PadRight(43)}║");
            process.AppendLine($"║ Cipher text: {ciphertext.PadRight(35)}║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine("║  Cặp  │ Vị trí 1  │ Vị trí 2  │ Quy tắc   │ KQ   ║");
            process.AppendLine("╠══════════════════════════════════════════════════╣");

            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                if (i + 1 >= ciphertext.Length) break;

                char a = ciphertext[i];
                char b = ciphertext[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == -1 || row2 == -1) continue;

                char c1, c2;
                string rule;

                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 4) % 5];
                    c2 = _matrix[row2, (col2 + 4) % 5];
                    rule = "Same Row ";
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 4) % 5, col1];
                    c2 = _matrix[(row2 + 4) % 5, col2];
                    rule = "Same Col ";
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                    rule = "Rectangle";
                }

                result.Append(c1);
                result.Append(c2);

                process.AppendLine($"║  {a}{b}   │ ({row1},{col1})      │ ({row2},{col2})      │ {rule} │ {c1}{c2}   ║");
            }

            process.AppendLine("╠══════════════════════════════════════════════════╣");
            process.AppendLine($"║ RESULT: {result.ToString().PadRight(39)}║");
            process.AppendLine("╚══════════════════════════════════════════════════╝");

            return (result.ToString(), process.ToString());
        }

        /// <summary>
        /// Decrypt and return data as List for DataGridView
        /// </summary>
        public (string result, List<EncryptionStep> steps) DecryptWithProcessData(string ciphertext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            ciphertext = ciphertext.ToUpper().Replace(" ", "").Replace("J", "I");
            StringBuilder result = new StringBuilder();
            var steps = new List<EncryptionStep>();

            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                if (i + 1 >= ciphertext.Length) break;

                char a = ciphertext[i];
                char b = ciphertext[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == -1 || row2 == -1) continue;

                char c1, c2;
                string rule;

                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 4) % 5];
                    c2 = _matrix[row2, (col2 + 4) % 5];
                    rule = "Same Row";
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 4) % 5, col1];
                    c2 = _matrix[(row2 + 4) % 5, col2];
                    rule = "Same Col";
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                    rule = "Rectangle";
                }

                result.Append(c1);
                result.Append(c2);

                steps.Add(new EncryptionStep
                {
                    Pair = $"{a}{b}",
                    Pos1 = $"({row1},{col1})",
                    Pos2 = $"({row2},{col2})",
                    Rule = rule,
                    Result = $"{c1}{c2}"
                });
            }

            return (result.ToString(), steps);
        }

        public string Decrypt(string ciphertext)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            ciphertext = ciphertext.ToUpper().Replace(" ", "").Replace("J", "I");
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                if (i + 1 >= ciphertext.Length) break;

                char a = ciphertext[i];
                char b = ciphertext[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == -1 || row2 == -1) continue;

                if (row1 == row2)
                {
                    // Same row - shift left
                    result.Append(_matrix[row1, (col1 + 4) % 5]);
                    result.Append(_matrix[row2, (col2 + 4) % 5]);
                }
                else if (col1 == col2)
                {
                    // Same column - shift up
                    result.Append(_matrix[(row1 + 4) % 5, col1]);
                    result.Append(_matrix[(row2 + 4) % 5, col2]);
                }
                else
                {
                    // Rectangle - swap columns
                    result.Append(_matrix[row1, col2]);
                    result.Append(_matrix[row2, col1]);
                }
            }

            return result.ToString();
        }

        public string GetMatrixDisplay()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    sb.Append(_matrix[i, j]);
                    if (j < 4) sb.Append("  ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #region File Encryption/Decryption

        /// <summary>
        /// Mã hóa file sử dụng PlayFair Cipher
        /// File được mã hóa bằng cách chuyển từng byte thành 2 ký tự, sau đó mã hóa
        /// </summary>
        public void EncryptFile(string inputPath, string outputPath, IProgress<int>? progress = null)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            // Đọc file dưới dạng bytes
            byte[] fileBytes = File.ReadAllBytes(inputPath);
            
            // Chuyển mỗi byte thành 2 ký tự (bỏ qua J vì PlayFair không có J)
            // 0-8 -> A-I, 9-15 -> K-Q (bỏ J)
            StringBuilder hexLetters = new StringBuilder();
            for (int i = 0; i < fileBytes.Length; i++)
            {
                byte b = fileBytes[i];
                int high = b >> 4;       // 4 bit cao (0-15)
                int low = b & 0x0F;      // 4 bit thấp (0-15)
                
                // Chuyển nibble thành ký tự, bỏ qua J
                hexLetters.Append(NibbleToChar(high));
                hexLetters.Append(NibbleToChar(low));
                
                progress?.Report((int)(i * 50.0 / fileBytes.Length)); // 50% cho việc chuyển đổi
            }

            // Mã hóa PlayFair
            string toEncrypt = hexLetters.ToString();
            StringBuilder encrypted = new StringBuilder();
            
            for (int i = 0; i < toEncrypt.Length; i += 2)
            {
                char a = toEncrypt[i];
                char b = toEncrypt[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                char c1, c2;
                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 1) % 5];
                    c2 = _matrix[row2, (col2 + 1) % 5];
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 1) % 5, col1];
                    c2 = _matrix[(row2 + 1) % 5, col2];
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                }
                encrypted.Append(c1);
                encrypted.Append(c2);
                
                progress?.Report(50 + (int)(i * 50.0 / toEncrypt.Length)); // 50-100% cho mã hóa
            }

            // Lưu với header để nhận dạng
            string header = $"PLAYFAIR_ENCRYPTED|{fileBytes.Length}\n";
            File.WriteAllText(outputPath, header + encrypted.ToString());
            progress?.Report(100);
        }

        /// <summary>
        /// Giải mã file đã được mã hóa bằng PlayFair Cipher
        /// </summary>
        public void DecryptFile(string inputPath, string outputPath, IProgress<int>? progress = null)
        {
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentException("Key cannot be empty!");

            string content = File.ReadAllText(inputPath);
            
            // Kiểm tra và parse header
            if (!content.StartsWith("PLAYFAIR_ENCRYPTED|"))
                throw new InvalidDataException("File này không được mã hóa bằng PlayFair hoặc đã bị hỏng!");

            int headerEnd = content.IndexOf('\n');
            string header = content.Substring(0, headerEnd);
            string[] headerParts = header.Split('|');
            
            if (headerParts.Length != 2 || !int.TryParse(headerParts[1], out int originalLength))
                throw new InvalidDataException("Header file bị hỏng!");

            string encryptedContent = content.Substring(headerEnd + 1);

            // Giải mã PlayFair
            StringBuilder decrypted = new StringBuilder();
            
            for (int i = 0; i < encryptedContent.Length; i += 2)
            {
                if (i + 1 >= encryptedContent.Length) break;
                
                char a = encryptedContent[i];
                char b = encryptedContent[i + 1];

                var (row1, col1) = FindPosition(a);
                var (row2, col2) = FindPosition(b);

                if (row1 == -1 || row2 == -1) 
                    throw new InvalidDataException("Không thể giải mã file. Sai key hoặc file bị hỏng!");

                char c1, c2;
                if (row1 == row2)
                {
                    c1 = _matrix[row1, (col1 + 4) % 5];
                    c2 = _matrix[row2, (col2 + 4) % 5];
                }
                else if (col1 == col2)
                {
                    c1 = _matrix[(row1 + 4) % 5, col1];
                    c2 = _matrix[(row2 + 4) % 5, col2];
                }
                else
                {
                    c1 = _matrix[row1, col2];
                    c2 = _matrix[row2, col1];
                }
                decrypted.Append(c1);
                decrypted.Append(c2);
                
                progress?.Report((int)(i * 50.0 / encryptedContent.Length)); // 0-50% cho giải mã
            }

            // Chuyển từ ký tự về bytes (dùng CharToNibble để xử lý đúng việc bỏ J)
            string hexLetters = decrypted.ToString();
            byte[] fileBytes = new byte[originalLength];
            
            try
            {
                for (int i = 0; i < originalLength; i++)
                {
                    int idx = i * 2;
                    if (idx + 1 >= hexLetters.Length)
                        throw new InvalidDataException("Dữ liệu giải mã không đủ!");
                    
                    int high = CharToNibble(hexLetters[idx]);
                    int low = CharToNibble(hexLetters[idx + 1]);
                    
                    if (high < 0 || high > 15 || low < 0 || low > 15)
                        throw new InvalidDataException("Không thể giải mã file. Sai key hoặc file bị hỏng!");
                    
                    fileBytes[i] = (byte)((high << 4) | low);
                    
                    progress?.Report(50 + (int)(i * 50.0 / originalLength)); // 50-100% cho chuyển đổi
                }
                
                File.WriteAllBytes(outputPath, fileBytes);
            }
            catch (Exception ex) when (ex is not InvalidDataException)
            {
                throw new InvalidDataException("Không thể giải mã file. Sai key hoặc file bị hỏng!");
            }
            
            progress?.Report(100);
        }

        /// <summary>
        /// Chuyển nibble (0-15) thành ký tự, bỏ qua J
        /// 0-8 -> A-I, 9-15 -> K-Q
        /// </summary>
        private static char NibbleToChar(int nibble)
        {
            if (nibble <= 8) return (char)('A' + nibble);       // A-I (0-8)
            else return (char)('A' + nibble + 1);               // K-Q (9-15, bỏ J)
        }

        /// <summary>
        /// Chuyển ký tự về nibble (0-15), bỏ qua J
        /// A-I -> 0-8, K-Q -> 9-15
        /// </summary>
        private static int CharToNibble(char c)
        {
            c = char.ToUpper(c);
            if (c <= 'I') return c - 'A';                       // A-I -> 0-8
            else if (c >= 'K') return c - 'A' - 1;              // K-Q -> 9-15 (K=10-1=9, ..., Q=16-1=15)
            else return -1;                                      // J không hợp lệ
        }

        /// <summary>
        /// Kiểm tra file có được mã hóa bằng PlayFair không
        /// </summary>
        public static bool IsPlayFairEncryptedFile(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                string? firstLine = reader.ReadLine();
                return firstLine != null && firstLine.StartsWith("PLAYFAIR_ENCRYPTED|");
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
