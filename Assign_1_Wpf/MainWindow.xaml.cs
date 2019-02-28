using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace Assign_1_Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private BackgroundWorker backgroundWorker;
        bool browseForFile = false;
        public MainWindow()
        {
            InitializeComponent();

            //Use background resource that was created
            backgroundWorker = (BackgroundWorker)FindResource("backgroundWorker");
        }

        
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            e.Result = ShowProgress((int)e.Argument, bgw, e);

        }

        private object ShowProgress(int progress, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int result = 0;
            // i equals the number records/files/lines to read etc...
            for (int data = 0; data <= progress; data++)
            {
                if (worker != null)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    if (worker.WorkerReportsProgress)
                    {
                        int dataRemaining = progress - data;
                        int percentComplete = (int)((float)data / (float)progress * 100);
                        string updateMessage = string.Format("{0} %. Data remaining: {1} data.", percentComplete, dataRemaining);
                        //string updateMessage = string.Format("{0} of {1}", i, progress);
                        worker.ReportProgress(percentComplete, updateMessage);
                    }
                }

                Thread.Sleep(100);
                result = data;
            }

            return result;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                outputBox.Text = e.Error.Message;
                MessageBox.Show(e.Error.StackTrace);

            }
            else if (e.Cancelled)
            {
                outputBox.Text = "Cancelled";
                progressBarLines.Value = 0;
            }
            else
            {
                outputBox.Text = e.Result.ToString();
                progressBarLines.Value = 0;
            }
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void BackgroundWorker_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            progressBarLines.Value = e.ProgressPercentage;
            outputBox.Text = (string)e.UserState;
        }

       

        //Some work that can be done while the background worker is busy
        private void AddNumbers(object sender, RoutedEventArgs e)
        {
            int num1, num2, total;
            num1 = Convert.ToInt32(textBoxNum1.Text);
            num2 = Convert.ToInt32(textBoxNum2.Text);
            total = num1 + num2;
            lblTotal.Content = total.ToString();
        }

        private void BtnStartLoading_Click(object sender, RoutedEventArgs e)
        {
            if (browseForFile)
            {
                int iterations;
                ProcessFile();
                bool success = Int32.TryParse(outputBox.Text, out iterations);
                if (success)
                {
                    if (!backgroundWorker.IsBusy)
                    {
                        backgroundWorker.RunWorkerAsync(iterations);
                        btnStart.IsEnabled = false;
                        btnStop.IsEnabled = true;
                        outputBox.Text = "";
                    }

                }
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
            }
            else
                MessageBox.Show("First select a file");
        }

        private void ProcessFile()
        {
            int lines = 0;
            int bytesToLoad = 0;
            string filePath = txtboxFilePath.Text;
            if (File.Exists(filePath))
            {
                StreamReader streamReader = new StreamReader(filePath);
                string[] fileData = new string[File.ReadAllLines(filePath).Length];
                byte[] fileInBytes = new byte[File.ReadAllBytes(filePath).Length];
                bytesToLoad = fileInBytes.Length;
                lines = fileData.Length;
                if (lines > 0)
                {
                    outputBox.Text = lines.ToString();
                    outputBoxBytes.Text = bytesToLoad.ToString();
                }
                else
                {
                    MessageBox.Show("File is empty");
                    return;
                }
                
            }
            else
            {
                MessageBox.Show("No such file");
                return;
            }
            
        }

        private void BtnStop_Click_1(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            backgroundWorker.CancelAsync();

        }

        private void BtnGetFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            if (openFileDialog.ShowDialog() == true)
            {
                txtboxFilePath.Text = (openFileDialog.FileName);
                browseForFile = true;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
