using System.Text;
using System.Text.RegularExpressions;

namespace SafeMap
{
    public sealed class SafeText
    {
        private readonly string? _value;

        private SafeText(string? value)
        {
            _value = value;
        }

        /// <summary>
        /// Factory Method
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SafeText From(string? value)
        {
            // Accept null/empty and keep it; transforms handle null safely.
            return new SafeText(value);
        }

        /// <summary>
        /// Extension via Safe.Guard following our design pattern.
        /// </summary>
        /// <param name="maybeString"></param>
        /// <returns></returns>
        public static SafeText From(object? maybeString)
        {
            return new SafeText(maybeString?.ToString());
        }

        /// <summary>
        /// Converts a reference to SafeText easily
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static SafeText FromGuarded<T>(SafeValue<T> sv) where T : class
        {
            if (sv == null) return new SafeText(null);

            var v = sv.Value();

            return new SafeText(v?.ToString());
        }

        /// <summary>
        /// Trim whitespace from both ends safely
        /// </summary>
        /// <returns></returns>
        public SafeText Trim()
        {
            if (_value == null) 
                return this;

            return new SafeText(_value.Trim());
        }

        /// <summary>
        /// Remove all spaces safely
        /// </summary>
        /// <returns></returns>
        public SafeText RemoveSpaces()
        {
            if (_value == null) 
                return this;
            
            return new SafeText(_value.Replace(" ", string.Empty));
        }

        /// <summary>
        /// ToUpperInvariant safely
        /// </summary>
        /// <returns></returns>
        public SafeText ToUpper()
        {
            if (_value == null) 
                return this;
            
            return new SafeText(_value.ToUpperInvariant());
        }

        /// <summary>
        /// ToLowerInvariant safely
        /// </summary>
        /// <returns></returns>
        public SafeText ToLower()
        {
            if (_value == null) 
                return this;
            
            return new SafeText(_value.ToLowerInvariant());
        }

        /// <summary>
        /// Collapse multiple spaces into single space
        /// </summary>
        /// <returns></returns>
        public SafeText CollapseSpaces()
        {
            if (_value == null) 
                return this;
            
            var s = Regex.Replace(_value, @"\s+", " ");
            
            return new SafeText(s);
        }

        /// <summary>
        /// Basic normalization: trim + collapse spaces + optionally remove non-printable
        /// </summary>
        /// <param name="removeNonPrintable"></param>
        /// <returns></returns>
        public SafeText Normalize(bool removeNonPrintable = false)
        {
            if (_value == null) return this;
            var v = _value.Trim();
            v = Regex.Replace(v, @"\s+", " ");
            
            if (removeNonPrintable)
            {
                var sb = new StringBuilder();
            
                foreach (var ch in v)
                {
                    if (!char.IsControl(ch)) sb.Append(ch);
                }
                v = sb.ToString();
            }

            return new SafeText(v);
        }

        /// <summary>
        /// Replace a regex pattern (safe)
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public SafeText ReplaceRegex(string pattern, string replacement)
        {
            if (_value == null) return this;
            
            try
            {
                var s = Regex.Replace(_value, pattern, replacement);
            
                return new SafeText(s);
            }
            catch
            {
                return this;
            }
        }

        /// <summary>
        /// Map via a user func (can return null)
        /// </summary>
        /// <param name="projector"></param>
        /// <returns></returns>
        public SafeText Map(Func<string, string?> projector)
        {
            if (_value == null) 
                return this;

            try
            {
                var r = projector(_value);
                
                return new SafeText(r);
            }
            catch
            {
                return new SafeText(null);
            }
        }

        public bool IsNullOrEmpty() => string.IsNullOrEmpty(_value);

        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(_value);

        public bool EqualsIgnoreCase(string? other) =>
            string.Equals(_value, other, StringComparison.OrdinalIgnoreCase);

        public bool ContainsIgnoreCase(string? substring) =>
            _value != null && substring != null && _value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;

        public bool Matches(string pattern)
        {
            if (_value == null) 
                return false;
            
            try 
            { 
                return Regex.IsMatch(_value, pattern); 
            } 
            catch 
            { 
                return false; 
            }
        }

        public string? OrDefault(string? fallback, bool treatEmptyAsNull = true)
        {
            if (_value == null) 
                return fallback;
            
            if (treatEmptyAsNull && _value.Length == 0) 
                return fallback;

            return _value;
        }

        public string? OrIfEmpty(string? fallback)
        {
            if (_value == null) 
                return null;
            
            return _value.Length == 0 ? fallback : _value;
        }

        public string? OrIfWhitespace(string? fallback)
        {
            if (_value == null) 
                return fallback;
            
            return string.IsNullOrWhiteSpace(_value) ? fallback : _value;
        }

        public string? OrIfInvalid(IEnumerable<string?> invalidValues, string? fallback)
        {
            if (_value == null) 
                return fallback;
            
            var set = new HashSet<string>(invalidValues.Where(x => x != null).Select(x => x!.Trim()), StringComparer.OrdinalIgnoreCase);
            
            return set.Contains(_value.Trim()) ? fallback : _value;
        }

        public string? OrIfNotMatch(Func<string, bool> predicate, string? fallback)
        {
            if (_value == null) 
                return fallback;
            
            try
            {
                return predicate(_value) ? _value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        public string? Value() => _value;

        public string ValueOr(string fallback) => _value ?? fallback;
    }
}