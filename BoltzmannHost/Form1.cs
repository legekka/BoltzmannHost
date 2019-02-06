using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoltzmannHost
{
    public partial class Form1 : Form
    {
        private static wsServer wsServer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            wsServer = new wsServer(Convert.ToInt32(textBox1.Text));
            wsServer.StartServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            wsServer.StopServer();
        }

        
        public async void ListBoxClear()
        {
            BeginInvoke(new Action(() =>
            {
                listBox1.Items.Clear();
            }));
        }
        public async void AddListBoxElement(string name)
        {
            BeginInvoke(new Action(() =>
            {
                listBox1.Items.Add(name);
            }));
        }
        public async void SetInfoText(string text)
        {
            BeginInvoke(new Action(() =>
            {
                textBox2.Text = text;
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Open Blender File",
                DefaultExt = "blend",
                Filter = "blend files (*.blend)|*.blend",
                FilterIndex = 2,
                CheckFileExists = true,
                CheckPathExists = true,
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string filename = textBox3.Text;
            int sample = Convert.ToInt32(textBox4.Text);
            wsServer.SendJobs(filename, sample);
        }
    }
}
