using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// evaluate infix expression. 
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// variable -> int, to get int value of a variable
        /// </summary>
        /// <param name="v">variable</param>
        /// <returns>int value of the variable</returns>
        public delegate int Lookup(String v);


        ///<summary>help methods to analyze if parameter is a int number</summary> 
        private static bool IsIntNum(String s)
        {
            s = s.Trim();
            return s.All(char.IsDigit);
        }

        ///<summary>help methods to analyze if parameter is a valid variable</summary> 
        private static bool IsVar(String s)
        {
            /// using regular expression to match String start with letter, then followed by number
            string pattern = @"^[A-Za-z]+[0-9]+$";
            return Regex.IsMatch(s, pattern);
        }

        /// <summary>help methods to analyze if parameter is valid operator</summary>
        private static bool isOperator(String s) { return s == "+" || s == "-" || s == "*" || s == "/" || s == "(" || s == ")"; }

        /// <summary>help method to analyze "+" or "-"</summary>
        private static bool IsPlusOrSubt(String s) { return s == "+" || s == "-"; }

        /// <summary>help method to analyze "*" or "/"</summary>
        private static bool IsMulOrDiv(String s) { return s == "*" || s == "/"; }


        /// <summary>
        /// a help method to calculate infix expression
        /// </summary>
        /// <param name="num1">first int number</param>
        /// <param name="opr">the operator</param>
        /// <param name="num2">second int number</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">throw when division by zerr</exception>
        /// <exception cref="ArgumentException">if throws unexpectied issue, means program has bug</exception>
        private static int Calculate(int num1, String opr, int num2)
        {
            if (opr == "+") { return num1 + num2; }
            else if (opr == "-") { return num1 - num2; }
            else if (opr == "*") { return num1 * num2; }
            else if (opr == "/")
            {
                if (num2 == 0) { throw new ArgumentException("A division by zero occurs"); }
                return num1 / num2;
            }
            throw new ArgumentException("unexpectied issue occur in Calculate method with num1 = " + num1 + " num2 = " + num2 + " opr = " + opr);
        }


        /// <summary>
        /// the core code. make different action based on different condition.
        /// </summary>
        /// <param name="exp">the original infix expression</param>
        /// <param name="variableEvaluator">to get the int number from variable</param>
        /// <returns>returns Integer result of original infix expression</returns>
        /// <exception cref="ArgumentException">thorws when any issue occurs</exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            if (String.IsNullOrWhiteSpace(exp)) throw new ArgumentException("empty or white space input");
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            Stack<int> values = new Stack<int>();
            Stack<String> operators = new Stack<String>();

            foreach (String item in substrings)
            {
                // meet empty or null item, continue
                if (String.IsNullOrWhiteSpace(item)) { continue; }

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
                else if (item == "*" || item == "/" || item == "(") { operators.Push(item); }

                // is ")"
                else if (item == ")")
                {
                    if (operators.Count != 0 && IsPlusOrSubt(operators.Peek()))
                    {
                        if (values.Count <= 1) { throw new ArgumentException("value stack is less than two"); }
                        int intItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));
                    }
                    if (operators.Count == 0 || operators.Pop() != "(") { throw new ArgumentException("'(' isn't found where expected"); }
                    if (operators.Count != 0 && IsMulOrDiv(operators.Peek()))
                    {
                        if (values.Count <= 1) { throw new ArgumentException("value stack is less than two"); }
                        int intItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), intItem));

                    }
                }

                // for an invalid input, throw exception
                else { throw new ArgumentException(item + " is not a valid input"); }
            }

            // three cases when last token has been processed
            // Case 1:  Operator stack is empty
            if (values.Count == 1 && operators.Count == 0) { return values.Pop(); }

            // Case 2:  Operator stack is not empty
            else if (values.Count == 2 && operators.Count == 1)
            {
                int intItem = values.Pop();
                return Calculate(values.Pop(), operators.Pop(), intItem);
            }

            // lack operator or number
            else { throw new ArgumentException("lack operator or number"); }
        }
    }
}