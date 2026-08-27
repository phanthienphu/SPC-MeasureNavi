using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;

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