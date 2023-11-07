using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YDC_Inspection
{
    
    public partial class FrmJob : Form
    {
        private CogToolBlock cogTB = null;
        public FrmJob(CogToolBlock cogTB1)
        {
            cogTB = cogTB1;
            InitializeComponent();
        }

        private void FrmJob_Load(object sender, EventArgs e)
        {
            cogToolBlockEditV21.Subject = cogTB;
        }

        private void FrmJob_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogToolBlockEditV21.Subject = null;
            
        }
    }
}
