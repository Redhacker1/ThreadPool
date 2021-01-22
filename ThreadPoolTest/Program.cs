using System;
using System.Diagnostics;
using System.Collections.Generic;
using ThreadingLearning;

namespace ThreadPoolTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ThreadPoolClass<object> ThreadPool = new ThreadPoolClass<object>();
            List<int> GetReturns = new List<int>(); 
            ThreadPool.InitializePool(0);
            int TestAmount = short.MaxValue;
            Console.WriteLine("{0}, Total Threads", ThreadPool.Threads);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < TestAmount; i++)
            {
                GetReturns.Add(ThreadPool.AddRequest(() => TestThreading_old(1)));
            }

            ThreadPool.IgniteThreadPool();

            // Spinlock while we wait for threads to finish (complain all you want about this)
            while (!ThreadPool.AllThreadsIdle())
            {

            };
            watch.Stop();

            Console.WriteLine("Completed, Time in seconds {0}", watch.ElapsedMilliseconds* 0.001);

            watch.Start();
            for (int i = 0; i < TestAmount; i++)
            {
                GetReturns.Add(ThreadPool.AddRequest(() => TestThreading()));
            }

            while (!ThreadPool.AllThreadsIdle())
            {

            };
            watch.Stop();
            Console.WriteLine("Completed, Time in seconds {0}", watch.ElapsedMilliseconds * 0.001);

            Console.WriteLine("Shutting down");
            ThreadPool.ShutDownHandler();
        }

        static string TestThreading()
        {
            return "hello" + "World";
        }

        /// <summary>
        /// Calculates pi, just for testing
        /// </summary>
        /// <returns>pi</returns>
        static double TestThreading_old(int Arg1)
        {
            double pi = 0;
            for (int k = 0; k <= ushort.MaxValue; k++)
            {
                double delta = ((Math.Pow((-1), k)) / (Math.Pow(2, (10 * k)))) *
                    (-(Math.Pow(2, 5) / ((4 * k) + 1))
                     - (1 / ((4 * k) + 3))
                     + (Math.Pow(2, 8) / ((10 * k) + 1))
                     - (Math.Pow(2, 6) / ((10 * k) + 3))
                     - (Math.Pow(2, 2) / ((10 * k) + 5))
                     - (Math.Pow(2, 2) / ((10 * k) + 7))
                     + (1 / ((10 * k) + 9)));

                delta /= (Math.Pow(2, 6));
                pi += delta;
            }
            return pi;
        }
    }
}
