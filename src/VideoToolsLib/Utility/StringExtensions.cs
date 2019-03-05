
namespace VideoTools
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static partial class StringExtensions
    {
        private const string RegexPattern = @"(?<start>\$\{)+(?<property>\w+)(?<format>:[^}]+)?(?<end>\})+";
        private static readonly Regex _regex = new Regex(RegexPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static string WithEnvionmentVariables(this string str)
        {
            return Environment.ExpandEnvironmentVariables(str);
        }

        public static string FormatWith(this string format, object source)
        {
            return FormatWith(format, default(IFormatProvider), source);
        }

        /// <summary>
        /// Code by James Newton-King http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
        /// </summary>
        public static string FormatWith(this string format, IFormatProvider provider, object source)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            var values = new List<object>();
            string rewrittenFormat = _regex.Replace(format, delegate (Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                var value = default(object);
                if (propertyGroup.Value == "0")
                {
                    value = source;
                }
                else
                {
                    if (ReflectionHelper.TryGetPropertyValue(source, propertyGroup.Value, out value) == false)
                    {
                        value = m.Value;
                    }
                }

                values.Add(value);

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                  + new string('}', endGroup.Captures.Count);
            });
            return string.Format(provider, rewrittenFormat, values.ToArray());
        }
    }
}
