using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CURSE
{
    public partial class Help : Form
    { 
        public Help()
        {
            InitializeComponent();
        }
        public void OpenHelp(int value)
        {
            switch (value)
            {
                case 0:
                    tabControl1.SelectedTab = tabPage1;
                    break;
                case 1:
                    tabControl1.SelectedTab = tabPage2;
                    break;
                case 2:
                    tabControl1.SelectedTab = tabPage3;
                break;
                case 3:
                    tabControl1.SelectedTab = tabPage4;
                    break;
                case 4:
                    tabControl1.SelectedTab = tabPage5;
                    break;

                default:
                    tabControl1.SelectedTab = tabPage1;
                    break;
            }
        }
        private void Справка_Load(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
