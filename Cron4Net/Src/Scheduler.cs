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
using System.IO;
using System.Threading;

namespace Tup.Cron4Net
{
    /**
     * <p>
     * The cron4j scheduler.
     * </p>
     * 
     * @author Carlo Pelliccia
     */
    public class Scheduler
    {

        /**
         * A GUID for this scheduler.
         */
        private string guid = GUIDGenerator.Generate();

        /**
         * The time zone applied by the scheduler.
         */
        private TimeSpan timezone = TimeSpan.Zero;

        /**
         * The daemon flag. If true the scheduler and its spawned threads acts like
         * daemons.
         */
        private bool daemon = false;

        /**
         * The state flag. If true the scheduler is started and running, otherwise
         * it is paused and no task is launched.
         */
        private bool started = false;

        /**
         * Registered {@link TaskCollector}s list.
         */
        private List<ITaskCollector> collectors = new List<ITaskCollector>();

        /**
         * The {@link MemoryTaskCollector} used for memory stored tasks. Represented
         * here for convenience, it is also the first element in the
         * {@link Scheduler#collectors} list.
         */
        private MemoryTaskCollector memoryTaskCollector = new MemoryTaskCollector();

        /**
         * The {@link FileTaskCollector} used for reading tasks from files.
         * Represented here for convenience, it is also the second element in the
         * {@link Scheduler#collectors} list.
         */
        private FileTaskCollector fileTaskCollector = new FileTaskCollector();

        /**
         * Registered {@link SchedulerListener}s list.
         */
        private List<SchedulerListener> listeners = new List<SchedulerListener>();

        /**
         * The thread checking the clock and requesting the spawning of launcher
         * threads.
         */
        private TimerThread timer = null;

        /**
         * Currently running {@link LauncherThread} instances.
         */
        private List<LauncherThread> launchers = null;

        /**
         * Currently running {@link TaskExecutor} instances.
         */
        private List<TaskExecutor> executors = null;

        /**
         * Internal lock, used to synchronize status-aware operations.
         */
        private object lockObj = new object();

        /**
         * It builds and prepares a brand new Scheduler instance.
         */
        public Scheduler()
        {
            collectors.Add(memoryTaskCollector);
            collectors.Add(fileTaskCollector);
        }

        /**
         * It returns the GUID for this scheduler.
         * 
         * @return The GUID for this scheduler.
         */
        public Object getGuid()
        {
            return guid;
        }

        /**
         * <p>
         * Sets the time zone applied by the scheduler.
         * </p>
         * <p>
         * Current system time is adapted to the supplied time zone before comparing
         * it with registered scheduling patterns. The result is that any supplied
         * scheduling pattern is treated according to the specified time zone. In
         * example, suppose:
         * </p>
         * <ul>
         * <li>System time: 10:00</li>
         * <li>System time zone: GMT+1</li>
         * <li>Scheduler time zone: GMT+3</li>
         * </ul>
         * <p>
         * The scheduler, before comparing system time with patterns, translates
         * 10:00 from GMT+1 to GMT+3. It means that 10:00 becomes 12:00. The
         * resulted time is then used by the scheduler to activate tasks. So, in the
         * given configuration at the given moment, any task scheduled as
         * <em>0 12 * * *</em> will be executed, while any <em>0 10 * * *</em> will
         * not.
         * </p>
         * 
         * @param timezone
         *            The time zone applied by the scheduler.
         */
        public void setTimeZone(TimeSpan timezone)
        {
            lock (lockObj)
            {
                this.timezone = timezone;
            }
        }

        /**
         * Returns the time zone applied by the scheduler.
         * 
         * @return The time zone applied by the scheduler.
         */
        public TimeSpan getTimeZone()
        {
            lock (lockObj)
            {
                return timezone != TimeSpan.Zero ? timezone : TimeSpan.FromTicks(0L);
            }
        }

        /**
         * Tests whether this scheduler is a daemon scheduler.
         * 
         * @return true if this scheduler is a daemon scheduler; false otherwise.
         */
        public bool isDaemon()
        {
            lock (lockObj)
            {
                return daemon;
            }
        }

        /**
         * Marks this scheduler daemon flag. When a scheduler is marked as a daemon
         * scheduler it spawns only daemon threads. The Java Virtual Machine exits
         * when the only threads running are all daemon threads.
         * 
         * This method must be called before the scheduler is started.
         * 
         * @param on
         *            If true, the scheduler will spawn only daemon threads.
         * @throws IllegalStateException
         *             If the scheduler is started.
         */
        public void setDaemon(bool on)
        {
            lock (lockObj)
            {
                if (started)
                {
                    throw new SystemException("Scheduler already started");
                }
                this.daemon = on;
            }
        }

