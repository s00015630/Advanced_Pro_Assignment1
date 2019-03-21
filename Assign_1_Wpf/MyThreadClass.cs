using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Assign_1_Wpf
{
    public class MyThreadClass : IDisposable
    {
        //create delegate
        public delegate void ProgressHandler(int Progress, int Max, int senderId);

        //Setup handler 
        public event ProgressHandler OnProgressEvent;


        //progress bar max
        readonly int MaxProgressbar2 = 100;

        #region
        public void MyThread1()
        {
            for (int i = 0; i < MaxProgressbar2; i++)
            {             
                MyProgressTrigger(i, MaxProgressbar2, 1);
                //Not Runnable State
                Thread.Sleep(100);
            }
            
            MyProgressTrigger(MaxProgressbar2, MaxProgressbar2, 1);
        }
            
        public void MyThread2()
        {
            for (int i = 0; i < MaxProgressbar2; i++)
            {
                
                MyProgressTrigger(i, MaxProgressbar2, 2);
                //Not Runnable State
                Thread.Sleep(100);
            }
            
            MyProgressTrigger(MaxProgressbar2, MaxProgressbar2, 2);
        }
        #endregion
        
        //process the work
        private void MyProgressTrigger(int Progress, int Max, int senderId)
        {
            OnProgressEvent?.Invoke(Progress, Max, senderId);
        }


        //Clean up using garbage collection
        #region "routines"
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        ~MyThreadClass()
        {
            Dispose();
        }
        #endregion
    }
}
