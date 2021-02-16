using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingLearning
{
    public class PooledThreadClass<T>
    {

        // The result, I see no reason to do this...
        /// <summary>
        /// result of the thread
        /// </summary>
        private T Result = default;

        private EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Indicates if the thread has "work" to do
        /// </summary>
        public bool bIsAlive = false;

        /// <summary>
        /// Used to pause the thread
        /// </summary>
        public bool bIsPaused = false;

        /// <summary>
        /// Is true if the thread has no work to do and the manual reset event is triggered.
        /// </summary>
        public bool bIsIdle = false;

        /// <summary>
        /// ID used for Identifing the thread when order actually matters. You declare the thread synchronusly and then keep track of it in order, then run it asynchronusly if order really does not matter you can just ignore this, just run a foreach loop on the result list and pull the value
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// Underlying thread class that runs, really should private as there is no way to ensure that the thread is not running/finished therefore it is unsafe to manipulate outside of the thread class on the GrabEvent and/or constructor. You should expose the isAlive etc through this 
        /// </summary>
        public Thread InternalThread;

        /// <summary>
        /// Keeps track of the order this thread was "spawned in"
        /// </summary>
        public int ThreadNumber;
        
        /// <summary>
        /// Underlying threadpool class that "owns" this thread instance
        /// </summary>
        private readonly ThreadPoolClass<T> ThreadPool;

        //Constructor seems fine no complaints
        public PooledThreadClass(ThreadPoolClass<T> pool)
        {
            ThreadPool = pool;
        }

        public void DestroyThread()
        {
            ReigniteThread();
            ewh.Set();
        }

        public void PrepareThread()
        {
            bIsAlive = true;
            InternalThread = new Thread(ThreadLoop);
            InternalThread.Start();
        }

        public void ReigniteThread()
        {
            bIsIdle = false;
            ewh.Set();
        }

        //WHY DID YOU DO THIS? JUST MAKE Result PUBLIC DUMBASS -Author of code (Donovan)
        public T GetReturn()
        {
            return Result;
        }

        //Finds a task and uses it unless there are no more tasks then just returns
        /// <summary>
        /// Loop the thread takes
        /// </summary>
        /// 
        public void ThreadLoop()
        {
            ThreadTaskRequest<T> Current_Task;
            bIsAlive = true;
            while(ThreadPool.bIsRunning)
            {
                Current_Task = null;

                lock (ThreadPool.TaskListLock)
                {
                    if (ThreadPool.TasksRemaining.Count > 0)
                    {
                        Current_Task = ThreadPool.TasksRemaining[ThreadPool.TasksRemaining.Count - 1];
                        ThreadPool.TasksRemaining.RemoveAt(ThreadPool.TasksRemaining.Count - 1);
                    }
                    else
                    {
                        bIsIdle = true;
                    }
                }
                if (Current_Task != null)
                {
                    Result = Current_Task.Method();
                    lock (ThreadPool.ReturnValueLock)
                    {
                        ThreadPool.Return_Values[Current_Task.ID] = Result;
                    }
                    lock (ThreadPool.CompletedTaskLock)
                    {
                        ThreadPool.CompletedTasks.Add(Current_Task);
                    }
                }
                if (bIsIdle || bIsPaused)
                {
                    // Blocks the current thread until it is enabled
                    // ewh.WaitOne();
                    continue;
                }
            }
            bIsAlive = false;
        }
    }
}