        /**
         * Tests if this scheduler is started.
         * 
         * @return true if the scheduler is started, false if it is stopped.
         */
        public bool isStarted()
        {
            lock (lockObj)
            {
                return started;
            }
        }

        /**
         * Adds a {@link File} instance to the scheduler. Every minute the file will
         * be parsed. The scheduler will execute any declared task whose scheduling
         * pattern matches the current system time.
         * 
         * See {@link CronParser} documentation for informations about the file
         * contents syntax.
         * 
         * @param file
         *            The {@link File} instance.
         */
        public void scheduleFile(FileInfo file)
        {
            fileTaskCollector.addFile(file);
        }

        /**
         * Removes a {@link File} instance previously scheduled with the
         * {@link Scheduler#scheduleFile(File)} method.
         * 
         * @param file
         *            The {@link File} instance.
         */
        public void descheduleFile(FileInfo file)
        {
            fileTaskCollector.removeFile(file);
        }

        /**
         * Returns an array containing any {@link File} previously scheduled with
         * the {@link Scheduler#scheduleFile(File)} method.
         * 
         * @return An array containing any {@link File} previously scheduled with
         *         the {@link Scheduler#scheduleFile(File)} method.
         */
        public FileInfo[] getScheduledFiles()
        {
            return fileTaskCollector.getFiles();
        }

        /**
         * Adds a custom {@link TaskCollector} instance to the scheduler. The
         * supplied object, once added to the scheduler, will be query every minute
         * for its task list. The scheduler will execute any of the returned tasks
         * whose scheduling pattern matches the current system time.
         * 
         * @param collector
         *            The custom {@link TaskCollector} instance.
         */
        public void addTaskCollector(ITaskCollector collector)
        {
            lock (collectors)
            {
                collectors.Add(collector);
            }
        }

        /**
         * Removes a previously registered custom {@link TaskCollector} instance.
         * 
         * @param collector
         *            The custom {@link TaskCollector} instance.
         */
        public void removeTaskCollector(ITaskCollector collector)
        {
            lock (collectors)
            {
                collectors.Remove(collector);
            }
        }

        /**
         * Returns an array containing any custom {@link TaskCollector} instance
         * previously registered in the scheduler with the
         * {@link Scheduler#addTaskCollector(TaskCollector)} method.
         * 
         * @return An array containing any custom {@link TaskCollector} instance
         *         previously registered in the scheduler with the
         *         {@link Scheduler#addTaskCollector(TaskCollector)} method.
         */
        public ITaskCollector[] getTaskCollectors()
        {
            lock (collectors)
            {
                // Discard the first 2 elements in the list.
                int size = collectors.Count - 2;
                ITaskCollector[] ret = new ITaskCollector[size];
                for (int i = 0; i < size; i++)
                {
                    ret[i] = (ITaskCollector)collectors[i + 2];
                }
                return ret;
            }
        }

        /**
         * Adds a {@link SchedulerListener} to the scheduler. A
         * {@link SchedulerListener} is notified every time a task is launching, has
         * succeeded or has failed.
         * 
         * @param listener
         *            The listener.
         */
        public void addSchedulerListener(SchedulerListener listener)
        {
            lock (listeners)
            {
                listeners.Add(listener);
            }
        }

        /**
         * Removes a {@link SchedulerListener} previously registered with the
         * {@link Scheduler#addSchedulerListener(SchedulerListener)} method.
         * 
         * @param listener
         *            The listener.
         */
        public void removeSchedulerListener(SchedulerListener listener)
        {
            lock (listeners)
            {
                listeners.Remove(listener);
            }
        }

        /**
         * Returns an array containing any {@link SchedulerListener} previously
         * registered with the
         * {@link Scheduler#addSchedulerListener(SchedulerListener)} method.
         * 
         * @return An array containing any {@link SchedulerListener} previously
         *         registered with the
         *         {@link Scheduler#addSchedulerListener(SchedulerListener)} method.
         */
        public SchedulerListener[] getSchedulerListeners()
        {
            lock (listeners)
            {
                return listeners.ToArray();
            }
        }

