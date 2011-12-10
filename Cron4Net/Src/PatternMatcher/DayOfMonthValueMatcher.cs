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
    /// A ValueMatcher whose rules are in a plain array of int values. When asked
    /// to validate a value, this ValueMatcher checks if it is in the array and, if
    /// not, checks whether the last-day-of-month setting applies.
    /// </summary>
    /// <remarks>
    /// @author Paul Fernley
    /// </remarks>
    class DayOfMonthValueMatcher
        : IntArrayValueMatcher
    {
        private static readonly int[] lastDays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        /// <summary>
        /// Builds the ValueMatcher.
        /// </summary>
        /// <param name="ints">
        /// An ArrayList of int elements, one for every value accepted
        /// by the matcher. The match() method will return true only if
        /// its parameter will be one of this list or the
        /// last-day-of-month setting applies.
        /// </param>
        public DayOfMonthValueMatcher(List<int> ints)
            : base(ints)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="month"></param>
        /// <param name="isLeapYear"></param>
        /// <returns>
        /// Returns true if the given value is included in the matcher list or the
        /// last-day-of-month setting applies.
        /// </returns>
        public bool Match(int value, int month, bool isLeapYear)
        {
            return (base.Match(value) ||
                        (value > 27 && base.Match(32) && IsLastDayOfMonth(value, month, isLeapYear)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="month"></param>
        /// <param name="isLeapYear"></param>
        /// <returns></returns>
        private static bool IsLastDayOfMonth(int value, int month, bool isLeapYear)
        {
            if (isLeapYear && month == 2)
            {
                return value == 29;
            }
            else
            {
                return value == lastDays[month - 1];
            }
        }
    }
}
