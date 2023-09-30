// write by Zelin Yi for CS 3500
// Last updated: September 2023

using SpreadsheetUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SS;

/// <summary>
/// empty Cells is not saved in dictionary, for we have infinity much. any valid
/// name empty cells are exist, with "" as content
///
/// dependencyGrapy save the relationship between cells, can use to check if
/// circular exist
///
/// Cell class include content and value field * content: contents of a cell in
/// Excel is what is displayed on the editing line when the cell is selected,
/// valid types are string, double, Formula * value: in Excel, is what displayed
/// on the cells space, so is "" when empty, allowed type are string double or
/// FormulaError
///
/// </summary>
public class Spreadsheet : AbstractSpreadsheet
{
    // define the input and return type for isValid and normalized function
    // isValid return bool of if input is valid
    private Func<string, bool> isValid;

    // return normalized string of input
    private Func<string, string> normalized;

    // save the relationship of cells
    private DependencyGraph dependencyGraph;

    // store all cells. dictionary will not store empty cells, cells not in
    //dictionary means they are empty
    [JsonInclude]
    public Dictionary<string, Cell> Cells;

    /// <summary>
    /// inner class Cell, store all required fields
    /// </summary>
    public class Cell
    {
        // store the content of cells, can be string, double, Formula
        internal object content;

        // store the value of cells, can be string double, or FormulaError
        internal object value;

        // string form of content, Json serialize and deserialize will use
        [JsonInclude]
        public string stringForm;

        // if its a double or text, then stringForm = conetnet.tosrting
        // if its a formula, stringForm = "=" + f.toString();
        [JsonConstructor]
        public Cell(string stringForm)
        {
            value = "";
            content = "";
            this.stringForm = stringForm;
        }

        // used when create cell, not use with json
        internal Cell(object content)
        {
            this.content = content;
            if (content is double)
            {
                stringForm = ((double)content).ToString();
                value = content;
            }
            else if (content is string)
            {
                stringForm = (string)content;
                value = content;
            }
            else
            {
                stringForm = "=" + ((Formula)content).ToString();
                value = "";
            }
        }
    }

    // load dictionary and set version for json deserialize
    [JsonConstructor]
    public Spreadsheet(Dictionary<string, Cell> Cells, string version) :
    this(s => true, s => s, version)
    {
        this.Cells = Cells;
    }

    /// <summary>
    /// zero-argument constructor, version is default, isValid and normalize function will do nothing
    /// </summary>
    public Spreadsheet() :
        this(s => true, s => s, "default")
    {
    }