        /**
         * Returns an array containing any currently executing task, in the form of
         * {@link TaskExecutor} objects. Each running task is executed by a
         * different thread. A {@link TaskExecutor} object allows the control of the
         * running task. The inner {@link Task} representation could be retrieved,
         * the status of the task could be detected and the thread could be
         * interrupted using any standard {@link Thread} method (
         * {@link Thread#interrupt()}, {@link Thread#isAlive() etc}.
         * 
         * @return An array containing any currently executing task, in the form of
         *         {@link TaskExecutor} objects.
         */
        public TaskExecutor[] getExecutingTasks()
        {
            lock (executors)
            {
                return executors.ToArray();
            }
        }

        /**
         * This method schedules a task execution.
         * 
         * @param schedulingPattern
         *            The scheduling pattern for the task.
         * @param task
         *            The task, as a plain Runnable object.
         * @return The task auto-generated ID assigned by the scheduler. This ID can
         *         be used later to reschedule and deschedule the task, and also to
         *         retrieve informations about it.
         * @throws InvalidPatternException
         *             If the supplied pattern is not valid.
         */
        public string schedule(string schedulingPattern, IRunnable task)
        {
            if (String.IsNullOrEmpty(schedulingPattern))
                throw new ArgumentException("schedulingPattern is null or empty.", "schedulingPattern");
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");

            return schedule(schedulingPattern, new RunnableTask(task));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schedulingPattern"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public string schedule(string schedulingPattern, Action<TaskExecutionContext> task)
        {
            if (String.IsNullOrEmpty(schedulingPattern))
                throw new ArgumentException("schedulingPattern is null or empty.", "schedulingPattern");
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");

            return schedule(schedulingPattern, new RunnableTask(task));
        }
        /**
         * This method schedules a task execution.
         * 
         * @param schedulingPattern
         *            The scheduling pattern for the task.
         * @param task
         *            The task, as a plain Runnable object.
         * @return The task auto-generated ID assigned by the scheduler. This ID can
         *         be used later to reschedule and deschedule the task, and also to
         *         retrieve informations about it.
         * @throws InvalidPatternException
         *             If the supplied pattern is not valid.
         * @since 2.0
         */
        public string schedule(string schedulingPattern, Task task)
        {
            if (String.IsNullOrEmpty(schedulingPattern))
                throw new ArgumentException("schedulingPattern is null or empty.", "schedulingPattern");
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");

            return schedule(new SchedulingPattern(schedulingPattern), task);
        }

        /**
         * This method schedules a task execution.
         * 
         * @param schedulingPattern
         *            The scheduling pattern for the task.
         * @param task
         *            The task, as a plain Runnable object.
         * @return The task auto-generated ID assigned by the scheduler. This ID can
         *         be used later to reschedule and deschedule the task, and also to
         *         retrieve informations about it.
         * @since 2.0
         */
        public string schedule(SchedulingPattern schedulingPattern, Task task)
        {
            if (schedulingPattern == null)
                throw new ArgumentNullException("schedulingPattern", "schedulingPattern is null.");
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");

            return memoryTaskCollector.Add(schedulingPattern, task);
        }

        /**
         * This method changes the scheduling pattern of a task.
         * 
         * @param id
         *            The ID assigned to the previously scheduled task.
         * @param schedulingPattern
         *            The new scheduling pattern for the task.
         * @throws InvalidPatternException
         *             If the supplied pattern is not valid.
         * @deprecated Use {@link Scheduler#reschedule(String, String)}.
         */
        public void reschedule(object id, string schedulingPattern)
        {            
            reschedule((string)id, new SchedulingPattern(schedulingPattern));
        }

        /**
         * This method changes the scheduling pattern of a task.
         * 
         * @param id
         *            The ID assigned to the previously scheduled task.
         * @param schedulingPattern
         *            The new scheduling pattern for the task.
         * @throws InvalidPatternException
         *             If the supplied pattern is not valid.
         */
        public void reschedule(string id, string schedulingPattern)
        {            
            reschedule(id, new SchedulingPattern(schedulingPattern));
        }

        /**
         * This method changes the scheduling pattern of a task.
         * 
         * @param id
         *            The ID assigned to the previously scheduled task.
         * @param schedulingPattern
         *            The new scheduling pattern for the task.
         * @since 2.0
         */
        public void reschedule(string id, SchedulingPattern schedulingPattern)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("id is null or empty.", "id");
            if (schedulingPattern == null)
                throw new ArgumentNullException("schedulingPattern", "schedulingPattern is null.");
            
            memoryTaskCollector.Update(id, schedulingPattern);
        }

        /**
         * This methods cancels the scheduling of a task.
         * 
         * @param id
         *            The ID of the task.
         * @deprecated Use {@link Scheduler#deschedule(String)}.
         */
        public void deschedule(object id)
        {
            deschedule((string)id);
        }

