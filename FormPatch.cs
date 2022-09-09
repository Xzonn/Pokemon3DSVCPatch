using System;
using System.Windows.Forms;

namespace Pokemon3DSVCPatch
{
    public partial class FormPatch : Form
    {
        public FormPatch()
        {
            InitializeComponent();
            Label_AutoPosition(label1, null, 240, 60);
            label1.SizeChanged += (sender, e) => Label_AutoPosition(sender, e, 240, 60);
            Label_AutoPosition(label2, null, 240, 140);
            label2.SizeChanged += (sender, e) => Label_AutoPosition(sender, e, 240, 140);
            Label_AutoPosition(label3, null, 240, 220);
            label3.SizeChanged += (sender, e) => Label_AutoPosition(sender, e, 240, 220);
            Label_AutoPosition(label4, null, 240, 300);
            label4.SizeChanged += (sender, e) => Label_AutoPosition(sender, e, 240, 300);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "原始CIA",
                Filter = "Nintendo 3DS CIA文件|*.cia|所有文件|*.*"
            };
            ofd.ShowDialog();
            textBox1.Text = ofd.FileName;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "汉化ROM",
                Filter = "Game Boy Color ROM文件|*.gbc|所有文件|*.*"
            };
            ofd.ShowDialog();
            textBox2.Text = ofd.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "VC补丁",
                Filter = "补丁文件|*.patch|所有文件|*.*"
            };
            ofd.ShowDialog();
            textBox3.Text = ofd.FileName;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "输出CIA",
                Filter = "Nintendo 3DS CIA文件|*.cia"
            };
            sfd.ShowDialog();
            textBox4.Text = sfd.FileName;
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (CheckIfFileExists(filePath))
            {
                ((TextBox)sender).Text = filePath;
            }
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && ((Array)e.Data.GetData(DataFormats.FileDrop)).Length == 1)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // I don't know why, but it works
        private void Label_AutoPosition(object sender, EventArgs e, int x, int y)
        {
            Label label = (Label)sender;
            // 192F is from FormPatch.Designer.cs
            float scaleX = CurrentAutoScaleDimensions.Width / 192F, scaleY = CurrentAutoScaleDimensions.Height / 192F;
            label.Location = new System.Drawing.Point((int)((x * scaleX) - label.Width), (int)((y * scaleY) - label.Height / 2F));
        }
    }
}
