using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

/**
 * @ author:      Zelin Yi
 * @ github id:   yi-zelin 
 * @ UID:         u1442451
 * @ Date:        Sep. 3. 2023
 */
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
            s = s.Trim();
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
            //string[] substrings = Regex.Split(Regex.Replace(exp, @"\s+", ""), "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
           /* foreach (var v in substrings)
            {
                Console.WriteLine(v);
            }*/

            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            foreach (String item in substrings)
            {
                // meet empty or null item, continue
                if (String.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                // int number or variable as int number
                else if (IsIntNum(item) || IsVar(item))
                {
                    int intItem;
                    if (IsVar(item)) { intItem = variableEvaluator(item); } 
                    else { 
                        intItem = int.Parse(item); }

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
                    throw new ArgumentException(item + " is not a valid input");
                }
            }

            // two cases when last token has been processed
            // Operator stack is empty
            if (values.Count == 1 && operators.Count == 0) { return values.Pop(); }

            // Operator stack is not empty
            else if (values.Count == 2 && operators.Count == 1)
            {
                int intItem = values.Pop();
                return Calculate(values.Pop(),operators.Pop(), intItem);
            }

            // unknow cases, bug if appear
            else { throw new ArgumentException("an unexpected exception occur in result stack"); }
        }
    }
}