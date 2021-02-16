using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace ThreadingLearning
{
    public class ThreadPoolClass<T>: IDisposable
    {
        /// <summary>
        /// The list of total threadIDs not yet sure what this is used for? may be useless
        /// </summary>
        public List<int> ThreadIDs = new List<int>();
        /// <summary>
        /// A concurrent dictionary of return values
        /// </summary>
        public ConcurrentDictionary<int, T> Return_Values = new ConcurrentDictionary<int, T>();

        /// <summary>
        /// Not sure why this is not a concurrent dictionary
        /// </summary>
        //public Dictionary<int, PooledThreadClass<T>> ProcessIDtoThread = new Dictionary<int, PooledThreadClass<T>>();

        /// <summary>
        /// Currently useless, leftover from when I had a dedicated "Scheduler" function which was pretty inefficent, cannot be bothered to reimplement ATM though is dead simple to do so...
        /// </summary>
        public bool bIsRunning = true;

        // An array of threads
        private PooledThreadClass<T>[] pooledThreadClasses;

        /// <summary>
        /// A list of currently queued but not completed tasks
        /// </summary>
        public List<ThreadTaskRequest<T>> TasksRemaining = new List<ThreadTaskRequest<T>>();

        /// <summary>
        /// A list of completed tasks
        /// </summary>
        public List<ThreadTaskRequest<T>> CompletedTasks = new List<ThreadTaskRequest<T>>();

        /// <summary>
        /// Number of threads currently in use
        /// </summary>
        public byte Threads;

        // Locking objects
        /// <summary>
        /// Be sure to use this while manipulating TasksRemaining
        /// </summary>
        public object TaskListLock = new object();
        /// <summary>
        /// Obsolete due to removal of scheduler kept here because ShutDownHandler uses it (which is ATM useless)
        /// </summary>
        public object SchedulerLock = new object();
        /// <summary>
        /// Use while manipulating contents of ProcessIDToThread (Do not do this!)
        /// </summary>
        public object PIDLookupTableLock = new object();

        /// <summary>
        /// Use when manipulating ReturnValues (Really no need for this, you should not manipulate this unless you actually know what you are doing and want to GC stuff yourself)
        /// </summary>
        public object ReturnValueLock = new object();

        // used when manipulating the threadcount
        readonly object ThreadPoolLock = new object();

        /// <summary>
        /// Use when manipulating CompletedTasks (Really no need for this, you should not manipulate this unless you actually know what you are doing and want to GC stuff yourself)
        /// </summary>
        readonly public object CompletedTaskLock = new object();

        /// <summary>
        /// Sets the threadpool up to be used
        /// </summary>
        /// <param name="threads"> Threads to use in threadpool leave at 0 if unsure (Unsure if this works) </param>
        public void InitializePool(byte threads = 0)
        {
            if (threads == 0)
            {
                Threads = (byte)Environment.ProcessorCount;
            }
            else
            {
                Threads = (byte)threads;
            }
            SetMaxThreadCount(Threads);
            return;
        }
        /// <summary>
        /// Sets the max thread count after intialization (Again unsure if it works)
        /// </summary>
        /// <param name="ThreadCount">Threads to allocate</param>
        public void SetMaxThreadCount(byte ThreadCount)
        {
            lock(ThreadPoolLock)
            {
                pooledThreadClasses = new PooledThreadClass<T>[ThreadCount];
                for (int i = 0; i < Threads; i++)
                {
                    pooledThreadClasses[i] = new PooledThreadClass<T>(this);
                }
            }
        }
        /// <summary>
        /// Add request to task Queue, that way it can be run.
        /// </summary>
        /// <param name="Method"> Method you want to call, format in lambda AKA like: () => {method in question} </param>
        /// <returns>Returns ID so that it can be referenced later assuming you need a specific ID or that order of information is important, otherwise safe to ignore</returns>
        public int AddRequest(Func<T> Method, object instance = null)
        {
            bool bRelightThreadPool = false;
            if (TasksRemaining.Count == 0)
            {
                bRelightThreadPool = true;
            }

            if(instance == null)
            {
                instance = Activator.CreateInstance(Method.Method.DeclaringType);
            }

            lock (TaskListLock)
            {
                ThreadTaskRequest<T> TaskClass = new ThreadTaskRequest<T>( Method, this ,instance);
                TasksRemaining.Insert(0, TaskClass);
                if (bRelightThreadPool)
                {
                    foreach (PooledThreadClass<T> threadClass in pooledThreadClasses)
                    {
                        if(threadClass.bIsIdle)
                        {
                            threadClass.ReigniteThread();
                        }
                    }
                }   
                return TaskClass.ID;
            }

        }

        /// <summary>
        /// Actually runs the threadpool, the name scheme is set up like this because the threadpool acts like a fire of sorts, you stoke it with fuel (Tasks) then you let it burn until all the "fuel" is gone, then it stops, will make an autoignite featurel later
        /// </summary>
        public void IgniteThreadPool()
        {
            lock(ThreadPoolLock)
            {
                for (int threadNumber = 0; threadNumber < pooledThreadClasses.Length; threadNumber++)
                {
                    var thread = pooledThreadClasses[threadNumber];
                    thread.PrepareThread();
                }
            }

        }

        /// <summary>
        /// Currently Non-functional 
        /// </summary>
        public void ShutDownHandler()
        {
            bIsRunning = false;
            if (AllThreadsIdle())
            {
                foreach (PooledThreadClass<T> pooledThreadClass in pooledThreadClasses)
                {
                    pooledThreadClass.DestroyThread();
                }
            }
        }

        /// <summary>
        /// Unsure of this works, this might crash the program, that being said is supposed to signal true if the threadpool is completly idle
        /// </summary>
        /// <returns> true if the threadpool is idle, false if it is not</returns>
        public bool AllThreadsIdle()
        {
            foreach (PooledThreadClass<T> Thread in pooledThreadClasses)
            {
                if (Thread.bIsIdle == false && TasksRemaining.Count != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets return value of thread ID
        /// </summary>
        /// <param name="TaskID"></param>
        /// <returns>The return value of the thread</returns>
        public T GetReturnValue(int TaskID)
        {
            lock(ReturnValueLock)
            {
                if(Return_Values.ContainsKey(TaskID))
                {
                    return Return_Values[TaskID];
                }
                else
                {
                    return default;
                }
            }
        }

        public void Dispose()
        {
            ShutDownHandler();
        }
    }
}
