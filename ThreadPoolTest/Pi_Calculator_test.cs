using System;
using BadThreadPool;

namespace ThreadPoolTest
{
    public class PiCalculatorTest
    {
        /// <summary>
        /// Calculates pi, just for testing
        /// </summary>
        /// <returns>pi</returns>
        public object Calc_Pi()
        {
            double pi = 0;
            for (int k = 0; k <= short.MaxValue; k++)
            {
                double delta = ((Math.Pow((-1), k)) / (Math.Pow(2, (10 * k)))) *
                               (-(Math.Pow(2, 5) / ((4 * k) + 1))
                                - 1f / (4 * k + 3)
                                + (Math.Pow(2, 8) / ((10 * k) + 1))
                                - (Math.Pow(2, 6) / ((10 * k) + 3))
                                - (Math.Pow(2, 2) / ((10 * k) + 5))
                                - (Math.Pow(2, 2) / ((10 * k) + 7))
                                + 1f / (k * 10 + 9));

                delta /= (Math.Pow(2, 6));
                pi += delta;
            }
            return pi;
        }

        public void Test_Threading_callback(CallbackArgs<object> args)
        {
            
        }
    }
}