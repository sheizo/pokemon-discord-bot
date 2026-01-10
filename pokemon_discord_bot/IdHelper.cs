using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokemon_discord_bot
{
    public static class IdHelper
    {
        public static string ToBase36(int value)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";

            if (value == 0)
                return "0";

            var result = new StringBuilder();

            while (value > 0)
            {
                result.Insert(0, chars[value % 36]);
                value /= 36;
            }

            return result.ToString();
        }

        public static int FromBase36(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) throw new FormatException("Input string was empty or whitespace.");

            long result = 0;

            foreach (char ch in value)
            {
                char c = char.ToLowerInvariant(ch);
                int digit;

                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'a' && c <= 'z')
                    digit = c - 'a' + 10;
                else
                    throw new FormatException($"Invalid character '{ch}' in base36 string.");

                result = result * 36 + digit;

                if (result > int.MaxValue)
                    throw new OverflowException("Value too large for Int32.");
            }

            return (int)result;
        }
    }
}
