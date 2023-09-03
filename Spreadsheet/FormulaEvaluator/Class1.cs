using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /*
         * is integer
         * help methods to analyze if is int number
         */
        private static bool IsIntNum(String s)
        {
            return s.All(char.IsDigit);
        }

        /*
         * is a variable
         * using regular expression to match begin with >=1 letter, end with >=1 number, and nothing in between
         */
        private static bool IsVar(String s)
        {
            string pattern = @"^[A-Za-z]+[0-9]+$";
            return Regex.IsMatch(s, pattern);
        }

        // is valid operator
        private static bool isOperator(String s)
        {
            return s == "+" || s == "-" || s == "*" || s == "/" || s == "(" || s == ")";
        }

        // is + or -
        private static bool IsPlusOrSubt(String s)
        {
            return s == "+" || s == "-";
        }

        // is * or /
        private static bool IsMulOrDiv(String s)
        {
            return s == "*" || s == "/";
        }

        /* a help method to calculate int operate int case
         * throw: ArgumentException when division by zero
         */
        private static int Calculate(int num1, String opr, int num2)
        {
            if (opr == "+") { return num1 + num2; }
            else if (opr == "-") {  return num1 - num2; }
            else if (opr == "*") {  return num1 * num2; }
            else if (opr == "/") { 
                if (num2 == 0) { throw new ArgumentException("A division by zero occurs"); }
                return num1 / num2; 
            }
            throw new ArgumentException("unexpectied issue occur in Calculate method with num1 = "+num1+" num2 = "+num2+" opr = "+opr);
        }





        public static int Evaluate(String exp, Lookup variableEvaluator) //, Lookup variableEvaluator
        {
            string[] substrings = Regex.Split(exp.Trim(), "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            foreach (String item in substrings)
            {
                // meet empty or null item, continue
                if (String.IsNullOrEmpty(item))
                {
                    continue;
                }

                // int number or variable as int number
                else if (IsIntNum(item) || IsVar(item))
                {
                    int intItem;
                    if (IsVar(item)) { intItem = variableEvaluator(item); } 
                    else { intItem = int.Parse(item); }

                    // operator stack is not empty and '*' or '/' is at the top of the operator stack
                    if (operators.Count != 0 && IsMulOrDiv(operators.Peek()))
                    {
                        // throw excepetion if value stack is empty
                        if (values.Count == 0) { throw new ArgumentException("value stack is empty"); }
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));
                        continue;
                    }

                    // operator stack is empty or * / is not at top of opr stack
                    values.Push(intItem);
                }

                // is "+" or "-"
                else if (IsPlusOrSubt(item))
                {
                    if (operators.Count != 0 && IsPlusOrSubt(operators.Peek()))
                    {
                        if (values.Count <= 1) { throw new ArgumentException("value stack is less than two"); }
                        int intItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));
                    }
                    operators.Push(item);
                }

                //is "*", "/", "("
                else if (item == "*" || item == "/" || item  == "(")
                {
                    operators.Push(item);
                }

                // is "("
                else if (item == ")")
                {
                    if (operators.Count != 0 && IsPlusOrSubt(operators.Peek()))
                    {
                        if (values.Count <= 1) { throw new ArgumentException("value stack is less than two"); }
                        int intItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));

                    }
                    if (operators.Pop() != "(") { throw new ArgumentException("'(' isn't found where expected"); }
                    if (operators.Count != 0 && IsMulOrDiv(operators.Peek()))
                    {
                        if (values.Count <= 1) { throw new ArgumentException("value stack is less than two"); }
                        int intItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));

                    }
                }

                // for an invalid input, throw exception
                else
                {
                    throw new ArgumentException(item + " is not a valid inpumt");
                }
            }


            Console.WriteLine(values.Count());
            Console.WriteLine(operators.Count());
            Console.WriteLine(values.Pop());
            Console.WriteLine("!!!!");

            // test
            /*foreach (var item in values)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("!!!!!");
            foreach (var item in operators)
            {
                Console.WriteLine(item);
            }*/

            // return values.Pop();
            return 1;
        }
    }
}