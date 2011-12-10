/*
 * cron4j - A pure Java cron-like scheduler
 * 
 * Copyright (C) 2007-2010 Carlo Pelliccia (www.sauronsoftware.it)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License version
 * 2.1, as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License 2.1 for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License version 2.1 along with this program.
 * If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Threading;

namespace Tup.Cron4Net
{
    /**
     * <p>
     * Represents a task executor, which is something similar to threads.
     * </p>
     * <p>
     * Each time a task is launched, a new executor is spawned, executing and
     * watching the task
     * </p>
     * <p>
     * Alive task executors can be retrieved with the
     * {@link Scheduler#getExecutingTasks()} method, and they expose method to
     * control the ongoing execution.
     * </p>
     * 
     * @see Scheduler#getExecutingTasks()
     * @author Carlo Pelliccia
     * @since 2.0
     */
    public class TaskExecutor
    {
        /**
         * The scheduler whose this executor belongs to.
         */
        private Scheduler scheduler;

        /**
         * The executed task.
         */
        private Task task;

        /**
         * A task execution context.
         */
        private MyContext context;

        /**
         * A unique ID for this executor (used also as a lock object).
         */
        private string guid = GUIDGenerator.Generate();

        /**
         * An alternative to this (inner classes need it).
         */
        private TaskExecutor myself = null;

        /**
         * A list of {@link TaskExecutorListener} instances.
         */
        private List<ITaskExecutorListener> listeners = new List<ITaskExecutorListener>();

        /**
         * A time stamp reporting the start time of this thread.
         */
        private long startTime = -1;

        /**
         * The thread actually executing the task.
         */
        private Thread thread;

        /**
         * Is this executor paused now?
         */
        private bool paused = false;

        /**
         * Has been this executor stopped?
         */
        private bool stopped = false;

        /**
         * A lock object, for synchronization purposes.
         */
        private object lockObj = new object();

        /**
         * Builds the executor.
         * 
         * @param scheduler
         *            The scheduler whose this executor belongs to.
         * @param task
         *            The task that has to be executed.
         */
        internal TaskExecutor(Scheduler scheduler, Task task)
        {
            this.scheduler = scheduler;
            this.task = task;
            this.context = new MyContext(this);
            this.myself = this;
        }

        /**
         * Adds a listener to the executor.
         * 
         * @param listener
         *            The listener.
         */
        public void AddTaskExecutorListener(ITaskExecutorListener listener)
        {
            lock (listeners)
            {
                listeners.Add(listener);
            }
        }

        /**
         * Removes a listener from the executor.
         * 
         * @param listener
         *            The listener.
         */
        public void RemoveTaskExecutorListener(ITaskExecutorListener listener)
        {
            lock (listeners)
            {
                listeners.Remove(listener);
            }
        }

        /**
         * Returns an array containing any {@link TaskExecutorListener} previously
         * registered with the
         * {@link TaskExecutor#addTaskExecutorListener(TaskExecutorListener)}
         * method.
         * 
         * @return An array containing any {@link TaskExecutorListener} previously
         *         registered with the
         *         {@link TaskExecutor#addTaskExecutorListener(TaskExecutorListener)}
         *         method.
         */
        public ITaskExecutorListener[] GetTaskExecutorListeners()
        {
            lock (listeners)
            {
                return listeners.ToArray();
            }
        }

        /**
         * Returns a GUID for this executor.
         * 
         * @return A GUID for this executor.
         */
        public string GetGuid()
        {
            return guid;
        }

        /**
         * Returns the {@link Scheduler} instance whose this executor belongs to.
         * 
         * @return The scheduler.
         */
        public Scheduler GetScheduler()
        {
            return scheduler;
        }

        /**
         * Returns the representation of the executed task.
         * 
         * @return The executing/executed task.
         */
        public Task GetTask()
        {
            return task;
        }

        /**
         * Returns a time stamp reporting the start time of this executor, or a
         * value less than 0 if this executor has not been yet started.
         * 
         * @return A time stamp reporting the start time of this executor, or a
         *         value less than 0 if this executor has not been yet started.
         */
        public long GetStartTime()
        {
            return startTime;
        }

        /**
         * Checks whether this executor supports pausing.
         * 
         * @return true if this executor supports pausing.
         */
        public bool CanBePaused()
        {
            return task.CanBePaused();
        }

        /**
         * Checks whether this executor supports stopping.
         * 
         * @return true if this executor supports stopping.
         */
        public bool CanBeStopped()
        {
            return task.CanBeStopped();
        }

        /**
         * Checks whether this executor provides completeness tracking informations.
         * 
         * @return true if this executor provides completeness tracking
         *         informations.
         */
        public bool SupportsCompletenessTracking()
        {
            return task.SupportsCompletenessTracking();
        }

        /**
         * Checks whether this executor provides status tracking messages.
         * 
         * @return true if this executor provides status tracking messages.
         */
        public bool SupportsStatusTracking()
        {
            return task.SupportsStatusTracking();
        }

        /**
         * Starts executing the task (spawns a secondary thread).
         * 
         * @param daemon
         *            true to spawn a daemon thread; false otherwise.
         */
        internal void Start(bool daemon)
        {
            lock (lockObj)
            {
                startTime = System.DateTime.Now.Ticks;
                string name = "cron4j::scheduler[" + scheduler.getGuid() + "]::executor[" + guid + "]";
                thread = new Thread(new ThreadStart(new Runner(this).Run));
                thread.IsBackground = daemon;
                thread.Name = name;
                thread.Start();
            }
        }

        /**
         * Pauses the ongoing execution.
         * 
         * @throws UnsupportedOperationException
         *             The operation is not supported if
         *             {@link TaskExecutor#canBePaused()} returns <em>false</em>.
         */
        public void Pause()
        {
            if (!CanBePaused())
            {
                throw new NotSupportedException("Pause not supported");
            }
            lock (lockObj)
            {
                if (thread != null && !paused)
                {
                    NotifyExecutionPausing();
                    paused = true;
                }
            }
        }

        /**
         * Resumes the execution after it has been paused.
         */
        public void Resume()
        {
            lock (lockObj)
            {
                if (thread != null && paused)
                {
                    NotifyExecutionResuming();
                    paused = false;
                    Monitor.PulseAll(lockObj);
                }
            }
        }

        /**
         * Stops the ongoing execution.
         * 
         * @throws UnsupportedOperationException
         *             The operation is not supported if
         *             {@link TaskExecutor#canBeStopped()} returns <em>false</em>.
         */
        public void Stop()
        {
            if (!CanBeStopped())
            {
                throw new NotSupportedException("Stop not supported");
            }
            bool joinit = false;
            lock (lockObj)
            {
                if (thread != null && !stopped)
                {
                    stopped = true;
                    if (paused)
                    {
                        Resume();
                    }
                    NotifyExecutionStopping();
                    thread.Interrupt();
                    joinit = true;
                }
            }
            if (joinit)
            {
                do
                {
                    try
                    {
                        thread.Join();
                        break;
                    }
                    catch (ThreadInterruptedException e)
                    {
                        e = null;
                        continue;
                    }
                } while (true);
                thread = null;
            }
        }

        /**
         * Waits for this executor to die.
         * 
         * @throws InterruptedException
         *             If any thread has interrupted the current thread. The
         *             interrupted status of the current thread is cleared when this
         *             exception is thrown.
         */
        public void Join()
        {
            if (thread != null)
            {
                thread.Join();
            }
        }

        /**
         * Tests if this executor is alive. An executor is alive if it has been
         * started and has not yet died.
         * 
         * @return true if this executor is alive; false otherwise.
         */
        public bool IsAlive()
        {
            if (thread != null)
            {
                return thread.IsAlive;
            }
            else
            {
                return false;
            }
        }

        /**
         * Returns the current status message.
         * 
         * @return The current status message.
         * @throws UnsupportedOperationException
         *             The operation is not supported if
         *             {@link TaskExecutor#supportsStatusTracking()} returns
         *             <em>false</em>.
         */
        public string GetStatusMessage()
        {
            if (!SupportsStatusTracking())
            {
                throw new NotSupportedException("Status tracking not supported");
            }
            return context.GetStatusMessage();
        }

        /**
         * Returns the current completeness value, which is a value between 0 and 1.
         * 
         * @return The current completeness value, which is a value between 0 and 1.
         * @throws UnsupportedOperationException
         *             The operation is not supported if
         *             {@link TaskExecutor#supportsCompletenessTracking()} returns
         *             <em>false</em>.
         */
        public double GetCompleteness()
        {
            if (!SupportsCompletenessTracking())
            {
                throw new NotSupportedException("Completeness tracking not supported");
            }
            return context.GetCompleteness();
        }

        /**
         * Tests whether this executor has been paused.
         * 
         * @return true if this executor is paused; false otherwise.
         */
        public bool IsPaused()
        {
            return paused;
        }

        /**
         * Tests whether this executor has been stopped.
         * 
         * @return true if this executor is stopped; false otherwise.
         */
        public bool IsStopped()
        {
            return stopped;
        }

        /**
         * Notify registered listeners the execution has been paused.
         */
        private void NotifyExecutionPausing()
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.ExecutionPausing(this);
                }
            }
        }

        /**
         * Notify registered listeners the execution has been resumed.
         * 
         */
        private void NotifyExecutionResuming()
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.ExecutionResuming(this);
                }
            }
        }

        /**
         * Notify registered listeners the executor is stopping.
         */
        private void NotifyExecutionStopping()
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.ExecutionStopping(this);
                }
            }
        }

        /**
         * Notify registered listeners the execution has been terminated.
         * 
         * @param exception
         *            If the execution has been terminated due to an error, this is
         *            the encountered exception; otherwise the parameter is null.
         */
        private void NotifyExecutionTerminated(ThrowableException exception)
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.ExecutionTerminated(this, exception);
                }
            }
        }

        /**
         * Notify registered listeners the execution status message has changed.
         * 
         * @param statusMessage
         *            The new status message.
         */
        private void NotifyStatusMessageChanged(string statusMessage)
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.StatusMessageChanged(this, statusMessage);
                }
            }
        }

        /**
         * Notify registered listeners the execution completeness value has changed.
         * 
         * @param completenessValue
         *            The new completeness value.
         */
        private void NotifyCompletenessValueChanged(double completenessValue)
        {
            lock (listeners)
            {
                foreach (var l in listeners)
                {
                    l.CompletenessValueChanged(this, completenessValue);
                }
            }
        }

        /**
         * Inner Runnable class.
         */
        private class Runner : IRunnable
        {
            /// <summary>
            /// 
            /// </summary>
            private TaskExecutor cTaskExecutor = null;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="executor"></param>
            public Runner(TaskExecutor executor)
            {
                this.cTaskExecutor = executor;
            }

            /**
             * It implements {@link Thread#run()}, executing the wrapped task.
             */
            public void Run()
            {
                cTaskExecutor.startTime = System.DateTime.Now.Ticks;
                ThrowableException error = null;
                try
                {
                    // Notify.
                    cTaskExecutor.scheduler.notifyTaskLaunching(cTaskExecutor.myself);
                    // Task execution.
                    cTaskExecutor.task.Execute(cTaskExecutor.context);
                    // Succeeded.
                    cTaskExecutor.scheduler.notifyTaskSucceeded(cTaskExecutor.myself);
                }
                catch (Exception exception)
                {
                    // Failed.
                    error = new ThrowableException(exception);
                    cTaskExecutor.scheduler.notifyTaskFailed(cTaskExecutor.myself, error);
                }
                finally
                {
                    // Notify.
                    cTaskExecutor.NotifyExecutionTerminated(error);
                    cTaskExecutor.scheduler.notifyExecutorCompleted(cTaskExecutor.myself);
                }
            }
        }

        /**
         * Inner TaskExecutionHelper implementation.
         */
        private class MyContext
            : TaskExecutionContext
        {

            /**
             * Status message.
             */
            private string message = "";

            /**
             * Completeness value.
             */
            private double completeness = 0D;
            /// <summary>
            /// 
            /// </summary>
            private TaskExecutor cTaskExecutor = null;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="executor"></param>
            public MyContext(TaskExecutor executor)
            {
                this.cTaskExecutor = executor;
            }

            public Scheduler GetScheduler()
            {
                return cTaskExecutor.scheduler;
            }

            public TaskExecutor GetTaskExecutor()
            {
                return cTaskExecutor.myself;
            }

            public bool IsStopped()
            {
                return cTaskExecutor.stopped;
            }

            public void PauseIfRequested()
            {
                lock (cTaskExecutor.lockObj)
                {
                    if (cTaskExecutor.paused)
                    {
                        try
                        {
                            Monitor.Wait(cTaskExecutor.lockObj);
                        }
                        catch (ThreadInterruptedException e)
                        {
                            e = null;
                        }
                    }
                }
            }

            public void SetCompleteness(double completeness)
            {
                if (completeness >= 0D && completeness <= 1D)
                {
                    this.completeness = completeness;
                    cTaskExecutor.NotifyCompletenessValueChanged(completeness);
                }
            }

            public void SetStatusMessage(string message)
            {
                this.message = message != null ? message : "";
                cTaskExecutor.NotifyStatusMessageChanged(message);
            }

            /**
             * Returns the current status message.
             * 
             * @return The current status message.
             */
            public string GetStatusMessage()
            {
                return message;
            }

            /**
             * Returns the current completeness value, which is a value between 0
             * and 1.
             * 
             * @return The current completeness value, which is a value between 0
             *         and 1.
             */
            public double GetCompleteness()
            {
                return completeness;
            }
        }
    }
}
