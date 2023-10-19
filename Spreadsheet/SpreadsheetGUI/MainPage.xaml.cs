using Microsoft.Maui.Controls;
using SS;
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
        // add into _data, spreadsheet will auto calculate result
        var updatelist = _data.SetContentsOfCell(cellName.Text, cellContent.Text);
        // update change
        foreach (string item in  updatelist)
        {
            string tempValue = _data.GetCellValue(item).ToString();
            VarToAddr(item, out int col, out int row);
            spreadsheetGrid.SetValue(col, row, tempValue);
        }
        // update value entry
        cellValue.Text = _data.GetCellValue(updatelist[0]).ToString();
    }

    /// <summary>
    /// help method, mapping variable to address
    /// </summary>
    /// <param name="v"></param>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void VarToAddr (string v, out int col, out int row)
    {
        int[] i = new int[2];
        col = (((int)v[0]) - 65);
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
                Console.WriteLine("Successfully chose file: " + fileResult.FileName);
                // for windows, replace Console.WriteLine statements with:
                //System.Diagnostics.Debug.WriteLine( ... );

                string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }


}