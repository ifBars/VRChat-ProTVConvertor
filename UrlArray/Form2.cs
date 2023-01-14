using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UrlArray
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        string name;
        bool clicked = false;

        public string openName()
        {
            label2.Text = Form1.realVideoTitle;
            this.Show();
            while (!clicked)
            {
                Application.DoEvents();
            }
            if (name != "")
            {
                this.Hide();
                return name;
            }
            return "";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != "")
            {
                name = textBox1.Text;
                clicked = true;
            } else
            {
                name = Form1.realVideoTitle;
                clicked = true;
            }
        }
    }
}
