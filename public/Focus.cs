using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

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

            runButton = new Button { Left = 280, Top = 10, Width = 80, Text = "Start Timer" };
            endButton = new Button { Left = 370, Top = 10, Width = 80, Text = "End Timer" };
            outputTextBox = new RichTextBox
            {
                Left = 10,
                Top = 40,
                Width = 760,
                Height = 420,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right // ← this line makes it resizable
            };

            runButton.Click += RunButton_Click;
            endButton.Click += endButton_Click;

            this.Controls.Add(runButton);
            this.Controls.Add(endButton);
            this.Controls.Add(outputTextBox);
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            process = new Process();
            process.StartInfo.FileName = @"..\src\timer.exe";
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
                    outputTextBox.AppendText(line + Environment.NewLine);
                    outputTextBox.SelectionStart = outputTextBox.Text.Length;
                    outputTextBox.ScrollToCaret();
                    // Limit to last 500 lines
                    if (outputTextBox.Lines.Length > 500)
                    {
                        outputTextBox.Lines = outputTextBox.Lines.Skip(outputTextBox.Lines.Length - 500).ToArray();
                    }
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
                    outputTextBox.AppendText("Process terminated." + Environment.NewLine);
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
}
