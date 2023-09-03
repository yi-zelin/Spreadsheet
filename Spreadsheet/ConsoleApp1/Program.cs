using FormulaEvaluator;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp1
{
    internal class Program
    {
        static int One(String a) { return 1; }

        static void Main(string[] args) {

            //Evaluator.Evaluate("(2+35)*A7");
            /*String[] strings = { "A", "A2a","cc2c","5","7c","680A","32A34","6a","66a6","aaa3a" };
            foreach (var item in  strings)
            {
                 Console.WriteLine(Evaluator.IsVar(item));
            }
            Console.WriteLine(Evaluator.IsVar("A2"));*/


            FormulaEvaluator.Evaluator.Lookup a = One;
            try
            {
                Evaluator.Evaluate("1+1*2", a);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

           /* try
            {
                Console.WriteLine(Evaluator.Calculate(100, "+", 500));
                Console.WriteLine(Evaluator.Calculate(100, "-", 500));
                Console.WriteLine(Evaluator.Calculate(100, "*", 500));
                Console.WriteLine(Evaluator.Calculate(100, "/", 500));
                Console.WriteLine(Evaluator.Calculate(100, "^", 500));
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }*/
            

        }
    }
}