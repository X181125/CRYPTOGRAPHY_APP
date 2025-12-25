namespace Cryptography_App.Helpers
{
    /// <summary>
    /// Helper for file preview functionality
    /// </summary>
    public static class FilePreviewHelper
    {
        // Supported file types
        private static readonly string[] TextExtensions = { ".txt", ".csv", ".json", ".xml", ".html", ".htm", ".css", ".js", ".cs", ".py", ".java", ".cpp", ".c", ".h", ".md", ".log", ".ini", ".cfg", ".yaml", ".yml" };
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".tiff", ".webp" };
        private static readonly string[] PdfExtensions = { ".pdf" };

        public enum FileType
        {
            Unknown,
            Text,
            Image,
            Pdf,
            Binary
        }

        public static FileType GetFileType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (TextExtensions.Contains(ext))
                return FileType.Text;
            if (ImageExtensions.Contains(ext))
                return FileType.Image;
            if (PdfExtensions.Contains(ext))
                return FileType.Pdf;

            // Check if file is text by reading first bytes
            if (File.Exists(filePath) && IsTextFile(filePath))
                return FileType.Text;

            return FileType.Binary;
        }

        private static bool IsTextFile(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[Math.Min(8192, stream.Length)];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Check for null bytes (binary indicator)
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 0)
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetFileIcon(FileType type)
        {
            return type switch
            {
                FileType.Text => "TXT",
                FileType.Image => "IMG",
                FileType.Pdf => "PDF",
                FileType.Binary => "BIN",
                _ => "FILE"
            };
        }

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        public static string GetTextPreview(string filePath, int maxChars = 50000)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                char[] buffer = new char[maxChars];
                int charsRead = reader.Read(buffer, 0, maxChars);
                string content = new string(buffer, 0, charsRead);

                if (reader.Peek() != -1)
                    content += "\n\n... [File truncated for preview]";

                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        public static Image? GetImagePreview(string filePath, int maxWidth = 800, int maxHeight = 600)
        {
            try
            {
                using var original = Image.FromFile(filePath);

                // Calculate scaling
                double ratioX = (double)maxWidth / original.Width;
                double ratioY = (double)maxHeight / original.Height;
                double ratio = Math.Min(ratioX, ratioY);

                if (ratio >= 1)
                {
                    // Image is smaller than max, return copy
                    return new Bitmap(original);
                }

                int newWidth = (int)(original.Width * ratio);
                int newHeight = (int)(original.Height * ratio);

                var thumbnail = new Bitmap(newWidth, newHeight);
                using (var g = Graphics.FromImage(thumbnail))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(original, 0, 0, newWidth, newHeight);
                }
                return thumbnail;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] GetBinaryPreview(string filePath, int maxBytes = 1024)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[Math.Min(maxBytes, stream.Length)];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public static string FormatHexDump(byte[] data, int bytesPerLine = 16)
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < data.Length; i += bytesPerLine)
            {
                sb.Append($"{i:X8}  ");

                // Hex part
                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (i + j < data.Length)
                        sb.Append($"{data[i + j]:X2} ");
                    else
                        sb.Append("   ");

                    if (j == 7) sb.Append(" ");
                }

                sb.Append(" ");

                // ASCII part
                for (int j = 0; j < bytesPerLine && i + j < data.Length; j++)
                {
                    byte b = data[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
