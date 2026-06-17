using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Compressors.BZip2;

namespace t9s2t.Engines
{
    /// <summary>
    /// 压缩文件解压工具
    /// 使用 SharpCompress 库原生支持 tar.bz2 / tar.gz / zip 等格式
    /// 无需外部工具（tar.exe / 7-Zip）
    /// </summary>
    public static class TarBz2Extractor
    {
        /// <summary>
        /// 解压压缩文件到指定目录（异步）
        /// </summary>
        public static Task ExtractAsync(string archivePath, string destinationDir)
        {
            return Task.Run(() => Extract(archivePath, destinationDir));
        }

        /// <summary>
        /// 解压压缩文件到指定目录
        /// </summary>
        public static void Extract(string archivePath, string destinationDir)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("压缩文件不存在", archivePath);

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            Debug.WriteLine($"[t9s2t] 开始解压: {archivePath} -> {destinationDir}");

            bool isBz2 = IsTarBz2File(archivePath);

            if (isBz2)
            {
                // bzip2 需要两步：先解压 bzip2 得到 .tar，再解 tar
                string tempTar = Path.Combine(Path.GetTempPath(), "model_temp_" + Guid.NewGuid().ToString("N") + ".tar");
                try
                {
                    using (var inStream = File.OpenRead(archivePath))
                    using (var bz2Stream = new BZip2Stream(inStream, SharpCompress.Compressors.CompressionMode.Decompress, false))
                    using (var outStream = File.Create(tempTar))
                    {
                        bz2Stream.CopyTo(outStream);
                    }
                    Debug.WriteLine($"[t9s2t] bzip2 解压完成，临时 tar: {tempTar}");

                    using (var archive = TarArchive.Open(tempTar))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                            {
                                entry.WriteToDirectory(destinationDir, new ExtractionOptions
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }
                }
                finally
                {
                    try { File.Delete(tempTar); } catch { }
                }
            }
            else
            {
                // 其他格式（zip, tar.gz 等）直接解压
                using (var archive = ArchiveFactory.Open(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.WriteToDirectory(destinationDir, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
            }

            Debug.WriteLine("[t9s2t] 解压完成");
        }

        /// <summary>
        /// 根据文件扩展名判断是否为 tar.bz2
        /// </summary>
        public static bool IsTarBz2(string filePath)
        {
            return filePath != null &&
                   (filePath.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".tbz2", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 通过读取文件头部魔数判断是否为 bzip2 压缩文件
        /// bzip2 魔数: 'B' 'Z' 'h' (0x42 0x5A 0x68)
        /// </summary>
        public static bool IsTarBz2File(string filePath)
        {
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] header = new byte[3];
                    if (fs.Read(header, 0, 3) == 3)
                    {
                        return header[0] == 0x42 && header[1] == 0x5A && header[2] == 0x68;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 根据文件扩展名判断是否为支持的压缩格式
        /// </summary>
        public static bool IsSupportedArchive(string filePath)
        {
            return filePath != null &&
                   (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".tbz2", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
                    filePath.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase));
        }
    }
}
