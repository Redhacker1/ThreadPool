using System;
using System.Collections.Generic;
using System.Diagnostics;
using BadThreadPool;

namespace ThreadPoolTest
{
    internal static class Program
    {
        static void Main()
        {
            const int testAmount = short.MaxValue;
            List<ThreadTaskRequest> getReturns = new();

            Stopwatch watch = new();
            watch.Start();
            ThreadPoolClass threadPool = new();

            Console.WriteLine(testAmount
                );
            threadPool.InitializePool();
            threadPool.IgniteThreadPool();

            for (uint i = 0; i < testAmount; i++)
            {
                getReturns.Add(threadPool.AddRequest(TestThreading_old));
            }

            // Spinlock while we wait for threads to finish (complain all you want about this)
            while (!threadPool.AllThreadsIdle())
            {
                
            }

            watch.Stop();

            double mtTime = watch.ElapsedMilliseconds * 0.001;
            Console.WriteLine("Completed, Time in seconds {0}", mtTime);

            watch.Restart();
            for (uint i = 0; i < testAmount; i++)
            {
                getReturns.Add(threadPool.AddRequest(TestThreading_old, Test_Threading_callback));
            }

            while (!threadPool.AllThreadsIdle())
            {

            }
            watch.Stop();
            Console.WriteLine("Completed, Time in seconds {0}", watch.ElapsedMilliseconds * 0.001);

            Console.WriteLine("Shutting down");
            threadPool.ShutDownHandler();

            Console.WriteLine("Single Threaded Test!");
            watch.Reset();
            watch.Start();
            for (uint i = 0; i < testAmount; i++)
            { 
                TestThreading_old();
            }
            Console.WriteLine("Completed, Time in seconds {0}", watch.ElapsedMilliseconds * 0.001);
            Console.WriteLine("Single core performance was: {0}% slower", CalculatePercentDifference(watch.ElapsedMilliseconds * 0.001, mtTime));
            Console.WriteLine(getReturns[0].Result);
        }

        static int CalculatePercentDifference(double a, double b)
       {
            return (int)((a - b) / Math.Abs(b) *100);
       }

        /// <summary>
        /// Calculates pi, just for testing
        /// </summary>
        /// <returns>pi</returns>
        static object TestThreading_old()
        {
            double pi = 0;
            for (int k = 0; k <= short.MaxValue; k++)
            {
                double delta = ((Math.Pow((-1), k)) / (Math.Pow(2, (10 * k)))) *
                               (-(Math.Pow(2, 5) / ((4 * k) + 1))
                                - 1f / (4 * k + 3)
                                + (Math.Pow(2, 8) / ((10 * k) + 1))
                                - (Math.Pow(2, 6) / ((10 * k) + 3))
                                - (Math.Pow(2, 2) / ((10 * k) + 5))
                                - (Math.Pow(2, 2) / ((10 * k) + 7))
                                + 1f / (k * 10 + 9));

                delta /= (Math.Pow(2, 6));
                pi += delta;
            }
            return pi;
        }

        static void Test_Threading_callback(CallbackArgs<object> args)
        {
            
        }
    }
}
