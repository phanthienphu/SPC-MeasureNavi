using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionMeasure.Helper
{
    public static class Extensions
    {
        public static void CopyDirectoryWithTimestampCheck(string sourceDir, string destDir)
        {
            // 1. Tạo thư mục đích ở máy trạm nếu chưa có
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 2. Lấy danh sách file trong thư mục hiện tại và đối soát thời gian
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));

                // Chỉ copy nếu file máy trạm chưa có hoặc file trên server mới hơn
                bool shouldUpdate = !File.Exists(destFile) ||
                                    (File.GetLastWriteTime(file) > File.GetLastWriteTime(destFile));

                if (shouldUpdate)
                {
                    File.Copy(file, destFile, true);
                }
            }

            // 3. Duyệt đệ quy để quét tiếp các thư mục con bên trong (nếu thư mục có nhiều lớp)
            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (string subDir in subDirs)
            {
                string nextDestDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectoryWithTimestampCheck(subDir, nextDestDir);
            }
        }
    }
}
