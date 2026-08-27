using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.SqlClient;

using System.Reflection;
using System.Configuration;

using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;

//Khai ba ket noi mouse
using Microsoft.Win32;

//Khai bao ket noi COM
using System.IO;
using System.IO.Ports;

//References
using System.Runtime.InteropServices;
using ZedGraph;

using System.Threading;
using System.Diagnostics;
using System.Net;
using SendKeys;

namespace InspectionMeasure
{
    public partial class frmDoINS : DevExpress.XtraEditors.XtraForm
    {

        string windown = "";
        int i, diem = 0;
        float setvalue = 0;
        string grouplevel="", levelmachine="", type ="", productname="", checkhieuchinh="", CheckKey="", TimerInterval="", AddValue="", CheckBeforePlating="", TableSave="";
        bool PlaingProcess = false;
        string connectstring = "";
        DateTime datefile;
        SqlDataAdapter da;
        SqlDataAdapter da1;
        SqlConnection connection;
        SqlCommand cmd;
        DateTime LastCSV;
        double max_up = 20.25;
        double min_up = 20.15;
        double tb_up = 20.2;
        int dem = 0; int dem1 = 0;
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);
        private const int MOUSEEVENTF_MOVE = 0x0001; /* mouse move */
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */
        private const int MOUSEEVENTF_RIGHTUP = 0x0010; /* right button up */
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //************Khai báo các biến*******************
        SerialPort P = new SerialPort(); // Khai báo 1 Object SerialPort mới.
        string InputData = String.Empty; // Khai báo string buff dùng cho hiển thị dữ liệu sau này.
        delegate void SetTextCallback(string text); // Khai bao delegate SetTextCallBack voi tham so string
        string[] ports; string bien = "";
        //************Khai báo các biến*******************
        SerialPort P1 = new SerialPort(); // Khai báo 1 Object SerialPort mới.
        string InputData1 = String.Empty; // Khai báo string buff dùng cho hiển thị dữ liệu sau này.
        delegate void SetTextCallback1(string text); // Khai bao delegate SetTextCallBack voi tham so string
        string[] ports1; string bien1 = "";
        int x=0, y=0;

        [DllImport("user32")]
        static extern IntPtr SetParent(IntPtr hwc, IntPtr hwp);
        /// <summary>
        /// Find all the top leve windows running and add supported keys in the 'All Keys' listbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        public frmDoINS()
        {
            try
            {
                InitializeComponent();
                //    EncryptFile("connectionStrings");
            }
            catch
            {
            }
        }

