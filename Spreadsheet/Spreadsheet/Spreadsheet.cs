// write by Zelin Yi for CS 3500
// Last updated: September 2023

using SpreadsheetUtilities;
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
    private DependencyGraph dependencyGraph;

    // this dictionary will not store empty cells, cells not in dictionary means
    // they are empty
    private Dictionary<string, Cell> cellTable;

    /// <summary>
    /// inner class Cell, store all required fields
    /// </summary>
    private class Cell
    {
        // don't need name, for name are saved in dictionary, when we get Cell
        // form dictionary, which means we already have the name

        internal object content;
        internal object? value;

        internal Cell(object content)
        {
            this.content = content;
            value = null;
        }
    }

    /// <summary>
    /// constructor to create an empty Spreadsheet
    /// </summary>
    public Spreadsheet()
    {
        dependencyGraph = new DependencyGraph();
        cellTable = new Dictionary<string, Cell>();
    }

    /// <summary>
    /// help method
    /// if input is valid, return original input, otherwise throw exception
    /// </summary>
    /// <param name="s">check s is valid name</param>
    /// <returns></returns>
    /// <exception cref="InvalidNameException"></exception>
    private string ValidName(string s)
    {
        // match start with one or more letter or _, then end or then followed
        // by number till end
        if (!Regex.IsMatch(s, @"^[a-zA-Z_]+[0-9]*$"))
        {
            throw new InvalidNameException();
        }
        return s;
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
        // ValidName() checked validity
        if (cellTable.ContainsKey(ValidName(name))) { return cellTable[name].content; }
        return "";
    }

    /// <summary>
    /// help method
    /// set contents for cell.
    /// name check, circular check, dependees update for dependencyGrapy
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj">only 3 cases: double, string, Formula</param>
    private void SetContents(string name, object obj)
    {
        // cell not exist in dictionary
        if (!cellTable.ContainsKey(ValidName(name)))
            cellTable.Add(name, new Cell(obj));

        // here cell exist, then set content
        cellTable[name].content = obj;

        // only if context is formula might have a not empty dependees list
        if (obj is Formula)
        {
            dependencyGraph.ReplaceDependees(name, ((Formula)obj).GetVariables());
            return;
        }

        // else update dependees into empty, and value equal content for double and string
        dependencyGraph.ReplaceDependees(name, new List<string>());
        cellTable[name].value = obj;
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
    public override IList<string> SetCellContents(string name, double number)
    {
        SetContents(name, number);
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
    public override IList<string> SetCellContents(string name, string text)
    {
        SetContents(name, text);
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
    public override IList<string> SetCellContents(string name, Formula formula)
    {
        SetContents(name, formula);
        return GetCellsToRecalculate(name).ToList();
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
}