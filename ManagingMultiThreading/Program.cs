using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagingMultiThreading
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //MonitorExample m = new MonitorExample();
            //InterlockedExample i = new InterlockedExample();
            //CancellingTokenExample e = new CancellingTokenExample();
            CancellingWithExceptionExample e = new CancellingWithExceptionExample();
        }
    }

    class MonitorExample
    {
        long sharedTotal;
        object sharedTotalLock = new object();

        int[] items = Enumerable.Range(0, 5000000).ToArray();

        void addRangeOfValues(int start, int end)
        {
            long subTotal = 0;
            while (start < end)
            {
                //sharedTotal = sharedTotal + items[start];
                subTotal = subTotal + items[start];
                start++;
            }

            try
            {
                Monitor.Enter(sharedTotalLock);
                sharedTotal = sharedTotal + subTotal;
            }
            finally
            {
                Monitor.Exit(sharedTotalLock);
            }
        }

        public MonitorExample()
        {
            List<Task> tasks = new List<Task>();

            int rangeSize = 1000;
            int rangeStart = 0;

            while (rangeStart < items.Length)
            {
                int rangeEnd = rangeStart + rangeSize;

                if (rangeEnd > items.Length)
                {
                    rangeEnd = items.Length;
                }

                // Create local copies of the parameters
                int rs = rangeStart;
                int re = rangeEnd;

                tasks.Add(Task.Run(() => addRangeOfValues(rs, re)));
                rangeStart = rangeEnd;
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("The total is: {0}", sharedTotal);
            Console.ReadKey();
        }
    }
    
    class InterlockedExample
    {
        long sharedTotal;
        object sharedTotalLock = new object();

        int[] items = Enumerable.Range(0, 5000000).ToArray();

        void addRangeOfValues(int start, int end)
        {
            long subTotal = 0;
            while (start < end)
            {
                //sharedTotal = sharedTotal + items[start];
                subTotal = subTotal + items[start];
                start++;
            }

            Interlocked.Add(ref sharedTotal, subTotal);
        }

        public InterlockedExample()
        {
            List<Task> tasks = new List<Task>();

            int rangeSize = 1000;
            int rangeStart = 0;

            while (rangeStart < items.Length)
            {
                int rangeEnd = rangeStart + rangeSize;

                if (rangeEnd > items.Length)
                {
                    rangeEnd = items.Length;
                }

                // Create local copies of the parameters
                int rs = rangeStart;
                int re = rangeEnd;

                tasks.Add(Task.Run(() => addRangeOfValues(rs, re)));
                rangeStart = rangeEnd;
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("The total is: {0}", sharedTotal);
            Console.ReadKey();
        }
    }

    class CancellingTokenExample
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        void Clock()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Tick");
                Thread.Sleep(500);

            }
        }

        public CancellingTokenExample()
        {
            Task.Run(() => Clock());
            Console.WriteLine("Press any key to stop the clock");
            Console.ReadKey();
            cancellationTokenSource.Cancel();
            Console.WriteLine("Clock stopped");
            Console.ReadKey();
        }
    }

    class CancellingWithExceptionExample
    {

        void Clock(CancellationToken cancellationToken)
        {
            int tickCount = 0;
            while (!cancellationToken.IsCancellationRequested && tickCount < 20)
            {
                tickCount++;
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public CancellingWithExceptionExample()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task clock = Task.Run(() => Clock(cancellationTokenSource.Token));
            Console.WriteLine("Press any key to stop the clock");
            Console.ReadKey();
            if (clock.IsCompleted)
            {
                Console.WriteLine("Clock task completed");
            }
            else
            {
                try
                {
                    cancellationTokenSource.Cancel();
                    clock.Wait();
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine("Clock stopped: {0}", ex.InnerExceptions[0].ToString());
                }
            }

            Console.ReadKey();
        }
    }
}
