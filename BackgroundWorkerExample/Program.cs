﻿/* 
 * Code example from Microsoft .NET 
 * URL: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.backgroundworker?view=netframework-4.7.2
 * 
 * BackgroundWorker Class
 * Executes an operation on a separate thread.
 * 
 * The following code example demonstrates the use of the BackgroundWorker class 
 * for executing a time-consuming operation asynchronously. 
 * The following illustration shows an example of the output. 
 * The operation computes the selected Fibonacci number, 
 * reports progress updates as the calculation proceeds, 
 * and permits a pending calculation to be canceled. 
 */
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BackgroundWorkerExample
{
    public class FibonacciForm : System.Windows.Forms.Form
    {
        private int numberToCompute = 0;
        private int highestPercentageReached = 0;

        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button startAsyncButton;
        private System.Windows.Forms.Button cancelAsyncButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label resultLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;

        private class WorkerArguments
        {
            public int Arg1 { get; set; }
            public int Arg2 { get; set; }
            public WorkerArguments(int a1, int a2)
            {
                Arg1 = a1;
                Arg2 = a2;
            }
        }

        private class WorkerResults
        {
            public long Res1 { get; set; }
            public long Res2 { get; set; }
            public WorkerResults(long r1, long r2)
            {
                Res1 = r1;
                Res2 = r2;
            }
        }

        public FibonacciForm()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        // Set up the BackgroundWorker object by 
        // attaching event handlers. 
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        private void startAsyncButton_Click(System.Object sender, System.EventArgs e)
        {
            // Reset the text in the result label.
            resultLabel.Text = String.Empty;

            // Disable the UpDown control until 
            // the asynchronous operation is done.
            this.numericUpDown1.Enabled = false;

            // Disable the Start button until 
            // the asynchronous operation is done.
            this.startAsyncButton.Enabled = false;

            // Enable the Cancel button while 
            // the asynchronous operation runs.
            this.cancelAsyncButton.Enabled = true;

            // Get the value from the UpDown control.
            numberToCompute = (int)numericUpDown1.Value;
            int half1 = numberToCompute / 2;
            int half2 = numberToCompute - half1;

            // Reset the variable for percentage tracking.
            highestPercentageReached = 0;

            // Start the asynchronous operation.
            backgroundWorker1.RunWorkerAsync(new WorkerArguments(half1, half2));
        }

        private void cancelAsyncButton_Click(System.Object sender, System.EventArgs e)
        {
            // Cancel the asynchronous operation.
            this.backgroundWorker1.CancelAsync();

            // Disable the Cancel button.
            cancelAsyncButton.Enabled = false;
        }

        // This event handler is where the actual,
        // potentially time-consuming work is done.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the 
            // RunWorkerCompleted eventhandler.
            long fib = ComputeFibonacci(((WorkerArguments)(e.Argument)).Arg1 + ((WorkerArguments)(e.Argument)).Arg2, worker, e);
            long half1 = fib / 2;
            long half2 = fib - half1;
            e.Result = new WorkerResults(half1, half2);
        }

        // This event handler deals with the results of the
        // background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                resultLabel.Text = "Canceled";
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                resultLabel.Text = (((WorkerResults)e.Result).Res1 + ((WorkerResults)e.Result).Res2).ToString();
            }

            // Enable the UpDown control.
            this.numericUpDown1.Enabled = true;

            // Enable the Start button.
            startAsyncButton.Enabled = true;

            // Disable the Cancel button.
            cancelAsyncButton.Enabled = false;
        }

        // This event handler updates the progress bar.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        // This is the method that does the actual work. For this
        // example, it computes a Fibonacci number and
        // reports progress as it does its work.
        long ComputeFibonacci(int n, BackgroundWorker worker, DoWorkEventArgs e)
        {
            // The parameter n must be >= 0 and <= 91.
            // Fib(n), with n > 91, overflows a long.
            if ((n < 0) || (n > 91))
            {
                throw new ArgumentException("value must be >= 0 and <= 91", "n");
            }

            long result = 0;

            // Abort the operation if the user has canceled.
            // Note that a call to CancelAsync may have set 
            // CancellationPending to true just after the
            // last invocation of this method exits, so this 
            // code will not have the opportunity to set the 
            // DoWorkEventArgs.Cancel flag to true. This means
            // that RunWorkerCompletedEventArgs.Cancelled will
            // not be set to true in your RunWorkerCompleted
            // event handler. This is a race condition.

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                if (n < 2)
                {
                    result = 1;
                }
                else
                {
                    result = ComputeFibonacci(n - 1, worker, e) + ComputeFibonacci(n - 2, worker, e);
                }

                // Report progress as a percentage of the total task.
                int percentComplete = (int)((float)n / (float)numberToCompute * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
            }
            return result;
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.startAsyncButton = new System.Windows.Forms.Button();
            this.cancelAsyncButton = new System.Windows.Forms.Button();
            this.resultLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(16, 16);
            this.numericUpDown1.Maximum = new System.Decimal(new int[] { 92, 0, 0, 0 });
            this.numericUpDown1.Minimum = new System.Decimal(new int[] { 1, 0, 0, 0 });
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new System.Decimal(new int[] { 1, 0, 0, 0 });
            // 
            // startAsyncButton
            // 
            this.startAsyncButton.Location = new System.Drawing.Point(16, 72);
            this.startAsyncButton.Name = "startAsyncButton";
            this.startAsyncButton.Size = new System.Drawing.Size(120, 23);
            this.startAsyncButton.TabIndex = 1;
            this.startAsyncButton.Text = "Start Async";
            this.startAsyncButton.Click += new System.EventHandler(this.startAsyncButton_Click);
            // 
            // cancelAsyncButton
            // 
            this.cancelAsyncButton.Enabled = false;
            this.cancelAsyncButton.Location = new System.Drawing.Point(153, 72);
            this.cancelAsyncButton.Name = "cancelAsyncButton";
            this.cancelAsyncButton.Size = new System.Drawing.Size(119, 23);
            this.cancelAsyncButton.TabIndex = 2;
            this.cancelAsyncButton.Text = "Cancel Async";
            this.cancelAsyncButton.Click += new System.EventHandler(this.cancelAsyncButton_Click);
            // 
            // resultLabel
            // 
            this.resultLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.resultLabel.Location = new System.Drawing.Point(112, 16);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(160, 23);
            this.resultLabel.TabIndex = 3;
            this.resultLabel.Text = "(no result)";
            this.resultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(18, 48);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(256, 8);
            this.progressBar1.Step = 2;
            this.progressBar1.TabIndex = 4;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            // 
            // FibonacciForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 118);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.resultLabel);
            this.Controls.Add(this.cancelAsyncButton);
            this.Controls.Add(this.startAsyncButton);
            this.Controls.Add(this.numericUpDown1);
            this.Name = "FibonacciForm";
            this.Text = "Fibonacci Calculator";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FibonacciForm());
        }
    }
}
