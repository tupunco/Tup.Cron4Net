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

namespace Tup.Cron4Net
{
    /// <summary>
    /// A {@link TaskCollector} implementation managing a task list in memory.
    /// 
    /// @author Carlo Pelliccia
    /// @since 2.0
    /// </summary>
    class MemoryTaskCollector
        : ITaskCollector
    {

        /// <summary>
        /// The inner scheduling pattern list/task list.
        /// IDs for task-pattern couples.
        /// </summary>
        private Dictionary<string, KeyValuePair<SchedulingPattern, Task>> patterns = new Dictionary<string, KeyValuePair<SchedulingPattern, Task>>();

        /// <summary>
        /// Counts how many task are currently collected by this collector.
        /// </summary>
        /// <returns>The size of the currently collected task list.</returns>
        public int Size()
        {
            lock (patterns)
            {
                return patterns.Count;
            }
        }

        /**
         * Adds a pattern and a task to the collector.
         * 
         * @param pattern
         *            The scheduling pattern.
         * @param task
         *            The task.
         * @return An ID for the scheduled operation.
         */
        public string Add(SchedulingPattern pattern, Task task)
        {
            string id = GUIDGenerator.Generate();
            lock (patterns)
            {
                patterns.Add(id, new KeyValuePair<SchedulingPattern, Task>(pattern, task));
            }
            return id;
        }

        /**
         * Updates a scheduling pattern in the collector.
         * 
         * @param id
         *            The ID of the scheduled couple.
         */
        public void Update(string id, SchedulingPattern pattern)
        {
            KeyValuePair<SchedulingPattern, Task> v;
            lock (patterns)
            {
                if (patterns.TryGetValue(id, out  v))
                {
                    patterns[id] = new KeyValuePair<SchedulingPattern, Task>(pattern, v.Value);
                }
            }
        }

        /**
         * Removes a task and its scheduling pattern from the collector.
         * 
         * @param id
         *            The ID of the scheduled couple.
         */
        public void Remove(string id)
        {
            lock (patterns)
            {
                patterns.Remove(id);
            }
        }

        /**
         * Retrieves a task from the collector.
         * 
         * @param id
         *            The ID of the scheduled couple.
         * @return The task with the specified assigned ID, or null if it doesn't
         *         exist.
         */
        public Task GetTask(string id)
        {
            KeyValuePair<SchedulingPattern, Task> v;
            lock (patterns)
            {
                if (patterns.TryGetValue(id, out  v))
                    return v.Value;
                else
                    return null;
            }
        }

        /**
         * Retrieves a scheduling pattern from the collector.
         * 
         * @param id
         *            The ID of the scheduled couple.
         * @return The scheduling pattern with the specified assigned ID, or null if
         *         it doesn't exist.
         */
        public SchedulingPattern GetSchedulingPattern(string id)
        {
            KeyValuePair<SchedulingPattern, Task> v;
            lock (patterns)
            {
                if (patterns.TryGetValue(id, out  v))
                    return v.Key;
                else
                    return null;
            }
        }

        /**
         * Implements {@link TaskCollector#getTasks()}.
         */
        public TaskTable GetTasks()
        {
            TaskTable ret = new TaskTable();
            lock (patterns)
            {
                foreach (var item in patterns.Values)
                    ret.Add(item.Key, item.Value);
            }
            return ret;
        }
    }
}