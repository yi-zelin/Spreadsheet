using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace FormulaTests
{


    [TestClass]
    public class FormulaTests
    {
        /// <summary>
        /// analyze if throws correct exception with match message little part
        /// from Assert.ThrowsException source code write this method for
        /// ThrowsException can't check if message match when throws exception.
        /// </summary>
        /// <typeparam name="T">Exception type</typeparam>
        /// <param name="f">action</param>
        /// <param name="s">string to match</param>
        /// <returns></returns>
        private void ExceptionWithMsg<T>(Func<object> f, string s) where T : Exception
        {
            try
            {
                f();
                Assert.Fail("no exception throws");
            }
            catch (Exception e)
            {
                bool res = typeof(T)!.Equals(((object)e).GetType()) && Regex.IsMatch(e.Message, s);
                // not an correct exception
                if (!typeof(T)!.Equals(((object)e).GetType()))
                {
                    Assert.Fail("incorrect Exception type: " + ((object)e).GetType());
                }
                // correct exception but not include message
                else if (!Regex.IsMatch(e.Message, s))
                {
                    Assert.Fail("incorrect Message: " + e.Message);
                }
            }
        }


        private string N(string input)
        {
            return input.ToUpper();
        }

        private bool V(string input)
        {
            if (input.Length != 2)
                return false;

            return char.IsLetter(input[0]) && char.IsDigit(input[1]);
        }

        private IEnumerable<string> GetTokens(string formula)
        {
            // Patterns for individual tokens
            string lpPattern = @"\(";
            string rpPattern = @"\)";
            string opPattern = @"[\+\-*/]";
            string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            string spacePattern = @"\s+";

            // Overall pattern
            string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }

        [TestMethod]
        public void FormulaConstructorTest()
        {
            // help to analyze what's the exactly problem 
            List<string> a = GetTokens("9.-1").ToList();
            foreach (string s in a)
            {
                Console.WriteLine(s);
            }
            List<string> names = new List<string>() { "John", "Anna", "Monica" };
            var result = String.Join("", names.ToArray());
            Console.WriteLine(result);
            Console.WriteLine("" + double.Parse("3e3"));
            Console.WriteLine("" + double.Parse("3E3"));
            Console.WriteLine("" + double.Parse("3000"));


            // correct case
            Assert.IsNotNull(new Formula("x1+y3", N, V));
            Assert.IsNotNull(new Formula("3e5", N, V));
            Assert.IsNotNull(new Formula("3E5", N, V));
            Assert.IsNotNull(new Formula("9.4", N, V));
            Assert.IsNotNull(new Formula("9.4000000", N, V));
            Assert.IsNotNull(new Formula("9.", N, V));

            // Ruin One Token Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("       ", N, V), "One Token Rule");

            // Ruin Right Parentheses Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("3 + 1)", N, V), "Right Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(3 + 1))", N, V), "Right Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(3 + x2))", N, V), "Right Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(((x3 + x2))))", N, V), "Right Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(x3 + x2) * 8 -1 )", N, V), "Right Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x3)))))", N, V), "Right Parentheses Rule");

            // Ruin Balanced Parentheses Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("((1)", N, V), "Balanced Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1)+(6 + 6", N, V), "Balanced Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("((((((1)", N, V), "Balanced Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("((3 + 1)", N, V), "Balanced Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(((((x3 + x2))))", N, V), "Balanced Parentheses Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(((((x3 + x2))))+((((1)", N, V), "Balanced Parentheses Rule");

            // Ruin Starting Toke Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x+y3", N, V), "Starting Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x22+y3", N, V), "Starting Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula(")+1", N, V), "Starting Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("+1", N, V), "Starting Token Rule");

            // Ruin Ending Token Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+yy", N, V), "Ending Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+", N, V), "Ending Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+(", N, V), "Ending Token Rule");

            // Ruin Parenthesis/Operator Following Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(+1)", N, V), "Parenthesis/Operator Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+)", N, V), "Parenthesis/Operator Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("()", N, V), "Parenthesis/Operator Following Rule");

            // Ruin Extra Following Rule
            // for 2x will sliced to 2 X two element by GetTokens, so hit extra following rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("2x+y3", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+x5)2+x3", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+x5(2+x3))", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1 1 + x5*(2+x3))", N, V), "Extra Following Rule");

            // negative and invalid symbol

        }

        [TestMethod]
        public void FormulaEvaluateTest()
        {
            Assert.AreEqual(300000.0,new Formula("3e5", N, V).Evaluate(s=>1));

            Assert.AreEqual(2.0,new Formula("s1*(3-2)+1", N, V).Evaluate(s=>1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(7.0, new Formula("2+4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1*s1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("5.0", N, V).Evaluate(s => 1));

            Assert.IsTrue((5/12 - (double) new Formula("(2+3)/(4+8)", N, V).Evaluate(s => 1)) <= 0.0000001);

            Console.WriteLine(new Formula("5/0", N, V).Evaluate(s => 1));
        }
    }
}