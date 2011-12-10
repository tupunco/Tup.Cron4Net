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

namespace Tup.Cron4Net
{
    /// <summary>
    /// A GUID generator.
    /// </summary>
    static class GUIDGenerator
    {
        /// <summary>
        /// Generates a GUID (36 chars).
        /// </summary>
        /// <returns>The generated GUID.</returns>
        public static string Generate()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}