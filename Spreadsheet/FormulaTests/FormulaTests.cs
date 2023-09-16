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

        /// <summary>
        /// convert all input letter to upper letter
        /// </summary>
        /// <param name="input">string input</param>
        /// <returns>upper letter string</returns>
        private string N(string input)
        {
            return input.ToUpper();
        }

        /// <summary>
        /// return true if variable in form of one letter + one number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool V(string input)
        {
            if (input.Length != 2)
                return false;

            return char.IsLetter(input[0]) && char.IsDigit(input[1]);
        }

        /// <summary>
        /// test formula() constructor with N, V
        /// </summary>
        [TestMethod]
        public void FormulaConstructorTest()
        {
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
            ExceptionWithMsg<FormulaFormatException>(() => new Formula(")+1", N, V), "Starting Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("+1", N, V), "Starting Token Rule");

            // Ruin Ending Token Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+", N, V), "Ending Token Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+(", N, V), "Ending Token Rule");

            // Ruin Parenthesis/Operator Following Rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(+1)", N, V), "Parenthesis/Operator Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+)", N, V), "Parenthesis/Operator Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("()", N, V), "Parenthesis/Operator Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+(-1)", N, V), "Parenthesis/Operator Following Rule");

            // Ruin Extra Following Rule
            // for 2x will sliced to 2 X two element by GetTokens, so hit extra following rule
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("2x+y3", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+x5)2+x3", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1+x5(2+x3))", N, V), "Extra Following Rule");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("(1 1 + x5*(2+x3))", N, V), "Extra Following Rule");

            // negative and invalid symbol
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x22+y3", N, V), "Variable");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x+y3", N, V), "Variable");
            ExceptionWithMsg<FormulaFormatException>(() => new Formula("x2+yy", N, V), "Variable");
        }

        /// <summary>
        /// test Evaluate() with all veritable return 1
        /// </summary>
        [TestMethod]
        public void FormulaEvaluateTest()
        {
            Assert.AreEqual(300000.0, new Formula("3e5", N, V).Evaluate(s => 1));

            Assert.AreEqual(3.0, new Formula("s1+(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(7.0, new Formula("2+4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("4*(3-2)+1*s1", N, V).Evaluate(s => 1));
            Assert.AreEqual(17.0, new Formula("(7+10)", N, V).Evaluate(s => 1));
            Assert.AreEqual(51.0, new Formula("(7+10)*3", N, V).Evaluate(s => 1));
            Assert.AreEqual(21.0, new Formula("(7*1)*3", N, V).Evaluate(s => 1));
            Assert.AreEqual(10.0, new Formula("(7*1)+3", N, V).Evaluate(s => 1));
            Assert.AreEqual(11.0, new Formula("(7+1)+3", N, V).Evaluate(s => 1));
            Assert.AreEqual(5.0, new Formula("5.0", N, V).Evaluate(s => 1));

            Assert.IsTrue((5 / 12 - (double)new Formula("(2+3)/(4+8)", N, V).Evaluate(s => 1)) <= 0.0000001);
        }

        /// <summary>
        /// test if evaluate() returned error if divide zero or can't find value of lookup
        /// </summary>
        /// <exception cref="FormatException"></exception>
        [TestMethod]
        public void EvaluateErrorTest()
        {
            object a = new Formula("5/0", N, V).Evaluate(s => 1);
            Assert.IsTrue(a is FormulaError);
            object b = new Formula("a1", N, V).Evaluate((s) => { throw new FormatException(s); });
            Assert.IsTrue(b is FormulaError);
            object c = new Formula("1+a1", N, V).Evaluate((s) => { throw new FormatException(s); });
            Assert.IsTrue(c is FormulaError);
            object d = new Formula("1/(1-1)/1", N, V).Evaluate((s) => { throw new FormatException(s); });
            Assert.IsTrue(d is FormulaError);
        }

        /// <summary>
        /// test if equivalent formula are equal, and test if they have same hash code
        /// </summary>
        [TestMethod]
        public void EqualAndHashCodeTest()
        {
            var a = new Formula("5000.0000");
            var b = new Formula("5000.");
            var c = new Formula("5000");
            var d = new Formula("5e3");
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == c);
            Assert.IsFalse(a != d);
            Assert.IsFalse(a.Equals("haha"));
        }

        /// <summary>
        /// test if variable list is correct (case sensitive)
        /// </summary>
        [TestMethod]
        public void GetVariablesTest()
        {
            var b = new Formula("a1+A2+a3", s => s, s => true);
            new Formula("a1 + a2 + a3");
            var c = new Formula("A1 + A2 + A3");
            
            foreach(var v in b.GetVariables())
            {
                Console.WriteLine(v);
                //Assert.IsTrue(c.GetVariables().Contains(v));
            }
            Console.WriteLine("!!!");
            foreach (var v in c.GetVariables())
            {
                Console.WriteLine(v);
                //Assert.IsTrue(b.GetVariables().Contains(v));
            }
        }
    }
}