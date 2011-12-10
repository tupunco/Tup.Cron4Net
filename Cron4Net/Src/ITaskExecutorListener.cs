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
    /// <summary>
    /// <para>
    /// A TaskExecutorListener is notified with events from a {@link TaskExecutor}.
    /// You can add listeners to a TaskExecutor by calling its
    /// {@link TaskExecutor#addTaskExecutorListener(TaskExecutorListener)} method.
    /// </para>
    /// @see TaskExecutor
    /// @author Carlo Pelliccia
    /// @since 2.0
    /// </summary>
    public interface ITaskExecutorListener
    {

        /**
         * Called when the execution has been requested to be paused.
         * 
         * @param executor
         *            The source executor.
         */
        void ExecutionPausing(TaskExecutor executor);

        /**
         * Called when the execution has been requested to be resumed.
         * 
         * @param executor
         *            The source executor.
         */
        void ExecutionResuming(TaskExecutor executor);

        /**
         * Called when the executor has been requested to be stopped.
         * 
         * @param executor
         *            The source executor.
         */
        void ExecutionStopping(TaskExecutor executor);

        /**
         * Called at execution end. If the execution has failed due to an error, the
         * encountered exception is reported.
         * 
         * @param executor
         *            The source executor.
         * @param exception
         *            If the execution has been terminated due to an error, this is
         *            the encountered exception; otherwise the parameter is null.
         */
        void ExecutionTerminated(TaskExecutor executor, ThrowableException exception);

        /**
         * Called every time the execution status message changes.
         * 
         * @param executor
         *            The source executor.
         * @param statusMessage
         *            The new status message.
         */
        void StatusMessageChanged(TaskExecutor executor, string statusMessage);

        /**
         * Called every time the execution completeness value changes.
         * 
         * @param executor
         *            The source executor.
         * @param completenessValue
         *            The new completeness value.
         */
        void CompletenessValueChanged(TaskExecutor executor, double completenessValue);
    }
}