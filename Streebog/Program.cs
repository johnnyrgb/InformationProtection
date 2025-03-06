using Streebog;

var streebog = new Streebog.Streebog(512);

var input = "Hello, world!";
var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
var result = streebog.Run(inputBytes);

// Convert the result to a base64 string
var resultBase64 = Convert.ToBase64String(result);
var resultString = BitConverter.ToString(result).Replace("-", "").ToLower();
Console.WriteLine(resultString);
Console.WriteLine(resultBase64);
