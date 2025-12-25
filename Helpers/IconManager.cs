using System;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace Cryptography_App.Helpers
{
    /// <summary>
    /// Quản lý việc load và cache các icon từ embedded resources
    /// </summary>
    public static class IconManager
    {
        private static readonly Dictionary<string, Image?> _iconCache = new();
        private static readonly string _resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources");

        // Định nghĩa các loại icon
        public enum IconType
        {
            Lock,
            Security,
            Folder,
            File,
            Txt,
            Image,
            Pdf,
            Zip,
            Save,
            Trash,
            Search,
            Settings,
            Help,
            Info,
            Alert,
            Swap,
            Run,
            Home,
            Drive,
            Pack,
            Console,
            Notepad,
            Globe,
            Mail,
            Calculator
        }

        // Mapping icon type to file name
        private static readonly Dictionary<IconType, string> _iconFiles = new()
        {
            { IconType.Lock, "lock.png" },
            { IconType.Security, "security.png" },
            { IconType.Folder, "folder.png" },
            { IconType.File, "file.png" },
            { IconType.Txt, "txt.png" },
            { IconType.Image, "image.png" },
            { IconType.Pdf, "pdf.png" },
            { IconType.Zip, "zip.png" },
            { IconType.Save, "save.png" },
            { IconType.Trash, "trash.png" },
            { IconType.Search, "search.png" },
            { IconType.Settings, "settings.png" },
            { IconType.Help, "help.png" },
            { IconType.Info, "info.png" },
            { IconType.Alert, "alert.png" },
            { IconType.Swap, "swap.png" },
            { IconType.Run, "run.png" },
            { IconType.Home, "home.png" },
            { IconType.Drive, "drive.png" },
            { IconType.Pack, "pack.png" },
            { IconType.Console, "console.png" },
            { IconType.Notepad, "notepad.png" },
            { IconType.Globe, "globe.png" },
            { IconType.Mail, "mail.png" },
            { IconType.Calculator, "calculator.png" }
        };

        /// <summary>
        /// Lấy icon theo loại với kích thước mặc định
        /// </summary>
        public static Image? GetIcon(IconType type, int size = 24)
        {
            string cacheKey = $"{type}_{size}";
            
            if (_iconCache.TryGetValue(cacheKey, out var cachedIcon))
                return cachedIcon;

            var originalIcon = LoadIcon(type);
            if (originalIcon == null)
                return null;

            // Resize icon về kích thước mong muốn
            var resizedIcon = ResizeImage(originalIcon, size, size);
            _iconCache[cacheKey] = resizedIcon;
            
            return resizedIcon;
        }

        /// <summary>
        /// Lấy icon từ file với custom size
        /// </summary>
        public static Image? GetIconFromFile(string fileName, int size = 24)
        {
            string cacheKey = $"file_{fileName}_{size}";
            
            if (_iconCache.TryGetValue(cacheKey, out var cachedIcon))
                return cachedIcon;

            try
            {
                string filePath = Path.Combine(_resourcePath, fileName);
                if (!File.Exists(filePath))
                {
                    // Thử tìm trong thư mục Resources bên cạnh exe
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
                }
                
                if (File.Exists(filePath))
                {
                    using var img = Image.FromFile(filePath);
                    var resized = ResizeImage(img, size, size);
                    _iconCache[cacheKey] = resized;
                    return resized;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Load icon từ Resources folder
        /// </summary>
        private static Image? LoadIcon(IconType type)
        {
            if (!_iconFiles.TryGetValue(type, out var fileName))
                return null;

            string cacheKey = $"original_{type}";
            if (_iconCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                // Thử load từ Resources folder (debug mode)
                string filePath = Path.Combine(_resourcePath, fileName);
                if (!File.Exists(filePath))
                {
                    // Thử tìm trong thư mục Resources bên cạnh exe (release mode)
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
                }

                if (File.Exists(filePath))
                {
                    var img = Image.FromFile(filePath);
                    _iconCache[cacheKey] = img;
                    return img;
                }

                // Fallback: Thử load từ embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"Cryptography_App.Resources.{fileName}";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    var img = Image.FromStream(stream);
                    _iconCache[cacheKey] = img;
                    return img;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Resize ảnh với chất lượng cao
        /// </summary>
        private static Image ResizeImage(Image original, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            using var wrapMode = new System.Drawing.Imaging.ImageAttributes();
            wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }

        /// <summary>
        /// Lấy icon dựa trên extension của file
        /// </summary>
        public static Image? GetFileTypeIcon(string extension, int size = 24)
        {
            extension = extension.TrimStart('.').ToLowerInvariant();

            return extension switch
            {
                "txt" or "log" or "md" or "cs" or "json" or "xml" or "html" or "css" or "js" => GetIcon(IconType.Txt, size),
                "jpg" or "jpeg" or "png" or "gif" or "bmp" or "ico" or "svg" or "webp" => GetIcon(IconType.Image, size),
                "pdf" => GetIcon(IconType.Pdf, size),
                "zip" or "rar" or "7z" or "tar" or "gz" => GetIcon(IconType.Zip, size),
                "encrypted" => GetIcon(IconType.Lock, size),
                _ => GetIcon(IconType.File, size)
            };
        }

        /// <summary>
        /// Xóa cache để giải phóng bộ nhớ
        /// </summary>
        public static void ClearCache()
        {
            foreach (var icon in _iconCache.Values)
            {
                icon?.Dispose();
            }
            _iconCache.Clear();
        }

        /// <summary>
        /// Lấy tất cả icon types có sẵn
        /// </summary>
        public static IEnumerable<IconType> GetAllIconTypes()
        {
            return Enum.GetValues<IconType>();
        }
    }
}
