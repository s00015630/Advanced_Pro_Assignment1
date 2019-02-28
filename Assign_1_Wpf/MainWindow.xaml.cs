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
        int totalFileSize;
        static string fileName;
        public MainWindow()
        {
            InitializeComponent();
            //Call from XAML window
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
            FileStream readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            BinaryReader read = new BinaryReader(readStream);
            int countRemaining = 0;

            while (read.PeekChar() != -1)
            {
                progress = Convert.ToInt32(read.ReadByte());
                countRemaining++;

                if (worker != null)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    if (worker.WorkerReportsProgress)
                    {
                        int dataRemaining = totalFileSize - countRemaining;
                        int percentComplete = (int)((float)countRemaining / (float)totalFileSize * 100);
                        string updateMessage = string.Format("{0} %. Data remaining: {1}", percentComplete, dataRemaining);
                        //string updateMessage = string.Format("{0} of {1}", i, progress);
                        worker.ReportProgress(percentComplete, updateMessage);
                        Thread.Sleep(1);
                    }
                    result = countRemaining;
                }

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
        
       
        private void Add_Numbers_click(object sender, RoutedEventArgs e)
        {
            DoAddition();
        }

        //Some work that can be done while the background worker is busy
        private void DoAddition()
        {
            Thread.Sleep(100);
            int num1, num2, total;
            num1 = Convert.ToInt32(textBoxNum1.Text);
            num2 = Convert.ToInt32(textBoxNum2.Text);
            total = num1 + num2;
            lblTotal.Content = total.ToString();
        }
        
        //Get a file and set the text box
        private void BtnGetFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                txtboxFilePath.Text = (openFileDialog.FileName);
                browseForFile = true;
                fileName = openFileDialog.FileName;
            }
        }

        private void CheckFile()
        {
            int bytesToLoad = 0;
            string filePath = txtboxFilePath.Text;

            if (File.Exists(filePath))
            {
                StreamReader streamReader = new StreamReader(filePath);
                byte[] fileInBytes = new byte[File.ReadAllBytes(filePath).Length];
                totalFileSize = File.ReadAllBytes(filePath).Length;
                bytesToLoad = fileInBytes.Length;

                if (bytesToLoad > 0)
                {
                    outputBox.Text = bytesToLoad.ToString();
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

        private void BtnStartLoading_Click(object sender, RoutedEventArgs e)
        {
            if (browseForFile)
            {
                int iterations;
                CheckFile();
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
            }
            else
                MessageBox.Show("First select a file");


        }

        private void BtnStop_Click_1(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            backgroundWorker.CancelAsync();

        }


        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
