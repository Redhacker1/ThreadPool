using System;
using System.Collections.Generic;
using System.Diagnostics;
using BadThreadPool;

namespace ThreadPoolTest
{
    internal class Program
    {
        static void Main()
        {
            PiCalculatorTest piCalc = new();
            const int testAmount = int.MaxValue >> 1 >> 1 >> 1 >> 1 >> 1;
            List<ThreadTaskRequest> getReturns = new();

            Stopwatch watch = new();
            watch.Start();
            ThreadPoolClass threadPool = new();

            Console.WriteLine(testAmount);
            threadPool.InitializePool();
            threadPool.IgniteThreadPool();

            for (uint i = 0; i < testAmount; i++)
            {
                threadPool.AddRequest(piCalc.Calc_Pi);
            }
            
            
            while (!threadPool.AllThreadsIdle())
            {

            }
            
            watch.Stop();

            double mtTime = watch.ElapsedMilliseconds * 0.001;
            Console.WriteLine("Completed, Time in seconds {0}", mtTime);

            watch.Restart();
            for (uint i = 0; i < testAmount; i++)
            {
                getReturns.Add(threadPool.AddRequest(piCalc.Calc_Pi, piCalc.Test_Threading_callback));
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
                piCalc.Calc_Pi();
            }
            Console.WriteLine("Completed, Time in seconds {0}", watch.ElapsedMilliseconds * 0.001);
            Console.WriteLine("Single core performance was: {0}% slower", CalculatePercentDifference(watch.ElapsedMilliseconds * 0.001, mtTime));
            Console.WriteLine(getReturns[0].Result);
        }

        static int CalculatePercentDifference(double a, double b)
       {
            return (int)((a - b) / Math.Abs(b) *100);
       }
    }
}
