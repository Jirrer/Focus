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
        static bool timerRunning = false;
        Button runButton;
        Button endButton;
        RichTextBox outputTextBox;
        private Process process;

        public Focus()
        {
            this.Text = "Focus";
            this.Width = 590;
            this.Height = 320;

            this.MinimumSize = new Size(590, 320);
            this.MaximumSize = new Size(590, 320);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            runButton = new Button { Left = 10, Top = 8, Width = 280, Height = 100, Text = "Start Timer" };
            endButton = new Button { Left = 290, Top = 8, Width = 280, Height = 100, Text = "End Timer" };
            outputTextBox = new RichTextBox
            {
                Left = 10,
                Top = 110,
                Width = 560,
                Height = 195,
                ReadOnly = true,
            };

            runButton.Font = new Font("Calibri Light", 14);
            endButton.Font = new Font("Calibri Light", 14);
            outputTextBox.Font = new Font("Calibri Light", 28);

            outputTextBox.Text = "----- Click Start to Begin Timer -----";
            outputTextBox.CenterText();

            runButton.Click += RunButton_Click;
            endButton.Click += endButton_Click;

            this.Controls.Add(runButton);
            this.Controls.Add(endButton);
            this.Controls.Add(outputTextBox);

            this.FormClosing += Focus_FormClosing;
        }


        private async void RunButton_Click(object sender, EventArgs e) {
            if (timerRunning) { return; }
            timerRunning = true;

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

        private void endButton_Click(object sender, EventArgs e) {
            timerRunning = false;

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

        private void Focus_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.Kill();
                }
                catch { /* Ignore errors if already exited */ }
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

            int totalLines = box.Height / box.Font.Height;
            int textLines = box.Lines.Length;
            int paddingLines = Math.Max(0, (totalLines - textLines) / 2);

            if (paddingLines > 0)
            {
                var lines = box.Lines.ToList();
                lines.InsertRange(0, Enumerable.Repeat(string.Empty, paddingLines));
                box.Lines = lines.ToArray();
            }

            box.SelectAll();
            box.SelectionAlignment = HorizontalAlignment.Center;
        }
    }
}
