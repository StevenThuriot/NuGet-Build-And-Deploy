﻿using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NuGet.BuildAndDeploy
{
	/// <summary>
	/// A parser class to easily retreive start up arguments.
	/// </summary>
	public static class ArgumentParser
	{
		/// <summary>
		/// A parse method that parses the Start Up arguments into a dictionary.
		/// You want to use this method when every command has exactly one value.
		/// Commands will still be added as a key if they do not have a value.
		/// Commands also have to start with a "/".
		/// </summary>
		/// <param name="arguments">The Start Up arguments.</param>
		/// <returns>A dictionary of commands and values.</returns>
		public static IDictionary<string, string> Parse(string[] arguments)
		{
			var dictionary = new Dictionary<string, string>();

			var key = "";
			var builder = new StringBuilder();

			for (var i = 0; i < arguments.Length; i++)
			{
				var argument = arguments[i];

				if (argument.StartsWith("/", true, CultureInfo.CurrentCulture))
				{
					var strippedArgument = argument.Substring(1).ToUpperInvariant();

					if (i == 0)
					{
						key = strippedArgument;
					}
					else
					{
						dictionary.Add(key, builder.ToString());

						key = strippedArgument;
						builder.Length = 0;
					}
				}
				else
				{
					builder.Append(" ");
					builder.Append(argument);
				}
			}

			dictionary.Add(key, builder.ToString());

			return dictionary;
		}

		/// <summary>
		/// A parse method that parses the Start Up arguments into a dictionary.
		/// You want to use this method when every command has one or more values.
		/// Commands will still be added as a key if they do not have a value.
		/// Commands also have to start with a "/".
		/// </summary>
		/// <param name="arguments">The Start Up arguments.</param>
		/// <returns>A dictionary of commands and values.</returns>
		public static IDictionary<string, IList<string>> ParseMultiValues(string[] arguments)
		{
			var dictionary = new Dictionary<string, IList<string>>();

			var key = "";
			var values = new List<string>();

			for (var i = 0; i < arguments.Length; i++)
			{
				var argument = arguments[i];

				if (argument.StartsWith("/", true, CultureInfo.CurrentCulture))
				{
					var strippedArgument = argument.Substring(1).ToUpperInvariant();

					if (i == 0)
					{
						key = strippedArgument;
					}
					else
					{
						dictionary.Add(key, values);

						key = strippedArgument;
						values = new List<string>();
					}
				}
				else
				{
					values.Add(argument);
				}
			}

			dictionary.Add(key, values);

			return dictionary;
		}
	}
}