        private void GetEmp()
        {
            try
            {
                string sql = "";
                sql = "Select Name from EmployeeServer Where EmpID='" + txtManhanvien.Text + "'";
                connection = new SqlConnection(connectstring);
                connection.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, connection);
                SqlDataReader reader = sqlcmd.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        txtTennhanvien.Text = reader["Name"].ToString();
                        string pathfile = @"\\192.168.0.6\n14\Hinh SPC\" + txtManhanvien.Text + ".jpg";
                        if (System.IO.File.Exists(pathfile))
                        {
                            Bitmap bmp = new Bitmap(pathfile);
                            picEmp.Image = bmp;
                        }
                        else
                        {
                            Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                            picEmp.Image = bmp;
                        }
                        //txtPO.Focus();
                        if (ConfigurationManager.AppSettings["Typetxt"] != "" && ConfigurationManager.AppSettings["AFtxt"] != "")
                        {
                            txtType.Text = ConfigurationManager.AppSettings["Typetxt"];
                            txtAfterPlating.Text = ConfigurationManager.AppSettings["AFtxt"];
                            txtPO.Focus();
                        }
                        else txtType.Focus();
                    }
                }
                else
                {
                    txtManhanvien.Focus();
                    txtManhanvien.SelectAll();
                }
                reader.Close();
                reader.Dispose();
            }
            catch
            {
            }
        }
        
        public bool CancelKey(string Chuoi)
        {
            bool condition = true;
            try
            {
                string sql="";
                if (Chuoi.Length > 3)
                {
                    sql = "Select CancelCode from Inspection_CancelCode Where CancelCode='" + clsPublic.Left(Chuoi, 3) + "'";
                }
                else
                {
                    sql = "Select CancelCode from Inspection_CancelCode Where CancelCode='" + Chuoi + "'";
                }
                SqlCommand sqlcmd = new SqlCommand(sql, connection);
                SqlDataReader reader = sqlcmd.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        condition = true;
                        break;
                    }
                    reader.Close();
                    reader.Dispose();
                }
                else
                {
                    condition = false;
                }
            }
            catch
            {
                condition = false;
            }
            return condition;
        }

        #region CryptConfig
        private void EncryptFile(string AreaChoose)
        {
            EncryptConnectionString(true, System.Reflection.Assembly.GetExecutingAssembly().Location, AreaChoose);
        }

        private void DecryptFile(string AreaChoose)
        {
            EncryptConnectionString(false, System.Reflection.Assembly.GetExecutingAssembly().Location, AreaChoose);
        }

        public static void EncryptConnectionString(bool encrypt, string fileName, string Area)
        {
            Configuration configuration = null;
            try
            {
                // Open the configuration file and retrieve the connectionStrings section.
                configuration = ConfigurationManager.OpenExeConfiguration(fileName);
                ConnectionStringsSection configSection = configuration.GetSection(Area) as ConnectionStringsSection;
                if ((!(configSection.ElementInformation.IsLocked)) && (!(configSection.SectionInformation.IsLocked)))
                {
                    if (encrypt && !configSection.SectionInformation.IsProtected)
                    {
                        //this line will encrypt the file
                        configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    }

                    if (!encrypt && configSection.SectionInformation.IsProtected)//encrypt is true so encrypt
                    {
                        //this line will decrypt the file. 
                        configSection.SectionInformation.UnprotectSection();
                    }
                    //re-save the configuration file section
                    configSection.SectionInformation.ForceSave = true;
                    // Save the current configuration

                    configuration.Save();
                    //Process.Start("notepad.exe", configuration.FilePath);
                    //configFile.FilePath 
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.ToString());
            }
        }
        #endregion
        private void cmdDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GetCongdoan()
        {
            try
            {
                string sql = "";
                sql = "Select Inspection_Master_Congdoan.IDNumber,Thuocdo,Congdoan,Tencongdoan,Kichthuoc,Kichthuocchuan,Dungsaiduoi,Dungsaitren,";
                sql = sql + "MinVal,MaxVal,'' as CheckProcess,'' as Sodo,";
                if(txtCheckQty.Text=="1" && txtCheckFlow.Text=="1")
                {
                    if(lblQtyReceive.Text=="0")
                        sql = sql + "ActiveFlag,ReferID";
                    else
                        sql = sql + "ActiveFlag2 as ActiveFlag,ReferID2 as ReferID";
                }
                else
                {
                    sql = sql + "ActiveFlag,ReferID";
                }
                sql = sql + " from Inspection_Master_Congdoan left outer join Inspection_MachineDefine on Inspection_Master_Congdoan.IDNumber=Inspection_MachineDefine.IDNumber";
                sql = sql + " Where GroupName='" + txtGroup.Text.ToString() +"' and MachineID='" + txtTenmay.Text.ToString() + "'";
                da = new SqlDataAdapter(sql, connection);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DataView data = new DataView(dt);
                grdProcess.DataSource = data;
                grdProcess.Columns[0].Visible = false;
                grdProcess.Columns[1].Visible = false;
                grdProcess.Columns[5].Visible = false;
                grdProcess.Columns[6].Visible = false;
                grdProcess.Columns[7].Visible = false;
                grdProcess.Columns[11].Visible = false;
                grdProcess.Columns[12].Visible = false;
                grdProcess.Columns[13].Visible = false;
                if (txtCheckFlow.Text == "1")
                {
                    grdProcess.Columns[2].HeaderText = "VALUE";
                }
                else
                {
                    grdProcess.Columns[2].HeaderText = "PROCESS";
                }
                grdProcess.Columns[3].HeaderText = "PROCESS NAME";
                grdProcess.Columns[4].HeaderText = "ĐO";
                grdProcess.Columns[8].HeaderText = "MIN";
                grdProcess.Columns[9].HeaderText = "MAX";
                grdProcess.Columns[10].HeaderText = "OK";
                grdProcess.Columns[2].Width = 56;
                grdProcess.Columns[3].Width = 185;
                grdProcess.Columns[4].Width = 25;
                grdProcess.Columns[8].Width = 36;
                grdProcess.Columns[9].Width = 36;
                grdProcess.Columns[10].Width = 33;

                for (int n = 0; n < grdProcess.RowCount; n++)
                {
                    if (grdProcess.Rows[n].Cells[5].Value == DBNull.Value)
                    {
                        int congdoan = n + 1;
                        if (PlaingProcess == true && CheckBeforePlating == "1")
                        {
                            sql = "SELECT IDNumber,GroupName,Tenhang,Kichthuocchuan1 as Kichthuocchuan,Dungsaiduoi1 as Dungsaiduoi,";
                            sql = sql + "Dungsaitren1 as Dungsaitren,MinVal1 as MinVal,MaxVal1 as MaxVal";
                        }
                        else
                        {
                            sql = "SELECT [IDNumber],[GroupName],[Tenhang],[Kichthuocchuan],[Dungsaiduoi],[Dungsaitren],[MinVal],[MaxVal]";
                        }
                        sql = sql + " FROM [Inspection_Master_Congdoantheotenhang]";
                        sql = sql + " Where GroupName='" + txtGroup.Text.ToString() + "' and IDNumber='" + congdoan + "' ORDER BY Tenhang DESC";
                        SqlCommand sqlcmd = new SqlCommand(sql, connection);
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                if (lblTenhang.Text.ToString().IndexOf(reader["Tenhang"].ToString()) > 0)
                                {
                                    grdProcess.Rows[n].Cells[5].Value = reader["Kichthuocchuan"];
                                    grdProcess.Rows[n].Cells[6].Value = reader["Dungsaiduoi"];
                                    grdProcess.Rows[n].Cells[7].Value = reader["Dungsaitren"];
                                    grdProcess.Rows[n].Cells[8].Value = reader["MinVal"];
                                    grdProcess.Rows[n].Cells[9].Value = reader["MaxVal"];
                                    break;
                                }
                                else
                                {
                                }
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                    if (txtCheckFlow.Text == "1" && levelmachine != "0" && txtPO.Text.Length > 0)
                    {
                        int congdoan = n + 1;
                        sql = "Select max([" + congdoan + "]) from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and STTDo='" + (int.Parse(lblQtyReceive.Text) + 1).ToString() + "'";
                        sql = sql + " and MachineID in (select MachineID from Inspection_MachineGroup Where GroupLevel='" + grouplevel + "' and LevelMachine='" + (int.Parse(levelmachine.ToString()) - 1).ToString() + "')";
                        if (PlaingProcess == false)
                        {
                            sql = sql + " and (Notice=0 or Notice is null)";
                        }
                        else
                        {
                            sql = sql + " and Notice=1";
                        }
                        SqlCommand sqlcmd = new SqlCommand(sql, connection);
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                grdProcess.Rows[n].Cells[2].Value = reader[0].ToString();
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                    }
                }
                Giatrido();
            }
            catch
            {
            }
        }

        private void txtDo_KeyDown(object sender, KeyEventArgs e)
        {
           
            try
            {
               // MessageBox.Show("5");
                if (e.KeyCode == Keys.Enter && txtDo.Text.ToString().Length > 0 && grdProcess.RowCount > 0)
                {
                    Conn();// kết nối database
                    if (CheckKey == "1")
                    {
                        if (CancelKey(txtDo.Text.ToString()) == true)
                        {
                          //  MessageBox.Show("6");
                            txtDo.Text = "";
                            txtDo.Focus();
                        }
                    }
                    else
                    {
                        if (clsPublic.IsNumeric(txtDo.Text.ToString()) == true)
                        {
                          //  MessageBox.Show("7");
                            float giatrimax = 0;
                            float giatrimin = 0;
                            float giatrido = 0;
                            
                            cmdAlarm.Text = "";
                            cmdAlarm.BackColor = Color.FromArgb(255, 255, 192);

                            giatrimax = float.Parse(txtMax.Text);
                            giatrimin = float.Parse(txtMin.Text);
                            if(int.Parse(grdProcess.Rows[i].Cells[13].Value.ToString()) == 0)
                            {
                                giatrido = float.Parse(txtDo.Text);
                            }
                            else
                            {
                                giatrido = float.Parse(txtDo.Text) - float.Parse(grdProcess.Rows[int.Parse(grdProcess.Rows[i].Cells[13].Value.ToString()) - 1].Cells[11].Value.ToString());
                            }
                            if (giatrido <= giatrimax && giatrido >= giatrimin)
                            {
                             //   MessageBox.Show("8");
                                cmdAlarm.BackColor = Color.LightSkyBlue;
                                cmdAlarm.Text = txtDo.Text + "\r\nOK";
                                grdProcess.Rows[i].Cells[10].Value = "OK";
                                grdProcess.Rows[i].Cells[11].Value = txtDo.Text;
                                grdProcess.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                i = i + 1;
                               
                                Giatrido();
                                
                            }
                            else
                            {
                                cmdAlarm.BackColor = Color.Red;
                                cmdAlarm.Text = txtDo.Text + "\r\nNG";
                                grdProcess.Rows[i].Cells[10].Value = "NG";
                                grdProcess.Rows[i].Cells[11].Value = txtDo.Text;

                                txtDo.SelectAll();
                            }
                        }
                        else
                        {
                           
                               BarcodeButtonDO(txtDo.Text.ToString());
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void LoadDataInput()
        {
            lbNOBF.Text = lbREBF.Text = lbADBF.Text = lbTTBF.Text = "0";
            lbNOAF.Text = lbREAF.Text = lbADAF.Text = lbTTAF.Text = "0";

            string query = "SELECT  [MachineID],[Status],[PO], count(PO) as SumINS"
                       + " FROM [InspectionMeasure].[dbo].[V_DataINS_JB]"
                       + " where po like '" + txtPO.Text + "' and [MachineID] like '" + txtTenmay.Text + "' and ";
            if(txtAfterPlating.Text == "BF")
            {
                query = query + " Notice = '1'";
            }
            else
            {
                query = query + " Notice = '0'";
            }
            query = query + " group by [MachineID],[Status],[PO]";

                SqlDataAdapter adap1 = new SqlDataAdapter(query, connection);
                DataTable dataTable1 = new DataTable();
            adap1.Fill(dataTable1);
            lbPO.Text = "PO: " + txtPO.Text;
                if (dataTable1 != null && dataTable1.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable1.Rows.Count; i++)
                    {
                    //if (dataTable.Rows[i]["MachineID"].ToString() == txtTenmay.Text )
                    //{
                    if (txtAfterPlating.Text == "BF")
                    {
                        if (dataTable1.Rows[i]["Status"].ToString() == "NO")
                        {
                            lbNOBF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        if (dataTable1.Rows[i]["Status"].ToString() == "RE")
                        {
                            lbREBF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        if (dataTable1.Rows[i]["Status"].ToString() == "AD")
                        {
                            lbADBF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        lbTTBF.Text = (Int32.Parse(lbNOBF.Text) + Int32.Parse(lbREBF.Text) + Int32.Parse(lbADBF.Text)).ToString();
                    }
                    else
                    {
                        if (dataTable1.Rows[i]["Status"].ToString() == "NO")
                        {
                            lbNOAF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        if (dataTable1.Rows[i]["Status"].ToString() == "RE")
                        {
                            lbREAF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        if (dataTable1.Rows[i]["Status"].ToString() == "AD")
                        {
                            lbADAF.Text = dataTable1.Rows[i]["SumINS"].ToString();
                        }
                        lbTTAF.Text = (Int32.Parse(lbNOAF.Text) + Int32.Parse(lbREAF.Text) + Int32.Parse(lbADAF.Text)).ToString();
                    }
                        
                    }
                //}
            }
        }
        private void DataInput()
        {
            if (txtAfterPlating.Text == "BF")
            {
                if (txtType.Text  == "NO")
                {
                    lbNOBF.Text = (Int32.Parse(lbNOBF.Text) +1).ToString ();
                }
                if (txtType.Text == "RE")
                {
                    lbREBF.Text = (Int32.Parse(lbREBF.Text)+1).ToString ();
                }
                if (txtType.Text == "AD")
                {
                    lbADBF.Text =( Int32.Parse(lbADBF.Text)+1).ToString ();
                }
                lbTTBF.Text = (Int32.Parse(lbNOBF.Text) + Int32.Parse(lbREBF.Text) + Int32.Parse(lbADBF.Text)).ToString();
            }
            else
            {
                if (txtType.Text == "NO")
                {
                    lbNOAF.Text = (Int32.Parse(lbNOAF.Text)+1).ToString ();
                }
                if (txtType.Text == "RE")
                {
                    lbREAF.Text =( Int32.Parse(lbREAF.Text)+1).ToString ();
                }
                if (txtType.Text == "AD")
                {
                    lbADAF.Text = (Int32.Parse(lbADAF.Text)+1).ToString ();
                }
                lbTTAF.Text = (Int32.Parse(lbNOAF.Text) + Int32.Parse(lbREAF.Text) + Int32.Parse(lbADAF.Text)).ToString();
            }
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                //i = 0;
                //for (int n = 0; n < grdProcess.RowCount; n++)
                //{
                //    if (n == 0)
                //    {
                //        grdProcess.Rows[n].DefaultCellStyle.BackColor = Color.Red;
                //    }
                //    else
                //    {
                //        grdProcess.Rows[n].DefaultCellStyle.BackColor = Color.White;
                //    }
                //    grdProcess.Rows[n].Cells[10].Value = grdProcess.Rows[n].Cells[11].Value = "";
                //}
                //txtDo.Text = "";
                //txtDo.Focus();
                i = 0;
                for (int n = 0; n < grdProcess.RowCount; n++)
                {
                    grdProcess.Rows[n].DefaultCellStyle.BackColor = Color.White;
                    grdProcess.Rows[n].Cells[10].Value = grdProcess.Rows[n].Cells[11].Value = "";
                }
                Giatrido();
                //cmdAlarm.Text = "";
                //cmdAlarm.BackColor = Color.LightYellow;
            }
            catch
            {
            }
        }
        private void Conn()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            connection.Open();
        }
        private void frmDo_Load(object sender, EventArgs e)
        {
            try
            {
                connectstring = "Server=" + ConfigurationManager.AppSettings["ServerName"] + ";Initial Catalog=" + ConfigurationManager.AppSettings["Database"] + ";User ID=admin;Password=cuapheta;MultipleActiveResultSets=True;";
                connection = new SqlConnection(connectstring);
                Conn();// kết nối database

                // xoa dữ liệu trong file csv, tránh đọc dữ liệu cũ đã truyền vào trước khi mở chương trình 
                if (File.Exists(@"" + ConfigurationManager.AppSettings["CSVFile"]))
                {
                    StreamWriter wr = new StreamWriter(@"" + ConfigurationManager.AppSettings["CSVFile"]);
                    wr.WriteLine("");
                    wr.Close();
                }
                //StreamWriter wr1 = new StreamWriter(@"" + ConfigurationManager.AppSettings["CSVFile"]);
                //wr1.WriteLine("");
                //wr1.Close();

                FileInfo CheckFile = new FileInfo(@"" + ConfigurationManager.AppSettings["CSVFile"]);
                LastCSV = CheckFile.LastWriteTime;

               // MessageBox.Show(LastCSV.ToString());

                txtManhanvien.Text = ConfigurationManager.AppSettings["EmpDefault"];
                if (txtManhanvien.Text == "")
                    txtManhanvien.Enabled = true;
                else
                {
                    txtManhanvien.Enabled = false;
                }
                if (txtManhanvien.Text.Length > 0)
                {
                    GetEmp();
                }

                txtShift.Text = ConfigurationManager.AppSettings["Shift"];
                if (txtShift.Text == "")
                    txtShift.Enabled = true;
                else
                {
                    txtShift.Enabled = false;
                }

                txtTenmay.Text = ConfigurationManager.AppSettings["MachineName"];
                if (txtTenmay.Text == "")
                    txtTenmay.SelectedIndex = 0;

                txtSTT.Text = ConfigurationManager.AppSettings["STTDo"];
                if (txtSTT.Text == "")
                    txtSTT.Enabled = true;
                else
                {
                    txtSTT.Enabled = false;
                }

                if (txtManhanvien.Enabled == false)
                {
                    txtPO.Focus();
                }
                else
                {
                    txtManhanvien.Focus();
                }
                CheckKey = ConfigurationManager.AppSettings["CheckKey"]; //Dung de cho to Shaft,cancel nhung barcode khong can thiet
                AddValue = ConfigurationManager.AppSettings["AddValue"]; // Dung de them gia tri o cong doan dau tien
                TimerInterval = ConfigurationManager.AppSettings["TimerIntervalSet"];
                CheckBeforePlating = ConfigurationManager.AppSettings["CheckBeforePlating"]; // Dung de cho to JigBush lam hang truoc ma va sau ma
                if (TimerInterval == "0")
                {
                    timer1.Enabled = false;
                }
                else
                {
                    timer1.Enabled = true;
                    timer1.Interval = int.Parse(TimerInterval);
                }
                if (ConfigurationManager.AppSettings["TimerIntervalSet1"] == "0")
                {
                    timer2.Enabled = false;
                }
                else
                {
                    if (timer1.Enabled == true)
                    {
                        timer2.Enabled = true;
                        timer2.Interval = int.Parse(ConfigurationManager.AppSettings["TimerIntervalSet1"]);
                    }
                }
                txtCheckFlow.Text = ConfigurationManager.AppSettings["CheckFlow"]; //Dung cho nhom H10 dung cho may MC den may Heiken
                //if (txtCheckFlow.Text == "1")
                //{
                //    txtCheckQty.Text = "0";
                //}
                //else
                //{
                //    //txtCheckQty.Text = ConfigurationManager.AppSettings["CheckQty"];
                //}
                checkhieuchinh = ConfigurationManager.AppSettings["CheckHieuchinh"]; //Dung cho nhom H10 hien thi cong doan sau tru cong doan truoc
                if (checkhieuchinh == "1")
                {
                    splHieuchinh.SplitterPosition = 326;
                }
                else
                {
                    splHieuchinh.SplitterPosition = 0;
                }
                txtCheckQty.Text = ConfigurationManager.AppSettings["CheckQty"];
                if (txtCheckQty.Text == "1")
                {
                    cmdAlarm.Left = 148;
                    cmdAlarm.Top = 175;
                    lblSoluongdado.Visible = true;
                    lblQtyReceive.Visible = true;
                    txtQtyAct.Enabled = false;
                }
                else
                {
                    cmdAlarm.Left = 98;
                    cmdAlarm.Top = 175;
                    lblSoluongdado.Visible = false;
                    lblQtyReceive.Visible = false;
                    txtQtyAct.Enabled = true;
                }

                //Check xem may dang gia cong thuoc bo phan nao
                if (txtTenmay.Text.Length > 0)
                {
                    string sql1 = "Select * from Inspection_MachineGroup Where MachineID='" + txtTenmay.Text + "' and (DeleteFlag=0 or DeleteFlag is null)";
                    SqlCommand sqlcmd1 = new SqlCommand(sql1, connection);
                    SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                    if (reader1.HasRows == true)
                    {
                        while (reader1.Read())
                        {
                            txtGroup.Text = reader1[1].ToString();
                            txtTableName.Text = reader1[2].ToString();
                            grouplevel = reader1["GroupLevel"].ToString();
                            levelmachine = reader1["LevelMachine"].ToString();
                            TableSave = reader1["TableResult"].ToString();
                        }
                    }

                    string sql = "Select Max(IDNumber) from Inspection_Master_Congdoan Where GroupName='" + txtGroup.Text + "'";
                    SqlCommand sqlcmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = sqlcmd.ExecuteReader();
                    if (reader.HasRows == true)
                    {
                        while (reader.Read())
                        {
                            txtDemsocongdoan.Text = reader[0].ToString();
                        }
                    }
                }

                Loaddulieuluoi();
                cmdRefresh.Enabled = false;
                cmdNext.Enabled = false;
                cmdConfirm.Enabled = false;
                cmdOK.Enabled = false;
                cmdNG.Enabled = false;
                cmdFinish.Enabled = false;
                cmdRepair.Enabled = false;
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();

                if (ConfigurationManager.AppSettings["ShowCom"].ToString() == "1")
                {
                    groupBoxconnect.Visible = true;
                }
                else
                {
                    groupBoxconnect.Visible = false;
                }

                if (ConfigurationManager.AppSettings["ShowCom1"].ToString() == "1")
                {
                    groupBoxconnect1.Visible = true;
                }
                else
                {
                    groupBoxconnect1.Visible = false;
                }

                if (ConfigurationManager.AppSettings["LoadForm"].ToString() == "1")
                {
                    if (ConfigurationManager.AppSettings["PortCOM"].ToString() != "")
                    {
                        KiemtraConnect();
                    }
                }
                if (ConfigurationManager.AppSettings["LoadForm1"].ToString() == "1")
                {
                    if (ConfigurationManager.AppSettings["PortCOM1"].ToString() != "")
                    {
                        KiemtraConnect1();
                    }
                }

              //  txtManhanvien.Text = "9471\n";
            }
            catch
            {
            }
        }

        private void txtManhanvien_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter && txtManhanvien.Text.ToString().Length > 0)
                {
                    if (CheckKey == "1")
                    {
                        if (CancelKey(txtManhanvien.Text.ToString()) == true )
                        {
                            txtManhanvien.Text = "";
                            txtManhanvien.Focus();
                        }
                    }
                    else
                    {
                        GetEmp();
                        Tranfer(txtManhanvien.Text);
                        this.Activate();
                    }
                }
            }
            catch
            {
            }
        }
        private void Tranfer(string data)
        {
            GetTaskWindows();
            int iHandle = NativeWin32.FindWindow(null, windown);
            NativeWin32.SetForegroundWindow(iHandle);
            System.Windows.Forms.SendKeys.Send(data + "{ENTER}");
        }
        private void GetTaskWindows()
        {
            try
            {
                // Get the desktopwindow handle
                int nDeshWndHandle = NativeWin32.GetDesktopWindow();
                // Get the first child window
                int nChildHandle = NativeWin32.GetWindow(nDeshWndHandle, NativeWin32.GW_CHILD);

                while (nChildHandle != 0)
                {
                    //If the child window is this (SendKeys) application then ignore it.
                    if (nChildHandle == this.Handle.ToInt32())
                    {
                        nChildHandle = NativeWin32.GetWindow(nChildHandle, NativeWin32.GW_HWNDNEXT);
                    }

                    // Get only visible windows
                    if (NativeWin32.IsWindowVisible(nChildHandle) != 0)
                    {
                        StringBuilder sbTitle = new StringBuilder(1024);
                        // Read the Title bar text on the windows to put in combobox
                        NativeWin32.GetWindowText(nChildHandle, sbTitle, sbTitle.Capacity);
                        String sWinTitle = sbTitle.ToString();
                        {
                            if (sWinTitle.Length > 0 && sWinTitle.ToString().IndexOf(ConfigurationManager.AppSettings["WindowTitle"]) >= 0)
                            {
                                windown = sWinTitle.ToString();
                            }

                        }
                    }
                    // Look for the next child.
                    nChildHandle = NativeWin32.GetWindow(nChildHandle, NativeWin32.GW_HWNDNEXT);
                }
            }
            catch
            {
            }
        }
        private void txtPO_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    Conn();// kết nối database
                    if (txtPO.Text.ToUpper() == "CMDRE")
                    {
                        txtAfterPlating.Text = "";
                        txtType.Text = "";
                        txtPO.Text = "";
                        txtType.Focus();
                        return;
                    }
                    string sql = "";
                    if(CheckKey == "1")
                    {
                        if (CancelKey(txtPO.Text.ToString()) == true || (clsPublic.IsNumeric(txtPO.Text.ToString()) == false && txtPO.Text.ToString().IndexOf("@") <= 0))
                        {
                            txtPO.Text = "";
                            txtPO.Focus();
                            return;
                        }

                        if (txtPO.Text.ToString().IndexOf("@") > 0)
                        {
                            string chuoitemp = clsPublic.Mid(txtPO.Text.ToString(), txtPO.Text.ToString().IndexOf("@") + 1);
                            if (chuoitemp.ToString().IndexOf("@") > 0)
                            {
                                txtPO.Text = clsPublic.Left(chuoitemp, chuoitemp.IndexOf("@"));
                            }
                        }
                    }
                    //  chức năng kiểm tra INS trc đã hoàn thành chưa
                    //if (levelmachine != "0" && txtCheckFlow.Text.ToString() == "1")
                    //{
                    //    sql = "Select * from " + TableSave + " Where PO='" + txtPO.Text + "' and Finish='OK'";
                    //    sql = sql + " and MachineID in (select MachineID from Inspection_MachineGroup Where GroupLevel='" + grouplevel + "' and LevelMachine='" + (int.Parse(levelmachine.ToString()) - 1).ToString() + "')";
                    //    SqlCommand sqlcmd1 = new SqlCommand(sql, connection);
                    //    SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                    //    if (reader1.HasRows == false)
                    //    {
                    //        txtPO.SelectAll();
                    //        reader1.Close();
                    //        reader1.Dispose();
                    //        return;
                    //    }


                    ////}
                    //sql = "Select sum(XMNGA) as OsakaQty from DT_JDAY_POREQNO_OSAKA Where AUFNR='" + txtPO.Text + "'";
                    //SqlCommand sqlcmdO = new SqlCommand(sql, connection);
                    //SqlDataReader readerO = sqlcmdO.ExecuteReader();
                    //if (readerO.HasRows == true)
                    //{
                    //    while (readerO.Read())
                    //    {
                    //        txtosaka.Text = Math.Round(float.Parse(readerO["OsakaQty"].ToString()), 0).ToString();
                    //    }
                    //}

                    if (AddValue == "1") //Danh cho Shaft group them vao tu master
                    {
                        sql = "Select a.LotNo, a.Qty, a.ITEMCD, a.TYPE, a.TSIZE,a.Length, isnull(b.Type,0) as Type, isnull(b.ProductName,0) as ProductName";
                        sql = sql + " from " + txtTableName.Text.ToString() + " a left outer join Inspection_TypeProduct b on a.ITEMCD=b.Name";
                        sql = sql + " Where POREQNO='" + txtPO.Text + "'";
                    }
                    else
                    {
                        sql = "Select a.LotNo, a.Qty, a.ITEMCD, a.TYPE, a.TSIZE, isnull(Temp.Notice,0) as CheckPlating";
                        sql = sql + " from " + txtTableName.Text.ToString() + " a left outer join ";
                        sql = sql + " (Select distinct Tenhang,[Notice] FROM [Inspection_Master_Congdoantheotenhang] where GroupName='" + txtGroup.Text + "') as Temp on Temp.Tenhang=a.ItemCD";
                        sql = sql + " Where POREQNO='" + txtPO.Text + "'";
                    }
                    SqlCommand sqlcmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = sqlcmd.ExecuteReader();
                    if (reader.HasRows == true)
                    {
                        while (reader.Read())
                        {
                            type = reader["Type"].ToString();
                            txtQty.Text = reader["Qty"].ToString();
                            PlaingProcess = false;
                            if (AddValue == "1")
                            {
                                setvalue = float.Parse(reader["Length"].ToString());
                            }

                            if (txtCheckQty.Text.ToString() == "1")
                            {
                                if (CheckBeforePlating == "1")
                                {
                                    if (reader["CheckPlating"].ToString() == "0" || clsPublic.Left(txtPO.Text.ToString(),2)=="90")
                                    {
                                        PlaingProcess = false;
                                        txtAfterPlating.Text = "AF";
                                    }
                                    else
                                    {
                                        sql = "Select distinct POREQNO from T_WORK_BATCH_POLST Where POREQNO='" + txtPO.Text.ToString() + "'";
                                        //sql = "Select distinct PO from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Notice is null or Notice=0)"; //Xet Truoc khi di ma Notice=1, neu di ma roi thi =0 hoac Null
                                        SqlCommand sqlcmd2 = new SqlCommand(sql, connection);
                                        SqlDataReader reader2 = sqlcmd2.ExecuteReader();
                                        if (reader2.HasRows == true)
                                        {
                                            while (reader2.Read())
                                            {
                                                PlaingProcess = false;
                                            }
                                        }
                                        else
                                        {
                                            if (txtAfterPlating.Text == "AF")
                                            {
                                                MessageBox.Show("Vui lòng thông báo cho Leader kiểm tra kỹ lại hàng này có đi mạ chưa?");
                                                txtPO.SelectAll();
                                                return;
                                            }
                                            else
                                            {
                                                PlaingProcess = true;
                                            }
                                        }
                                        reader2.Close();
                                        reader2.Dispose();
                                    }
                                }
                                if (PlaingProcess == true)
                                {
                                    sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and Notice=1";
                                }
                                else
                                {
                                    sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and (Notice is null or Notice=0)";
                                }
                                switch (txtType.Text)
                                {
                                    case "NO":
                                        sql = sql + " and (Status='NO' or Status is null)";
                                        break;
                                    case "RE":
                                        sql = sql + " and Status='RE'";
                                        break;
                                    case "AD":
                                        sql = sql + " and Status='AD'";
                                        break;
                                }
                                SqlCommand sqlcmd1 = new SqlCommand(sql, connection);
                                SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                                if (reader1.HasRows == true)
                                {
                                    while (reader1.Read())
                                    {
                                        txtQtyAct.Text = (int.Parse(txtQty.Text.ToString()) - int.Parse(reader1["QtyOshaka"].ToString())).ToString();
                                    }
                                }
                                reader1.Close();
                                reader1.Dispose();

                                LoadDataInput();

                                CheckReceive();
                                if (float.Parse(txtQtyAct.Text) <= float.Parse(lblQtyReceive.Text.ToString()))
                                {
                                    if (PlaingProcess == true)
                                    {
                                        PlaingProcess = false;
                                        sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and (Notice is null or Notice=0)";
                                        SqlCommand sqlcmd2 = new SqlCommand(sql, connection);
                                        SqlDataReader reader2 = sqlcmd2.ExecuteReader();
                                        if (reader2.HasRows == true)
                                        {
                                            while (reader2.Read())
                                            {
                                                txtQtyAct.Text = (int.Parse(txtQty.Text.ToString()) - int.Parse(reader2["QtyOshaka"].ToString())).ToString();
                                            }
                                        }
                                        reader1.Close();
                                        reader1.Dispose();

                                        CheckReceive();
                                        if (float.Parse(reader["Qty"].ToString()) <= float.Parse(lblQtyReceive.Text.ToString()))
                                        {
                                            txtPO.SelectAll();
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        txtPO.SelectAll();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                txtQtyAct.Text = reader["Qty"].ToString();
                            }
                            txtLotNo.Text = reader["LotNo"].ToString();
                            lblTenhang.Text = "Tên : " + reader["ITEMCD"].ToString();
                            if (reader["TSIZE"].ToString().IndexOf("-") > 0)
                            {
                                lblTsize.Text = clsPublic.Left(reader["TSIZE"].ToString(), reader["TSIZE"].ToString().IndexOf("-"));
                            }
                            else
                            {
                                lblTsize.Text = reader["TSIZE"].ToString();
                            }
                            lblType.Text = reader["TYPE"].ToString();

                            GetCongdoan();
                            if (txtSTT.Enabled == false)
                            {
                                if (txtQtyAct.Enabled == false)
                                {
                                    txtShift.Focus();
                                }
                                else
                                {
                                    txtQtyAct.Focus();
                                }
                            }
                            else
                            {
                                txtSTT.Focus();
                            }

                            cmdRefresh.Enabled = true;
                            cmdNext.Enabled = true;
                            cmdConfirm.Enabled = true;
                            cmdOK.Enabled = true;
                            cmdNG.Enabled = true;
                            cmdFinish.Enabled = true;
                            cmdRepair.Enabled = true;
                        }
                        // chuyển PO qua supro
                        Tranfer(txtPO.Text);
                        this.Activate();
                    }
                    else
                    {
                        txtPO.SelectAll();
                    }
                    reader.Close();
                    reader.Dispose();
                }
            }
            catch
            {
            }
        }
        private void LoadPO()
        {
            try
            {
                
                    if (txtPO.Text.ToUpper() == "CMDRE")
                    {
                        txtAfterPlating.Text = "";
                        txtType.Text = "";
                        txtPO.Text = "";
                        txtType.Focus();
                        return;
                    }
                    string sql = "";
                    if (CheckKey == "1")
                    {
                        if (CancelKey(txtPO.Text.ToString()) == true || (clsPublic.IsNumeric(txtPO.Text.ToString()) == false && txtPO.Text.ToString().IndexOf("@") <= 0))
                        {
                            txtPO.Text = "";
                            txtPO.Focus();
                            return;
                        }

                        if (txtPO.Text.ToString().IndexOf("@") > 0)
                        {
                            string chuoitemp = clsPublic.Mid(txtPO.Text.ToString(), txtPO.Text.ToString().IndexOf("@") + 1);
                            if (chuoitemp.ToString().IndexOf("@") > 0)
                            {
                                txtPO.Text = clsPublic.Left(chuoitemp, chuoitemp.IndexOf("@"));
                            }
                        }
                    }
                    if (levelmachine != "0" && txtCheckFlow.Text.ToString() == "1")
                    {
                        sql = "Select * from " + TableSave + " Where PO='" + txtPO.Text + "' and Finish='OK'";
                        sql = sql + " and MachineID in (select MachineID from Inspection_MachineGroup Where GroupLevel='" + grouplevel + "' and LevelMachine='" + (int.Parse(levelmachine.ToString()) - 1).ToString() + "')";
                        SqlCommand sqlcmd1 = new SqlCommand(sql, connection);
                        SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                        if (reader1.HasRows == false)
                        {
                            txtPO.SelectAll();
                            reader1.Close();
                            reader1.Dispose();
                            return;
                        }
                    }
                    if (AddValue == "1") //Danh cho Shaft group them vao tu master
                    {
                        sql = "Select a.LotNo, a.Qty, a.ITEMCD, a.TYPE, a.TSIZE,a.Length, isnull(b.Type,0) as Type, isnull(b.ProductName,0) as ProductName";
                        sql = sql + " from " + txtTableName.Text.ToString() + " a left outer join Inspection_TypeProduct b on a.ITEMCD=b.Name";
                        sql = sql + " Where POREQNO='" + txtPO.Text + "'";
                    }
                    else
                    {
                        sql = "Select a.LotNo, a.Qty, a.ITEMCD, a.TYPE, a.TSIZE, isnull(Temp.Notice,0) as CheckPlating";
                        sql = sql + " from " + txtTableName.Text.ToString() + " a left outer join ";
                        sql = sql + " (Select distinct Tenhang,[Notice] FROM [Inspection_Master_Congdoantheotenhang] where GroupName='" + txtGroup.Text + "') as Temp on Temp.Tenhang=a.ItemCD";
                        sql = sql + " Where POREQNO='" + txtPO.Text + "'";
                    }
                    SqlCommand sqlcmd = new SqlCommand(sql, connection);
                    SqlDataReader reader = sqlcmd.ExecuteReader();
                    if (reader.HasRows == true)
                    {
                        while (reader.Read())
                        {
                            type = reader["Type"].ToString();
                            txtQty.Text = reader["Qty"].ToString();
                            PlaingProcess = false;
                            if (AddValue == "1")
                            {
                                setvalue = float.Parse(reader["Length"].ToString());
                            }

                            if (txtCheckQty.Text.ToString() == "1")
                            {
                                if (CheckBeforePlating == "1")
                                {
                                    if (reader["CheckPlating"].ToString() == "0" || clsPublic.Left(txtPO.Text.ToString(), 2) == "90")
                                    {
                                        PlaingProcess = false;
                                        txtAfterPlating.Text = "AF";
                                    }
                                    else
                                    {
                                        sql = "Select distinct POREQNO from T_WORK_BATCH_POLST Where POREQNO='" + txtPO.Text.ToString() + "'";
                                        //sql = "Select distinct PO from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Notice is null or Notice=0)"; //Xet Truoc khi di ma Notice=1, neu di ma roi thi =0 hoac Null
                                        SqlCommand sqlcmd2 = new SqlCommand(sql, connection);
                                        SqlDataReader reader2 = sqlcmd2.ExecuteReader();
                                        if (reader2.HasRows == true)
                                        {
                                            while (reader2.Read())
                                            {
                                                PlaingProcess = false;
                                            }
                                        }
                                        else
                                        {
                                            if (txtAfterPlating.Text == "AF")
                                            {
                                                MessageBox.Show("Vui lòng thông báo cho Leader kiểm tra kỹ lại hàng này có đi mạ chưa?");
                                                txtPO.SelectAll();
                                                return;
                                            }
                                            else
                                            {
                                                PlaingProcess = true;
                                            }
                                        }
                                        reader2.Close();
                                        reader2.Dispose();
                                    }
                                }
                                if (PlaingProcess == true)
                                {
                                    sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and Notice=1";
                                }
                                else
                                {
                                    sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and (Notice is null or Notice=0)";
                                }
                                switch (txtType.Text)
                                {
                                    case "NO":
                                        sql = sql + " and (Status='NO' or Status is null)";
                                        break;
                                    case "RE":
                                        sql = sql + " and Status='RE'";
                                        break;
                                    case "AD":
                                        sql = sql + " and Status='AD'";
                                        break;
                                }
                                SqlCommand sqlcmd1 = new SqlCommand(sql, connection);
                                SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                                if (reader1.HasRows == true)
                                {
                                    while (reader1.Read())
                                    {
                                        txtQtyAct.Text = (int.Parse(txtQty.Text.ToString()) - int.Parse(reader1["QtyOshaka"].ToString())).ToString();
                                    }
                                }
                                reader1.Close();
                                reader1.Dispose();

                            lbNOBF.Text = lbREBF.Text = lbADBF.Text = lbTTBF.Text = "0";
                            lbNOAF.Text = lbREAF.Text = lbADAF.Text = lbTTAF.Text = "0";

                            CheckReceive();
                                if (float.Parse(txtQtyAct.Text) <= float.Parse(lblQtyReceive.Text.ToString()))
                                {
                                    if (PlaingProcess == true)
                                    {
                                        PlaingProcess = false;
                                        sql = "Select isnull(Count(PO),0) as QtyOshaka from " + TableSave + " Where PO='" + txtPO.Text.ToString() + "' and (Finish='NG' or Finish='RE') and (Notice is null or Notice=0)";
                                        SqlCommand sqlcmd2 = new SqlCommand(sql, connection);
                                        SqlDataReader reader2 = sqlcmd2.ExecuteReader();
                                        if (reader2.HasRows == true)
                                        {
                                            while (reader2.Read())
                                            {
                                                txtQtyAct.Text = (int.Parse(txtQty.Text.ToString()) - int.Parse(reader2["QtyOshaka"].ToString())).ToString();
                                            }
                                        }
                                        reader1.Close();
                                        reader1.Dispose();

                                        CheckReceive();
                                        if (float.Parse(reader["Qty"].ToString()) <= float.Parse(lblQtyReceive.Text.ToString()))
                                        {
                                            txtPO.SelectAll();
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        txtPO.SelectAll();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                txtQtyAct.Text = reader["Qty"].ToString();
                            }
                            txtLotNo.Text = reader["LotNo"].ToString();
                            lblTenhang.Text = "Tên : " + reader["ITEMCD"].ToString();
                            if (reader["TSIZE"].ToString().IndexOf("-") > 0)
                            {
                                lblTsize.Text = clsPublic.Left(reader["TSIZE"].ToString(), reader["TSIZE"].ToString().IndexOf("-"));
                            }
                            else
                            {
                                lblTsize.Text = reader["TSIZE"].ToString();
                            }
                            lblType.Text = reader["TYPE"].ToString();

                            GetCongdoan();
                            if (txtSTT.Enabled == false)
                            {
                                if (txtQtyAct.Enabled == false)
                                {
                                    txtShift.Focus();
                                }
                                else
                                {
                                    txtQtyAct.Focus();
                                }
                            }
                            else
                            {
                                txtSTT.Focus();
                            }

                            cmdRefresh.Enabled = true;
                            cmdNext.Enabled = true;
                            cmdConfirm.Enabled = true;
                            cmdOK.Enabled = true;
                            cmdNG.Enabled = true;
                            cmdFinish.Enabled = true;
                            cmdRepair.Enabled = true;
                        }
                    }
                    else
                    {
                        txtPO.SelectAll();
                    }
                    reader.Close();
                    reader.Dispose();
                
            }
            catch
            {
            }
        }

        private void Giatrido()
        {
            try
            {
                if (i > grdProcess.RowCount - 1)
                {
                    CheckBeforeSave();
                }
                else
                {
                    while (i <= grdProcess.RowCount - 1 && (grdProcess.Rows[i].Cells[12].Value.ToString() == "0" || grdProcess.Rows[i].Cells[8].Value.ToString()=="0"))
                    {
                        i = i + 1;
                    }
                    if (i > grdProcess.RowCount - 1)
                    {
                        CheckBeforeSave();
                    }
                    else
                    {
                        if (CheckBeforePlating == "1")
                        {
                            if (PlaingProcess == true)
                            {
                                lblCongdoan.Text = grdProcess.Rows[i].Cells[3].Value.ToString() + "/Before Plating";
                            }
                            else
                            {
                                lblCongdoan.Text = grdProcess.Rows[i].Cells[3].Value.ToString() + "/After Plating"; ;
                            }
                        }
                        else
                        {
                            lblCongdoan.Text = grdProcess.Rows[i].Cells[3].Value.ToString();
                        }
                        txtMin.Text = grdProcess.Rows[i].Cells[8].Value.ToString();
                        txtMax.Text = grdProcess.Rows[i].Cells[9].Value.ToString();
                        max_up = double.Parse(grdProcess.Rows[i].Cells[8].Value.ToString());
                        min_up = double.Parse(grdProcess.Rows[i].Cells[9].Value.ToString());
                        tb_up = (max_up + min_up) / 2;
                        if (txtCheckFlow.Text == "1" && levelmachine != "0" && txtPO.Text.Length > 0 && checkhieuchinh == "1" && grdProcess.Rows[i].Cells[13].Value.ToString() != "0")
                        {
                            txtCDTruoc.Text = grdProcess.Rows[int.Parse(grdProcess.Rows[i].Cells[13].Value.ToString()) - 1].Cells[2].Value.ToString();
                            txtCDSau.Text = grdProcess.Rows[i].Cells[5].Value.ToString();
                            txtHieuchinh.Text = (float.Parse(txtCDSau.Text.ToString()) - float.Parse(txtCDTruoc.Text.ToString())).ToString();
                        }
                        LoaddulieuChart(i + 1);
                        DrawChart(i + 1);
                        grdProcess.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        string pathfile = Application.StartupPath + "\\Pic\\Thuoc do\\" + grdProcess.Rows[i].Cells[1].Value.ToString() + ".png";
                        if (System.IO.File.Exists(pathfile))
                        {
                            Bitmap bmp = new Bitmap(pathfile);
                            picThuoc.Image = bmp;
                        }
                        else
                        {
                            Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                            PicBanve.Image = bmp;
                        }

                        pathfile = Application.StartupPath + "\\Pic\\Ban ve\\" + txtGroup.Text + "\\" + grdProcess.Rows[i].Cells[0].Value.ToString() + ".JPG";
                        if (System.IO.File.Exists(pathfile))
                        {
                            Bitmap bmp = new Bitmap(pathfile);
                            PicBanve.Image = bmp;
                        }
                        else
                        {
                            Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                            PicBanve.Image = bmp;
                        }

                        pathfile = Application.StartupPath + "\\Pic\\Thao tac\\" + txtGroup.Text + "\\" + grdProcess.Rows[i].Cells[0].Value.ToString() + ".JPG";
                        if (System.IO.File.Exists(pathfile))
                        {
                            Bitmap bmp = new Bitmap(pathfile);
                            PicThaotac.Image = bmp;
                        }
                        else
                        {
                            Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                            PicThaotac.Image = bmp;
                        }

                        txtDo.Text = "";
                        txtDo.Focus();
                    }
                }
            }
            catch
            {
                txtDo.Text = "";
                txtDo.Focus();
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = "";
                string ketqua = "";
                int dem = 0;
                string chuoifield = "";
                for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                {
                    chuoifield = chuoifield + ",[" + hh + "],[R" + hh + "]";
                }
                sql = "Insert into " + TableSave + "([EmpID],DateProcess,[MachineID],[ShiftID],[PORefer],[PO],ItemCD,[Status],[STTDo],[QtyAct]";
                sql = sql + chuoifield;
                sql = sql + ",[Finish],[Reason],[Notice])";
                sql = sql + " VALUES ";
                sql = sql + "('" + txtManhanvien.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + txtTenmay.Text + "'";
                switch (txtType.Text)
                {
                    case "NO":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','NO',";
                        break;
                    case "AD":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-02','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','AD',";
                        break;
                    case "RE":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-RE','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','RE',";
                        break;
                }
                if (txtCheckQty.Text == "1")
                {
                    sql = sql + "'" + lblQtyReceive.Text + "',";
                }
                else
                {
                    sql = sql + "'" + txtSTT.Text + "',";
                }
                sql = sql + "'" + txtQtyAct.Text + "'";
                for (int a = 0; a < int.Parse(txtDemsocongdoan.Text.ToString()); a++)
                {
                    ketqua = ketqua + ",'" + grdProcess.Rows[a].Cells[11].Value + "','" + grdProcess.Rows[a].Cells[10].Value + "'";
                    if (txtReason.Text.ToString().Length > 0)
                    {
                    }
                    else
                    {
                        if (grdProcess.Rows[a].Cells[10].Value.ToString() == "NG")
                        {
                            lblThongbao.Text = "Đo " + grdProcess.Rows[a].Cells[3].Value + " bị NG. Nếu xác nhận OK thì vui lòng nhập lý do";
                            return;
                        }
                    }
                    if (grdProcess.Rows[a].Cells[10].Value.ToString().Length == 0)
                    {
                        dem = dem + 1;
                    }
                    if (dem == int.Parse(txtDemsocongdoan.Text.ToString()))
                    {
                        lblThongbao.Text = "Chưa nhập dữ liệu để đo";
                        return;
                    }
                }
                sql = sql + ketqua + ",'OK','" + txtReason.Text + "'";
                if (PlaingProcess == true)
                {
                    sql = sql + ",'1')";
                }
                else
                {
                    sql = sql + ",'0')";
                }
                cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();

                string sql1 = "Select OrderNo from Inspection_MachineINS Where OrderNo='" + txtPO.Text + "'";
                SqlCommand sqlcmd1 = new SqlCommand(sql1, connection);
                SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                if (reader1.HasRows == false)
                {
                    sql = "Insert into Inspection_MachineINS (MachineINS, OrderNo, Inputdate) VALUES ('" + Dns.GetHostName() + "','" + txtPO.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "')";
                    cmd = new SqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                }

                Loaddulieuluoi();
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            //    MessageBox.Show("9");
                XoadulieutextboxOKNG();
            }
            catch
            {
            }
        }

        private void cmdNG_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtReason.Text.ToString().Length > 0)
                {
                    string sql = "";
                    string ketqua = "";
                    int dem = 0;
                    string chuoifield = "";
                    for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                    {
                        chuoifield = chuoifield + ",[" + hh + "],[R" + hh + "]";
                    }
                    sql = "Insert into " + TableSave + "([EmpID],DateProcess,[MachineID],[ShiftID],[PORefer],[PO],ItemCD,[Status],[STTDo],[QtyAct]";
                    sql = sql + chuoifield;
                    sql = sql + ",[Finish],[Reason],Notice)";
                    sql = sql + " VALUES ";
                    sql = sql + "('" + txtManhanvien.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + txtTenmay.Text + "'";
                    switch (txtType.Text)
                    {
                        case "NO":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','NO',";
                            break;
                        case "AD":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-02','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','AD',";
                            break;
                        case "RE":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-RE','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','RE',";
                            break;
                    }
                    if(txtCheckQty.Text=="1")
                    {
                        sql=sql + "'" + lblQtyReceive.Text + "',";
                    }
                    else
                    {
                        sql=sql + "'" + txtSTT.Text + "',";
                    }
                    sql= sql + "'" + txtQtyAct.Text + "'";
                    for (int a = 0; a < int.Parse(txtDemsocongdoan.Text.ToString()); a++)
                    {
                        ketqua = ketqua + ",'" + grdProcess.Rows[a].Cells[11].Value + "','" + grdProcess.Rows[a].Cells[10].Value + "'";

                        if (grdProcess.Rows[a].Cells[10].Value.ToString().Length == 0)
                        {
                            dem = dem + 1;
                        }
                        if (dem == int.Parse(txtDemsocongdoan.Text.ToString()))
                        {
                            lblThongbao.Text = "Chưa nhập dữ liệu để đo";
                            return;
                        }
                    }
                    sql = sql + ketqua + ",'NG','" + txtReason.Text + "'";
                    if (PlaingProcess == true)
                    {
                        sql = sql + ",'1')";
                    }
                    else
                    {
                        sql = sql + ",'0')";
                    }
                    cmd = new SqlCommand(sql, connection);
                  cmd.ExecuteNonQuery();
                }
                else
                {
                    lblThongbao.Text = "Vui lòng nhập lý do bị NG";
                    return;
                }
                Loaddulieuluoi();
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                XoadulieutextboxOKNG();
            }
            catch
            {
            }
        }

        private void Loaddulieuluoi()
        {
            try
            {
                string sql = "";
                string chuoifield = "";
                for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                {
                    chuoifield = chuoifield + ",[" + hh + "]";
                }
                sql = "Select top 10 [PO],[STTDo],[QtyAct]";
                sql = sql + chuoifield;
                sql = sql + ",[Finish],[Reason]";
                sql = sql + " from " + TableSave + "";
                sql = sql + " Where EmpID='" + txtManhanvien.Text + "' and MachineID='" + txtTenmay.Text + "'";
                //sql = sql + " and DateProcess between '" + DateTime.Now.ToString("yyyy/MM/dd 00:00:00") + "' and '" + DateTime.Now.ToString("yyyy/MM/dd 23:59:59") + "'";
                sql = sql + " order by DateProcess desc";
                DataTable dt = new DataTable();
                da = new SqlDataAdapter(sql, connection);
                da.Fill(dt);
                DataView data = new DataView(dt);
                grdDetail.DataSource = data;
            }
            catch
            {
            }
        }

        private void LoaddulieuChart(int Process)
        {
            try
            {
                string sql;
                string chuoifield = "";
                for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                {
                    chuoifield = chuoifield + ",[" + hh + "]";
                }
                sql = "Select top 10 [PO]";
                sql = sql + chuoifield;
                sql = sql + ",[Finish],[Reason]";
                sql = sql + " from " + TableSave + "";
                sql = sql + " Where [" + Process + "] is not null and [" + Process + "] <>0 and MachineID='" + txtTenmay.Text + "'";
                sql = sql + " and ItemCD='" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "'";
                //sql = sql + " and DateProcess between dateadd(day,-1,'" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "') and '" + DateTime.Now.ToString("yyyy/MM/dd 23:59:59") + "'";
                //sql = sql + " and DateProcess between '" + DateTime.Now.ToString("yyyy/MM/dd 00:00:00") + "' and '" + DateTime.Now.ToString("yyyy/MM/dd 23:59:59") + "'";
                if (CheckBeforePlating == "1")
                {
                    if (PlaingProcess == true)
                    {
                        sql = sql + " and Notice=1";
                    }
                    else
                    {
                        sql = sql + " and (Notice is null or Notice=0)";
                    }
                }
                else
                {
                    sql = sql + " and (Notice is null or Notice=0)";
                }
                sql = sql + " order by DateProcess desc";
                DataTable dt1 = new DataTable();
                da1 = new SqlDataAdapter(sql, connection);
                da1.Fill(dt1);
                DataView data1 = new DataView(dt1);
                grdChartView.DataSource = data1;
            }
            catch
            {
            }
        }

        private void XoadulieutextboxOKNG()
        {
            try
            {
                txtReason.Text = "";

                //grdProcess.DataSource = null;
                //picThuoc.Image = null;
                //PicBanve.Image = null;
                //PicThaotac.Image = null;
                //lblCongdoan.Text = "";

                txtDo.Text = "";
                //cmdAlarm.Text = "";
                //cmdAlarm.BackColor = Color.FromArgb(255, 255, 192);

                //txtDo.Focus();
                //lblThongbao.Text = " ";
                i = 0;
                cmdRefresh.PerformClick();
                //GetCongdoan();
            }
            catch
            {
            }
        }
        private void XoadulieutextboxFinish_Linemore()
        {
            try
            {
                // txtPO.Text = "";
                txtLotNo.Text = "";
                txtQty.Text = "";
                //  txtSTT.Text = "";
                txtQtyAct.Text = "";
                txtReason.Text = "";
                txtDo.Text = "";
                grdProcess.DataSource = null;
                picThuoc.Image = null;
                PicBanve.Image = null;
                PicThaotac.Image = null;
                lblCongdoan.Text = "";
                lblTenhang.Text = "";
                lblTsize.Text = "";
                lblType.Text = "";
                //  txtType.Text = "";
                //   txtAfterPlating.Text = "";
                cmdAlarm.Text = "";
                cmdAlarm.BackColor = Color.FromArgb(255, 255, 192);
                txtMax.Text = "";
                txtMin.Text = "";
                if (txtShift.Enabled == true)
                {
                    txtShift.Text = "";
                }
                if (txtTenmay.Enabled == true)
                {
                    txtTenmay.Text = "";
                }
                //if (txtManhanvien.Enabled == false)
                //{
                //    txtPO.Focus();
                //}
                //else
                //{
                //  //  txtManhanvien.Text = "";
                //  //  txtTennhanvien.Text = "";
                // //   Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                // //   picEmp.Image = bmp;
                // //   txtManhanvien.Focus();
                //}
                cmdRefresh.Enabled = false;
                cmdNext.Enabled = false;
                cmdConfirm.Enabled = false;
                cmdOK.Enabled = false;
                cmdNG.Enabled = false;
                cmdFinish.Enabled = false;
                cmdRepair.Enabled = false;
                lblThongbao.Text = " ";
                i = 0;

                txtPO.Focus();
                // txtPO.SelectionStart = txtPO.Text.Length;
                txtPO.Text = txtPO.Text + "\n";
            }
            catch
            {
            }
        }
        private void XoadulieutextboxFinish()
        {
            try
            {
                txtPO.Text = "";
                txtLotNo.Text = "";
                txtQty.Text = "";
                txtSTT.Text = "";
                txtQtyAct.Text = "";
                txtReason.Text = "";
                txtDo.Text = "";
                grdProcess.DataSource = null;
                picThuoc.Image = null;
                PicBanve.Image = null;
                PicThaotac.Image = null;
                lblCongdoan.Text = "";
                lblTenhang.Text = "";
                lblTsize.Text = "";
                lblType.Text = "";
                txtType.Text = "";
                txtAfterPlating.Text = "";
                cmdAlarm.Text = "";
                cmdAlarm.BackColor = Color.FromArgb(255, 255, 192);
                txtMax.Text = "";
                txtMin.Text = "";
                if (txtShift.Enabled == true)
                {
                    txtShift.Text = "";
                }
                if (txtTenmay.Enabled == true)
                {
                    txtTenmay.Text = "";
                }
                if (txtManhanvien.Enabled == false)
                {
                    txtPO.Focus();
                }
                else
                {
                    txtManhanvien.Text = "";
                    txtTennhanvien.Text = "";
                    Bitmap bmp = new Bitmap(Application.StartupPath + "\\Pic\\AddPicture.jpg");
                    picEmp.Image = bmp;
                    txtManhanvien.Focus();
                }
                cmdRefresh.Enabled = false;
                cmdNext.Enabled = false;
                cmdConfirm.Enabled = false;
                cmdOK.Enabled = false;
                cmdNG.Enabled = false;
                cmdFinish.Enabled = false;
                cmdRepair.Enabled = false;
                lblThongbao.Text = " ";
                i = 0;
            }
            catch
            {
            }
        }

        private void cmdNext_Click(object sender, EventArgs e)
        {
            try
            {
                cmdAlarm.BackColor = Color.FromArgb(255, 255, 192);
                cmdAlarm.Text = "";
                if (i <= grdProcess.RowCount - 1)
                {
                    grdProcess.Rows[i].Cells[10].Value = "Pass";
                    grdProcess.Rows[i].Cells[11].Value = "";
                    grdProcess.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }
                i = i + 1;
                if (i > grdProcess.RowCount - 1)
                {
                    CheckBeforeSave();
                    //txtReason.Focus();
                }
                else
                {
                    Giatrido();
                    //txtDo.Text = "";
                    //txtDo.Focus();
                }
            }
            catch
            {
            }
        }

        private void txtQtyAct_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    if (txtQtyAct.Text.Length > 0)
                    {
                        if (CancelKey(txtQtyAct.Text.ToString()) == true && CheckKey == "1")
                        {
                            txtQtyAct.Text = "";
                            txtQtyAct.Focus();
                        }
                        else
                        {
                            switch (clsPublic.Right(txtQtyAct.Text.ToString(), 2).ToUpper())
                            {
                                case "BS":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 3);
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N0":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "0";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N1":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "1";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N2":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "2";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N3":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "3";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N4":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "4";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N5":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "5";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N6":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "6";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N7":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "7";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N8":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "8";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N9":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "9";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "NA":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + "00";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "N.":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2) + ".";
                                        txtQtyAct.SelectionLength = 0;
                                        txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
                                        break;
                                    }
                                case "EN":
                                    {
                                        txtQtyAct.Text = clsPublic.Left(txtQtyAct.Text.ToString(), txtQtyAct.Text.Length - 2);
                                        if (txtTenmay.Enabled == false)
                                        {
                                            if (txtShift.Enabled == false)
                                            {
                                                txtDo.Focus();
                                            }
                                            else
                                            {
                                                txtShift.Focus();
                                            }
                                        }
                                        else
                                        {
                                            txtTenmay.Focus();
                                        }
                                        break;
                                    }
                            }
                            //int isNumber = 0;
                            //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                        }
                    }
                    else
                    {
                        if (clsPublic.Right(txtQtyAct.Text.ToString(), 2) == "BS")
                        {
                            e.Handled = false;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void txtConfirm_KeyPress(object sender, KeyPressEventArgs e)
        {
            //try
            //{
                if (e.KeyChar == (char)Keys.Enter && txtConfirm.Text.Length > 0)
                {
                    if (txtConfirm.Text.ToUpper() == "CMDNC")
                    {
                        txtConfirm.Text = "";
                        grpConfirm.Visible = false;
                        txtDo.Focus();
                    }
                    else
                    {
                        string sql = "";
                        sql = "select distinct Inspection_UserConfirmPassword.*";
                        sql = sql + " from Inspection_MachineGroup left outer join Inspection_UserConfirmPassword on Inspection_MachineGroup.ConfirmID=Inspection_UserConfirmPassword.ConfirmID";
                        sql = sql + " Where MachineID='" + txtTenmay.Text.ToUpper() + "' and Password='" + txtConfirm.Text.ToUpper() + "'";
                        SqlCommand sqlcmd = new SqlCommand(sql, connection);
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                grdProcess.Rows[i].Cells[10].Value = "OK-" + reader["ConfirmID"].ToString();
                                grdProcess.Rows[i].Cells[11].Value = clsPublic.Left(cmdAlarm.Text.ToString(), cmdAlarm.Text.ToString().IndexOf("\r\n") - 1);
                                grdProcess.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                cmdAlarm.BackColor = Color.LightSkyBlue;
                                cmdAlarm.Text = cmdAlarm.Text.ToString() + "\r\nConfirm";
                                if (i > grdProcess.RowCount - 1)
                                {
                                    //MessageBox.Show("Da hoan thanh, vui long nhan ok de tiep tuc");
                                    //cmdRefresh.PerformClick();
                                    txtReason.Focus();
                                }
                                else
                                {
                                    i = i + 1;
                                    Giatrido();
                                    txtConfirm.Text = "";
                                    grpConfirm.Visible = false;
                                    //txtDo.Text = "";
                                    //txtDo.Focus();
                                }
                                break;
                            }
                        }
                        else
                        {
                            txtConfirm.SelectAll();
                        }
                        reader.Close();
                        reader.Dispose();
                    }
                }
            //}
            //catch
            //{
            //}
        }

        private void cmdConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (grpConfirm.Visible == true)
                {
                    grpConfirm.Visible = false;
                    txtDo.Focus();
                }
                else
                {
                    grpConfirm.Visible = true;
                    txtConfirm.Focus();
                }
            }
            catch
            {
            }
        }

        #region Mouse with Com32
        private void DataReceive(object obj, SerialDataReceivedEventArgs e)
        {
            if (P.IsOpen==false)
            {
                try
                {
                    P.Open();
                }
                catch
                {
                }
            }
            //  InputData = P.ReadExisting();
            InputData1 = P1.ReadLine();
            if (InputData != String.Empty)
            {
                SetText(InputData);
            }
        }
        private void SetText(string text)
        {
            if (this.receive.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(SetText); // khởi tạo 1 delegate mới gọi đến SetText
                    this.Invoke(d, new object[] { text });
                }
                catch
                {
                }
            }
            else
            {
                //receive.Text = InputData;
                bien = InputData;
                // txtIn.Text = InputData; // Ko dùng đc như thế này vì khác threads .
                //SetText(InputData); // Chính vì vậy phải sử dụng ủy quyền tại đây. Gọi delegate đã khai báo trước đó.
                //******************************************************************
                if (bien == "T")
                {
                    dem = dem + 1;
                    if (dem == 1)
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Maximized;
                        dem = 0;
                    }
                }
            }// this.receive.Text += text;
        }
        private void Chon_com_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (P.IsOpen)
            {
                try
                {
                    P.Close(); // Nếu đang mở Port thì phải đóng lại
                }
                catch
                {
                }
            }
            P.PortName = combcomport.SelectedItem.ToString(); // Gán PortName bằng COM đã chọn 
        }
        private void BAUD_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (P.IsOpen)
            {
                try
                {
                    P.Close();
                }
                catch
                {
                }
            }
            P.BaudRate = Convert.ToInt32(combbaud.Text);
        }
        private void connect()
        {
            //if (P.IsOpen)
            //{
            //    P.Close();
            //}
            try
            {
                
                P.Open(); combcomport.Enabled = false;
                combbaud.Enabled = false;
                lblConnectCom.Enabled = false; lblConnectCom.Text = combcomport.SelectedItem.ToString() + " is connected";
            }
            catch (Exception)
            {
                //lblConnectCom.Text = "COM " + combcomport.SelectedItem.ToString() + " connected fail";
            }
        }
        private void disconnect()
        {
            try
            {
                P.Close();
                combcomport.Enabled = true;
                combbaud.Enabled = true;
                //Status.Enabled = true;
                lblConnectCom.Text = "Not connect";
            }
            catch { lblConnectCom.Text = "Không tồn tại cổng COM"; }
        }
        private void send_data()
        {
            try { P.Write("HUNG"); }
            catch
            {
                //lblConnectCom.Text = "Không kết nối cổng COM";
                timer1.Start();
            }
        }
        private void send_data2()
        {
            try { P.Write("HUNG"); }
            catch
            {
                //lblConnectCom.Text = "Không kết nối cổng COM";
                timer1.Start();
            }
        }
        #endregion

        #region Mouse with Com32 COM1
        private void DataReceive1(object obj, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (P1.IsOpen == false)
                {

                    P1.Open();

                }
              //  MessageBox.Show("4");
                InputData1 = P1.ReadLine();
                //InputData1 = P1.ReadExisting();
                if (InputData1 != String.Empty)
                {
                  ////  MessageBox.Show("3");
                    SetText1(InputData1);

                }
            }
            catch
            { }
            // try
            //    {
            //if (P1.IsOpen == false)
            //{

            //        P1.Open();

            //}
            //InputData1 = P1.ReadExisting();
            //if (InputData1 != String.Empty)
            //{

            //        SetText1(InputData1);

            //}
            //    }
            // catch
            // { }
        }
        string DataComText1 = "";
        private void SetText1(string text1)
        {
            if (this.receive1.InvokeRequired)
            {
                try
                {
                    SetTextCallback1 d1 = new SetTextCallback1(SetText1); // khởi tạo 1 delegate mới gọi đến SetText
                    this.Invoke(d1, new object[] { text1 });
                }
                catch
                {
                }
            }
            else
            {
                //  
                try
                {
                  //  MessageBox.Show("1");
                    Thread.Sleep(50);
                    txtDo.Clear();
                    // MessageBox.Show (InputData1.Length.ToString());
                    if (InputData1.Length == 13 || InputData1.Length == 14)
                    {
                      //  MessageBox.Show("2");
                        txtDo.Text = clsPublic.Left(InputData1.Trim(), InputData1.Trim().Length - 4);
                        System.Windows.Forms.SendKeys.Send("{ENTER}");
                        return;
                    }

                }
                catch
                {
                }
            }
            //if (this.receive1.InvokeRequired)
            //{
            //    try
            //    {
            //        SetTextCallback1 d1 = new SetTextCallback1(SetText1); // khởi tạo 1 delegate mới gọi đến SetText
            //        this.Invoke(d1, new object[] { text1 });
            //    }
            //    catch
            //    {
            //    }
            //}
            //else
            //{
            //    DataComText1 += text1;
            //    if (DataComText1.IndexOf("\r") != -1 && DataComText1.IndexOf("\n") != -1)
            //    {
            //        DataComText1 = DataComText1.Trim().Substring(0, DataComText1.Trim().IndexOf(' '));
            //        txtDo.Text = DataComText1.Trim();
            //        System.Windows.Forms.SendKeys.Send("{ENTER}");
            //        DataComText1 = "";
            //    }
            //    //  
            //    //try
            //    //{
            //    //    //MessageBox.Show(InputData.Length.ToString());
            //    //    Thread.Sleep(50);
            //    //    //textBox1.Clear();
            //    //    //textBox2.Clear();
            //    //    txtDo.Clear();
            //    //   // MessageBox.Show (InputData1.Length.ToString());
            //    //    if (InputData1.Length == 13 || InputData1.Length == 14)
            //    //    {
            //    //        txtDo.Text = clsPublic.Left(InputData1.Trim(), InputData1.Trim().Length - 4);
            //    //        //textBox2.Text = clsPublic.Right(InputData.Trim(), InputData.Trim().Length - 8);
            //    //        //textBox1.Text = clsPublic.Left(InputData.Trim(), InputData.Trim().Length - 4);
            //    //        System.Windows.Forms.SendKeys.Send("{ENTER}");
            //    //        return;
            //    //    }
            //    //    else
            //    //    {
            //    //        txtDo.Clear();
            //    //        //textBox1.Clear();
            //    //        //textBox2.Clear();
            //    //        //send_data1();
            //    //    }

            //    //}
            //    //catch
            //    //{
            //    //    //send_data1();
            //    //}
            //}
        }
        private void Chon_com1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (P1.IsOpen)
            {
                try
                {
                    P1.Close(); // Nếu đang mở Port thì phải đóng lại
                }
                catch
                {
                }
            }
            P1.PortName = combcomport1.SelectedItem.ToString(); // Gán PortName bằng COM đã chọn 
        }
        private void BAUD1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (P1.IsOpen)
            {
                try
                {
                    P1.Close();
                }
                catch
                {
                }
            }
            P1.BaudRate = Convert.ToInt32(combbaud1.Text);
        }
        private void connect1()
        {
            //if (P1.IsOpen)
            //{
            //    P1.Close();
            //}
            try
            {
                P1.Open(); combcomport1.Enabled = false; combbaud1.Enabled = false;
                lblConnectCom.Enabled = false; lblConnectCom.Text = combcomport1.SelectedItem.ToString() + " is connected";
            }
            catch (Exception)
            {
                //lblConnectCom.Text = "COM " + combcomport.SelectedItem.ToString() + " connected fail";
            }
        }
        private void disconnect1()
        {
            try
            {
                P1.Close();
                combcomport1.Enabled = true;
                combbaud1.Enabled = true;
                //Status.Enabled = true;
                lblConnectCom.Text = "Not connect";
            }
            catch { lblConnectCom.Text = "Không tồn tại cổng COM"; }
        }
        private void send_data1()
        {
            try { P1.Write("D"); }
            catch
            {
                lblConnectCom.Text = "Không kết nối cổng COM";
            }
        }
        #endregion

        private void txtTenmay_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    if (txtTenmay.Text.Length > 0)
                    {
                        string sql1 = "Select * from Inspection_MachineGroup Where MachineID='" + txtTenmay.Text + "' and (DeleteFlag=0 or DeleteFlag is null)";
                        SqlCommand sqlcmd1 = new SqlCommand(sql1, connection);
                        SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                        if (reader1.HasRows == true)
                        {
                            while (reader1.Read())
                            {
                                txtGroup.Text = reader1[1].ToString();
                                txtTableName.Text = reader1[2].ToString();
                                grouplevel = reader1["GroupLevel"].ToString();
                                levelmachine = reader1["LevelMachine"].ToString();
                                TableSave = reader1["TableResult"].ToString();
                            }
                        }

                        string sql = "Select Max(IDNumber) from Inspection_Master_Congdoan Where GroupName='" + txtGroup.Text + "'";
                        SqlCommand sqlcmd = new SqlCommand(sql, connection);
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        if (reader.HasRows == true)
                        {
                            while (reader.Read())
                            {
                                txtDemsocongdoan.Text = reader[0].ToString();
                            }
                        }
                    }

                    if (txtShift.Enabled == false)
                    {
                        txtTenmay.Text = txtTenmay.Text.ToUpper();
                        txtDo.Focus();
                    }
                    else
                    {
                        txtShift.Focus();
                    }
                }
            }
            catch
            {
            }
        }

        private void txtShift_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    if (CancelKey(txtShift.Text.ToString()) == true && CheckKey == "1")
                    {
                        txtShift.Text = "";
                        txtShift.Focus();
                    }
                    else
                    {
                        txtDo.Focus();
                    }
                }
            }
            catch
            {
            }
        }

        private void BarcodeButtonDO(string Barcodetruyen)
        {
            try
            {
                switch (Barcodetruyen.ToString().ToUpper())
                {
                    case "CMDRE":
                        {
                            txtDo.Text = "";
                            cmdRefresh.PerformClick();
                            break;
                        }
                    case "CMDNE":
                        {
                            txtDo.Text = "";
                            cmdNext.PerformClick();
                            break;
                        }
                    case "CMDCF":
                        {
                            txtDo.Text = "";
                            cmdConfirm.PerformClick();
                            break;
                        }
                    case "TXTRS":
                        {
                            txtDo.Text = "";
                            txtReason.Focus();
                            break;
                        }
                    case "CMDOK":
                        {
                            txtDo.Text = "";
                            if (txtCheckQty.Text == "1")
                            {
                                lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                if (int.Parse(lblQtyReceive.Text) >= int.Parse(txtQtyAct.Text))
                                {
                                    FinishPO(0);
                                    lblQtyReceive.Text = "0";
                                }
                                else
                                {
                                    MessageBox.Show("9");
                                    cmdOK.PerformClick();
                                }
                            }
                            else
                            {
                                cmdOK.PerformClick();
                            }
                            break;
                        }
                    case "CMDFI":
                        {
                            txtDo.Text = "";
                            if (txtCheckQty.Text == "1")
                            {
                                lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                if (int.Parse(lblQtyReceive.Text) >= int.Parse(txtQtyAct.Text))
                                {
                                    FinishPO(0);
                                    lblQtyReceive.Text = "0";
                                }
                                else
                                {
                                    cmdOK.PerformClick();
                                }
                            }
                            else
                            {
                                cmdFinish.PerformClick();
                            }
                            break;
                        }
                    case "CMDNG":
                        {
                            txtDo.Text = "";
                            if (txtCheckQty.Text == "1")
                            {
                                lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                if (int.Parse(lblQtyReceive.Text) >= int.Parse(txtQtyAct.Text))
                                {
                                    FinishPO(1);
                                    lblQtyReceive.Text = "0";
                                }
                                else
                                {
                                    cmdNG.PerformClick();
                                }
                            }
                            else
                            {
                                cmdNG.PerformClick();
                            }
                            break;
                        }
                    case "CMDRP":
                        {
                            txtDo.Text = "";
                            if (txtCheckQty.Text == "1")
                            {
                                lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                if (int.Parse(lblQtyReceive.Text) >= int.Parse(txtQtyAct.Text))
                                {
                                    FinishPO(2);
                                    lblQtyReceive.Text = "0";
                                }
                                else
                                {
                                    cmdNG.PerformClick();
                                }
                            }
                            else
                            {
                                cmdNG.PerformClick();
                            }
                            break;
                        }
                    case "CMDCO":
                        {
                            txtDo.Text = "";
                            KiemtraConnect();
                            break;
                        }
                    case "CMDDI":
                        {
                            txtDo.Text = "";
                            disconnect();
                            break;
                        }
                }
            }
            catch
            {
            }
        }

        private void BarcodeButtonReason(string Barcodetruyen)
        {
            try
            {
                if (Barcodetruyen.Length >= 5)
                {
                    switch (clsPublic.Right(Barcodetruyen, 5).ToUpper())
                    {
                        case "CMDXO":
                            {
                                txtReason.Text = "";
                                break;
                            }
                        case "CMDOK":
                            {
                                txtReason.Text = clsPublic.Left(txtReason.Text.ToString(), txtReason.Text.Length - 5);
                                if (txtCheckQty.Text == "1")
                                {
                                    lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                    if (int.Parse(lblQtyReceive.Text) == int.Parse(txtQtyAct.Text))
                                    {
                                        lblQtyReceive.Text = "0";
                                        cmdFinish.PerformClick();
                                    }
                                    else
                                    {
                                        cmdOK.PerformClick();
                                    }
                                }
                                else
                                {
                                    cmdOK.PerformClick();
                                }
                                break;
                            }
                        case "CMDFI":
                            {
                                txtReason.Text = clsPublic.Left(txtReason.Text.ToString(), txtReason.Text.Length - 5);
                                if (txtCheckQty.Text == "1")
                                {
                                    lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                    if (int.Parse(lblQtyReceive.Text) == int.Parse(txtQtyAct.Text))
                                    {
                                        cmdFinish.PerformClick();
                                        lblQtyReceive.Text = "0";
                                    }
                                    else
                                    {
                                        cmdOK.PerformClick();
                                    }
                                }
                                else
                                {
                                    cmdFinish.PerformClick();
                                }
                                break;
                            }
                        case "CMDNG":
                            {
                                txtReason.Text = clsPublic.Left(txtReason.Text.ToString(), txtReason.Text.Length - 5);
                                if (txtCheckQty.Text == "1")
                                {
                                    lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                    if (int.Parse(lblQtyReceive.Text) == int.Parse(txtQtyAct.Text))
                                    {
                                        FinishPO(1);
                                        lblQtyReceive.Text = "0";
                                    }
                                    else
                                    {
                                        cmdNG.PerformClick();
                                    }
                                }
                                else
                                {
                                    cmdNG.PerformClick();
                                }
                                break;
                            }
                        case "CMDRE":
                            {
                                txtReason.Text = "";
                                cmdRefresh.PerformClick();
                                break;
                            }
                        case "CMDRP":
                            {
                                txtReason.Text = clsPublic.Left(txtReason.Text.ToString(), txtReason.Text.Length - 5);
                                if (txtCheckQty.Text == "1")
                                {
                                    lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                                    if (int.Parse(lblQtyReceive.Text) == int.Parse(txtQtyAct.Text))
                                    {
                                        FinishPO(2);
                                        lblQtyReceive.Text = "0";
                                    }
                                    else
                                    {
                                        cmdRepair.PerformClick();
                                    }
                                }
                                else
                                {
                                    cmdRepair.PerformClick();
                                }
                                break;
                            }
                    }
                }
            }
            catch
            {
            }
        }

        private void txtReason_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && txtReason.Text.ToString().Length > 0)
                {
                    if (CheckKey == "1")
                    {
                        if (CancelKey(txtReason.Text.ToString()) == true)
                        {
                            txtReason.Text = "";
                            txtReason.Focus();
                            return;
                        }
                    }
                    BarcodeButtonReason(txtReason.Text.ToString());
                }
            }
            catch
            {
            }
        }

        private void txtSTT_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (txtSTT.Text.Length > 0)
                    {
                        switch (clsPublic.Right(txtSTT.Text.ToString(), 2).ToUpper())
                        {
                            case "BS":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 3);
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N0":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "0";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N1":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "1";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N2":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "2";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N3":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "3";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N4":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "4";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N5":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "5";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N6":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "6";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N7":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "7";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N8":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "8";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N9":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "9";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "NA":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + "00";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "N.":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2) + ".";
                                    txtSTT.SelectionLength = 0;
                                    txtSTT.SelectionStart = txtSTT.Text.Length;
                                    break;
                                }
                            case "EN":
                                {
                                    txtSTT.Text = clsPublic.Left(txtSTT.Text.ToString(), txtSTT.Text.Length - 2);
                                    txtQtyAct.Focus();
                                    break;
                                }
                        }
                        //int isNumber = 0;
                        //e.Handled = !int.TryParse(e.KeyChar.ToString(), out isNumber);
                    }
                    else
                    {
                        if (clsPublic.Right(txtSTT.Text.ToString(), 2).ToUpper() == "BS")
                        {
                            e.Handled = false;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void txtQtyAct_Enter(object sender, EventArgs e)
        {
            try
            {
                txtQtyAct.SelectionLength = 0;
                txtQtyAct.SelectionStart = txtQtyAct.Text.Length;
            }
            catch
            {
            }
        }

        private void txtSTT_Enter(object sender, EventArgs e)
        {
            try
            {
                txtSTT.SelectionLength = 0;
                txtSTT.SelectionStart = txtSTT.Text.Length;
            }
            catch
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            try
            {
                if (File.Exists(@"" + ConfigurationManager.AppSettings["CSVFile"]))
                {
                    if (txtMin.Text != "" && txtMax.Text != "")
                    {
                        string pathCSV = @"" + ConfigurationManager.AppSettings["CSVFile"];
                        FileInfo CheckFile = new FileInfo(pathCSV);
                        if (LastCSV < CheckFile.LastWriteTime)
                        {
                            StreamReader sr = new StreamReader(pathCSV);
                            string strline = "";
                            while (!sr.EndOfStream)
                            {
                                strline = sr.ReadLine();
                                if (strline != "")
                                {
                                    if (strline.Substring(1, 3) != "MAX" && strline.Substring(1, 3) != "MIN")
                                    {
                                        //if (txtTenmay.Text == "JB-INS2")
                                        //{
                                        //    txtDo.Text = Math.Round(double.Parse(strline), 3, MidpointRounding.ToEven).ToString();
                                        //}
                                        //if (txtTenmay.Text == "JB-INS3")
                                        //{
                                            // máy đo chỉ xuất được số không có dấu chấm động: 1234 <=> 1.234, 10001 <=> 10.001 
                                            strline = strline.Substring(0, strline.Length - 3) + "." + strline.Substring(strline.Length - 3, 3);
                                            txtDo.Text = Math.Round(double.Parse(strline), 3, MidpointRounding.ToEven).ToString();
                                        //}
                                    }
                                    else
                                    {
                                        if (strline.Substring(1, 3) == "MAX")
                                        {
                                            txtDo.Text = txtMax.Text;
                                            txtReason.Text = "CHECK PIN MAX";
                                        }
                                        if (strline.Substring(1, 3) == "MIN")
                                        {
                                            txtDo.Text = txtMin.Text;
                                            txtReason.Text = "CHECK PIN MIN";
                                        }
                                    }

                                    txtDo.Focus();
                                    txtDo.SelectionLength = txtDo.Text.Length;
                                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                                    System.Windows.Forms.SendKeys.Send("{ENTER}");
                                }
                            }
                            sr.Close();

                            StreamWriter wr = new StreamWriter(pathCSV);
                            wr.WriteLine("");
                            wr.Close();

                            FileInfo CheckFile1 = new FileInfo(pathCSV);
                            LastCSV = CheckFile1.LastWriteTime;
                        }
                    }
                }
                else
                {
                    if (ConfigurationManager.AppSettings["PortCOM"].ToString() != "")
                    {
                        KiemtraConnect();
                    }
                    if (ConfigurationManager.AppSettings["PortCOM1"].ToString() != "")
                    {
                        KiemtraConnect1();
                    }
                    timer1.Stop();
                }

            }
            catch
            {
            }
        }

        private void txtShift_Leave(object sender, EventArgs e)
        {
            try
            {
                txtShift.Text = txtShift.Text.ToString().ToUpper();
            }
            catch
            {
            }
        }

        private void KiemtraConnect()
        {
            try
            {
                combcomport.Items.Clear();
           
                //combbaud.Items.Clear();
                ports = SerialPort.GetPortNames();
                combcomport.Items.AddRange(ports);
                P.ReadTimeout = 1000;
                P.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
                //string[] BaudRate = { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
                //combbaud.Items.AddRange(BaudRate);
                //combbaud.SelectedIndex = int.Parse(ConfigurationManager.AppSettings["Baund"].ToString());
                //  combbaud.Text = ConfigurationManager.AppSettings["Baund"].ToString();
                
                combcomport.SelectedIndex = int.Parse(ConfigurationManager.AppSettings["PortCOM"].ToString());
                combbaud.Items.Add(ConfigurationManager.AppSettings["Baund"].ToString());
                combbaud.SelectedIndex = 0;
          
                connect();
            }
            catch
            {
                lblConnectCom.Text = "COM connected Fail";
            }
        }

        private void KiemtraConnect1()
        {
            try
            {
                combcomport1.Items.Clear();
                //combbaud1.Items.Clear();
                ports1 = SerialPort.GetPortNames();
                combcomport1.Items.AddRange(ports1);
                P1.ReadTimeout = 1000;
                P1.DataReceived += new SerialDataReceivedEventHandler(DataReceive1);
                //string[] BaudRate = { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
                //combbaud.Items.AddRange(BaudRate);
                //combbaud.SelectedIndex = int.Parse(ConfigurationManager.AppSettings["Baund"].ToString());
              //  combbaud.Text = ConfigurationManager.AppSettings["Baund"].ToString();
                combcomport1.SelectedIndex = int.Parse(ConfigurationManager.AppSettings["PortCOM1"].ToString());
                combbaud1.Items.Add(ConfigurationManager.AppSettings["Baund1"].ToString());
                combbaud1.SelectedIndex = 0;
                
                connect1();
            }
            catch
            {
                lblConnectCom.Text = "COM connected Fail";
            }
        }

        private void Khoitaodothi_upper()
        {
            try
            {
                // khi khởi động sẽ được chạy
                GraphPane myPane = zedGraphControl1.GraphPane; // Khai báo sửa dụng Graph loại GraphPane;
                // Các thông tin cho đồ thị của mình
                myPane.Title.Text = "TOP 10 of Point Chart"; // + DateTime.Now.AddDays(-2).ToString("yyyy/MMM/dd") + " and " + DateTime.Now.ToString("yyyy/MMM/dd");
                myPane.XAxis.Title.Text = "Measure Count";
                myPane.YAxis.Title.Text = "Measure value";

                // Định nghĩa list để vẽ đồ thị.
                RollingPointPairList list1 = new RollingPointPairList(200000);
                RollingPointPairList list2 = new RollingPointPairList(200000);
                RollingPointPairList list3 = new RollingPointPairList(200000);
                RollingPointPairList list4 = new RollingPointPairList(200000);
                RollingPointPairList list5 = new RollingPointPairList(200000);
                RollingPointPairList list6 = new RollingPointPairList(200000);
                RollingPointPairList list7 = new RollingPointPairList(200000);
                // dòng dưới là định nghĩa curve để vẽ.
                LineItem curve1 = myPane.AddCurve("UCL", list1, Color.Blue, SymbolType.None); // Color màu đỏ, đặc trưng cho đường 1            
                LineItem curve2 = myPane.AddCurve("LCL", list2, Color.SpringGreen, SymbolType.None); // Color màu đỏ, đặc trưng cho đường 3
                LineItem curve3 = myPane.AddCurve("CL", list3, Color.Black, SymbolType.None); // Color màu đỏ, đặc trưng cho đường 1   
                LineItem curve4 = myPane.AddCurve("W", list4, Color.Red, SymbolType.Diamond); //  Color màu Xanh, đặc trưng cho đường 2
                //   LineItem curve5 = myPane.AddCurve("L", list5, Color.Lime, SymbolType.Star); //  Color màu Xanh, đặc trưng cho đường 2
                //   LineItem curve6 = myPane.AddCurve("L", list6, Color.Cyan, SymbolType.Star); //  Color màu Xanh, đặc trưng cho đường 2
                //   LineItem curve7 = myPane.AddCurve("L2", list7, Color.Fuchsia, SymbolType.Star); //  Color màu Xanh, đặc trưng cho đường 2
                // Định hiện thị cho trục thời gian (Trục X)
                myPane.XAxis.Scale.Min = 0;  // Min  = 0;
                myPane.XAxis.Scale.Max = 5; // Mã  = 30;
                myPane.XAxis.Scale.MinorStep = 1;  // Đơn vị chia nhỏ nhất 1
                myPane.XAxis.Scale.MajorStep = 1;// Đơn vị chia lớn 5

                myPane.YAxis.Scale.Min = 0;  // Min  = 0;
                myPane.YAxis.Scale.Max = 5; // Mã  = 30;
                myPane.YAxis.Scale.MinorStep = 0;  // Đơn vị chia nhỏ nhất 1
                myPane.YAxis.Scale.MajorStep = 0.05;// Đơn vị chia lớn 5

                // Gọi hàm xác định cỡ trục
                zedGraphControl1.AxisChange();
            }
            catch
            {
            }
        }

        public void draw_upper(double setpoint1, double setpoint2, double setpoint3, double setpoint4, double setpoint5, double setpoint6, double setpoint7,int Process) // Ở ví dụ này chúng ta có 2 đường
        {
            try
            {
                int time = 0;
                diem = 0;
                GraphPane myPane = zedGraphControl1.GraphPane;
                if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                    return;
                // Kiểm tra việc khởi tạo các đường curve
                // Đưa về điểm xuất phát
                LineItem curve1 = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
                LineItem curve2 = zedGraphControl1.GraphPane.CurveList[1] as LineItem;
                LineItem curve3 = zedGraphControl1.GraphPane.CurveList[2] as LineItem;
                LineItem curve4 = zedGraphControl1.GraphPane.CurveList[3] as LineItem;
                //LineItem curve5 = zedGraphControl1.GraphPane.CurveList[4] as LineItem;
                //LineItem curve6 = zedGraphControl1.GraphPane.CurveList[5] as LineItem;
                //LineItem curve7 = zedGraphControl1.GraphPane.CurveList[6] as LineItem;
                if (curve1 == null)
                    return;
                if (curve2 == null)
                    return;
                if (curve3 == null)
                    return;
                if (curve4 == null)
                    return;
                //if (curve5 == null)
                //    return;
                //if (curve6 == null)
                //    return;
                //if (curve7 == null)
                //    return;

                // list chứa các điểm. 
                // Get the PointPairList
                IPointListEdit list1 = curve1.Points as IPointListEdit;
                IPointListEdit list2 = curve2.Points as IPointListEdit;
                IPointListEdit list3 = curve3.Points as IPointListEdit;
                IPointListEdit list4 = curve4.Points as IPointListEdit;
                //      IPointListEdit list5 = curve5.Points as IPointListEdit;
                //       IPointListEdit list6 = curve6.Points as IPointListEdit;
                //       IPointListEdit list7 = curve7.Points as IPointListEdit;

                if (list1 == null)
                    return;
                if (list2 == null)
                    return;
                if (list3 == null)
                    return;
                if (list4 == null)
                    return;
                //if (list5 == null)
                //    return;
                //if (list6 == null)
                //    return;
                //if (list7 == null)
                //    return;
                for (int j = 0; j < grdChartView.RowCount - 1; j++)
                {
                    time = time + 1;
                    diem = diem + 1;
                    list1.Add(time, max_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                    list2.Add(time, min_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                    list3.Add(time, tb_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                    list4.Add(diem, float.Parse(grdChartView.Rows[time].Cells[Process].Value.ToString()));

                    //list5.Add(time, float.Parse(dgvdataup.Rows[time].Cells[2].Value.ToString()));
                    //list6.Add(time, float.Parse(dgvdataup.Rows[time].Cells[3].Value.ToString()));
                    //list7.Add(time, float.Parse(dgvdataup.Rows[time].Cells[4].Value.ToString()));
                }
                for (int l = 0; l < 2; l++)
                {
                    time = time + 1;
                    list1.Add(time, max_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                    list2.Add(time, min_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                    list3.Add(time, tb_up); // Đây chính là hàm hiển thị dữ liệu của mình lên đồ thị
                }
                //curve4.Line.IsVisible = false;
                // đoạn chương trình thực hiện vẽ đồ thị
                //Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
                //if (time > xScale.Max - xScale.MajorStep)
                //{
                //    xScale.Max = time + xScale.MajorStep;
                //    xScale.Min = 0;
                //}
                //zedGraphControl1.Size.Width=
                // Vẽ đồ thị
                //     myPane.CurveList.
                //   myPane.BarSettings.CalcClusterScaleWidth = 3.2;
                //  myPane.ScaledPenWidth(20, 30);
                //  zedGraphControl1.Scal
                // zedGraphControl1.s

                zedGraphControl1.AxisChange();
                // Force a redraw
                zedGraphControl1.Invalidate();
                zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
            }
            catch
            {
            }
        }

        private void DrawChart(int ProcessMain)
        {
            try
            {
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                Khoitaodothi_upper();
                draw_upper(1, 1, 1, 1, 1, 1, 1, ProcessMain);
            }
            catch
            {
            }
        }

        private void CheckReceive()
        {
            string sql;
            if (PlaingProcess == true)
            {
                sql = "Select isnull(Count(PO),0) as QtyReceive from " + TableSave + " Where PO='" + txtPO.Text + "' and Finish='OK' and MachineID='" + txtTenmay.Text + "' and Notice=1";
            }
            else
            {
                sql = "Select isnull(Count(PO),0) as QtyReceive from " + TableSave + " Where PO='" + txtPO.Text + "' and Finish='OK' and MachineID='" + txtTenmay.Text + "' and (Notice=0 or Notice is null)";
            }
            switch (txtType.Text)
            {
                case "NO":
                    sql = sql + " and (Status='NO' or Status is null)";
                    break;
                case "RE":
                    sql = sql + " and Status='RE'";
                    break;
                case "AD":
                    sql = sql + " and Status='AD'";
                    break;
            }
            SqlCommand sqlcmd2 = new SqlCommand(sql, connection);
            SqlDataReader reader2 = sqlcmd2.ExecuteReader();
            if (reader2.HasRows == true)
            {
                while (reader2.Read())
                {
                    lblQtyReceive.Text = reader2["QtyReceive"].ToString();
                }
            }
            reader2.Close();
            reader2.Dispose();
        }

        private void txtPO_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void groupControl2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void CheckBeforeSave()
        {
            txtDo.Text = "";
            if (txtCheckQty.Text == "1")
            {
                DataInput();
                lblQtyReceive.Text = Convert.ToString(int.Parse(lblQtyReceive.Text.ToString()) + 1);
                if (int.Parse(lblQtyReceive.Text) == int.Parse(txtQtyAct.Text))
                {
                    FinishPO(0);
                    lblQtyReceive.Text = "0";
                }
                else
                {
                    cmdOK.PerformClick();
                }
               
            }
            else
            {
                txtReason.Focus();
            }
        }

        private void cmdFinish_Click(object sender, EventArgs e)
        {
            
              FinishPO(0);
        }

        private void FinishPO(int StatusProcess) //0 : OK,1: NG,2 : RE
        {
            try
            {
                string sql = "";
                string ketqua = "";
                int dem = 0;
                string chuoifield = "";
                for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                {
                    chuoifield = chuoifield + ",[" + hh + "],[R" + hh + "]";
                }
                sql = "Insert into " + TableSave + "([EmpID],DateProcess,[MachineID],[ShiftID],[PORefer],[PO],ItemCD,[Status],[STTDo],[QtyAct]";
                sql = sql + chuoifield;
                sql = sql + ",[Finish],[Reason],Notice)";
                sql = sql + " VALUES ";
                sql = sql + "('" + txtManhanvien.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + txtTenmay.Text + "'";
                switch (txtType.Text)
                {
                    case "NO":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text,lblTenhang.Text.Length-6) +  "','NO',";
                        break;
                    case "AD":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-02','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','AD',";
                        break;
                    case "RE":
                        sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-RE','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','RE',";
                        break;
                }
                if (txtCheckQty.Text == "1")
                {
                    sql = sql + "'" + lblQtyReceive.Text + "',";
                }
                else
                {
                    sql = sql + "'" + txtSTT.Text + "',";
                }
                sql = sql + "'" + txtQtyAct.Text + "'";
                for (int a = 0; a < int.Parse(txtDemsocongdoan.Text.ToString()); a++)
                {
                    ketqua = ketqua + ",'" + grdProcess.Rows[a].Cells[11].Value + "','" + grdProcess.Rows[a].Cells[10].Value + "'";
                    if (txtReason.Text.ToString().Length > 0)
                    {
                    }
                    else
                    {
                        if (grdProcess.Rows[a].Cells[10].Value.ToString() == "NG")
                        {
                            lblThongbao.Text = "Đo " + grdProcess.Rows[a].Cells[3].Value + " bị NG. Nếu xác nhận OK thì vui lòng nhập lý do";
                            return;
                        }
                    }
                    if (grdProcess.Rows[a].Cells[10].Value.ToString().Length == 0)
                    {
                        dem = dem + 1;
                    }
                    if (dem == int.Parse(txtDemsocongdoan.Text.ToString()))
                    {
                        lblThongbao.Text = "Chưa nhập dữ liệu để đo";
                        return;
                    }
                }
                switch(StatusProcess)
                {
                    case 0:
                        sql = sql + ketqua + ",'OK','" + txtReason.Text + "'";
                        break;
                    case 1:
                        sql = sql + ketqua + ",'NG','" + txtReason.Text + "'";
                        break;
                    case 2:
                        sql = sql + ketqua + ",'RE','" + txtReason.Text + "'";
                        break;
                }
                if (PlaingProcess == true)
                {
                    sql = sql + ",'1')";// BF
                }
                else
                {
                    sql = sql + ",'0')"; //AF
                }
                cmd = new SqlCommand(sql, connection);
               cmd.ExecuteNonQuery();



                string sql1 = "Select OrderNo from Inspection_MachineINS Where OrderNo='" + txtPO.Text + "'";
                SqlCommand sqlcmd1 = new SqlCommand(sql1, connection);
                SqlDataReader reader1 = sqlcmd1.ExecuteReader();
                if (reader1.HasRows == false)
                {
                    sql = "Insert into Inspection_MachineINS (MachineINS, OrderNo, Inputdate) VALUES ('" + Dns.GetHostName() + "','" + txtPO.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "')";
                    cmd = new SqlCommand(sql, connection);
                    cmd.ExecuteNonQuery();
                }

               
                Loaddulieuluoi();
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                // dùng để gộp Ins1 và Ins2 trên cùng một tablet. LineMore =1: ins1 và ins2 chung 1 tablet
                //  1. sau khi test ins1 finish tự động chuyển sang ins2 và giữ lại một số thông tin
                //  2. sau khi test ins2 finish tự động chuyển sang ins1 và xóa hết thông tin.
                if (ConfigurationManager.AppSettings["LineMore"] == "1" && txtTenmay.Text != "JB-INS3")
                {
                    if (txtTenmay.Text == "JB-INS1")
                    {
                        txtTenmay.Text = "JB-INS2";
                        XoadulieutextboxFinish_Linemore();
                        LoadPO();
                    }
                    else
                    {
                        txtTenmay.Text = "JB-INS1";
                        XoadulieutextboxFinish();
                    }
                }
                else
                {

                    XoadulieutextboxFinish();
                }
            }
            catch
            {
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (ConfigurationManager.AppSettings["PortCOM"].ToString() != "")
            {
                send_data();
            }
            if (ConfigurationManager.AppSettings["PortCOM1"].ToString() != "")
            {
                send_data2();
            }
        }

        private void txtType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (txtType.Text.ToString().ToUpper() != "NO" && txtType.Text.ToString().ToUpper() != "RE" && txtType.Text.ToString().ToUpper() != "AD")
                {
                    txtType.Text = "";
                    txtType.Focus();
                }
                else
                {
                    txtType.Text = txtType.Text.ToUpper();
                    txtAfterPlating.Focus();
                }
            }
        }

        private void txtAfterPlating_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (txtAfterPlating.Text.ToString().ToUpper() != "AF" && txtAfterPlating.Text.ToString().ToUpper() != "BF")
                {
                    txtAfterPlating.Text = "";
                    txtAfterPlating.Focus();
                }
                else
                {
                    txtAfterPlating.Text = txtAfterPlating.Text.ToUpper();
                    txtPO.Focus();
                }
            }
        }

        private void cmdRepair_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtReason.Text.ToString().Length > 0)
                {
                    string sql = "";
                    string ketqua = "";
                    int dem = 0;
                    string chuoifield = "";
                    for (int hh = 1; hh <= int.Parse(txtDemsocongdoan.Text.ToString()); hh++)
                    {
                        chuoifield = chuoifield + ",[" + hh + "],[R" + hh + "]";
                    }
                    sql = "Insert into " + TableSave + "([EmpID],DateProcess,[MachineID],[ShiftID],[PORefer],[PO],ItemCD,[Status],[STTDo],[QtyAct]";
                    sql = sql + chuoifield;
                    sql = sql + ",[Finish],[Reason],Notice)";
                    sql = sql + " VALUES ";
                    sql = sql + "('" + txtManhanvien.Text + "','" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + txtTenmay.Text + "'";
                    switch (txtType.Text)
                    {
                        case "NO":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','NO',";
                            break;
                        case "AD":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-02','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','AD',";
                            break;
                        case "RE":
                            sql = sql + ",'" + txtShift.Text + "','" + txtPO.Text + "-RE','" + txtPO.Text + "','" + clsPublic.Right(lblTenhang.Text, lblTenhang.Text.Length - 6) + "','RE',";
                            break;
                    }
                    if (txtCheckQty.Text == "1")
                    {
                        sql = sql + "'" + lblQtyReceive.Text + "',";
                    }
                    else
                    {
                        sql = sql + "'" + txtSTT.Text + "',";
                    }
                    sql = sql + "'" + txtQtyAct.Text + "'";
                    for (int a = 0; a < int.Parse(txtDemsocongdoan.Text.ToString()); a++)
                    {
                        ketqua = ketqua + ",'" + grdProcess.Rows[a].Cells[11].Value + "','" + grdProcess.Rows[a].Cells[10].Value + "'";

                        if (grdProcess.Rows[a].Cells[10].Value.ToString().Length == 0)
                        {
                            dem = dem + 1;
                        }
                        if (dem == int.Parse(txtDemsocongdoan.Text.ToString()))
                        {
                            lblThongbao.Text = "Chưa nhập dữ liệu để đo";
                            return;
                        }
                    }
                    sql = sql + ketqua + ",'RE','" + txtReason.Text + "'";
                    if (PlaingProcess == true)
                    {
                        sql = sql + ",'1')";
                    }
                    else
                    {
                        sql = sql + ",'0')";
                    }
                    cmd = new SqlCommand(sql, connection);
                 cmd.ExecuteNonQuery();
                }
                else
                {
                    lblThongbao.Text = "Vui lòng nhập lý do bị NG";
                    return;
                }
                Loaddulieuluoi();
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                XoadulieutextboxOKNG();
            }
            catch
            {
            }
        }

        private void cmdAlarm_Click(object sender, EventArgs e)
        {
            if (grdProcess.Rows[i].Cells[1].Value.ToString() == "DAG2000")
            {
                send_data1();
            }
            else if(grdProcess.Rows[i].Cells[1].Value.ToString()=="SJ210")
            {
                GetdataSJ210();
            }
            txtDo.Focus();
        }

        private void GetdataSJ210()
        {
            if (ConfigurationManager.AppSettings["ActiveSJ210"].ToString() == "1" && grdProcess.Rows[i].Cells[1].Value.ToString() == "SJ210")
            {
                timer3.Enabled = true;
                timer3.Interval = int.Parse(ConfigurationManager.AppSettings["TimerIntervalSet2"].ToString());

                //Check file moi nhat la ngay nao
                if (File.Exists(ConfigurationManager.AppSettings["SavePathSJ210"].ToString()))
                {
                    datefile = File.GetLastWriteTime(ConfigurationManager.AppSettings["SavePathSJ210"].ToString());
                }

                if (File.Exists(ConfigurationManager.AppSettings["AppPathSJ210"].ToString()))
                {
                    foreach (var process in Process.GetProcessesByName("SJ-Communication-Tool"))
                    {
                        process.Kill();
                    }

                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = ConfigurationManager.AppSettings["AppPathSJ210"].ToString();
                    p.Start();

                    // To avoid deadlocks, always read the output stream first and then wait.
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    //Process p = Process.Start(ConfigurationManager.AppSettings["AppPathSJ210"].ToString());
                    //p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    //Thread.Sleep(1000);
                    //p.WaitForInputIdle();
                    //SetParent(p.MainWindowHandle, this.Handle);
                    //Thread.Sleep(2500);
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{ENTER}");
                    //Thread.Sleep(25000);
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{TAB}");
                    //System.Windows.Forms.SendKeys.Send("{ENTER}");
                    //Thread.Sleep(1000);
                    ////System.Windows.Forms.SendKeys.Send("{TAB 23}");
                }

                //System.Windows.Forms.SendKeys.Send("{TAB}");
                //System.Windows.Forms.SendKeys.Send("{ENTER}");
                //p.WaitForExit(1000);
                //System.Windows.Forms.SendKeys.Send(txtFileName.Text);
                //System.Windows.Forms.SendKeys.Send("{ENTER}");
                //p.WaitForExit(3000);
                //System.Windows.Forms.SendKeys.Send("{ENTER}");
            }

        }

        private string[,] LoadCsv(string filename)
        {
            // Get the file's text.
            string whole_file = File.ReadAllText(filename);

            // Split into lines.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            // Return the values.
            return values;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            DateTime datetimecompare;
            if (File.Exists(ConfigurationManager.AppSettings["SavePathSJ210"].ToString()))
            {
                datetimecompare = File.GetLastWriteTime(ConfigurationManager.AppSettings["SavePathSJ210"].ToString());
                if (datefile < datetimecompare)
                {
                    datefile = File.GetLastWriteTime(ConfigurationManager.AppSettings["SavePathSJ210"].ToString());
                    //timer3.Enabled=false;

                    string[,] values = LoadCsv(ConfigurationManager.AppSettings["SavePathSJ210"].ToString());
                    txtDo.Text = values[0, 0].ToString().ToUpper().Replace("RA","").Replace("UM","").Trim();
                    System.Windows.Forms.SendKeys.Send("{ENTER}");
                }
            }
            //else
            //{
            timer3.Enabled = false;
            //}
        }
    }
}