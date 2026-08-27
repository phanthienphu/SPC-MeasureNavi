using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
namespace JB_Packing
{
    public partial class Main : Form
    {
        public int sendEmp = 0;
        public int sendPO = 0;
        public int FcuPO = 0;
        public int TB = 0;
        public int fs = 0;
        public string PO = "";
        SqlConnection connection;
        SqlCommand cmd;
        SqlCommand cmd1;
        string connectstring = "";
        int SumIns0=0;
        int SumIns1=0;
        int SumIns2=0;
        int SumIns3=0;
        int SumIns4 = 0;
        int SumIns7 = 0;
        int SumIns8 = 0;
        int SumIns9 = 0;
        int SumIns10 = 0;
        int SumIns11 = 0;
        int SumIns12 = 0;
        float  Osaka = 0;
        public Main()
        {
            setIE();
            InitializeComponent();

            connectstring = "Server=" + ConfigurationManager.AppSettings["ServerName"] + ";Initial Catalog=" + ConfigurationManager.AppSettings["Database"] + ";User ID=admin;Password=cuapheta;MultipleActiveResultSets=True;";
            connection = new SqlConnection(connectstring);
         //   connection.Open();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();
            dataGridView1.Rows.Add();

        }

        private void setIE()
        {
            try
            {
                //var appName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                //var fileName = appName.Substring(0, appName.IndexOf('.')) + ".exe";
                //RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION\\", true);
                //key.SetValue(fileName, (UInt32)11000, RegistryValueKind.DWord);
                int BrowserVer, RegVal;

                // get the installed IE version
                using (WebBrowser Wb = new WebBrowser())
                    BrowserVer = Wb.Version.Major;

                // set the appropriate IE version
                if (BrowserVer >= 11)
                    RegVal = 11001;
                else if (BrowserVer == 10)
                    RegVal = 10001;
                else if (BrowserVer == 9)
                    RegVal = 9999;
                else if (BrowserVer == 8)
                    RegVal = 8888;
                else
                    RegVal = 7000;

                // set the actual key
                using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    if (Key.GetValue(Process.GetCurrentProcess().ProcessName + ".exe") == null
                            || Key.GetValue(Process.GetCurrentProcess().ProcessName + ".exe").ToString() != RegVal.ToString())
                        Key.SetValue(Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
            }
            catch
            {

            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://10.4.24.113:8300/Login.aspx");
           // Thread.Sleep(2000);
            
            TB = 0;
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.Body.Style = "zoom:"+ ConfigurationManager.AppSettings["zoom"] + ";";
            timer1.Enabled = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ((Control)webBrowser1).Enabled = true;
                webBrowser1.Document.Body.Focus();
                // webBrowser1.Document.GetElementById("txtOperatorID").Focus();txtTerminalID
                sendEmp = 1;
                FcuPO = 0;
                //textBox2.Focus();
            }
            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (webBrowser1.Url.AbsoluteUri.Substring(0, 34) == "http://10.4.24.113:8300/Login.aspx" && TB ==0)
                {
                    Thread.Sleep(100);
                    webBrowser1.Document.GetElementById("txtTerminalID").Focus();
               //     Thread.Sleep(500);
                   SendKeys.Send(ConfigurationManager.AppSettings["TB"] + "\r");//ConfigurationManager.AppSettings["TB"]
                    TB = 1;
                    textBox1.Enabled = false ;
                    textBox2.Enabled = false ;
                }
                if (webBrowser1.Url.AbsoluteUri.Substring(0, 34) == "http://10.4.24.113:8300/sp/Standby")    //http://172.27.0.213:8300/sp/Standby.aspx?TK=b44da23dc76bd7176f55edae1ca87d86            
                {
                    if (fs == 0)
                    {
                        fs = 1;
                        textBox1.Enabled = true;
                        textBox2.Enabled = true;
                        Thread.Sleep(1000);
                    }
                    ((Control)webBrowser1).Enabled = true;
                   
                    var a = webBrowser1.Document.GetElementById("txtOperatorID").GetAttribute("value");
                    if ((a.ToString() != textBox1.Text || a.ToString().Length == 0) && sendEmp == 1 && textBox1 .Text != "")
                    {
                        sendEmp = 0;
                        SendKeys.Send(textBox1.Text + "\r\n");
                        //textBox2.Focus();
                    }
                    else
                    {
                        if (sendPO == 0)
                            ((Control)webBrowser1).Enabled = false;
                    }
                    if (a.ToString() == textBox1.Text && a.ToString().Length > 0 && FcuPO == 0)
                    {
                        textBox2.Focus();
                        FcuPO = 1;
                    }
                    if (sendPO == 1)
                    {
                        sendPO = 0;
                        SendKeys.Send(textBox2.Text + "\r\n");
                        
                        textBox1.Text = "";
                        textBox2.Text = "";
                        //  textBox2.SelectAll();
                        //  SendKeys.Send("1002593480\r\n");

                    }
                    //  var po = webBrowser1.Document.GetElementById("txtPO").GetAttribute("value");

                }
                else
                {
                    ((Control)webBrowser1).Enabled = true;
                    textBox2.Text = "";
                    textBox1.Text = "";
                }
            }
            catch
            {

            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (LoadDataInput(textBox2.Text)==1)
                {
                    try 
                    {
                        ((Control)webBrowser1).Enabled = true;
                        webBrowser1.Document.Body.Focus();
                        sendPO = 1;
                    }
                    catch
                    { }
                }
                else
                {
                    textBox2.SelectAll();
                }
                
            }
        }
        private void RESET()
        {
            if (dataGridView1.Rows.Count>1)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        dataGridView1.Rows[i].Cells[j].Value = "";
                    }
                }
            }
            SumIns0 = 0;
            SumIns1 = 0;
            SumIns2 = 0;
            SumIns3 = 0;
            SumIns4 = 0;
            SumIns7 = 0;
            SumIns8 = 0;
            SumIns9 = 0;
            SumIns10 = 0;
            SumIns11 = 0;
            SumIns12 = 0;
            Osaka = 0;

        }
        private int CheckDataTE(string PO)
        {
            int kq = 0;
            DataTable dataTable = new DataTable();
            try
            {
                if (ConfigurationManager.AppSettings["CheckDataTE"] == "1")
                {
                    OleDbConnection con = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ConfigurationManager.AppSettings["DataTE"] + ";Jet OLEDB:Database Password=admin123;");
                    con.Open();
                    string sql = "select EmpID from CategoryCheckPacking where PO = '" + PO + "'";
                    ///cmd.Connection = con;
                    OleDbDataAdapter dAdapter = new OleDbDataAdapter(sql, con);
                    OleDbCommandBuilder cBuilder = new OleDbCommandBuilder(dAdapter);
                    DataSet ds = new DataSet();

                    dAdapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        kq = 1;
                    }
                    con.Close();
                }
                return kq;
            }
            catch
            {
                return kq;
                MessageBox.Show("Pls! Can not connect to TE-Database", "Warning!!!");
            }
        }
        private int LoadDataInput(string PO)
        {
            


                int row = 0, column = 0;
                int kq = 0;
                int fs = 0;
            int dataTE = 0;
                string INS = "";
            string TypePro = "";
                RESET();
            lbdatate.Text = "";
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Open();
            }
            catch
            {
                MessageBox.Show(" Kết Nối Mạng lỗi! Vui lòng check lại Wifi");
            }
              ////////////////////////////////////////////////////
              if(CheckDataTE(PO) ==1)
            {
                string sql = "Insert into Packing_JB (PO, StatusPO) VALUES ('" + PO + "','TE')";
                cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                //  connection.Close();
                lbdatate.Text = "TE Data";
                dataTE = 1;
            }
            ////////////////////////////////////////////////////  

                string query = "Select POREQNO, QTY, sum(Osaka) AS Osaka,ITEMCD from QRY_MANUFA_JB where POREQNO = '" + PO + "' group by POREQNO, QTY,ITEMCD";
                SqlDataAdapter adap = new SqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adap.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    lbPO.Text = PO;
                    lbQty.Text = dt.Rows[0]["QTY"].ToString();
                    TypePro = dt.Rows[0]["ITEMCD"].ToString();
                if (dt.Rows[0]["Osaka"].ToString() != "")
                    {
                        Osaka = float.Parse(dt.Rows[0]["Osaka"].ToString());
                    }
                    else Osaka = 0;
                    lbOsaka.Text = Osaka.ToString();
                if (((ConfigurationManager.AppSettings["Ma_N"].Contains(TypePro.Substring(0, 3)) == true && TypePro.Contains(ConfigurationManager.AppSettings["key1"]) == true) || (ConfigurationManager.AppSettings["Ma_M"].Contains(TypePro.Substring(0, 3)) == true && TypePro.Contains(ConfigurationManager.AppSettings["key2"]) == true)))
                {
                    query = "SELECT  [MachineID],[Status],[PO],ItemCD, count(PO) as SumINS"
                         + " FROM [InspectionMeasure].[dbo].[V_DataINS_JB]"
                         + " where po like '" + PO + "'  group by [MachineID],[Status],[PO],ItemCD";
                } else
                {
                    query = "SELECT  [MachineID],[Status],[PO],ItemCD, count(PO) as SumINS"
                          + " FROM [InspectionMeasure].[dbo].[V_DataINS_JB]"
                          + " where po like '" + PO + "' and Notice = '0' group by [MachineID],[Status],[PO],ItemCD";
                }
                    SqlDataAdapter adap1 = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adap1.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            
                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS0")
                            {
                                if (fs == 0)
                                {
                                    // dataGridView1.Rows.Add();
                                    dataGridView1.Rows[0].Cells[0].Value = "JB-INS0";
                                    dataGridView1.Rows[0].Cells[1].Value = "0";
                                    dataGridView1.Rows[0].Cells[2].Value = "0";
                                    dataGridView1.Rows[0].Cells[3].Value = "0";
                                    fs = 1;
                                    INS = "0";
                                }
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {
                                    SumIns0 = SumIns0 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {

                                        dataGridView1.Rows[0].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[0].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[0].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }

                                }

                            }

                            else
                            {
                                if (fs == 0)
                                {
                                    //  dataGridView1.Rows.Add();
                                    dataGridView1.Rows[0].Cells[0].Value = "JB-INS1";
                                    dataGridView1.Rows[0].Cells[1].Value = "0";
                                    dataGridView1.Rows[0].Cells[2].Value = "0";
                                    dataGridView1.Rows[0].Cells[3].Value = "0";
                                    // dataGridView1.Rows.Add();
                                    dataGridView1.Rows[1].Cells[0].Value = "JB-INS2";
                                    dataGridView1.Rows[1].Cells[1].Value = "0";
                                    dataGridView1.Rows[1].Cells[2].Value = "0";
                                    dataGridView1.Rows[1].Cells[3].Value = "0";
                                    // dataGridView1.Rows.Add();
                                    dataGridView1.Rows[2].Cells[0].Value = "JB-INS3";
                                    dataGridView1.Rows[2].Cells[1].Value = "0";
                                    dataGridView1.Rows[2].Cells[2].Value = "0";
                                    dataGridView1.Rows[2].Cells[3].Value = "0";
                                //
                                    dataGridView1.Rows[3].Cells[0].Value = "JB-INS4";
                                    dataGridView1.Rows[3].Cells[1].Value = "0";
                                    dataGridView1.Rows[3].Cells[2].Value = "0";
                                    dataGridView1.Rows[3].Cells[3].Value = "0";
                                //
                                    dataGridView1.Rows[4].Cells[0].Value = "JB-INS7";
                                    dataGridView1.Rows[4].Cells[1].Value = "0";
                                    dataGridView1.Rows[4].Cells[2].Value = "0";
                                    dataGridView1.Rows[4].Cells[3].Value = "0";
                                //
                                    dataGridView1.Rows[5].Cells[0].Value = "JB-INS8";
                                    dataGridView1.Rows[5].Cells[1].Value = "0";
                                    dataGridView1.Rows[5].Cells[2].Value = "0";
                                    dataGridView1.Rows[5].Cells[3].Value = "0";
                                //
                                dataGridView1.Rows[6].Cells[0].Value = "JB-INS9";
                                dataGridView1.Rows[6].Cells[1].Value = "0";
                                dataGridView1.Rows[6].Cells[2].Value = "0";
                                dataGridView1.Rows[6].Cells[3].Value = "0";
                                //
                                dataGridView1.Rows[7].Cells[0].Value = "JB-INS10";
                                dataGridView1.Rows[7].Cells[1].Value = "0";
                                dataGridView1.Rows[7].Cells[2].Value = "0";
                                dataGridView1.Rows[7].Cells[3].Value = "0";
                                //
                                dataGridView1.Rows[8].Cells[0].Value = "JB-INS11";
                                dataGridView1.Rows[8].Cells[1].Value = "0";
                                dataGridView1.Rows[8].Cells[2].Value = "0";
                                dataGridView1.Rows[8].Cells[3].Value = "0";
                                //
                                dataGridView1.Rows[9].Cells[0].Value = "JB-INS12";
                                dataGridView1.Rows[9].Cells[1].Value = "0";
                                dataGridView1.Rows[9].Cells[2].Value = "0";
                                dataGridView1.Rows[9].Cells[3].Value = "0";

                                fs = 1;
                                    INS = "123";
                                }
                                if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS1")
                                {
                                    if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                    {

                                        SumIns1 = SumIns1 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                        if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                        {
                                            dataGridView1.Rows[0].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                        {
                                            dataGridView1.Rows[0].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else
                                        {
                                            dataGridView1.Rows[0].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                    }
                                }
                                if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS2")
                                {
                                    if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                    {

                                        SumIns2 = SumIns2 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                        if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                        {
                                            dataGridView1.Rows[1].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                        {
                                            dataGridView1.Rows[1].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else
                                        {
                                            dataGridView1.Rows[1].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                    }
                                }
                                if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS3")
                                {
                                    if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                    {

                                        SumIns3 = SumIns3 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                        if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                        {
                                            dataGridView1.Rows[2].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                        {
                                            dataGridView1.Rows[2].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                        else
                                        {
                                            dataGridView1.Rows[2].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                        }
                                    }
                                }
                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS4")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns4 = SumIns4 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[3].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[3].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[3].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }
                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS7")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns7 = SumIns7 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[4].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[4].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[4].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }

                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS8")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns8 = SumIns8 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[5].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[5].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[5].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }

                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS9")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns9 = SumIns9 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[6].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[6].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[6].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }
                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS10")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns10 = SumIns10 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[7].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[7].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[7].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }
                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS11")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns11 = SumIns11 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[8].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[8].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[8].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }

                            if (dataTable.Rows[i]["MachineID"].ToString() == "JB-INS12")
                            {
                                if (Int32.Parse(dataTable.Rows[i]["SumINS"].ToString()) > 0)
                                {

                                    SumIns12 = SumIns12 + Int32.Parse(dataTable.Rows[i]["SumINS"].ToString());
                                    if (dataTable.Rows[i]["Status"].ToString() == "NO")
                                    {
                                        dataGridView1.Rows[9].Cells[1].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else if (dataTable.Rows[i]["Status"].ToString() == "RE")
                                    {
                                        dataGridView1.Rows[9].Cells[2].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[9].Cells[3].Value = dataTable.Rows[i]["SumINS"].ToString();
                                    }
                                }
                            }
                        }


                        }

                        lbOKNG.Text = "OK";
                        lbOKNG.ForeColor = Color.Blue;
                        kq = 1;

                    if (SumIns0 + Osaka < Int32.Parse(lbQty.Text) && INS == "0")
                    {
                        lbOKNG.Text = "NG: SumIns0";
                        lbOKNG.ForeColor = Color.Red;
                        kq = 0;
                    }
                    else
                    {
                        if (SumIns8 == 0 && SumIns7 ==0 && SumIns9 == 0 && SumIns10 == 0 && SumIns11 == 0)
                        {
                            if (SumIns1 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns1";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                            if (SumIns2 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns2";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                            if (SumIns3 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns3";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                        }
                        else
                        {
                            if (SumIns8 != 0 )
                            {
                                if (SumIns8 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns8";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                                if (SumIns3 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns3";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                            }
                            else if (SumIns7 != 0)
                            {
                                if (SumIns7 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns7";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                                if (SumIns2 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns2";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                            }
                        }
                        if ( SumIns11 != 0)
                        {
                            if (SumIns9 != 0 || SumIns10 != 0)
                            {
                                if (ConfigurationManager.AppSettings["Type"].Contains(TypePro.Substring(0,3)) == true)// nhưng đơn tron master này thì chỉ do 2 con
                                {
                                    // số lượng >= qtycheck thì lấy số lượng đo là qtycheck, ngược lại lấy chính số lượng đơn hàng
                                    if (Int32.Parse(lbQty.Text) >= int.Parse(ConfigurationManager.AppSettings["QtyCheck"]))
                                    {
                                        if (SumIns9 + Osaka < int.Parse(ConfigurationManager.AppSettings["QtyCheck"]))
                                        {
                                            if (dataTE == 0)// không có dữ liệu TE thì check ins9 bình thường, nếu có thì pass quá
                                            {
                                                lbOKNG.Text = "NG: SumIns9";
                                                lbOKNG.ForeColor = Color.Red;
                                                kq = 0;
                                            }
                                        }
                                    }else
                                    {
                                        if (SumIns9 + Osaka < Int32.Parse(lbQty.Text))
                                        {
                                            if (dataTE == 0)// không có dữ liệu TE thì check ins9 bình thường, nếu có thì pass quá
                                            {
                                                lbOKNG.Text = "NG: SumIns9";
                                                lbOKNG.ForeColor = Color.Red;
                                                kq = 0;
                                            }
                                        }
                                    }
                                }
                                else if (SumIns9 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    if (dataTE == 0)// không có dữ liệu TE thì check ins9 bình thường, nếu có thì pass quá
                                    {
                                        lbOKNG.Text = "NG: SumIns9";
                                        lbOKNG.ForeColor = Color.Red;
                                        kq = 0;
                                    }
                                }
                                ////////////////////check ins10/////////////
                                if (SumIns10 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns10";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                                //////check ins12//////////////
                                if (SumIns12 + Osaka < Int32.Parse(lbQty.Text))
                                {
                                    lbOKNG.Text = "NG: SumIns12";
                                    lbOKNG.ForeColor = Color.Red;
                                    kq = 0;
                                }
                            }
                            if (SumIns11 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns11";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                            if (SumIns2 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns2";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                            if (SumIns3 + Osaka < Int32.Parse(lbQty.Text))
                            {
                                lbOKNG.Text = "NG: SumIns3";
                                lbOKNG.ForeColor = Color.Red;
                                kq = 0;
                            }
                        }
                        if (SumIns4 + Osaka < Int32.Parse(lbQty.Text) && (( ConfigurationManager.AppSettings["Ma_N"].Contains(TypePro.Substring(0, 3)) == true && TypePro.Contains (ConfigurationManager.AppSettings["key1"]) == true) || (ConfigurationManager.AppSettings["Ma_M"].Contains(TypePro.Substring(0, 3)) == true && TypePro.Contains(ConfigurationManager.AppSettings["key2"]) == true)))
                        {
                            lbOKNG.Text = "NG: SumIns4";
                            lbOKNG.ForeColor = Color.Red;
                            kq = 0;
                        }
                    }

                    //if(SumIns9 != 0 )
                    //{
                    //    if (SumIns9 + Osaka < Int32.Parse(lbQty.Text))
                    //    {
                    //        lbOKNG.Text = "NG: SumIns9";
                    //        lbOKNG.ForeColor = Color.Red;
                    //        kq = 0;
                    //    }
                        
                    //}

                    }
                }
            if (SumIns9 != 0 || SumIns10 != 0 || SumIns11 != 0)
            {
                SumIns9 = SumIns9 + SumIns12;
                string sql = "Insert into Packing_JB (PO, QtyPO, Osaka, InS0, InS1, Ins2, Ins3,Ins7, Ins8, StatusPO) VALUES ('" + lbPO.Text + "'," + Int32.Parse(lbQty.Text) + "," + Int32.Parse(lbOsaka.Text) + "," + SumIns9 + "," + SumIns10 + "," + SumIns11 + "," + SumIns2 + "," + SumIns3 + "," + SumIns8 + ",'" + lbOKNG.Text + "')";
                cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
           //     connection.Close();
            }
            else
            {
                string sql = "Insert into Packing_JB (PO, QtyPO, Osaka, InS0, InS1, Ins2, Ins3,Ins7, Ins8, StatusPO) VALUES ('" + lbPO.Text + "'," + Int32.Parse(lbQty.Text) + "," + Int32.Parse(lbOsaka.Text) + "," + SumIns0 + "," + SumIns1 + "," + SumIns2 + "," + SumIns3 + "," + SumIns7 + "," + SumIns8 + ",'" + lbOKNG.Text + "')";
                cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
             //   connection.Close();
            }
            if (SumIns4 != 0)
            {
                string sql = "Insert into Packing_JB (PO, QtyPO, Osaka, InS0, StatusPO) VALUES ('" + lbPO.Text + "'," + Int32.Parse(lbQty.Text) + "," + Int32.Parse(lbOsaka.Text) + "," + SumIns4 + ",'" + lbOKNG.Text + "')";
                cmd1 = new SqlCommand(sql, connection);
                cmd1.ExecuteNonQuery();
            }
            connection.Close();
            return kq;
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelControl9_Click(object sender, EventArgs e)
        {

        }

        //private void simpleButton1_Click(object sender, EventArgs e)
        //{
        //    webBrowser1.Document.GetElementById("txtTerminalID").Focus();
        //    Thread.Sleep(500);
        //    SendKeys.Send("TB4206" + "\r\n");
        //}

        //private void simpleButton2_Click(object sender, EventArgs e)
        //{
        //    webBrowser1.Document.Body.Focus();
        //    Thread.Sleep(500);
        //    SendKeys.Send("001002593480" + "\r\n");
        //}
    }
}