    /// <summary>
    /// three-argument constructor, set version, isValid and normalize function
    /// </summary>
    public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) :
        base(version)
    {
        Changed = false;
        this.isValid = isValid;
        this.normalized = normalize;
        Cells = new Dictionary<string, Cell>();
        dependencyGraph = new DependencyGraph();
    }

    /// <summary>
    /// four-argument constructor, this will call JsonSerializer.Deserialize,
    /// use to load Spreadsheet from file
    /// </summary>
    public Spreadsheet(string location, Func<string, bool> isValid, Func<string, string> normalize, string version) :
        this(isValid, normalize, version)
    {
        try
        {
            string s = File.ReadAllText(location);
            Spreadsheet? ss = JsonSerializer.Deserialize<Spreadsheet>(s);

            if (ss.Version != this.Version)
            {
                throw new Exception();
            }

            foreach (var v in ss.Cells)
            {
                this.SetContentsOfCell(v.Key, v.Value.stringForm);
            }
        }
        catch (Exception ex)
        {
            throw new SpreadsheetReadWriteException(ex.Message);
        }
    }

    /// <summary>
    /// help method,
    /// calculate and set the value of cell with formula
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="formula"></param>
    /// <exception cref="ArgumentException"></exception>
    private void SetFormulaValue(Cell cell)
    {
        cell.value = ((Formula)cell.content).Evaluate(LookUp);
    }

    private double LookUp(string s)
    {
        // depend on empty cells
        if (!Cells.TryGetValue(s, out Cell? temp))
        {
            throw new ArgumentException("Formula depend on empty cell");
        }
        // value is not empty
        else if (temp.value is not double)
        {
            throw new ArgumentException("Formula depend on a cell without a numerical value");
        }
        // value is double
        else
        {
            return (double)(temp.value);
        }
    }

    /// <summary>
    /// help method if input is valid and match isValid function, return
    /// normalized input, otherwise throw exception
    /// </summary>
    /// <param name="s">check s is valid name</param>
    /// <returns></returns>
    /// <exception cref="InvalidNameException"></exception>
    private string ValidName(string s)
    {
        // match start with one or more letter or _, then end or then followed
        // by number till end
        if (!Regex.IsMatch(s, @"^[a-zA-Z_]+[0-9]*$") || !isValid(s))
        {
            throw new InvalidNameException();
        }
        return normalized(s);
    }

    /// <summary>
    /// return name list of all non-empty cells
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<string> GetNamesOfAllNonemptyCells()
    {
        return Cells.Keys.ToArray();
    }

    /// <summary>
    /// check if name is valid
    /// return contents of cell
    /// if cell is empty, return empty string
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override object GetCellContents(string name)
    {
        // ValidName() check validity and normalize it
        name = ValidName(name);
        if (Cells.ContainsKey(name)) { return Cells[name].content; }
        return "";
    }

    /// <summary>
    /// determine which set method it will call, and update value of cells
    /// before return the list of name plus the names of all other cells whose
    /// value depends, directly or indirectly, on the named cell
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public override IList<string> SetContentsOfCell(string name, string content)
    {
        // ValidName() check validity and normalize it
        name = ValidName(name);
        Changed = true;
        List<string> temp = new List<string>();
        // won't add empty cell into CellTabel
        if (content.Length == 0)
        {
            if (Cells.Remove(name))
            {
                dependencyGraph.ReplaceDependees(name, new List<string>());
            }
            return new List<string>();
        }
        else if (double.TryParse(content, out double number))
        {
            // content is double
            temp = SetCellContents(name, number).ToList();
        }
        else if (content[0] == '=')
        {
            // content is Formula, Formula class will throw exception if its not
            temp = SetCellContents(name, new Formula(content.Remove(0, 1), normalized, isValid)).ToList();
        }
        else
        {
            // content is string
            temp = SetCellContents(name, content).ToList();
        }

        // update value of cells
        Cells[name].stringForm = content;

        foreach (string s in temp)
        {
            if (Cells[s].content is Formula)
            {
                SetFormulaValue(Cells[s]);
            }
        }
        return temp;
    }

    /// <summary>
    /// help method
    /// set contents for cell. and clear dependees for dependencyGrapy
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj">only 3 cases: double, string, Formula</param>
    private void SetClearContents(string name, object obj)
    {
        // cell not exist in dictionary, create cell with context and value
        if (!Cells.ContainsKey(name)) { Cells.Add(name, new Cell(obj)); }
        // cell exist, then set content, value, and update value of cell depend on it
        else
        {
            Cell temp = Cells[name];
            temp.content = obj;
            temp.value = obj;
        }

        // set dependees into empty, and value equal content for double and string
        dependencyGraph.ReplaceDependees(name, new List<string>());
    }

    /// <summary>
    /// invalid name throws exception
    ///
    /// need to update old dependees, for it's number, dependees should be empty
    /// in logic dependent keep same
    ///
    /// if don't depend on any, return list only contain itself
    /// else return all direct, indirect cell name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    protected override IList<string> SetCellContents(string name, double number)
    {
        SetClearContents(name, number);
        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    /// invalid name throws exception
    ///
    /// need to update old dependees, for it's text, dependees should be empty
    /// in logic dependent keep same
    ///
    /// if don't depend on any, return list only contain itself
    /// else return all direct, indirect cell name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    protected override IList<string> SetCellContents(string name, string text)
    {
        SetClearContents(name, text);
        return GetCellsToRecalculate(name).ToList();
    }

    /// <summary>
    /// invalid name throws exception
    ///
    /// need to update old dependees
    ///
    /// if don't depend on any, return list only contain itself
    /// else return all direct, indirect cell name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    protected override IList<string> SetCellContents(string name, Formula formula)
    {
        bool exist = Cells.ContainsKey(name);
        // save cell and relationship before change
        List<string> dependee = dependencyGraph.GetDependees(name).ToList();
        List<string> dependent = dependencyGraph.GetDependents(name).ToList();
        object? content = null;
        object? value = null;
        // if already exist, save data, else add into Cells
        if (exist)
        {
            content = Cells[name].content;
            value = Cells[name].value;
            Cells[name].content = formula;
        }
        else { Cells.Add(name, new Cell(formula)); }

        // update dependencyGraph
        dependencyGraph.ReplaceDependees(name, formula.GetVariables());

        // get ordered list to calculate value, if exist circular, throw
        // exception and keep spreadsheet unchanged
        List<string> variableList = new List<string>();

        // exception throws means exist circular, so restore to previous
        try
        {
            variableList = GetCellsToRecalculate(name).ToList();
        }
        catch
        {
            if (content != null && value != null)
            {
                dependencyGraph.ReplaceDependees(name, dependee);
                dependencyGraph.ReplaceDependents(name, dependent);
                Cells[name].content = content;
                Cells[name].value = value;
            }
            else
            {
                Cells.Remove(name);
            }
            throw;
        }
        // only when no exception throws would reach here

        Cell cell = Cells[name];
        SetFormulaValue(cell);
        // Evaluate will handle exception into FormulaError
        return variableList;
    }

    /// <summary>
    /// returns direct dependent
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected override IEnumerable<string> GetDirectDependents(string name)
    {
        return dependencyGraph.GetDependents(ValidName(name));
    }

    /// <summary>
    /// save .txt file into filename with Json rule
    /// </summary>
    /// <param name="filename"></param>
    /// <exception cref="SpreadsheetReadWriteException"> throws when illegal location or file name</exception>
    public override void Save(string filename)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filename, jsonString);
        }
        catch
        {
            throw new SpreadsheetReadWriteException("incorrect location / fileName");
        }
    }

    /// <summary>
    /// return the value of cell, if it's empty cell, return "";
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override object GetCellValue(string name)
    {
        if (Cells.TryGetValue(ValidName(name), out Cell? cell))
            return Cells[ValidName(name)].value;
        else
            return "";
    }
}