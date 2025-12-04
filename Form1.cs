using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Focus {
    public partial class Form1 : Form {
        public static bool userWorking = false;
        public Form1() {
            InitializeComponent();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        const int PBM_SETSTATE = 0x0410;
        const int PBST_NORMAL = 1; 
        const int PBST_ERROR = 2;  
        const int PBST_PAUSED = 3; 

        private void button1_Click(object sender, EventArgs e) {
            if (userWorking) {
                textBox1.Text = "Not Working";
                userWorking = false;
                progressBar1.Value = 100;
                SendMessage(progressBar1.Handle, PBM_SETSTATE, PBST_PAUSED, 0);
             
            } else {
                textBox1.Text = "Working";
                userWorking = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            
        }

        private void progressBar1_Click(object sender, EventArgs e) {

        }
    }
}
