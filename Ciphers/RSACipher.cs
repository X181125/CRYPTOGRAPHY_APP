using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Cryptography_App.Ciphers
{
    public class RSACipher
    {
        public BigInteger P { get; private set; }
        public BigInteger Q { get; private set; }
        public BigInteger N { get; private set; }
        public BigInteger Phi { get; private set; }
        public BigInteger E { get; private set; }
        public BigInteger D { get; private set; }

        public string PublicKey => $"({E}, {N})";
        public string PrivateKey => $"({D}, {N})";

        // Class để lưu thông tin từng bước mã hóa cho DataGridView
        public class EncryptionStep
        {
            public string Character { get; set; } = "";
            public string Ascii { get; set; } = "";
            public string M { get; set; } = "";
            public string Result { get; set; } = "";
        }

        public RSACipher()
        {
            GenerateKeys(512); // Default 512-bit for demo
        }

        public void GenerateKeys(int bitLength = 512)
        {
            P = GeneratePrime(bitLength / 2);
            Q = GeneratePrime(bitLength / 2);

            while (P == Q)
                Q = GeneratePrime(bitLength / 2);

            N = P * Q;
            Phi = (P - 1) * (Q - 1);

            // Choose E
            E = 65537;
            while (BigInteger.GreatestCommonDivisor(E, Phi) != 1)
            {
                E += 2;
            }

            // Calculate D
            D = ModInverse(E, Phi);
        }

        public void SetManualKeys(BigInteger p, BigInteger q, BigInteger e)
        {
            if (!IsProbablyPrime(p) || !IsProbablyPrime(q))
                throw new ArgumentException("P and Q must be prime numbers!");

            P = p;
            Q = q;
            N = P * Q;
            Phi = (P - 1) * (Q - 1);

            if (BigInteger.GreatestCommonDivisor(e, Phi) != 1)
                throw new ArgumentException("E must be coprime with ?(N)!");

            E = e;
            D = ModInverse(E, Phi);
        }

        public void SetKeysDirectly(BigInteger e, BigInteger d, BigInteger n)
        {
            E = e;
            D = d;
            N = n;
            // P, Q, Phi won't be known in this case
            P = 0;
            Q = 0;
            Phi = 0;
        }

        private BigInteger GeneratePrime(int bitLength)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[bitLength / 8];
                BigInteger candidate;

                do
                {
                    rng.GetBytes(bytes);
                    bytes[bytes.Length - 1] &= 0x7F; // Ensure positive
                    bytes[0] |= 0x01; // Ensure odd
                    candidate = new BigInteger(bytes);
                } while (!IsProbablyPrime(candidate) || candidate < 0);

                return candidate;
            }
        }

        private bool IsProbablyPrime(BigInteger n, int k = 10)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            // Write n-1 as 2^r * d
            BigInteger d = n - 1;
            int r = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[n.ToByteArray().Length];

                for (int i = 0; i < k; i++)
                {
                    BigInteger a;
                    do
                    {
                        rng.GetBytes(bytes);
                        a = new BigInteger(bytes);
                        a = BigInteger.Abs(a) % (n - 4) + 2;
                    } while (a < 2 || a >= n - 2);

                    BigInteger x = BigInteger.ModPow(a, d, n);

                    if (x == 1 || x == n - 1)
                        continue;

                    bool composite = true;
                    for (int j = 0; j < r - 1; j++)
                    {
                        x = BigInteger.ModPow(x, 2, n);
                        if (x == n - 1)
                        {
                            composite = false;
                            break;
                        }
                    }

                    if (composite)
                        return false;
                }
            }

            return true;
        }

        private BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m, x0 = 0, x1 = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = x0;

                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }

        public string Encrypt(string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            List<string> encrypted = new List<string>();

            // Encrypt each byte separately for simplicity
            foreach (byte b in bytes)
            {
                BigInteger m = new BigInteger(b);
                BigInteger c = BigInteger.ModPow(m, E, N);
                encrypted.Add(c.ToString());
            }

            return string.Join(" ", encrypted);
        }

        /// <summary>
        /// Mã hóa và trả về quá trình mã hóa chi tiết
        /// </summary>
        public (string result, string process) EncryptWithProcess(string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            List<string> encrypted = new List<string>();
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔════════════════════════════════════════════════════════════════════╗");
            process.AppendLine("║              QUÁ TRÌNH MÃ HÓA RSA CHI TIẾT                         ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ Public Key (E, N):                                                 ║");
            process.AppendLine($"║   E = {TruncateNumber(E, 60).PadRight(62)}║");
            process.AppendLine($"║   N = {TruncateNumber(N, 60).PadRight(62)}║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ Công thức: C = M^E mod N                                           ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ Plain Text: \"{plaintext}\"".PadRight(69) + "║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ Ký tự │ ASCII │ M (decimal) │ C = M^E mod N                        ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");

            int count = 0;
            foreach (byte b in bytes)
            {
                BigInteger m = new BigInteger(b);
                BigInteger c = BigInteger.ModPow(m, E, N);
                encrypted.Add(c.ToString());

                char ch = (char)b;
                string charDisplay = char.IsControl(ch) ? "?" : ch.ToString();
                string cTruncated = TruncateNumber(c, 40);
                
                if (count < 10) // Hiển thị 10 ký tự đầu
                {
                    process.AppendLine($"║  '{charDisplay}'   │  {b,3}  │    {m,-8}  │ {cTruncated.PadRight(39)}║");
                }
                count++;
            }

            if (count > 10)
            {
                process.AppendLine($"║  ... và {count - 10} ký tự khác ...".PadRight(69) + "║");
            }

            string result = string.Join(" ", encrypted);

            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ TỔNG SỐ KÝ TỰ ĐÃ MÃ HÓA: {count}".PadRight(69) + "║");
            process.AppendLine("╚════════════════════════════════════════════════════════════════════╝");

            return (result, process.ToString());
        }

        /// <summary>
        /// Mã hóa và trả về dữ liệu dạng List cho DataGridView
        /// </summary>
        public (string result, List<EncryptionStep> steps) EncryptWithProcessData(string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            List<string> encrypted = new List<string>();
            var steps = new List<EncryptionStep>();

            foreach (byte b in bytes)
            {
                BigInteger m = new BigInteger(b);
                BigInteger c = BigInteger.ModPow(m, E, N);
                encrypted.Add(c.ToString());

                char ch = (char)b;
                string charDisplay = char.IsControl(ch) ? "?" : $"'{ch}'";
                
                steps.Add(new EncryptionStep
                {
                    Character = charDisplay,
                    Ascii = b.ToString(),
                    M = b.ToString(),
                    Result = TruncateNumber(c, 25)
                });
            }

            return (string.Join(" ", encrypted), steps);
        }

        /// <summary>
        /// Giải mã và trả về dữ liệu dạng List cho DataGridView
        /// </summary>
        public (string result, List<EncryptionStep> steps) DecryptWithProcessData(string ciphertext)
        {
            string[] parts = ciphertext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<byte> decrypted = new List<byte>();
            var steps = new List<EncryptionStep>();

            foreach (string part in parts)
            {
                if (BigInteger.TryParse(part, out BigInteger c))
                {
                    BigInteger m = BigInteger.ModPow(c, D, N);
                    byte b = (byte)m;
                    decrypted.Add(b);

                    char ch = (char)b;
                    string charDisplay = char.IsControl(ch) ? "?" : $"'{ch}'";
                    
                    steps.Add(new EncryptionStep
                    {
                        Character = charDisplay,
                        Ascii = b.ToString(),
                        M = TruncateNumber(c, 15),
                        Result = b.ToString()
                    });
                }
            }

            return (Encoding.UTF8.GetString(decrypted.ToArray()), steps);
        }

        public string Decrypt(string ciphertext)
        {
            string[] parts = ciphertext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<byte> decrypted = new List<byte>();

            foreach (string part in parts)
            {
                if (BigInteger.TryParse(part, out BigInteger c))
                {
                    BigInteger m = BigInteger.ModPow(c, D, N);
                    decrypted.Add((byte)m);
                }
            }

            return Encoding.UTF8.GetString(decrypted.ToArray());
        }

        /// <summary>
        /// Giải mã và trả về quá trình giải mã chi tiết
        /// </summary>
        public (string result, string process) DecryptWithProcess(string ciphertext)
        {
            string[] parts = ciphertext.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<byte> decrypted = new List<byte>();
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔════════════════════════════════════════════════════════════════════╗");
            process.AppendLine("║              QUÁ TRÌNH GIẢI MÃ RSA CHI TIẾT                        ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ Private Key (D, N):                                                ║");
            process.AppendLine($"║   D = {TruncateNumber(D, 60).PadRight(62)}║");
            process.AppendLine($"║   N = {TruncateNumber(N, 60).PadRight(62)}║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ Công thức: M = C^D mod N                                           ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ STT │ C (Cipher)                      │ M │ Ký tự                  ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");

            int count = 0;
            foreach (string part in parts)
            {
                if (BigInteger.TryParse(part, out BigInteger c))
                {
                    BigInteger m = BigInteger.ModPow(c, D, N);
                    decrypted.Add((byte)m);

                    char ch = (char)(byte)m;
                    string charDisplay = char.IsControl(ch) ? "?" : ch.ToString();
                    string cTruncated = TruncateNumber(c, 30);

                    if (count < 10)
                    {
                        process.AppendLine($"║ {count + 1,3} │ {cTruncated.PadRight(32)}│ {(byte)m,3} │ '{charDisplay}'".PadRight(69) + "║");
                    }
                    count++;
                }
            }

            if (count > 10)
            {
                process.AppendLine($"║  ... và {count - 10} giá trị khác ...".PadRight(69) + "║");
            }

            string result = Encoding.UTF8.GetString(decrypted.ToArray());

            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ KẾT QUẢ: \"{result}\"".PadRight(69) + "║");
            process.AppendLine("╚════════════════════════════════════════════════════════════════════╝");

            return (result, process.ToString());
        }

        /// <summary>
        /// Mã hóa số và trả về quá trình chi tiết
        /// </summary>
        public (BigInteger result, string process) EncryptNumberWithProcess(BigInteger m)
        {
            BigInteger c = BigInteger.ModPow(m, E, N);
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔════════════════════════════════════════════════════════════════════╗");
            process.AppendLine("║              MÃ HÓA SỐ RSA CHI TIẾT                                ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ CÔNG THỨC: C = M^E mod N                                           ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ M (số cần mã hóa) = {TruncateNumber(m, 47).PadRight(48)}║");
            process.AppendLine($"║ E (public exp)    = {TruncateNumber(E, 47).PadRight(48)}║");
            process.AppendLine($"║ N (modulus)       = {TruncateNumber(N, 47).PadRight(48)}║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ C = {TruncateNumber(m, 15)}^{E} mod {TruncateNumber(N, 20)}".PadRight(69) + "║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ KẾT QUẢ: C = {TruncateNumber(c, 54).PadRight(55)}║");
            process.AppendLine("╚════════════════════════════════════════════════════════════════════╝");

            return (c, process.ToString());
        }

        /// <summary>
        /// Giải mã số và trả về quá trình chi tiết
        /// </summary>
        public (BigInteger result, string process) DecryptNumberWithProcess(BigInteger c)
        {
            BigInteger m = BigInteger.ModPow(c, D, N);
            StringBuilder process = new StringBuilder();

            process.AppendLine("╔════════════════════════════════════════════════════════════════════╗");
            process.AppendLine("║              GIẢI MÃ SỐ RSA CHI TIẾT                               ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine("║ CÔNG THỨC: M = C^D mod N                                           ║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ C (cipher)        = {TruncateNumber(c, 47).PadRight(48)}║");
            process.AppendLine($"║ D (private exp)   = {TruncateNumber(D, 47).PadRight(48)}║");
            process.AppendLine($"║ N (modulus)       = {TruncateNumber(N, 47).PadRight(48)}║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ M = {TruncateNumber(c, 15)}^D mod {TruncateNumber(N, 20)}".PadRight(69) + "║");
            process.AppendLine("╠════════════════════════════════════════════════════════════════════╣");
            process.AppendLine($"║ KẾT QUẢ: M = {TruncateNumber(m, 54).PadRight(55)}║");
            process.AppendLine("╚════════════════════════════════════════════════════════════════════╝");

            return (m, process.ToString());
        }

        public BigInteger EncryptNumber(BigInteger m)
        {
            return BigInteger.ModPow(m, E, N);
        }

        public BigInteger DecryptNumber(BigInteger c)
        {
            return BigInteger.ModPow(c, D, N);
        }

        private string TruncateNumber(BigInteger num, int maxLen)
        {
            string s = num.ToString();
            if (s.Length > maxLen)
                return s.Substring(0, maxLen / 2 - 1) + "..." + s.Substring(s.Length - maxLen / 2 + 2);
            return s;
        }

        public string GetKeyInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════");
            sb.AppendLine("      RSA KEY INFORMATION");
            sb.AppendLine("═══════════════════════════════════");
            if (P != 0)
            {
                sb.AppendLine($"  P = {TruncateNumber(P, 40)}");
                sb.AppendLine($"  Q = {TruncateNumber(Q, 40)}");
                sb.AppendLine($"  φ(N) = {TruncateNumber(Phi, 38)}");
            }
            sb.AppendLine($"  N = {TruncateNumber(N, 40)}");
            sb.AppendLine($"  E = {E}");
            sb.AppendLine($"  D = {TruncateNumber(D, 40)}");
            sb.AppendLine("═══════════════════════════════════");
            sb.AppendLine("  Public Key: (E, N)");
            sb.AppendLine("  Private Key: (D, N)");
            sb.AppendLine("═══════════════════════════════════");
            return sb.ToString();
        }

        public void ExportKeys(string publicKeyPath, string privateKeyPath)
        {
            File.WriteAllText(publicKeyPath, $"{E}\n{N}");
            File.WriteAllText(privateKeyPath, $"{D}\n{N}");
        }

        public void ImportPublicKey(string path)
        {
            string[] lines = File.ReadAllLines(path);
            E = BigInteger.Parse(lines[0]);
            N = BigInteger.Parse(lines[1]);
        }

        public void ImportPrivateKey(string path)
        {
            string[] lines = File.ReadAllLines(path);
            D = BigInteger.Parse(lines[0]);
            N = BigInteger.Parse(lines[1]);
        }

        #region File Encryption/Decryption

        /// <summary>
        /// Mã hóa file sử dụng RSA
        /// Do RSA chỉ mã hóa được số nhỏ hơn N, nên mỗi byte được mã hóa riêng
        /// </summary>
        public void EncryptFile(string inputPath, string outputPath, IProgress<int>? progress = null)
        {
            byte[] fileBytes = File.ReadAllBytes(inputPath);
            long totalBytes = fileBytes.Length;
            
            using var writer = new StreamWriter(outputPath);
            // Ghi header
            writer.WriteLine("RSA_ENCRYPTED");
            writer.WriteLine(fileBytes.Length); // Lưu kích thước gốc
            
            for (int i = 0; i < fileBytes.Length; i++)
            {
                BigInteger m = new BigInteger(fileBytes[i]);
                BigInteger c = BigInteger.ModPow(m, E, N);
                writer.WriteLine(c.ToString());
                
                if (i % 1000 == 0)
                    progress?.Report((int)(i * 100.0 / totalBytes));
            }
            
            progress?.Report(100);
        }

        /// <summary>
        /// Giải mã file đã được mã hóa bằng RSA
        /// </summary>
        public void DecryptFile(string inputPath, string outputPath, IProgress<int>? progress = null)
        {
            string[] lines = File.ReadAllLines(inputPath);
            
            // Kiểm tra header
            if (lines.Length < 2 || lines[0] != "RSA_ENCRYPTED")
                throw new InvalidDataException("File này không được mã hóa bằng RSA hoặc đã bị hỏng!");

            if (!long.TryParse(lines[1], out long fileSize))
                throw new InvalidDataException("File header không hợp lệ!");

            List<byte> decryptedBytes = new List<byte>();
            long totalLines = lines.Length - 2;
            
            for (int i = 2; i < lines.Length; i++)
            {
                if (BigInteger.TryParse(lines[i], out BigInteger c))
                {
                    BigInteger m = BigInteger.ModPow(c, D, N);
                    
                    // Kiểm tra giá trị hợp lệ cho byte
                    if (m >= 0 && m <= 255)
                    {
                        decryptedBytes.Add((byte)m);
                    }
                    else
                    {
                        throw new InvalidDataException("Không thể giải mã. Sai key hoặc file bị hỏng!");
                    }
                }
                
                if ((i - 2) % 1000 == 0)
                    progress?.Report((int)((i - 2) * 100.0 / totalLines));
            }

            if (decryptedBytes.Count != fileSize)
                throw new InvalidDataException("Kích thước file sau giải mã không khớp. File có thể bị hỏng!");

            File.WriteAllBytes(outputPath, decryptedBytes.ToArray());
            progress?.Report(100);
        }

        /// <summary>
        /// Kiểm tra file có được mã hóa bằng RSA không
        /// </summary>
        public static bool IsRSAEncryptedFile(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                string? firstLine = reader.ReadLine();
                return firstLine == "RSA_ENCRYPTED";
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
