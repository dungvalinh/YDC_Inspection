using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro;
using Basler.Pylon;
using Cognex.VisionPro.ToolBlock;
using McProtocol.Mitsubishi;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace YDC_Inspection
{
    public partial class Form1 : Form
    {

        McProtocolTcp mcPro = new McProtocolTcp("192.168.3.39", 3000, McFrame.MC4E);
        private CogToolBlock cogtoolblock = null;


        //ICogAcqFifo cogAcq = null;

        public Form1()
        {
            InitializeComponent();
            cogtoolblock = CogSerializer.LoadObjectFromFile(@"C:\Users\YDC\Desktop\job_file\Cam_CIC.vpp") as CogToolBlock;
            //cogtoolblock.Running += Cogtoolblock_Running;
            cogtoolblock.Changed += new CogChangedEventHandler(cogtoolblock_Changed);


        }

        private void cogtoolblock_Changed(object sender, CogChangedEventArgs e)
        {
            ICogRecord cogRecord = cogtoolblock.CreateLastRunRecord();
            if (cogRecord != null)
            {
                cogRecordDisplay1.Record = cogRecord.SubRecords["CogAcqFifoTool1.OutputImage"];
            }
        }

        private void Cogtoolblock_Running(object sender, EventArgs e)
        {
            //cogRecordDisplay1.Image = cogtoolblock.Outputs["Image_Output"].Value as ICogImage;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.BackColor == Color.LightGray)
            {
                //dmasasc
                mcPro.Open();
                timer2.Enabled = true;
                timer2.Interval = 10;
                timer1.Enabled = true;
                button2.BackColor = Color.Lime;
                lbConnect.Text = "Connected";
                int[] runData1 = new int[1];
                runData1[0] = 1;
                mcPro.WriteDeviceBlock("M602", 1, runData1);
                label4.Text = "Q03UDVCPU";
            }

            else if (button2.BackColor == Color.Lime)
            {
                int[] runData1 = new int[1];
                runData1[0] = 0;
                mcPro.WriteDeviceBlock("M602", 1, runData1);
                mcPro.Close();
                timer1.Enabled = false;
                button2.BackColor = Color.LightGray;
                lbConnect.Text = "Disconnected";
                label4.Text = "unknown";    
              
                timer2.Enabled = false;
               
            }
        }
        public static DirectoryInfo GetCreateMyFolder(string baseFolder)
        {
            var now = DateTime.Now;
            var yearName = now.ToString("yyyy");
            var monthName = now.ToString("MMMM");
            var dayName = now.ToString("dd");

            var folder = Path.Combine(baseFolder,
                           Path.Combine(yearName,
                             Path.Combine(monthName,
                               dayName)));

            return Directory.CreateDirectory(folder);
        }

        private void btnLive_Click(object sender, EventArgs e)
        {


        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmJob frm = new FrmJob(cogtoolblock);
            frm.Show();
        }
        double ReAll = 0; double ReOk = 0; double ReNG = 0; double ReVe;
        private void timer1_Tick(object sender, EventArgs e)
        {
            GetCreateMyFolder(@"F:\ImageLog\Camera_Cic\OK");
            GetCreateMyFolder(@"F:\ImageLog\Camera_Cic\NG");
            DateTime d = DateTime.Now;
            String imgOk = Path.Combine("F:\\ImageLog\\Camera_Cic\\OK", d.ToString("yyyy"), d.ToString("MMMM"), d.ToString("dd"));
            String imgNg = Path.Combine("F:\\ImageLog\\Camera_Cic\\NG", d.ToString("yyyy"), d.ToString("MMMM"), d.ToString("dd"));
            int[] oData1 = new int[1];
            mcPro.ReadDeviceBlock("D300", 1, oData1);
            int[] inData1 = new int[1];
            inData1[0] = 1;
            mcPro.WriteDeviceBlock("D301", 1, inData1);
            if (oData1[0] == 1)
            {
                lbTrigg.BackColor = Color.Lime;
                ReAll++;


                cogtoolblock.Run();

              
                var result = cogtoolblock.Outputs["Result"].Value;
                var reseult_1 = cogtoolblock.Outputs["Result_Fix"].Value;
                var result1 = cogtoolblock.Outputs["Image_Output"].Value as ICogImage;
                Bitmap re = result1.ToBitmap();
                int[] iData1 = new int[1];
                if (result != null && (int)reseult_1 == 1)
                {
                   
                    switch (result)
                    {
                        case CogToolResultConstants.Accept:
                            string filename = System.String.Format(imgOk + "\\{0}.bmp", lbCode.Text);
                            re.Save(filename, ImageFormat.Bmp);
                            re.Save(filename, ImageFormat.Bmp);
                            ReOk++;
                            lbResult.Text = "OK";
                            lbResult.BackColor = Color.Lime;
                            iData1[0] = 1;
                            mcPro.WriteDeviceBlock("D302", 1, iData1);
                            break;
                        case CogToolResultConstants.Error:
                        case CogToolResultConstants.Reject:
                            string filename1 = System.String.Format(imgNg + "\\{0}.bmp", lbCode.Text);
                            re.Save(filename1, ImageFormat.Bmp);
                            ReNG++;
                            lbResult.Text = "NG";
                            lbResult.BackColor = Color.Red;
                            iData1[0] = 1;
                            mcPro.WriteDeviceBlock("D303", 1, iData1);
                            break;
                        default:
                            label1.Text = "";
                            break;
                    }
                }
                else if ((int)reseult_1 == 0)
                {
                    string filename1 = System.String.Format(imgNg + "\\{0}.bmp", lbCode.Text);
                    re.Save(filename1, ImageFormat.Bmp);
                    ReNG++;
                    lbResult.Text = "NG";
                    lbResult.BackColor = Color.Red;
                    iData1[0] = 1;
                    mcPro.WriteDeviceBlock("D303", 1, iData1);
                }

            }
            else if (oData1[0] == 0)
            {
                lbTrigg.BackColor = Color.Silver;
            }

            {
                double ok1 = (ReOk / ReAll) * 100;
                double ng1 = (ReNG / ReAll) * 100;
                string ok = Math.Round(ok1, 2).ToString();
                string ng = Math.Round(ng1, 2).ToString();
                numAll.Text = ReAll.ToString();
                numOk.Text = ReOk.ToString() + "(" + ok + "%)";
                numNg.Text = ReNG.ToString() + "(" + ng + "%)";
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // CogSerializer.SaveObjectToFile(cogtoolblock, jobFile);
            DialogResult dgResult = MessageBox.Show("Do you want to exit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            int[] runData1 = new int[1];
            runData1[0] = 0;
            mcPro.WriteDeviceBlock("M602", 1, runData1);
            if (dgResult == DialogResult.OK)
            {
                Application.Exit();
            }
            else if (dgResult == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        int run = 0; int run1 = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            DateTime d = DateTime.Now;
            String imgOk = Path.Combine("F:\\ImageLog\\Camera_Cic\\OK" , d.ToString("yyyy"), d.ToString("MMMM"), d.ToString("dd"));
            String imgNg = Path.Combine("F:\\ImageLog\\Camera_Cic\\NG", d.ToString("yyyy"), d.ToString("MMMM"), d.ToString("dd"));
            lbTrigg.BackColor = Color.Lime;
            ReAll++;
            cogtoolblock.Run();
            var result = cogtoolblock.Outputs["Result"].Value;
            var reseult_1 = cogtoolblock.Outputs["Result_Fix"].Value;
            var result1 =cogtoolblock.Outputs["Image_Output"].Value as ICogImage;
            Bitmap re = result1.ToBitmap();
            int[] iData1 = new int[1];
            if (result != null&& (int)reseult_1==1)
            {
                
                switch (result)
                {
                    case CogToolResultConstants.Accept:
                        string filename = System.String.Format(imgOk + "\\{0}.bmp", lbCode.Text);
                        re.Save(filename, ImageFormat.Bmp);
                        ReOk++;
                        lbResult.Text = "OK";
                        lbResult.BackColor = Color.Lime;
                        iData1[0] = 1;
                        mcPro.WriteDeviceBlock("D302", 1, iData1);
                        break;
                    case CogToolResultConstants.Error:
                    case CogToolResultConstants.Reject:
                        button4.Visible = true;
                        button4.Enabled = true;
                        string filename1 = System.String.Format(imgNg + "\\{0}.bmp", lbCode.Text);
                        re.Save(filename1, ImageFormat.Bmp);
                        run1++;
                        //ReNG++;
                        //lbResult.Text = "NG";
                        //lbResult.BackColor = Color.Red;
                        //iData1[0] = 1;
                        //mcPro.WriteDeviceBlock("D303", 1, iData1);
                        break;
                    default:
                        label1.Text = "";
                        break;
                }
            }
            else if((int)reseult_1==0)
            {
                button4.Visible = true;
                button4.Enabled = true;
                string filename1 = System.String.Format(imgNg + "\\{0}.bmp", lbCode.Text);
                re.Save(filename1, ImageFormat.Bmp);
                run1++;
                //ReNG++;
                //lbResult.Text = "NG";
                //lbResult.BackColor = Color.Red;
                //iData1[0] = 1;
                //mcPro.WriteDeviceBlock("D303", 1, iData1);
            }
                double ok1 = (ReOk / ReAll) * 100;
                double ng1 = (ReNG / ReAll) * 100;
                string ok = Math.Round(ok1, 2).ToString();
                string ng = Math.Round(ng1, 2).ToString();
                numAll.Text = ReAll.ToString();
                numOk.Text = ReOk.ToString() + "(" + ok + "%)";
                numNg.Text = ReNG.ToString() + "(" + ng + "%)";
            var dicas = 0;

            
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            ReAll = 0; ReOk = 0; ReNG = 0; ReVe = 0;
            numVe.Text = "";
            numAll.Text = " ";
            numOk.Text = "";
            numNg.Text = "";
            lbResult.Text = "";
            lbResult.BackColor = Color.DarkGray;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            string c = " ";
            int data = 5047;
            for (int i = 0; i < 8; i++)
            {
                data += 1;
                int[] onData1 = new int[1];
                mcPro.ReadDeviceBlock("D" + data.ToString(), 1, onData1);
                char c1 = (char)(onData1[0] & 0xff);
                char c2 = (char)(onData1[0] >> 8);
                c += c1.ToString() + c2.ToString();

            }
            lbCode.Text = c;
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
            
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            
                //// Get file path of current process 
                //var filePath = Assembly.GetExecutingAssembly().Location;
                ////var filePath = Application.ExecutablePath;  // for WinForms

                //// Start program
                //Process.Start(filePath);

                //// For Windows Forms app
                //Application.Exit();

                //// For all Windows application but typically for Console app.
                ////Environment.Exit(0);
            
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
           
            int[] iData1 = new int[1];
            DialogResult dg = MessageBox.Show("Verify?", "Note", MessageBoxButtons.OKCancel);
            if(dg == DialogResult.OK)
            {
                ReVe++;
                lbResult.Text = "VERIFY";
                lbResult.BackColor = Color.DarkViolet;
                iData1[0] = 1;
                mcPro.WriteDeviceBlock("D302", 1, iData1);
                button4.Enabled = false;
                button4.Visible = false;
            }
            else if(dg == DialogResult.Cancel)
            {
                ReNG++;
                lbResult.Text = "NG";
                lbResult.BackColor = Color.Red;
                iData1[0] = 1;
                mcPro.WriteDeviceBlock("D303", 1, iData1);
                button4.Visible = false;
                button4.Enabled = false;
            }
            double ve1 = (ReVe / ReAll) * 100;
            string ve = Math.Round(ve1, 2).ToString();
            numVe.Text = ReVe.ToString() + "(" + ve + "%)";

            double ng1 = (ReNG / ReAll) * 100;
            string ng = Math.Round(ng1, 2).ToString();
            numNg.Text = ReNG.ToString() + "(" + ng + "%)";
        }
    }
}
