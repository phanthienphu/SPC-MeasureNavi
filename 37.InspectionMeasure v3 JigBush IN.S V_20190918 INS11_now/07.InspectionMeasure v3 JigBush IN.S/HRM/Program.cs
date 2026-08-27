using InspectionMeasure.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace InspectionMeasure
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string machineName = Environment.MachineName;
            string remoteBaseDir = $@"\\192.168.0.16\app\160_JB_Navigation_F4\ConfigApp\{machineName}";
            string remoteConfigPath = Path.Combine(remoteBaseDir, "InspectionMeasure.exe.config");
            string localConfigPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            string localBaseDir = Path.GetDirectoryName(localConfigPath);
            try
            {
                if (!Directory.Exists(remoteBaseDir))
                {
                    Logger.WriteLog("Directory.Exists(remoteBaseDir)", $"Thư mục {machineName} không tồn tại! Kiểm tra thư mục .16 -> app -> ConfigApp -> {machineName}.");
                    return;
                }
                if (File.Exists(remoteConfigPath))
                {
                    bool shouldUpdateConfig = !File.Exists(localConfigPath) ||
                                              (File.GetLastWriteTime(remoteConfigPath) > File.GetLastWriteTime(localConfigPath));

                    if (shouldUpdateConfig)
                    {
                        File.Copy(remoteConfigPath, localConfigPath, true);
                    }
                }

                string[] remoteSubDirs = Directory.GetDirectories(remoteBaseDir);
                foreach (string remoteSubDir in remoteSubDirs)
                {
                    string folderName = Path.GetFileName(remoteSubDir);
                    string localSubDir = Path.Combine(localBaseDir, folderName);

                    // Gọi hàm phụ trợ để xử lý copy thư mục và file bên trong
                    Extensions.CopyDirectoryWithTimestampCheck(remoteSubDir, localSubDir);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Downloads config file", ex);
                return;
            }

            if (ConfigurationManager.AppSettings["FORM"].ToString() == "1")
            {
                SingleInstance.SingleApplication.Run(new frmDoINS());
            }
            else
            {
                SingleInstance.SingleApplication.Run(new frmDoFAC());
            }
        }

    }
}