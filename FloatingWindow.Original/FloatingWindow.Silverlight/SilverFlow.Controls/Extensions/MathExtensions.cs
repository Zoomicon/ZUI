using System;

namespace SilverFlow.Controls.Extensions
{
    /// <summary>
    /// Math extensions
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Determines whether the number is NaN.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>
        /// <c>true</c> if is NaN; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNotSet(this double number)
        {
            return double.IsNaN(number);
        }

        /// <summary>
        /// Determines whether the specified number is near the test number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="testNumber">The test number.</param>
        /// <param name="accuracy">Accuracy.</param>
        /// <returns>
        /// <c>true</c> if the specified number is near the testing one; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNear(this double number, double testNumber, double accuracy)
        {
            return number >= (testNumber - accuracy) && number <= (testNumber + accuracy);
        }

        /// <summary>
        /// Compares two numbers without their signs and gets absolutely minimal value.
        /// </summary>
        /// <param name="num1">First number.</param>
        /// <param name="num2">Second number.</param>
        /// <returns>Absolutely minimal value.</returns>
        public static double AbsMin(double num1, double num2)
        {
            if (num1.IsNotSet() && num2.IsNotSet())
                return double.NaN;

            if (num1.IsNotSet()) return num2;
            if (num2.IsNotSet()) return num1;

            double abs1 = Math.Abs(num1);
            double abs2 = Math.Abs(num2);

            if (abs1 < abs2) return num1;
            if (abs2 < abs1) return num2;

            // Abs. values are equal. Return first number
            return num1;
        }

        /// <summary>
        /// Ensures the given number is in the specified range.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="low">The lower limit.</param>
        /// <param name="high">The upper limit.</param>
        /// <returns>A number in the specified range.</returns>
        public static double EnsureInRange(this double number, double low, double high)
        {
            if (number.IsNotSet()) return double.NaN;

            double result = Math.Max(number, low);
            return Math.Min(result, high);
        }

        /// <summary>
        /// Returns a double value if it is not a NAN, or zero.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>Double value if it is not a NAN, or zero.</returns>
        public static double ValueOrZero(this double number)
        {
            return double.IsNaN(number) ? 0 : number;
        }
    }
}
