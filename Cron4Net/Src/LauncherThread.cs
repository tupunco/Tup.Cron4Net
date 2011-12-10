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
     * LauncherThreads are used by {@link Scheduler} instances. A LauncherThread
     * retrieves a list of task from a set of {@link TaskCollector}s. Then it
     * launches, within a separate {@link TaskExecutor}, every retrieved task whose
     * scheduling pattern matches the given reference time.
     * 
     * @author Carlo Pelliccia
     * @since 2.0
     */
    class LauncherThread
    {

        /**
         * A GUID for this object.
         */
        private string guid = GUIDGenerator.Generate();

        internal Thread Thread = null;

        /**
         * The owner scheduler.
         */
        private Scheduler scheduler;

        /**
         * Task collectors, used to retrieve registered tasks.
         */
        private ITaskCollector[] collectors;

        /**
         * A reference time for task launching.
         */
        private long referenceTimeInMillis;

        /**
         * Builds the launcher.
         * 
         * @param scheduler
         *            The owner scheduler.
         * @param collectors
         *            Task collectors, used to retrieve registered tasks.
         * @param referenceTimeInMillis
         *            A reference time for task launching.
         */
        public LauncherThread(Scheduler scheduler, ITaskCollector[] collectors,
                long referenceTimeInMillis)
        {
            this.scheduler = scheduler;
            this.collectors = collectors;
            this.referenceTimeInMillis = referenceTimeInMillis;
            // Thread name.
            string name = "cron4j::scheduler[" + scheduler.getGuid()
                    + "]::launcher[" + guid + "]";
            Thread = new Thread(new ThreadStart(Run));
            Thread.Name = name;
        }

        /**
         * Returns the GUID for this object.
         * 
         * @return The GUID for this object.
         */
        public object GetGuid()
        {
            return guid;
        }
        public void Interrupt()
        {
            Thread.Interrupt();
        }
        public void setDaemon(bool daemon)
        {
            Thread.IsBackground = daemon;
        }
        public void Start()
        {
            Thread.Start();
        }
        /**
         * Overrides {@link Thread#run()}.
         */
        public void Run()
        {
            //outer:
            for (int i = 0; i < collectors.Length; i++)
            {
                TaskTable taskTable = collectors[i].GetTasks();
                int size = taskTable.Size();
                for (int j = 0; j < size; j++)
                {
                    //if (cThread.isInterrupted()) {
                    //    goto outer;
                    //}
                    SchedulingPattern pattern = taskTable.GetSchedulingPattern(j);
                    if (pattern.Match(scheduler.getTimeZone(),
                            referenceTimeInMillis))
                    {
                        Task task = taskTable.GetTask(j);
                        scheduler.spawnExecutor(task);
                    }
                }
            }
            // Notifies completed.
            scheduler.notifyLauncherCompleted(this);
        }
    }
}
