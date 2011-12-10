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
    /// to validate a value, this ValueMatcher checks if it is in the array.
    /// </summary>
    /// <remarks>
    /// @author Carlo Pelliccia
    /// </remarks>
    class IntArrayValueMatcher
        : IValueMatcher
    {
        /// <summary>
        /// The accepted values.
        /// </summary>
        private int[] values;

        /// <summary>
        /// Builds the ValueMatcher.
        /// </summary>
        /// <param name="ints">
        /// An ArrayList of int elements, one for every value accepted
        /// by the matcher. The match() method will return true only if
        /// its parameter will be one of this list.
        /// </param>
        public IntArrayValueMatcher(List<int> ints)
        {
            if (ints == null)
                return;

            values = ints.ToArray();
            Array.Sort(values);
        }

        /// <summary>
        /// Returns true if the given value is included in the matcher list.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Match(int value)
        {
            return Array.BinarySearch(values, value) > -1;
        }
    }
}