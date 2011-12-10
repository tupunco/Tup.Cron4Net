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
    /// A predictor is able to predict when a scheduling pattern will be matched.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Suppose you want to know when the scheduler will execute a task scheduled
    /// with the pattern <em>0 3 * jan-jun,sep-dec mon-fri</em>. You can predict the
    /// next <em>n</em> execution of the task using a Predictor instance:
    /// </para>
    /// <code>
    /// String pattern = &quot;0 3 * jan-jun,sep-dec mon-fri&quot;;
    /// Predictor p = new Predictor(pattern);
    /// for (int i = 0; i &lt; n; i++) {
    /// 	System.out.println(p.nextMatchingDate());
    /// }
    /// </code>
    /// @author Carlo Pelliccia
    /// @since 1.1
    /// </remarks>
    public class Predictor
    {
        /**
         * The scheduling pattern on which the predictor works.
         */
        private SchedulingPattern schedulingPattern;

        /**
         * The start time for the next prediction.
         */
        private DateTime time;

        /**
         * It builds a predictor with the given scheduling pattern and start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @param start
         *            The start time of the prediction.
         * @throws InvalidPatternException
         *             In the given scheduling pattern isn't valid.
         */
        public Predictor(string schedulingPattern, long start)
            : this(new SchedulingPattern(schedulingPattern), new DateTime(start))
        {
        }

        /**
         * It builds a predictor with the given scheduling pattern and start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @param start
         *            The start time of the prediction.
         * @throws InvalidPatternException
         *             In the given scheduling pattern isn't valid.
         */
        public Predictor(string schedulingPattern, DateTime start)
            : this(new SchedulingPattern(schedulingPattern), start)
        {
        }

        /**
         * It builds a predictor with the given scheduling pattern and the current
         * system time as the prediction start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @throws InvalidPatternException
         *             In the given scheduling pattern isn't valid.
         */
        public Predictor(string schedulingPattern)
            : this(new SchedulingPattern(schedulingPattern), DateTime.Now)
        {
        }

        /**
         * It builds a predictor with the given scheduling pattern and start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @param start
         *            The start time of the prediction.
         * @since 2.0
         */
        public Predictor(SchedulingPattern schedulingPattern, long start)
            : this(schedulingPattern, new DateTime(start))
        {
        }

        /**
         * It builds a predictor with the given scheduling pattern and start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @param start
         *            The start time of the prediction.
         * @since 2.0
         */
        public Predictor(SchedulingPattern schedulingPattern, DateTime start)
        {
            this.schedulingPattern = schedulingPattern;
            this.time = start;
        }

        /**
         * It builds a predictor with the given scheduling pattern and the current
         * system time as the prediction start time.
         * 
         * @param schedulingPattern
         *            The pattern on which the prediction will be based.
         * @since 2.0
         */
        public Predictor(SchedulingPattern schedulingPattern)
            : this(schedulingPattern, DateTime.Now)
        {
        }

        /**
         * It returns the next matching moment as a millis value.
         * 
         * @return The next matching moment as a millis value.
         */
        public DateTime NextMatchingDate()
        {
            // Go a minute ahead.
            time = time.AddMinutes(1);
            // Is it matching?
            if (schedulingPattern.Match(time))
            {
                return time;
            }
            // Go through the matcher groups.
            int size = schedulingPattern.matcherSize;
            DateTime[] times = new DateTime[size];
            for (int k = 0; k < size; k++)
            {
                // Ok, split the time!
                DateTime c = time;

                // Gets the matchers.
                IValueMatcher minuteMatcher = schedulingPattern.minuteMatchers[k];
                IValueMatcher hourMatcher = schedulingPattern.hourMatchers[k];
                IValueMatcher dayOfMonthMatcher = schedulingPattern.dayOfMonthMatchers[k];
                IValueMatcher dayOfWeekMatcher = schedulingPattern.dayOfWeekMatchers[k];
                IValueMatcher monthMatcher = schedulingPattern.monthMatchers[k];
                for (; ; )
                { // day of week
                    for (; ; )
                    { // month
                        for (; ; )
                        { // day of month
                            for (; ; )
                            { // hour
                                for (; ; )
                                { // minutes
                                    if (minuteMatcher.Match(c.Minute))
                                        break;
                                    else
                                        c = c.AddMinutes(1);
                                }
                                if (hourMatcher.Match(c.Hour))
                                    break;
                                else
                                {
                                    c = c.AddHours(1);
                                    c = new DateTime(c.Year, c.Month, c.Day, c.Hour, 0, 0);
                                }
                            }
                            if (dayOfMonthMatcher is DayOfMonthValueMatcher)
                            {
                                DayOfMonthValueMatcher aux = (DayOfMonthValueMatcher)dayOfMonthMatcher;
                                if (aux.Match(c.Day, c.Month, DateTime.IsLeapYear(c.Year)))
                                    break;
                                else
                                    c = c.AddDays(1).Date;
                            }
                            else if (dayOfMonthMatcher.Match(c.Day))
                                break;
                            else
                                c = c.AddDays(1).Date;
                        }
                        if (monthMatcher.Match(c.Month))
                            break;
                        else
                        {
                            c = c.AddMonths(1);
                            c = new DateTime(c.Year, c.Month, 1);
                        }
                    }
                    // Day of week.
                    int dayOfWeek = (int)c.DayOfWeek;
                    if (dayOfWeekMatcher.Match(dayOfWeek))
                        break;
                    else
                        c = c.AddDays(1).Date;
                }
                // Seems it matches!
                times[k] = c;
            }
            // Which one?
            DateTime min = DateTime.MaxValue;
            for (int k = 0; k < size; k++)
            {
                if (times[k] < min)
                {
                    min = times[k];
                }
            }
            // Updates the object current time value.
            time = min;
            // Here it is.
            return time;
        }

        /**
         * It returns the next matching moment as a {@link Date} object.
         * 
         * @return The next matching moment as a {@link Date} object.
         */
        public long NextMatchingTime()
        {
            return NextMatchingDate().Ticks;
        }
    }
}