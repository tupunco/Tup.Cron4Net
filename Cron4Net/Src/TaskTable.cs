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
    /**
     * <p>
     * A table coupling tasks with scheduling patterns.
     * </p>
     * 
     * @author Carlo Pelliccia
     * @since 2.0
     */
    public class TaskTable
    {
        /**
         * Pattern list.
         */
        private List<KeyValuePair<SchedulingPattern, Task>> patterns = new List<KeyValuePair<SchedulingPattern, Task>>();

        /**
         * Adds a task and an associated scheduling pattern to the table.
         * 
         * @param pattern
         *            The associated scheduling pattern.
         * @param task
         *            The task.
         */
        public void Add(SchedulingPattern pattern, Task task)
        {
            patterns.Add(new KeyValuePair<SchedulingPattern, Task>(pattern, task));
        }

        /**
         * Returns the size of the table, representing the number of the elements
         * stored in it.
         * 
         * @return The table size.
         */
        public int Size()
        {
            return patterns.Count;
        }

        /**
         * Returns the task at the specified position. Valid positions are between
         * <em>0</em> to <em>{@link TaskTable#size()} - 1</em>.
         * 
         * @param index
         *            The index.
         * @return The task at the specified position.
         * @throws IndexOutOfBoundsException
         *             If the supplied index is out of range.
         */
        public Task GetTask(int index)
        {
            return patterns[index].Value;
        }

        /**
         * Returns the scheduling pattern at the specified position. Valid positions
         * are between <em>0</em> to <em>{@link TaskTable#size()} - 1</em>.
         * 
         * @param index
         *            The index.
         * @return The scheduling pattern at the specified position.
         * @throws IndexOutOfBoundsException
         *             If the supplied index is out of range.
         */
        public SchedulingPattern GetSchedulingPattern(int index)
        {
            return patterns[index].Key;
        }

        /**
         * Remove a task from the table.
         * 
         * @param index
         *            The index of the task to remove.
         * @throws IndexOutOfBoundsException
         *             If the supplied index is not valid.
         * @since 2.1
         */
        public void Remove(int index)
        {
            patterns.RemoveAt(index);
        }
    }
}