        /**
         * This methods cancels the scheduling of a task.
         * 
         * @param id
         *            The ID of the task.
         */
        public void deschedule(string id)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("id is null or empty.", "id");
            
            memoryTaskCollector.Remove(id);
        }

        /**
         * This method retrieves a previously scheduled task.
         * 
         * @param id
         *            The task ID.
         * @return The requested task, or null if the task was not found.
         * @since 2.0
         */
        public Task getTask(string id)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("id is null or empty.", "id");
           
            return memoryTaskCollector.GetTask(id);
        }

        /**
         * This method retrieves a previously scheduled task scheduling pattern.
         * 
         * @param id
         *            The task ID.
         * @return The requested scheduling pattern, or null if the task was not
         *         found.
         * @since 2.0
         */
        public SchedulingPattern getSchedulingPattern(string id)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("id is null or empty.", "id");
            
            return memoryTaskCollector.GetSchedulingPattern(id);
        }

        /**
         * This method retrieves the Runnable object of a previously scheduled task.
         * 
         * @param id
         *            The task ID.
         * @return The Runnable object of the task, or null if the task was not
         *         found.
         * @deprecated Use {@link Scheduler#getTask(String)}.
         */
        public IRunnable getTaskRunnable(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id", "id is null.");
            
            Task task = getTask((string)id);
            if (task is RunnableTask)
            {
                RunnableTask rt = (RunnableTask)task;
                return rt.GetRunnable();
            }
            else
            {
                return null;
            }
        }

        /**
         * This method retrieves the scheduling pattern of a previously scheduled
         * task.
         * 
         * @param id
         *            The task ID.
         * @return The scheduling pattern of the task, or null if the task was not
         *         found.
         * @deprecated Use {@link Scheduler#getSchedulingPattern(String)}.
         */
        public string getTaskSchedulingPattern(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id", "id is null.");
            
            return getSchedulingPattern((string)id).ToString();
        }

        /**
         * Executes immediately a task, without scheduling it.
         * 
         * @param task
         *            The task.
         * @return The {@link TaskExecutor} executing the given task.
         * @throws IllegalStateException
         *             If the scheduler is not started.
         */
        public TaskExecutor launch(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");
            
            lock (lockObj)
            {
                if (!started)
                {
                    throw new SystemException("Scheduler not started");
                }
                return spawnExecutor(task);
            }
        }

        /**
         * This method starts the scheduler. When the scheduled is started the
         * supplied tasks are executed at the given moment.
         * 
         * @throws IllegalStateException
         *             Thrown if this scheduler is already started.
         */
        public void start()
        {
            lock (lockObj)
            {
                if (started)
                {
                    throw new SystemException("Scheduler already started");
                }
                // Initializes required lists.
                launchers = new List<LauncherThread>();
                executors = new List<TaskExecutor>();
                // Starts the timer thread.
                timer = new TimerThread(this);
                timer.SetDaemon(daemon);
                timer.Start();
                // Change the state of the scheduler.
                started = true;
            }
        }

        /**
         * This method stops the scheduler execution. Before returning, it waits the
         * end of all the running tasks previously launched. Once the scheduler has
         * been stopped it can be started again with a start() call.
         * 
         * @throws IllegalStateException
         *             Thrown if this scheduler is not started.
         */
        public void stop()
        {
            lock (lockObj)
            {
                if (!started)
                {
                    throw new SystemException("Scheduler not started");
                }
                // Interrupts the timer and waits for its death.
                timer.Interrupt();
                tillThreadDies(timer.Thread);
                timer = null;
                // Interrupts any running launcher and waits for its death.
                for (; ; )
                {
                    LauncherThread launcher = null;
                    lock (launchers)
                    {
                        if (launchers.Count == 0)
                        {
                            break;
                        }
                        launcher = launchers[0];
                        launchers.RemoveAt(0);
                    }
                    launcher.Interrupt();
                    tillThreadDies(launcher.Thread);
                }
                launchers = null;
                // Interrupts any running executor and waits for its death.
                // Before exiting wait for all the active tasks end.
                for (; ; )
                {
                    TaskExecutor executor = null;
                    lock (executors)
                    {
                        if (executors.Count == 0)
                        {
                            break;
                        }
                        executor = executors[0];
                        executors.RemoveAt(0);
                    }
                    if (executor.CanBeStopped())
                    {
                        executor.Stop();
                    }
                    tillExecutorDies(executor);
                }
                executors = null;
                // Change the state of the object.
                started = false;
            }
        }

        // -- PACKAGE RESERVED METHODS --------------------------------------------

        /**
         * Starts a launcher thread.
         * 
         * @param referenceTimeInMillis
         *            Reference time in millis for the launcher.
         * @return The spawned launcher.
         */
        internal LauncherThread spawnLauncher(long referenceTimeInMillis)
        {
            ITaskCollector[] nowCollectors;
            lock (collectors)
            {
                nowCollectors = collectors.ToArray();
            }
            LauncherThread l = new LauncherThread(this, nowCollectors,
                    referenceTimeInMillis);
            lock (launchers)
            {
                launchers.Add(l);
            }
            l.setDaemon(daemon);
            l.Start();
            return l;
        }

        /**
         * Starts the given task within a task executor.
         * 
         * @param task
         *            The task.
         * @return The spawned task executor.
         */
        internal TaskExecutor spawnExecutor(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task", "task is null.");
            
            TaskExecutor e = new TaskExecutor(this, task);
            lock (executors)
            {
                executors.Add(e);
            }
            e.Start(daemon);
            return e;
        }

        /**
         * This method is called by a launcher thread to notify that the execution
         * is completed.
         * 
         * @param launcher
         *            The launcher which has completed its task.
         */
        internal void notifyLauncherCompleted(LauncherThread launcher)
        {
            if (launcher == null)
                throw new ArgumentNullException("launcher", "launcher is null.");
            
            lock (launchers)
            {
                launchers.Remove(launcher);
            }
        }

        /**
         * This method is called by a task executor to notify that the execution is
         * completed.
         * 
         * @param executor
         *            The executor which has completed its task.
         */
        internal void notifyExecutorCompleted(TaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor", "executor is null.");
            
            lock (executors)
            {
                executors.Remove(executor);
            }
        }

        /**
         * Notifies every registered listener that a task is going to be launched.
         * 
         * @param executor
         *            The task executor.
         */
        internal void notifyTaskLaunching(TaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor", "executor is null.");
            
            lock (listeners)
            {
                int size = listeners.Count;
                for (int i = 0; i < size; i++)
                {
                    SchedulerListener l = (SchedulerListener)listeners[i];
                    l.TaskLaunching(executor);
                }
            }
        }

        /**
         * Notifies every registered listener that a task execution has successfully
         * completed.
         * 
         * @param executor
         *            The task executor.
         */
        internal void notifyTaskSucceeded(TaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor", "executor is null.");
            
            lock (listeners)
            {
                int size = listeners.Count;
                for (int i = 0; i < size; i++)
                {
                    SchedulerListener l = (SchedulerListener)listeners[i];
                    l.TaskSucceeded(executor);
                }
            }
        }

        /**
         * Notifies every registered listener that a task execution has failed due
         * to an uncaught exception.
         * 
         * @param executor
         *            The task executor.
         * @param exception
         *            The exception.
         */
        internal void notifyTaskFailed(TaskExecutor executor, ThrowableException exception)
        {
            if (executor == null)
                throw new ArgumentNullException("executor", "executor is null.");
            if (exception == null)
                throw new ArgumentNullException("exception", "exception is null.");
            
            lock (listeners)
            {
                int size = listeners.Count;
                if (size > 0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        SchedulerListener l = (SchedulerListener)listeners[i];
                        l.TaskFailed(executor, exception);
                    }
                }
                else
                {
                    // Logs on console if no one has been notified about it.
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        //// -- PRIVATE METHODS -----------------------------------------------------

        /**
         * It waits until the given thread is dead. It is similar to
         * {@link Thread#join()}, but this one avoids {@link ThreadInterruptedException}
         * instances.
         * 
         * @param thread
         *            The thread.
         */
        private void tillThreadDies(Thread thread)
        {
            if (thread == null)
                throw new ArgumentNullException("thread", "thread is null.");
            
            bool dead = false;
            do
            {
                try
                {
                    thread.Join();
                    dead = true;
                }
                catch (ThreadInterruptedException e)
                {
                    ;
                }
            } while (!dead);
        }

        /**
         * It waits until the given task executor is dead. It is similar to
         * {@link TaskExecutor#join()}, but this one avoids
         * {@link ThreadInterruptedException} instances.
         * 
         * @param executor
         *            The task executor.
         */
        private void tillExecutorDies(TaskExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor", "executor is null.");
            
            bool dead = false;
            do
            {
                try
                {
                    executor.Join();
                    dead = true;
                }
                catch (ThreadInterruptedException e)
                {
                    ;
                }
            } while (!dead);
        }
    }
}