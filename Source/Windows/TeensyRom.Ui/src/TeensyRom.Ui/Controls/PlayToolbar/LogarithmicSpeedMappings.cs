﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Controls.PlayToolbar
{

    internal static class LogarithmicSpeedMappings
    {
        private static readonly Dictionary<double, double> Mappings = GenerateMappings();

        public static double GetLogPercentage(this double value)
        {
            if (value == 0) return 0;

            var roundedValue = Math.Round(value, 2);

            if (Mappings.TryGetValue(roundedValue, out var percentage))
                return percentage;

            var closestKey = Mappings.Keys.OrderBy(k => Math.Abs(k - roundedValue)).First();

            return Mappings[closestKey];
        }


        public static double GetNearestPercentValue(this double currentValue, double percentageStep)
        {
            var currentPercentage = currentValue.GetLogPercentage();
            var targetPercentage = currentPercentage + percentageStep;
            var closest = Mappings
                .Where(kvp => kvp.Key != currentValue)
                .OrderBy(kvp => Math.Abs(kvp.Value - targetPercentage)).First();

            return closest.Key;
        }

        private static Dictionary<double, double> GenerateMappings()
        {
            Dictionary<double, double> baseMappings = new()
            {
                { 99, 10000 },
                { 98, 4900 },
                { 97, 3233 },
                { 96, 2400 },
                { 95, 1900 },
                { 94, 1567 },
                { 93, 1329 },
                { 92, 1150 },
                { 91, 1011 },
                { 90, 900 },
                { 89, 809 },
                { 88, 733 },
                { 87, 669 },
                { 86, 614 },
                { 85, 567 },
                { 84, 525 },
                { 83, 488 },
                { 82, 456 },
                { 81, 426 },
                { 80, 400 },
                { 79, 376 },
                { 78, 355 },
                { 77, 335 },
                { 76, 317 },
                { 75, 300 },
                { 74, 285 },
                { 73, 270 },
                { 72, 257 },
                { 71, 245 },
                { 70, 233 },
                { 69, 223 },
                { 68, 213 },
                { 67, 203 },
                { 66, 194 },
                { 65, 186 },
                { 64, 178 },
                { 63, 170 },
                { 62, 163 },
                { 61, 156 },
                { 60, 150 },
                { 59, 144 },
                { 58, 138 },
                { 57, 133 },
                { 56, 127 },
                { 55, 122 },
                { 54, 117 },
                { 53, 113 },
                { 52, 108 },
                { 51, 104 },
                { 50, 100 },
                { 49, 96 },
                { 48, 92 },
                { 47, 89 },
                { 46, 85 },
                { 45, 82 },
                { 44, 79 },
                { 43, 75 },
                { 42, 72 },
                { 41, 69 },
                { 40, 67 },
                { 39, 64 },
                { 38, 61 },
                { 37, 59 },
                { 36, 56 },
                { 35, 54 },
                { 34, 52 },
                { 33.334, 50 },
                { 33, 49 },
                { 32, 47 },
                { 31, 45 },
                { 30, 43 },
                { 29, 41 },
                { 28, 39 },
                { 27, 37 },
                { 26, 35 },
                { 25, 33 },
                { 24, 32 },
                { 23, 30 },
                { 22, 28 },
                { 21, 27 },
                { 20, 25 },
                { 19, 23 },
                { 18, 22 },
                { 17, 20 },
                { 16, 19 },
                { 15, 18 },
                { 14, 16 },
                { 13, 15 },
                { 12, 14 },
                { 11, 12 },
                { 10, 11 },
                { 9, 10 },
                { 8, 9 },
                { 7, 8 },
                { 6, 6 },
                { 5, 5 },
                { 4, 4 },
                { 3, 3 },
                { 2, 2 },
                { 1, 1 },
                { 0, 0 },
                { -1, -1 },
                { -2, -2 },
                { -3, -3 },
                { -4, -4 },
                { -5, -5 },
                { -6, -6 },
                { -7, -7 },
                { -8, -7 },
                { -9, -8 },
                { -10, -9 },
                { -11, -10 },
                { -12, -11 },
                { -13, -12 },
                { -14, -12 },
                { -15, -13 },
                { -16, -14 },
                { -17, -15 },
                { -18, -15 },
                { -19, -16 },
                { -20, -17 },
                { -21, -17 },
                { -22, -18 },
                { -23, -19 },
                { -24, -19 },
                { -25, -20 },
                { -26, -21 },
                { -27, -21 },
                { -28, -22 },
                { -29, -22 },
                { -30, -23 },
                { -31, -24 },
                { -32, -24 },
                { -33, -25 },
                { -34, -25 },
                { -35, -26 },
                { -36, -26 },
                { -37, -27 },
                { -38, -28 },
                { -39, -28 },
                { -40, -29 },
                { -41, -29 },
                { -42, -30 },
                { -43, -30 },
                { -44, -31 },
                { -45, -31 },
                { -46, -32 },
                { -47, -32 },
                { -48, -32 },
                { -49, -33 },
                { -50, -33 },
                { -51, -34 },
                { -52, -34 },
                { -53, -35 },
                { -54, -35 },
                { -55, -35 },
                { -56, -36 },
                { -57, -36 },
                { -58, -37 },
                { -59, -37 },
                { -60, -38 },
                { -61, -38 },
                { -62, -38 },
                { -63, -39 },
                { -64, -39 },
                { -65, -39 },
                { -66, -40 },
                { -67, -40 },
                { -68, -40 },
                { -69, -41 },
                { -70, -41 },
                { -71, -42 },
                { -72, -42 },
                { -73, -42 },
                { -74, -43 },
                { -75, -43 },
                { -76, -43 },
                { -77, -44 },
                { -78, -44 },
                { -79, -44 },
                { -80, -44 },
                { -81, -45 },
                { -82, -45 },
                { -83, -45 },
                { -84, -46 },
                { -85, -46 },
                { -86, -46 },
                { -87, -47 },
                { -88, -47 },
                { -89, -47 },
                { -90, -47 },
                { -91, -48 },
                { -92, -48 },
                { -93, -48 },
                { -94, -48 },
                { -95, -49 },
                { -96, -49 },
                { -97, -49 },
                { -98, -49 },
                { -99, -50 },
                { -100, -50 },
                { -101, -50 },
                { -102, -50 },
                { -103, -51 },
                { -104, -51 },
                { -105, -51 },
                { -106, -51 },
                { -107, -52 },
                { -108, -52 },
                { -109, -52 },
                { -110, -52 },
                { -111, -53 },
                { -112, -53 },
                { -113, -53 },
                { -114, -53 },
                { -115, -53 },
                { -116, -54 },
                { -117, -54 },
                { -118, -54 },
                { -119, -54 },
                { -120, -55 },
                { -121, -55 },
                { -122, -55 },
                { -123, -55 },
                { -124, -55 },
                { -125, -56 },
                { -126, -56 },
                { -127, -56 },
                { -128, -56 },
            };

            var fullMappings = new Dictionary<double, double>();

            var keys = baseMappings.Keys.OrderByDescending(k => k).ToList();

            for (int i = 0; i < keys.Count - 1; i++)
            {
                double highKey = keys[i];
                double lowKey = keys[i + 1];
                double highValue = baseMappings[highKey];
                double lowValue = baseMappings[lowKey];

                for (double key = highKey; key > lowKey; key -= 0.01)
                {
                    double proportion = (key - lowKey) / (highKey - lowKey);
                    double value = lowValue + proportion * (highValue - lowValue);
                    fullMappings[Math.Round(key, 2)] = Math.Round(value, 2);
                }
            }
            foreach (var kvp in baseMappings)
            {
                fullMappings[kvp.Key] = kvp.Value;
            }

            return fullMappings.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
