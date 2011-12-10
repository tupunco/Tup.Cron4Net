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
     * TimeThreads are used by {@link Scheduler} instances. A TimerThread spends
     * most of the time sleeping. It wakes up every minute and it requests to the
     * scheduler the spawning of a {@link LauncherThread}.
     * </p>
     * 
     * @author Carlo Pelliccia
     * @since 2.0
     */
    class TimerThread
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
         * Builds the timer thread.
         * 
         * @param scheduler
         *            The owner scheduler.
         */
        public TimerThread(Scheduler scheduler)
        {
            this.scheduler = scheduler;
            // Thread name.
            string name = "cron4j::scheduler[" + scheduler.getGuid() + "]::timer[" + guid + "]";
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
        public void SetDaemon(bool daemon)
        {
            Thread.IsBackground = daemon;
        }
        public void Start()
        {
            Thread.Start();
        }
        /**
         * It has been reported that the {@link Thread#sleep(long)} method sometimes
         * exits before the requested time has passed. This one offers an
         * alternative that sometimes could sleep a few millis more than requested,
         * but never less.
         * 
         * @param millis
         *            The length of time to sleep in milliseconds.
         * @throws InterruptedException
         *             If another thread has interrupted the current thread. The
         *             <i>interrupted status</i> of the current thread is cleared
         *             when this exception is thrown.
         * @see Thread#sleep(long)
         */
        private void SafeSleep(double millis)
        {
            Thread.Sleep((int)millis);
        }

        /**
         * Overrides {@link Thread#run()}.
         */
        public void Run()
        {
            // What time is it?
            var millis = System.DateTime.Now;
            // Calculating next minute.
            var nextMinute = millis.AddMinutes(1).AddSeconds(-1 * millis.Second + 1);
            // Work until the scheduler is started.
            for (; ; )
            {
                // Coffee break 'till next minute comes!
                var sleepTime = (nextMinute - System.DateTime.Now).TotalMilliseconds;
                if (sleepTime > 0)
                {
                    try
                    {
                        SafeSleep(sleepTime);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        // Must exit!
                        break;
                    }
                }
                // What time is it?
                millis = System.DateTime.Now;
                // Launching the launching thread!
                scheduler.spawnLauncher(millis.Ticks);
                // Calculating next minute.
                nextMinute = millis.AddMinutes(1);
            }
            // Discard scheduler reference.
            scheduler = null;
        }

    }
}