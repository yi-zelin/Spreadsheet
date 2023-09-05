using FormulaEvaluator;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp1
{
    internal class Program
    {
        /// <summary>return 1 for any variable</summary>
        static int One(String a) { return 1; }

        /// <summary>
        /// AssertEqual method, to test Evaluator
        /// </summary>
        /// <param name="a">the result we get by using Evaluator</param>
        /// <param name="expected">expected answer</param>
        /// <exception cref="ArgumentException">thorws when expected not match the result we get</exception>
        static void AssertEqual(int a, int expected)
        {
            if (a == expected) Console.WriteLine("success!");
            else
            {
                throw new ArgumentException("unsuccess, expected: " + expected + "get: " + a);
            }
        }

        /// <summary>
        /// to tast cases witch shold throws exception
        /// </summary>
        /// <param name="s">infix expression with issue</param>
        /// <exception cref="ArgumentException">thorws when Evaluator didn't throws any exception</exception>
        static void ThrowsException(String s)
        {
            bool a = false;
            try
            {
                Evaluator.Evaluate(s,One);
            } catch (Exception e)
            {
                Console.WriteLine("success! with "+ e.Message);
            }
                if (a) throw new ArgumentException("unsuccess, for no exception throws");
        }
        static void Main(string[] args) {

            FormulaEvaluator.Evaluator.Lookup a = One;
            try
            {
                AssertEqual(Evaluator.Evaluate("s1*(3-2)+1", a),2);
                AssertEqual(Evaluator.Evaluate("4*(3-2)+1", a),5);
                AssertEqual(Evaluator.Evaluate("2+4*(3-2)+1", a),7);
                AssertEqual(Evaluator.Evaluate("4*(3-2)+1", a),5);
                AssertEqual(Evaluator.Evaluate("4*(3-2)+1*s1", a),5);
                AssertEqual(Evaluator.Evaluate("4*(3-2)*3+1/s1", a),13);
                AssertEqual(Evaluator.Evaluate("(2+3)/(4+8)", a),0);
                AssertEqual(Evaluator.Evaluate("5", a),5);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            /// <summary>test invalid input, or other cases which shold throw ArgumentException</summary>

            try
            {
                ThrowsException("    ");            // empty input
                ThrowsException("");                // null input
                ThrowsException("4/0");             // division zero
                ThrowsException("1+5-");            // lack number

                ThrowsException("1+5 7");           // lack operator
                ThrowsException("7+(9*1");          // lack operator
                ThrowsException("7+9*1)");          // lack operator
                ThrowsException("9a * 3");          // invalid variable

                ThrowsException("a9a * 3");         // invalid variable
                ThrowsException("A * 3");           // invalid variable
                ThrowsException("7c+1");            // invalid variable
                ThrowsException("774a");            // invalid variable

                ThrowsException("+ +");             // invalid input
                ThrowsException("9 $ 10");          // invalid input
                ThrowsException("9$10");            // invalid input
            } catch (Exception e)
            {
                Console.WriteLine("\n\n!!!!!Test Failed!!!\n\n");
            }



            /// <summary>all be commented code are used to test help method. for they are not public after test, so commented</summary>

            /// <summary>this one is to test Calculate() help method</summary>
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


            /// <summary>this one is to test IsVar() help method</summary>
            /*String[] strings = { "A", "A2a","cc2c","5","7c","680A","32A34","6a","66a6","aaa3a" };
            foreach (var item in  strings)
            {
                 Console.WriteLine(Evaluator.IsVar(item));
            }
            Console.WriteLine(Evaluator.IsVar("A2"));*/


        }
    }
}