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
using System.Diagnostics;

namespace Assign_1_Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyThreadClass thread = new MyThreadClass();


        private BackgroundWorker backgroundWorker;
        bool browseForFile = false;
        public static int totalFileSize ;
        static string fileName;
        private Stopwatch stopwatchFirst = new Stopwatch();
        private Stopwatch stopwatchSecond = new Stopwatch();
        static bool fileHasALength = false;
        public MainWindow()
        {
            InitializeComponent();

            //subscribe to service, call ThreadOutPuts
            thread.OnProgressEvent += ThreadOutPuts;

            //Call from XAML window
            backgroundWorker = (BackgroundWorker)FindResource("backgroundWorker");         
        }

       
        void MainWindow_Closed(object sender, EventArgs e)
        {
            thread.OnProgressEvent -= ThreadOutPuts;
        }




        //backgroundworker methods
        #region 
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            e.Result = ShowProgress((int)e.Argument, bgw, e);

        }

        //Read all the bytes in a file. Using this for progressbars 
        private object ShowProgress(int progress, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int result = 0;
            FileStream readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
           
            stopwatchFirst.Start();
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

                        //Not Runnable State 
                        Thread.Sleep(1);
                    }
                    result = countRemaining;
                }

            }
            stopwatchFirst.Stop();
            
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
                lblTime1.Content = stopwatchFirst.Elapsed.ToString();
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

        //Using a background worker
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

        //Cancel background worker
        private void BtnStop_Click_1(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            backgroundWorker.CancelAsync();

        }
        #endregion





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
                    fileHasALength = true;
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


        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnLoadFileFaster_Click(object sender, RoutedEventArgs e)
        {
            //Has the number of bytes in the file been calculated
            //It uses the same value that the first progressbar is using to fill it
            if (fileHasALength)
            {
                //Create threads using MyThread class
                Thread t1 = new Thread(new ThreadStart(thread.MyThread1));
                Thread t2 = new Thread(new ThreadStart(thread.MyThread2));

                //Start each thread
                t1.Start();
                t2.Start();
                
            }
            else
                MessageBox.Show("Click the load 1 button first");
            
        }

        //Use the two methods in the MyThread class to split the work and do it faster
        void ThreadOutPuts(int Progress, int Max, int senderId)
        {
            
            //do work if id 1
            if (senderId == 1)
            {
                progressBar2.Dispatcher.Invoke(new Action(() =>
                {
                    progressBar2.Maximum = Max;
                    progressBar2.Value = Progress;
                    lblProgress2.Content = Progress.ToString()+" %";
                }));
            }
            //do work if id 2
            if (senderId == 2)
            {
                progressBar2.Dispatcher.Invoke(new Action(() =>
                {
                    progressBar2.Maximum = Max;
                    progressBar2.Value = Progress;
                    lblProgress2.Content = Progress.ToString() + " %";
                }));
            }
            
        }
       
    }
}
