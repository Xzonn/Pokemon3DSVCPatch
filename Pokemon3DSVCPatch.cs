using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Pokemon3DSVCPatch
{
    public partial class FormPatch : Form
    {
        private void PatchIt(object sender, EventArgs e)
        {
            string buttonConfirmText = buttonConfirm.Text;
            buttonConfirm.Text = "……";
            buttonConfirm.Enabled = false;
            string originalPath = textBox1.Text;
            string romPath = textBox2.Text;
            string patchPath = textBox3.Text;
            string outputPath = textBox4.Text;
            if (!CheckIfFileExists(originalPath)) return;
            if (!CheckIfFileExists(romPath)) return;
            // 创建临时文件夹
            string tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            while (Directory.Exists(tempPath) || File.Exists(tempPath))
            {
                tempPath = $"temp_{DateTime.Now.GetHashCode():X8}";
            }
            DirectoryInfo di = Directory.CreateDirectory(tempPath);
            di.Attributes |= FileAttributes.Hidden;
            // 复制cia
            File.Copy(originalPath, $"{tempPath}/input.cia", true);
            // 读取并写入外部exe
            foreach (string fileName in new string[] { "3dstool.exe", "decrypt.exe", "makerom.exe" })
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                BufferedStream inStream = new BufferedStream(asm.GetManifestResourceStream($"Pokemon3DSVCPatch.tools.{fileName}"));
                FileStream outStream = new FileStream($@"{tempPath}\{fileName}", FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[inStream.Length];
                inStream.Read(buffer, 0, (int)inStream.Length);
                outStream.Write(buffer, 0, (int)inStream.Length);
                inStream.Close();
                outStream.Close();
            }
            // 调用decrypt解密
            CreateProcess(tempPath, "decrypt.exe", "input.cia", true);
            // 调用3dstool拆包
            CreateProcess(tempPath, "3dstool.exe", "-xvtf cxi input.0.ncch --header ncchheader.bin --exh exheader.bin --logo logo.bcma.lz --plain plain.bin --exefs exefs.bin --romfs romfs.bin");
            CreateProcess(tempPath, "3dstool.exe", "-xvtf romfs romfs.bin --romfs-dir romfs");
            // 复制ROM
            string[] romNames = Directory.GetFiles($@"{tempPath}\romfs\rom");
            if (romNames.Length > 1)
            {
                return;
            }
            string romName = romNames[0].Split('\\').Last();
            File.Copy(romPath, $@"{tempPath}\romfs\rom\{romName}", true);
            if (File.Exists(patchPath))
            {
                File.Copy(patchPath, $@"{tempPath}\romfs\{romName}.patch", true);
            }
            // 调用3dstool打包
            CreateProcess(tempPath, "3dstool.exe", "-cvtf romfs romfs.bin --romfs-dir romfs");
            CreateProcess(tempPath, "3dstool.exe", "-cvtf cxi input.0.ncch --header ncchheader.bin --exh exheader.bin --logo logo.bcma.lz --plain plain.bin --exefs exefs.bin --romfs romfs.bin");
            // 调用makerom创建cia
            CreateProcess(tempPath, "makerom.exe", "-f cia -ignoresign -target p -o output.cia -i input.0.ncch:0:0 -i input.1.ncch:1:1");
            File.Copy($@"{tempPath}\output.cia", outputPath, true);
            // 删除临时文件夹
            Directory.Delete(tempPath, true);
            MessageBox.Show("已完成。", "完成");
            buttonConfirm.Text = buttonConfirmText;
            buttonConfirm.Enabled = true;
        }

        private bool CheckIfFileExists(string filePath)
        {
            bool exists = File.Exists(filePath);
            if (!exists)
            {
                MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return exists;
        }

        private void CreateProcess(string tempPath, string exeName, string arguments, bool redirectInput = false)
        {
            Process process = new Process();
            process.StartInfo.FileName = $@"{Application.StartupPath}\{tempPath}\{exeName}";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = $@"{Application.StartupPath}\{tempPath}";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            if (redirectInput)
            {
                process.StartInfo.RedirectStandardInput = true;
                process.Start();
                process.StandardInput.WriteLine();
            }
            else
            {
                process.Start();
            }
            process.OutputDataReceived += (s, _e) => Console.WriteLine(_e.Data);
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
        }
    }
}
