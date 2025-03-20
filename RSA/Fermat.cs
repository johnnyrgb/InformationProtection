using Extreme.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using BigInteger = Extreme.Mathematics.BigInteger;

namespace Fermat
{
    public class Fermat
    {
        public (BigInteger, BigInteger) Factorize(string stringNumber)
        {
            BigInteger number = BigInteger.Parse(stringNumber);
          
            // Шаг 1
            BigInteger x = BigInteger.Sqrt(number);
            Console.WriteLine(x);

            if (BigInteger.Pow(x, 2).CompareTo(number) == 0)
            {
                // Число простое
                return (x, x);
            }

            x += 1;

            while (true)
            {
                // Шаг 2
                if (x == ((BigFloat)number + 1) / 2)
                {
                    // Число простое
                    return (new BigInteger(1), number);
                }

                BigFloat y = BigFloat.Sqrt(BigInteger.Pow(x, 2) - number, AccuracyGoal.InheritAbsolute);

                if (y.IsInteger)
                {
                    return ((x + (BigInteger)y), (x - (BigInteger)y));
                }

                x += 1;
            }
            


        }

        public (BigInteger, BigInteger) PrimeFactors((BigInteger, BigInteger) factors)
        {
            var n = factors.Item1 * factors.Item2;
            var phi = (factors.Item1 - 1) * (factors.Item2 - 1);

            var S = n - phi + 1;
            var p = (S + BigFloat.Sqrt(BigFloat.Pow(S, 2) - 4 * n)) * 0.5;

            var q = (S - BigFloat.Sqrt(BigFloat.Pow(S, 2) - 4 * n)) * 0.5;

            return ((BigInteger)p, (BigInteger)q);
        }
    }
}
