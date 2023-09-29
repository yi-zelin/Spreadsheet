// write by Zelin Yi for CS 3500
// Last updated: September 2023

using SpreadsheetUtilities;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    private Func<string, bool> isValid;
    private Func<string, string> normalized;

    // save the relationship of cells
    private DependencyGraph dependencyGraph;

    // store all cells. dictionary will not store empty cells, cells not in
    // dictionary means they are empty
    [JsonInclude]
    [JsonPropertyName("Cells")]
    public Dictionary<string, Cell> cellTable;

    /// <summary>
    /// inner class Cell, store all required fields
    /// </summary>
    public class Cell
    {
        // store the content of cells, can be string, double, Formula
        internal object content;
        // store the value of cells, can be string double, or FormulaError
        internal object value;


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

    [JsonConstructor]
    public Spreadsheet(Dictionary<string, Cell> dict, string version) :
        this(s => true, s => s, version)
    {
        // empty
        if (dict.Count == 0)
            return;
        cellTable = dict;
        List<string> names = cellTable.Keys.ToList();
        List<Cell> cells = cellTable.Values.ToList();
        HashSet<string> unknowValues = new HashSet<string>();

        for (int i = 0; i < names.Count; i++)
        {
            Cell currentCell = cells[i];
            string name = names[i];

            // is formula
            if (currentCell.stringForm.Count() != 0 && currentCell.stringForm[0] == '=')
            {
                try
                {
                    Formula f = new Formula(currentCell.stringForm.Remove(0, 1));
                    // is valid formula
                    currentCell.content = f;
                    foreach (string item in f.GetVariables())
                    {
                        dependencyGraph.AddDependency(item, name);
                    }
                    unknowValues.Add(name);
                }
                catch { throw new SpreadsheetReadWriteException("invalid formula exist"); }
            }

            // is double
            else if (double.TryParse(currentCell.stringForm, out double number))
            {
                currentCell.content = number;
                currentCell.value = number;
            }

            // is string
            else
            {
                currentCell.content = currentCell.stringForm;
                currentCell.value = currentCell.stringForm;
            }
        }
        // evaluate value of formula cells

    }

    /// <summary>
    /// zero-argument constructor
    /// </summary>
    public Spreadsheet() :
        this(s => true, s => s, "default")
    {
    }

    /// <summary>
    /// three-argument constructor
    /// </summary>
    public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) :
        base(version)
    {
        this.isValid = isValid;
        this.normalized = normalize;
        cellTable = new Dictionary<string, Cell>();
        dependencyGraph = new DependencyGraph();
    }


    /// <summary>
    /// four-argument constructor
    /// </summary>
    public Spreadsheet(string location, Func<string, bool> isValid, Func<string, string> normalize, string version) :
        this(isValid, normalize, version)
    {

    }

    /// <summary>
    /// help method, to update value of 
    /// only formula need to update value, for other data won't depend on other cells
    /// </summary>
    /// <param name="name"></param>
    private void UpdateValue(string name)
    {
        List<string> recalculate = new List<string>();
        for (int i = 0; i < recalculate.Count; i++)
        {
            foreach (string item in dependencyGraph.GetDependees(recalculate[i]))
            {
                if (recalculate.Contains(item))
                    continue;
                else
                    recalculate.Add(item);
            }
        }
        // here, we have a list of all depend to cell name

        foreach (string item in recalculate)
        {
            Cell cell = cellTable[item];
            SetFormulaValue(cell);
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
        cell.value = ((Formula)cell.content).Evaluate((s) =>
        {
            // depend on empty cells
            if (!cellTable.TryGetValue(s, out Cell? temp))
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
                return (double)temp.value;
            }
        });
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
        return cellTable.Keys.ToArray();
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
        if (cellTable.ContainsKey(name)) { return cellTable[name].content; }
        return "";
    }


    public override IList<string> SetContentsOfCell(string name, string content)
    {
        // ValidName() check validity and normalize it
        name = ValidName(name);

        // won't add empty cell into CellTabel
        if (content.Length == 0)
        {
            if (cellTable.Remove(name))
            {
                dependencyGraph.ReplaceDependees(name, new List<string>());
            }
            return new List<string>();
        }
        else if (double.TryParse(content, out double number))
        {
            // content is double
            return SetCellContents(name, number);
        }
        else if (content[0] == '=')
        {
            // content is Formula, Formula class will throw exception if its not
            return SetCellContents(name, new Formula(content.Remove(0, 1), normalized, isValid));
        }
        else
        {
            // content is string
            return SetCellContents(name, content);
        }
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
        if (!cellTable.ContainsKey(name)) { cellTable.Add(name, new Cell(obj)); }
        // cell exist, then set content, value, and update value of cell depend on it
        else
        {
            Cell temp = cellTable[name];
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
        bool exist = cellTable.ContainsKey(name);
        // save cell and relationship before change
        List<string> dependee = dependencyGraph.GetDependees(name).ToList();
        List<string> dependent = dependencyGraph.GetDependents(name).ToList();
        object? content = null;
        object? value = null;
        // if already exist, save data, else add into cellTable
        if (exist)
        {
            content = cellTable[name].content;
            value = cellTable[name].value;
        }
        else { cellTable.Add(name, new Cell(formula)); }

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
                cellTable[name].content = content;
                cellTable[name].value = value;
            }
            else
            {
                cellTable.Remove(name);
            }
            throw;
        }
        // only when no exception throws would reach here

        Cell cell = cellTable[name];
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


    public override void Save(string filename)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(filename, jsonString);
        }
        catch
        {
            throw;
            //throw new SpreadsheetReadWriteException("incorrect location / fileName");
        }
    }

    /// <summary>
    /// return the value of cell, if it's empty cell, return "";
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override object GetCellValue(string name)
    {
        if (cellTable.TryGetValue(ValidName(name), out Cell? cell))
            return cellTable[ValidName(name)].value;
        else
            return "";
    }
}