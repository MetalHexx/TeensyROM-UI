using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendWithLimit(this StringBuilder builder, string value, int maxSize = 10000)
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than 0.");

            int availableSpace = maxSize - builder.Length;

            if (availableSpace <= 0) return builder;

            if (value.Length <= availableSpace)
            {
                builder.Append(value);
                return builder;
            }
            string truncatedValue = value[..availableSpace];
            builder.Append(truncatedValue);
            return builder;
        }

        public static StringBuilder AppendLineRolling(this StringBuilder builder, string value, int maxLogEntrySize = 10000, int totalMaxSize = 100000)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (value.Length > maxLogEntrySize)
            {
                string trimmedValue = value.Substring(0, maxLogEntrySize);
                builder.AppendLine($"{trimmedValue}... [Truncated due to length]");
            }
            else
            {
                builder.AppendLine(value);
            }
            if (builder.Length > totalMaxSize)
            {
                int removeLength = builder.Length - totalMaxSize;
                builder.Remove(0, removeLength);
            }
            return builder;
        }
    }
}
