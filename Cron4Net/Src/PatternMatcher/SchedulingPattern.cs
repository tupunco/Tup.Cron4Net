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
	/// A UNIX crontab-like pattern is a string split in five space separated parts.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Each part is intented as:
	/// </para>
	/// <ol>
	/// <li><b>Minutes sub-pattern</b>. During which minutes of the hour
	/// should the task been launched? The values range is from 0 to 59.</li>
	/// <li><b>Hours sub-pattern</b>. During which hours of the day should
	/// the task been launched? The values range is from 0 to 23.</li>
	/// <li><b>Days of month sub-pattern</b>. During which days of the
	/// month should the task been launched? The values range is from 1 to 31. The
	/// special value L can be used to recognize the last day of month.</li>
	/// <li><b>Months sub-pattern</b>. During which months of the year
	/// should the task been launched? The values range is from 1 (January) to 12
	/// (December), otherwise this sub-pattern allows the aliases &quot;jan&quot;,
	/// &quot;feb&quot;, &quot;mar&quot;, &quot;apr&quot;, &quot;may&quot;,
	/// &quot;jun&quot;, &quot;jul&quot;, &quot;aug&quot;, &quot;sep&quot;,
	/// &quot;oct&quot;, &quot;nov&quot; and &quot;dec&quot;.</li>
	/// <li><b>Days of week sub-pattern</b>. During which days of the week
	/// should the task been launched? The values range is from 0 (Sunday) to 6
	/// (Saturday), otherwise this sub-pattern allows the aliases &quot;sun&quot;,
	/// &quot;mon&quot;, &quot;tue&quot;, &quot;wed&quot;, &quot;thu&quot;,
	/// &quot;fri&quot; and &quot;sat&quot;.</li>
	/// </ol>
	/// <para>
	/// The star wildcard character is also admitted, indicating &quot;every minute
	/// of the hour&quot;, &quot;every hour of the day&quot;, &quot;every day of the
	/// month&quot;, &quot;every month of the year&quot; and &quot;every day of the
	/// week&quot;, according to the sub-pattern in which it is used.
	/// </para>
	/// <para>
	/// Once the scheduler is started, a task will be launched when the five parts in
	/// its scheduling pattern will be true at the same time.
	/// </para>
	/// <para>
	/// Some examples:
	/// </para>
	/// <para>
	/// <b>5 * * * *</b><br />
	/// This pattern causes a task to be launched once every hour, at the begin of
	/// the fifth minute (00:05, 01:05, 02:05 etc.).
	/// </para>
	/// <para>
	/// <b>* * * * *</b><br />
	/// This pattern causes a task to be launched every minute.
	/// </para>
	/// <para>
	/// <b>* 12 * * Mon</b><br />
	/// This pattern causes a task to be launched every minute during the 12th hour
	/// of Monday.
	/// </para>
	/// <para>
	/// <b>* 12 16 * Mon</b><br />
	/// This pattern causes a task to be launched every minute during the 12th hour
	/// of Monday, 16th, but only if the day is the 16th of the month.
	/// </para>
	/// <para>
	/// Every sub-pattern can contain two or more comma separated values.
	/// </para>
	/// <para>
	/// <b>59 11 * * 1,2,3,4,5</b><br />
	/// This pattern causes a task to be launched at 11:59AM on Monday, Tuesday,
	/// Wednesday, Thursday and Friday.
	/// </para>
	/// <para>
	/// Values intervals are admitted and defined using the minus character.
	/// </para>
	/// <para>
	/// <b>59 11 * * 1-5</b><br />
	/// This pattern is equivalent to the previous one.
	/// </para>
	/// <para>
	/// The slash character can be used to identify step values within a range. It
	/// can be used both in the form <em>*&#47;c</em> and <em>a-b/c</em>. The
	/// subpattern is matched every <em>c</em> values of the range
	/// <em>0,maxvalue</em> or <em>a-b</em>.
	/// </para>
	/// <para>
	/// <b>*&#47;5 * * * *</b><br />
	/// This pattern causes a task to be launched every 5 minutes (0:00, 0:05, 0:10,
	/// 0:15 and so on).
	/// </para>
	/// <para>
	/// <b>3-18&#47;5 * * * *</b><br />
	/// This pattern causes a task to be launched every 5 minutes starting from the
	/// third minute of the hour, up to the 18th (0:03, 0:08, 0:13, 0:18, 1:03, 1:08
	/// and so on).
	/// </para>
	/// <para>
	/// <b>*&#47;15 9-17 * * *</b><br />
	/// This pattern causes a task to be launched every 15 minutes between the 9th
	/// and 17th hour of the day (9:00, 9:15, 9:30, 9:45 and so on... note that the
	/// last execution will be at 17:45).
	/// </para>
	/// <para>
	/// All the fresh described syntax rules can be used together.
	/// </para>
	/// <para>
	/// <b>* 12 10-16&#47;2 * *</b><br />
	/// This pattern causes a task to be launched every minute during the 12th hour
	/// of the day, but only if the day is the 10th, the 12th, the 14th or the 16th
	/// of the month.
	/// </para>
	/// <para>
	/// <b>* 12 1-15,17,20-25 * *</b><br />
	/// This pattern causes a task to be launched every minute during the 12th hour
	/// of the day, but the day of the month must be between the 1st and the 15th,
	/// the 20th and the 25, or at least it must be the 17th.
	/// </para>
	/// <para>
	/// Finally cron4j lets you combine more scheduling patterns into one, with the
	/// pipe character:
	/// </para>
	/// <para>
	/// <b>0 5 * * *|8 10 * * *|22 17 * * *</b><br />
	/// This pattern causes a task to be launched every day at 05:00, 10:08 and
	/// 17:22.
	/// </para>
	/// @author Carlo Pelliccia
	/// @since 2.0
	/// </remarks>
	public class SchedulingPattern
	{
		/// <summary>
		/// The parser for the minute values.
		/// </summary>
		private static readonly IValueParser MINUTE_VALUE_PARSER = new MinuteValueParser();

		/// <summary>
		/// The parser for the hour values.
		/// </summary>
		private static readonly IValueParser HOUR_VALUE_PARSER = new HourValueParser();

		/// <summary>
		/// The parser for the day of month values.
		/// </summary>
		private static readonly IValueParser DAY_OF_MONTH_VALUE_PARSER = new DayOfMonthValueParser();

		/// <summary>
		/// The parser for the month values.
		/// </summary>
		private static readonly IValueParser MONTH_VALUE_PARSER = new MonthValueParser();

		/// <summary>
		/// The parser for the day of week values.
		/// </summary>
		private static readonly IValueParser DAY_OF_WEEK_VALUE_PARSER = new DayOfWeekValueParser();

		/// <summary>
		/// Validates a string as a scheduling pattern.
		/// </summary>
		/// <param name="schedulingPattern">The pattern to validate.</param>
		/// <returns>
		/// true if the given string represents a valid scheduling pattern;
		/// false otherwise.
		/// </returns>
		public static bool Validate(string schedulingPattern)
		{
			try
			{
				return (new SchedulingPattern()).TryParsePattern(schedulingPattern);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// The pattern as a string.
		/// </summary>
		private string asString;

		/// <summary>
		/// The ValueMatcher list for the "minute" field.
		/// </summary>
		internal List<IValueMatcher> minuteMatchers = new List<IValueMatcher>();

		/// <summary>
		/// The ValueMatcher list for the "hour" field.
		/// </summary>
		internal List<IValueMatcher> hourMatchers = new List<IValueMatcher>();

		/// <summary>
		/// The ValueMatcher list for the "day of month" field.
		/// </summary>
		internal List<IValueMatcher> dayOfMonthMatchers = new List<IValueMatcher>();

		/// <summary>
		/// The ValueMatcher list for the "month" field.
		/// </summary>
		internal List<IValueMatcher> monthMatchers = new List<IValueMatcher>();

		/// <summary>
		/// The ValueMatcher list for the "day of week" field.
		/// </summary>
		internal List<IValueMatcher> dayOfWeekMatchers = new List<IValueMatcher>();

		/// <summary>
		/// How many matcher groups in this pattern?
		/// </summary>
		internal int matcherSize = 0;

		/// <summary>
		/// Builds a SchedulingPattern.
		/// </summary>
		private SchedulingPattern() { }

		/// <summary>
		/// Builds a SchedulingPattern parsing it from a string.
		/// </summary>
		/// <param name="pattern">The pattern as a crontab-like string.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="InvalidPatternException">If the supplied string is not a valid pattern.</exception>
		public SchedulingPattern(string pattern)
		{
			TryParsePattern(pattern);
		}

		#region Methods Parse Pattern
		/// <summary>
		/// Builds a SchedulingPattern parsing it from a string.
		/// </summary>
		/// <param name="pattern">The pattern as a crontab-like string.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		private bool TryParsePattern(string pattern)
		{
			if (string.IsNullOrEmpty(pattern))
				throw new ArgumentNullException("pattern");

			this.asString = pattern;
			var st1 = pattern.Split('|');
			if (st1.Length < 1)
			{
				throw new InvalidPatternException("invalid pattern: \"" + pattern + "\"");
			}
			foreach (var st1Itme in st1)
			{
				var st2 = st1Itme.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (st2.Length != 5)
				{
					throw new InvalidPatternException("invalid pattern: \"" + st1Itme + "\"");
				}

				BuildValueMatcher(minuteMatchers, st2[0], MINUTE_VALUE_PARSER, st1Itme, "minutes");
				BuildValueMatcher(hourMatchers, st2[1], HOUR_VALUE_PARSER, st1Itme, "hours");
				BuildValueMatcher(dayOfMonthMatchers, st2[2], DAY_OF_MONTH_VALUE_PARSER, st1Itme, "month");
				BuildValueMatcher(monthMatchers, st2[3], MONTH_VALUE_PARSER, st1Itme, "months");
				BuildValueMatcher(dayOfWeekMatchers, st2[4], DAY_OF_WEEK_VALUE_PARSER, st1Itme, "week");

				matcherSize++;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cValueMatcher"></param>
		/// <param name="st"></param>
		/// <param name="cValueParser"></param>
		/// <param name="pattern"></param>
		/// <param name="cpType"></param>
		/// <exception cref="InvalidPatternException"></exception>
		private void BuildValueMatcher(List<IValueMatcher> cValueMatcher,
										string st,
										IValueParser cValueParser,
										string pattern,
										string cpType)
		{
			try
			{
				cValueMatcher.Add(BuildValueMatcher(st, cValueParser));
			}
			catch (Exception e)
			{
				throw new InvalidPatternException(string.Format(
							@"invalid pattern  ""{0}"". Error parsing {1} field: {2}.", pattern, cpType, e.Message));
			}
		}

		/// <summary>
		/// A ValueMatcher utility builder.
		/// </summary>
		/// <param name="str">The pattern part for the ValueMatcher creation.</param>
		/// <param name="parser">The parser used to parse the values.</param>
		/// <returns>The requested ValueMatcher.</returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.Exception"></exception>
		private IValueMatcher BuildValueMatcher(string str, IValueParser parser)
		{
			if (string.IsNullOrEmpty(str))
				throw new ArgumentNullException("str");

			if (str.Length == 1 && str == "*")
			{
				return new AlwaysTrueValueMatcher();
			}
			List<int> values = new List<int>();
			var st = str.Split(',');
			foreach (var element in st)
			{
				List<int> local;
				try
				{
					local = ParseListElement(element, parser);
				}
				catch (Exception e)
				{
					throw new Exception("invalid field \"" + str
							+ "\", invalid element \"" + element + "\", "
							+ e.Message);
				}
				foreach (var value in local)
				{
					if (!values.Contains(value))
					{
						values.Add(value);
					}
				}
			}
			if (values.Count == 0)
			{
				throw new Exception("invalid field \"" + str + "\"");
			}
			if (parser == DAY_OF_MONTH_VALUE_PARSER)
			{
				return new DayOfMonthValueMatcher(values);
			}
			else
			{
				return new IntArrayValueMatcher(values);
			}
		}

		/// <summary>
		/// Parses an element of a list of values of the pattern.
		/// </summary>
		/// <param name="str">The element string.</param>
		/// <param name="parser">The parser used to parse the values.</param>
		/// <returns>A list of ints representing the allowed values.</returns>
		/// <exception cref="System.Exception">If the supplied pattern part is not valid.</exception>
		private List<int> ParseListElement(string str, IValueParser parser)
		{
			var st = str.Split('/');
			int size = st.Length;
			if (size < 1 || size > 2)
			{
				throw new Exception("syntax error");
			}
			List<int> values;
			try
			{
				values = ParseRange(st[0], parser);
			}
			catch (Exception e)
			{
				throw new Exception("invalid range, " + e.Message);
			}
			if (size == 2)
			{
				string dStr = st[1];
				int div;
				try
				{
					div = int.Parse(dStr);
				}
				catch
				{
					throw new Exception("invalid divisor \"" + dStr + "\"");
				}
				if (div < 1)
				{
					throw new Exception("non positive divisor \"" + div + "\"");
				}
				List<int> values2 = new List<int>();
				for (int i = 0; i < values.Count; i += div)
				{
					values2.Add(values[i]);
				}
				return values2;
			}
			else
			{
				return values;
			}
		}

		/// <summary>
		/// Parses a range of values.
		/// </summary>
		/// <param name="str">The range string.</param>
		/// <param name="parser">The parser used to parse the values.</param>
		/// <returns>A list of ints representing the allowed values.</returns>
		/// <exception cref="System.Exception">If the supplied pattern part is not valid.</exception>
		private List<int> ParseRange(string str, IValueParser parser)
		{
			if (str == "*")
			{
				int min = parser.MinValue;
				int max = parser.MaxValue;
				List<int> values = new List<int>(max - min + 1);
				for (int i = min; i <= max; i++)
				{
					values.Add(i);
				}
				return values;
			}
			var st = str.Split('-');
			int size = st.Length;
			if (size < 1 || size > 2)
			{
				throw new Exception("syntax error");
			}
			string v1Str = st[0];
			int v1;
			try
			{
				v1 = parser.Parse(v1Str);
			}
			catch (Exception e)
			{
				throw new Exception("invalid value \"" + v1Str + "\", "
						+ e.Message);
			}
			if (size == 1)
			{
				List<int> values = new List<int>(1);
				values.Add(v1);
				return values;
			}
			else
			{
				string v2Str = st[1];
				int v2;
				try
				{
					v2 = parser.Parse(v2Str);
				}
				catch (Exception e)
				{
					throw new Exception("invalid value \"" + v2Str + "\", "
							+ e.Message);
				}
				List<int> values = new List<int>();
				if (v1 < v2)
				{
					for (int i = v1; i <= v2; i++)
					{
						values.Add(i);
					}
				}
				else if (v1 > v2)
				{
					int min = parser.MinValue;
					int max = parser.MaxValue;
					for (int i = v1; i <= max; i++)
					{
						values.Add(i);
					}
					for (int i = min; i <= v2; i++)
					{
						values.Add(i);
					}
				}
				else
				{
					// v1 == v2
					values.Add(v1);
				}
				return values;
			}
		} 
		#endregion

		#region Methods Match
		/// <summary>
		/// This methods returns true if the given timestamp (expressed as a 
		/// <see cref="System.DateTime.Ticks">DateTime.Ticks</see> value) matches the pattern, 
		/// according to the given time zone.
		/// </summary>
		/// <param name="timezone">A TimeSpan of time zone.</param>
		/// <param name="ticks">The timestamp, as a <see cref="System.DateTime.Ticks">DateTime.Ticks</see> value.</param>
		/// <returns>
		/// true if the given timestamp matches the pattern.
		/// </returns>
		public bool Match(TimeSpan timezone, long ticks)
		{
			DateTime gc = new DateTime((timezone + TimeSpan.FromTicks(ticks)).Ticks);
			int minute = gc.Minute;
			int hour = gc.Hour;
			int dayOfMonth = gc.Month;
			int month = gc.Month;
			int dayOfWeek = (int)gc.DayOfWeek;
			int year = gc.Year;
			for (int i = 0; i < matcherSize; i++)
			{
				IValueMatcher minuteMatcher = minuteMatchers[i];
				IValueMatcher hourMatcher = hourMatchers[i];
				IValueMatcher dayOfMonthMatcher = dayOfMonthMatchers[i];
				IValueMatcher monthMatcher = monthMatchers[i];
				IValueMatcher dayOfWeekMatcher = dayOfWeekMatchers[i];
				bool eval = minuteMatcher.Match(minute)
						&& hourMatcher.Match(hour)
						&& ((dayOfMonthMatcher is DayOfMonthValueMatcher)
									? ((DayOfMonthValueMatcher)dayOfMonthMatcher).Match(dayOfMonth, month, DateTime.IsLeapYear(year))
									: dayOfMonthMatcher.Match(dayOfMonth))
						&& monthMatcher.Match(month)
						&& dayOfWeekMatcher.Match(dayOfWeek);
				if (eval)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// This methods returns true if the given timestamp (expressed as a
		/// <see cref="System.DateTime.Ticks">DateTime.Ticks</see> value) matches the pattern, 
		/// according to the system default time zone.
		/// </summary>
		/// <param name="ticks">The timestamp, as a <see cref="System.DateTime.Ticks">DateTime.Ticks</see> value.</param>
		/// <returns>true if the given timestamp matches the pattern.</returns>
		public bool Match(long ticks)
		{
			return Match(TimeSpan.FromTicks(0L), ticks);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public bool Match(DateTime date)
		{
			return Match(date.Ticks);
		}
		#endregion

		/// <summary>
		/// Returns the pattern as a string.
		/// </summary>
		/// <returns>Returns the pattern as a string.</returns>
		public override string ToString()
		{
			return asString;
		}

		#region Class ValueParser
		/// <summary>
		/// This utility method changes an alias to an int value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="aliases">The aliases list.</param>
		/// <param name="offset">The offset appplied to the aliases list indices.</param>
		/// <returns>The parsed value.</returns>
		/// <exception cref="System.Exception">If the expressed values doesn't match any alias.</exception>
		private static int ParseAlias(string value, string[] aliases, int offset)
		{
			for (int i = 0; i < aliases.Length; i++)
			{
				if (string.Compare(aliases[i], value, true) == 0)
				{
					return offset + i;
				}
			}
			throw new Exception("invalid alias \"" + value + "\"");
		}

		/// <summary>
		/// Definition for a value parser.
		/// </summary>
		private interface IValueParser
		{
			/// <summary>
			/// Attempts to parse a value.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <returns>The parsed value.</returns>
			/// <exception cref="System.Exception">If the value can't be parsed.</exception>
			int Parse(string value);

			/// <summary>
			/// Returns the minimum value accepred by the parser.
			/// </summary>
			int MinValue { get; }

			/// <summary>
			/// Returns the maximum value accepred by the parser.
			/// </summary>
			int MaxValue { get; }
		}

		/// <summary>
		/// A simple value parser.
		/// </summary>
		private class SimpleValueParser :
			IValueParser
		{

			/// <summary>
			/// The minimum allowed value.
			/// </summary>
			protected int minValue;

			/// <summary>
			/// The maximum allowed value.
			/// </summary>
			protected int maxValue;

			/// <summary>
			/// Builds the value parser.
			/// </summary>
			/// <param name="minValue">The minimum allowed value.</param>
			/// <param name="maxValue">The maximum allowed value.</param>
			public SimpleValueParser(int minValue, int maxValue)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
			}

			/// <summary>
			/// Attempts to parse a value.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <returns>The parsed value.</returns>
			/// <exception cref="System.Exception">If the value can't be parsed.</exception>
			public virtual int Parse(string value)
			{
				int i;
				try
				{
					i = int.Parse(value);
				}
				catch
				{
					throw new Exception("invalid int value");
				}
				if (i < minValue || i > maxValue)
				{
					throw new Exception("value out of range");
				}
				return i;
			}
			/// <summary>
			/// 
			/// </summary>
			public int MinValue
			{
				get { return minValue; }
			}
			/// <summary>
			/// 
			/// </summary>
			public int MaxValue
			{
				get { return maxValue; }
			}
		}

		/// <summary>
		/// The minutes value parser.
		/// </summary>
		private class MinuteValueParser
			: SimpleValueParser
		{
			/// <summary>
			/// Builds the value parser.
			/// </summary>
			public MinuteValueParser()
				: base(0, 59)
			{
			}
		}

		/// <summary>
		/// The hours value parser.
		/// </summary>
		private class HourValueParser :
			SimpleValueParser
		{
			/// <summary>
			/// Builds the value parser.
			/// </summary>
			public HourValueParser()
				: base(0, 23)
			{
			}
		}

		/// <summary>
		/// The days of month value parser.
		/// </summary>
		private class DayOfMonthValueParser
			: SimpleValueParser
		{
			/// <summary>
			/// Builds the value parser.
			/// </summary>
			public DayOfMonthValueParser()
				: base(1, 31)
			{
			}

			/// <summary>
			/// Added to support last-day-of-month.
			/// </summary>
			/// <param name="value">The value to be parsed</param>
			/// <returns>the int day of the month or 32 for last day of the month</returns>
			/// <exception cref="System.Exception">if the input value is invalid</exception>
			public override int Parse(string value)
			{
				if (string.Compare(value, "L", true) == 0)
				{
					return 32;
				}
				else
				{
					return base.Parse(value);
				}
			}
		}

		/// <summary>
		/// The value parser for the months field.
		/// </summary>
		private class MonthValueParser
			: SimpleValueParser
		{
			/// <summary>
			/// Months aliases.
			/// </summary>
			private static readonly string[] ALIASES = { "jan", "feb", "mar", "apr", "may",
				"jun", "jul", "aug", "sep", "oct", "nov", "dec" };

			/// <summary>
			/// Builds the months value parser.
			/// </summary>
			public MonthValueParser()
				: base(1, 12)
			{
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public override int Parse(string value)
			{
				try
				{
					// try as a simple value
					return base.Parse(value);
				}
				catch
				{
					// try as an alias
					return ParseAlias(value, ALIASES, 1);
				}
			}
		}

		/// <summary>
		/// The value parser for the months field.
		/// </summary>
		private class DayOfWeekValueParser
			: SimpleValueParser
		{
			/// <summary>
			/// Days of week aliases.
			/// </summary>
			private static readonly string[] ALIASES = { "sun", "mon", "tue", "wed", "thu",
				"fri", "sat" };

			/// <summary>
			/// Builds the months value parser.
			/// </summary>
			public DayOfWeekValueParser()
				: base(0, 7)
			{
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public override int Parse(string value)
			{
				try
				{
					// try as a simple value
					return base.Parse(value) % 7;
				}
				catch
				{
					// try as an alias
					return ParseAlias(value, ALIASES, 0);
				}
			}
		}
		#endregion
	}
}