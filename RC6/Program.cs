// See https://aka.ms/new-console-template for more information

using RC6;

string origin = "iom-16-rus.jpg";
string modified = "iom-16-rus.jpg.rc6";
string keys = "iom-16-rus.jpg.key";
RC6Old.Encrypt(origin);
Console.WriteLine();
RC6Old.Decrypt(modified, keys);