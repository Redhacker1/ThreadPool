using System;
using System.Reflection;
using System.Text;

namespace ThreadingLearning
{
    public class ThreadTaskRequest<T>
    {
        // Random Number Generator
        static readonly Random RNG = new Random();
        readonly ThreadPoolClass<T> threadPool;

        public ThreadTaskRequest(MethodInfo method, ThreadPoolClass<T> Threadpool)
        {
            Method = new object[2] { Activator.CreateInstance(method.ReflectedType), method };

            threadPool = Threadpool;
            RandomUniqueThreadID();
        }
        // Generates a random ID to reference the task in case order of data is important, can ignore if order of data is not important
        private void RandomUniqueThreadID()
        {
            // Generates a random number between 1 and the max value of an integer (investigate a uint)
            id = RNG.Next(1, int.MaxValue);

            // If by some chance you manage to reroll the thread ID for another task reroll again... (is possible if you make tasks fast enough)
            if (threadPool.ThreadIDs.Contains(ID) || threadPool.Return_Values.ContainsKey(ID))
            {
                RandomUniqueThreadID();
            }
        }

        /// <summary>
        /// Method the thread is supposed to call when created
        /// </summary>
        public object[] Method { get; }
        //ID the thread uses
        private int id;
        /// <summary>
        /// ID to reference the task and return value by in case you need a specific thread or order of data is important otherwise safe to ignore
        /// </summary>
        public int ID { get => id;}
    }
}
