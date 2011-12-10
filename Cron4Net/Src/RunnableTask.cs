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
namespace Tup.Cron4Net
{
    /**
     * <p>
     * A {@link Task} implementation acting as a wrapper around a {@link Runnable}
     * object.
     * </p>
     * 
     * @author Carlo Pelliccia
     * @since 2.0
     */
    class RunnableTask : Task
    {
        /**
         * The wrapped runnable object.
         */
        private IRunnable _runnable;
        private Action<TaskExecutionContext> _actionRunnable;

        /**
         * Builds the task.
         * 
         * @param runnable
         *            The wrapped Runnable object.
         * @throws InvalidPatternException
         *             If the supplied pattern is not valid.
         */
        public RunnableTask(IRunnable runnable)
        {
            if (runnable == null)
                throw new ArgumentNullException("runnable", "runnable is null.");

            this._runnable = runnable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="runnable"></param>
        public RunnableTask(Action<TaskExecutionContext> runnable)
        {
            if (runnable == null)
                throw new ArgumentNullException("runnable", "runnable is null.");

            this._actionRunnable = runnable;
        }

        /**
         * Returns the wrapped Runnable object.
         * 
         * @return The wrapped Runnable object.
         */
        public IRunnable GetRunnable()
        {
            return _runnable;
        }

        /**
         * Implements {@link Task#execute(TaskExecutionContext)}, launching the
         * {@link Runnable#run()} method on the wrapped object.
         */
        public override void Execute(TaskExecutionContext context)
        {
            if (_runnable != null)
                _runnable.Run();
            else
                _actionRunnable(context);
        }

        /**
         * Overrides {@link Object#toString()}.
         */
        public override string ToString()
        {
            return string.Format("Task[runnable={0}]", _runnable);
        }
    }
}
