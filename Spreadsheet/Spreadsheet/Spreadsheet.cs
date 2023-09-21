using SpreadsheetUtilities;
using SS;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS;

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
        internal readonly Type[] contentTypes = {typeof(string), typeof(double), typeof(Formula)};
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
        if (!Regex.IsMatch(s, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
        {
            throw new InvalidNameException();
        }
        return s;
    }

    /// <summary>
    /// help method
    /// keep add directly dependent items into dependToList, until all item in
    /// the dependToList was accessed, this means all of the directly and
    /// indirectly dependent items are added in this list.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IList<string> GetDependToList(string name)
    {
        // name should already checked validity before calling this help method
        List<string> dependToList = new List<string> { name };
        List<string> currentList;

        // for keep add item into dependToList, so value of count is keep growth
        for (int i = 0; i < dependToList.Count; i++)
        {
            // load all dependent for current item
            currentList = dependencyGraph.GetDependents(dependToList[i]).ToList();
            // add two list together, once same item occurs, throw exception
            AddTwoDistinctList(dependToList, currentList);
        }

        return dependToList;
    }

    /// <summary>
    /// help method
    /// if mainList have same item with branchList, this means one item in
    /// mainList is directly or indirectly depend on itself, thus a circular
    /// appeared
    /// </summary>
    /// <param name="mainList"></param>
    /// <param name="branchList"></param>
    /// <exception cref="CircularException"></exception>
    private void AddTwoDistinctList(List<string> mainList, List<string> branchList)
    {
        foreach (string s in branchList)
        {
            if (mainList.Contains(s)) { throw new CircularException(); }
            mainList.Add(s);
        }
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
            dependencyGraph.ReplaceDependees(name, ((Formula)obj).GetVariables());

        // update dependees into empty
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
    public override IList<string> SetCellContents(string name, double number)
    {
        SetContents(name, number);
        return GetDependToList(name);
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
        return GetDependToList(name);
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
        return GetDependToList(name);
    }


    protected override IEnumerable<string> GetDirectDependents(string name)
    {
        return dependencyGraph.GetDependents(ValidName(name));
    }
}

