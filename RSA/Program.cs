var fermat = new Fermat.Fermat();

var test = fermat.Factorize("1342127");
Console.WriteLine("Тест:");
Console.WriteLine($"p = {test.Item1}, q = {test.Item2}");

var factors = fermat.Factorize("115792089237316195423570985008721211221144628262713908746538761285902758367353");
Console.WriteLine("Большое число из задания:");
Console.WriteLine($"p = {factors.Item1}, q = {factors.Item2}");

Console.WriteLine(factors.Item1 * factors.Item2);

var primeFactors = fermat.PrimeFactors(factors);
Console.WriteLine("p = " + primeFactors.Item1 + "\n" + "q = " + primeFactors.Item2);  
Console.WriteLine("n = " + primeFactors.Item1 * primeFactors.Item2);