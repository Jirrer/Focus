using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Focus
{
    public class Focus : Form
    {
        Button runButton;
        Button endButton;
        RichTextBox outputTextBox;
        private Process process;

        public Focus()
        {
            this.Text = "Focus";
            this.Width = 800;
            this.Height = 500;

            this.MinimumSize = new Size(800, 500);
            this.MaximumSize = new Size(800, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            runButton = new Button { Left = 260, Top = 8, Width = 110, Height = 30, Text = "Start Timer" };
            endButton = new Button { Left = 370, Top = 8, Width = 110, Height = 30, Text = "End Timer" };
            outputTextBox = new RichTextBox
            {
                Left = 10,
                Top = 40,
                Width = 760,
                Height = 420,
                ReadOnly = true,
            };

            runButton.Font = new Font("Calibri Light", 14);
            endButton.Font = new Font("Calibri Light", 14);
            outputTextBox.Font = new Font("Calibri Light", 28);

            outputTextBox.Text = "--------- Click Start to Begin Timer ---------";
            outputTextBox.CenterText();

            runButton.Click += RunButton_Click;
            endButton.Click += endButton_Click;

            this.Controls.Add(runButton);
            this.Controls.Add(endButton);
            this.Controls.Add(outputTextBox);
        }


        private async void RunButton_Click(object sender, EventArgs e)
        {
            process = new Process();
            process.StartInfo.FileName = @"src\timer.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            // Buffer for lines
            var linesBuffer = new System.Collections.Concurrent.ConcurrentQueue<string>();
            var updateTimer = new System.Windows.Forms.Timer { Interval = 100 };
            updateTimer.Tick += (s, ev) =>
            {
                while (linesBuffer.TryDequeue(out var line))
                {
                    outputTextBox.Text = line;
                    outputTextBox.CenterText();

                }
            };
            updateTimer.Start();

            // Read output in background
            await Task.Run(async () =>
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = await process.StandardOutput.ReadLineAsync();
                    linesBuffer.Enqueue(line);
                }
            });

            updateTimer.Stop();
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.Kill();
                    outputTextBox.Text = " ";
                }
                catch (Exception ex)
                {
                    outputTextBox.AppendText("Error terminating process: " + ex.Message + Environment.NewLine);
                }
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Focus()); // ← fixed
        }
    }
    

public static class RichTextBoxExtensions
    {
        public static void CenterText(this RichTextBox box)
        {
            box.SelectAll();
            box.SelectionAlignment = HorizontalAlignment.Center;
        }
    }
}
