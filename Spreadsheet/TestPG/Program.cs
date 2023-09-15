using System.Text.RegularExpressions;

namespace TestPG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String opeR = @"^[\+\-*/\)]$";
            String opeL = @"[\+\-*/\(]";

            Console.WriteLine(Regex.IsMatch("a+",opeR));
            Console.WriteLine(Regex.IsMatch("-",opeR));
            Console.WriteLine(Regex.IsMatch("*",opeR));
            Console.WriteLine(Regex.IsMatch("/",opeR));
            Console.WriteLine(Regex.IsMatch(")",opeR));
            Console.WriteLine(Regex.IsMatch("[",opeR));

            Console.WriteLine(Regex.IsMatch("+", opeL));
            Console.WriteLine(Regex.IsMatch("-", opeL));
            Console.WriteLine(Regex.IsMatch("*", opeL));
            Console.WriteLine(Regex.IsMatch("/", opeL));
            Console.WriteLine(Regex.IsMatch("(", opeL));
            Console.WriteLine(Regex.IsMatch(")", opeL));

            Console.WriteLine(Regex.IsMatch("908324", "^[0-9]+$"));
        }
    }
}