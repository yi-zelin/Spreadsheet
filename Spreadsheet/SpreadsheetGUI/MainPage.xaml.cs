using Microsoft.Maui.Controls;
using SpreadsheetUtilities;
using SS;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    private Spreadsheet _data;
    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(2, 3);
        cellName.Text = AddrToVar(2, 3);
        _data = new Spreadsheet(s => Regex.IsMatch(s, @"^[a-zA-Z][0-9][0-9]?$"), s => s.ToUpper(), "ps6");
    }

    /// <summary>
    /// emphasize current selected cell, and set cal and row
    /// </summary>
    /// <param name="grid"></param>
    private void displaySelection(ISpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);
        cellName.Text = AddrToVar(col, row);
        cellValue.Text = value;

        // update error message detail
        object t = _data.GetCellValue(AddrToVar(col, row));
        if ( t is FormulaError)
        {
            ToolTipProperties.SetText(cellValue, "#Error: " + ((FormulaError)t).Reason);
        } else
        {
            ToolTipProperties.SetText(cellValue, "Value");
        }

        // use cell.StringForm as content in excel
        if (!_data.Cells.TryGetValue(AddrToVar(col, row), out Spreadsheet.Cell cell))
            cellContent.Text = "";
        else cellContent.Text = cell.stringForm;
    }

    /// <summary>
    /// when finish input, add into _data, calculate result, update all change, and draw
    /// </summary>
    public void finishInput(Object sender, EventArgs e)
    {
        cellContent.Text = cellContent.Text.ToUpper();
        // add into _data, spreadsheet will auto calculate result
        IList<string> updatelist = _data.SetContentsOfCell(cellName.Text, cellContent.Text);
        // update change
        foreach (string item in updatelist)
        {
            string tempValue = FormalCellValue(item);
            VarToAddr(item, out int col, out int row);
            spreadsheetGrid.SetValue(col, row, tempValue);
        }
        // set empty cell into empty
        if (updatelist.Count == 0)
            return;
        // update value entry
        cellValue.Text = FormalCellValue(updatelist[0]);
    }

    private string FormalCellValue(string variable)
    {
        object t = _data.GetCellValue(variable);
        if (t is FormulaError)
        {
            ToolTipProperties.SetText(cellValue, "#Error: " + ((FormulaError)t).Reason);
            return "#Error!";
        }
        return t.ToString();
    }


    /// <summary>
    /// help method, mapping variable to address
    /// </summary>
    /// <param name="v"></param>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void VarToAddr (string v, out int col, out int row)
    {
        col = (((int) v[0]) - 65);
        row = (int.Parse(v.Substring(1))-1);
    }

    /// <summary>
    /// help method, mapping address of cell to variable in spreadsheet
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private string AddrToVar(int col, int row)
    {
        return ((char)(col + 65)).ToString() + (row+1);
    }

    /// <summary>
    /// clicked new, then create a new one
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();
        cellContent.Text = "";
        cellValue.Text = "";
        ToolTipProperties.SetText(cellValue, "Value");
        _data = new Spreadsheet(s => Regex.IsMatch(s, @"^[a-zA-Z][0-9][0-9]?$"), s => s.ToUpper(), "ps6");
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                System.Diagnostics.Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                System.Diagnostics.Debug.WriteLine("File Full Path: " + fileResult.FullPath);
                // for windows, replace Console.WriteLine statements with:
                //System.Diagnostics.Debug.WriteLine( ... );

                // remove exist data, upload _data, sync SpreadsheetGrid._values 
                spreadsheetGrid.Clear();
                cellContent.Text = "";
                cellValue.Text = "";
                ToolTipProperties.SetText(cellValue, "Value");
                _data = new Spreadsheet(fileResult.FullPath, s => Regex.IsMatch(s, @"^[a-zA-Z][0-9][0-9]?$"), s => s.ToUpper(), "ps6");

                foreach (string key in _data.Cells.Keys)
                {
                    VarToAddr(key, out int col, out int row);
                    object tempValue = _data.GetCellValue(key);
                    if ( tempValue is FormulaError)
                        spreadsheetGrid.SetValue(col, row, "#Error!");
                    spreadsheetGrid.SetValue(col, row, tempValue.ToString());
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex.Message);
        }
    }


    private void SaveClicked(Object sender, EventArgs e)
    {
        
    }

}