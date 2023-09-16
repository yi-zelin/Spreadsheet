// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private List<string> dataOfFormula;
    private HashSet<string> normFormVariables;

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        normFormVariables = new HashSet<string>();
        List<string> tempString = GetTokens(formula).ToList();
        int leftParentheses = 0;
        int rightParentheses = 0;
        int lastIndex = tempString.Count - 1;

        string opeR = @"^[\+\-*/\)]$";
        string opeL = @"^[\+\-*/\(]$";


        // One Token Rule
        // must have at least one token
        if (tempString.Count() == 0) { throw new FormulaFormatException("2 Violate One Token Rule"); }

        // Starting Token Rule
        // first number must be number, variable, or `(`
        if (!IsNumOrVar(tempString[0], isValid) && tempString[0] != "(")
        {
            throw new FormulaFormatException("5 Violate Starting Token Rule");
        }

        // Ending Token Rule
        // last number must be number, variable, or ')'
        if (!IsNumOrVar(tempString[lastIndex], isValid) && tempString[lastIndex] != ")")
        {
            throw new FormulaFormatException("6 Violate Ending Token Rule");
        }

        for (int i = 0; i <= tempString.Count - 1; i++)
        {
            // format integers, scientific notation to double
            // 
            // "3.0000", "3", "3.", "3.0"   => "3"
            // "3.1000", "3.1"              => "3.1"
            // "3e3", "3E3", "3000"         => "3000"
            if (Formula.IsDoubleNum(tempString[i]))
            {
                tempString[i] = $"{double.Parse(tempString[i])}";
            }

            tempString[i] = normalize(tempString[i]);
            string current = tempString[i];

            if (current == "(") { leftParentheses++; }
            else if (current == ")") { rightParentheses++; }

            // Right Parentheses Rule
            // numb of `(` must always >= numb of `)`
            if (leftParentheses < rightParentheses) { throw new FormulaFormatException("3 Violate Right Parentheses Rule"); }

            // Parenthesis/Operator Following Rule
            // must have sth followed by `(` or operator
            // the follower must be number, variable or `(`
            if (Regex.IsMatch(current, opeL))
            {
                // throw Exception or the follower is not (number, variable, `(`)
                // if don't have follower, this will hit ending rule
                if (!IsNumOrVar(tempString[i + 1], isValid) && tempString[i + 1] != "(")
                {
                    throw new FormulaFormatException("7 Violate Parenthesis/Operator Following Rule");
                }
            }

            // Extra Following Rule
            // if number, variable or `)` have follower, the follower must be operator or `)`
            if (IsNumOrVar(current, isValid) || current == ")")
            {
                if (i != lastIndex && !Regex.IsMatch(tempString[i + 1], opeR)) { throw new FormulaFormatException("8 Violate Extra Following Rule"); }

                // add variable into normFormOperator
                if (IsValidVar(current, isValid)) { normFormVariables.Add(current); }
            }
        }

        // Balanced Parentheses Rule
        // # of `(` must equal # of `)`
        if (leftParentheses != rightParentheses) { throw new FormulaFormatException("4 Violate Balanced Parentheses Rule"); }

        dataOfFormula = tempString;
    }


    /// <summary>
    /// help method, static for I want to use this in constructor. being used
    /// multiple times, make program easier to read and debug.
    /// number matches: 3e4, 3E4, 3.4, 34
    /// </summary>
    /// <param name="s">check if s is number or variable</param>
    /// <param name="isValid">isValid function</param>
    /// <returns>true if s is number or variable</returns>
    private static bool IsValidVar(string s, Func<string, bool> isValid)
    {
        if (Regex.IsMatch(s, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
        {
            if (!isValid(s)) { throw new FormulaFormatException("Invalid Variable"); }
            return true;
        }
        return false;
    }

    private static bool IsNumOrVar(string s, Func<string, bool> isValid)
    {
        return IsDoubleNum(s) || IsValidVar(s, isValid);
    }

    private static bool IsDoubleNum(string s)
    {
        try
        {
            double.Parse(s);
            return true;
        }
        catch { return false; }
    }

    /// <summary>help method to analyze "+" or "-"</summary>
    private bool IsPlusOrSubt(String s) => s == "+" || s == "-";

    /// <summary>help method to analyze "*" or "/"</summary>
    private bool IsMulOrDiv(String s) => s == "*" || s == "/";

    /// <summary>help method check if s in HashSet</summary>
    private bool IsVar(string s) => normFormVariables.Contains(s);


    /// <summary>
    /// a help method to calculate infix expression
    /// </summary>
    /// <param name="num1">first int number</param>
    /// <param name="opr">the operator</param>
    /// <param name="num2">second int number</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">throw when division by zerr</exception>
    /// <exception cref="ArgumentException">if throws unexpectied issue, means program has bug</exception>
    private static double Calculate(double num1, String opr, double num2)
    {
        if (opr == "+") { return num1 + num2; }
        else if (opr == "-") { return num1 - num2; }
        else if (opr == "*") { return num1 * num2; }
        else
        {
            if (num2 == 0) { throw new ArgumentException("A division by zero occurs"); }
            return num1 / num2;
        }
    }


    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<double> values = new Stack<double>();
        Stack<String> operators = new Stack<String>();

        foreach (String item in dataOfFormula)
        {
            // int number or variable as int number
            if (IsVar(item) || IsDoubleNum(item))
            {
                double doubleItem;
                if (IsVar(item))
                {
                    // for lookup return double, so require it throw exception
                    // when undefined variables occur
                    try
                    {
                        doubleItem = lookup(item);
                    }
                    catch (Exception e)
                    {
                        return new FormulaError(e.Message);
                    }
                }
                else { doubleItem = int.Parse(item); }

                // operator stack is not empty and '*' or '/' is at the top of the operator stack
                if (operators.Count != 0 && IsMulOrDiv(operators.Peek()))
                {
                    try
                    {
                        values.Push(Calculate(values.Pop(), operators.Pop(), doubleItem));
                    }
                    catch (ArgumentException e) { return new FormulaError(e.Message); }
                    continue;
                }

                // operator stack is empty or * / is not at top of opr stack
                values.Push(doubleItem);
            }

            // is "+" or "-"
            else if (IsPlusOrSubt(item))
            {
                if (operators.Count != 0 && IsPlusOrSubt(operators.Peek()))
                {
                    double doubleItem = values.Pop();
                    values.Push(Calculate(values.Pop(), operators.Pop(), doubleItem));
                }
                operators.Push(item);
            }

            //is "*", "/", "("
            else if (item == "*" || item == "/" || item == "(") { operators.Push(item); }

            // is ")"
            else if (item == ")")
            {
                if (operators.Count != 0)
                {
                    if (IsPlusOrSubt(operators.Peek()))
                    {
                        double doubleItem = values.Pop();
                        values.Push(Calculate(values.Pop(), operators.Pop(), doubleItem));
                    }
                }
                operators.Pop();
                if (operators.Count != 0 && IsMulOrDiv(operators.Peek()))
                {
                    double doubleItem = values.Pop();
                    try
                    {
                        values.Push(Calculate(values.Pop(), operators.Pop(), doubleItem));
                    }
                    catch (ArgumentException e) { return new FormulaError(e.Message); }
                }
            }
        }

        // three cases when last token has been processed
        // Case 1:  Operator stack is empty
        if (values.Count == 1 && operators.Count == 0) { return values.Pop(); }

        // Case 2:  Operator stack is not empty
        else
        {
            double doubleItem = values.Pop();
            return Calculate(values.Pop(), operators.Pop(), doubleItem);
        }

    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        return normFormVariables;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return string.Join("", dataOfFormula.ToArray());
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formula are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (!(obj is Formula)) { return false; }
        return GetHashCode() == obj.GetHashCode();
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !(f1 == f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formula have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
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
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}
