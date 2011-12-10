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

namespace Tup.Cron4Net
{
    /**
     * <p>
     * A {@link TaskCollector} implementation, reading the task list from a group of
     * files.
     * </p>
     * 
     * @author Carlo Pelliccia
     * @since 2.0
     */
    class FileTaskCollector : ITaskCollector
    {

        /**
         * File list.
         */
        private List<FileInfo> files = new List<FileInfo>();

        /**
         * Adds a file.
         * 
         * @param file
         *            The file.
         */
        public void addFile(FileInfo file)
        {
            lock (files)
            {
                files.Add(file);
            }
        }

        /**
         * Removes a file.
         * 
         * @param file
         *            The file.
         */
        public void removeFile(FileInfo file)
        {
            lock (files)
            {
                files.Remove(file);
            }
        }

        /**
         * Returns the file list.
         * 
         * @return The file list.
         */
        public FileInfo[] getFiles()
        {
            lock (files)
            {

                return files.ToArray();
            }
        }

        /**
         * Implements {@link TaskCollector#getTasks()}.
         */
        public TaskTable GetTasks()
        {
            TaskTable ret = new TaskTable();
            lock (files)
            {
                int size = files.Count;
                for (int i = 0; i < size; i++)
                {
                    FileInfo f = (FileInfo)files[i];
                    TaskTable aux = null;
                    try
                    {
                        aux = CronParser.parse(f);
                    }
                    catch (IOException e)
                    {
                        Exception e1 = new Exception("Cannot parse cron file: "
                                + f.FullName, e);

                        Console.WriteLine(e1.ToString());
                    }
                    if (aux != null)
                    {
                        int auxSize = aux.Size();
                        for (int j = 0; j < auxSize; j++)
                        {
                            ret.Add(aux.GetSchedulingPattern(j), aux.GetTask(j));
                        }
                    }
                }
            }
            return ret;
        }
    }
}
