using CommunityToolkit.Maui.Storage;
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
    // save data and calculate
    private Spreadsheet _data;
    // default save to this location, with name
    private string FileLocation;

    // for auto save method
    private Object sender;
    private EventArgs e;
    private bool changed;

    private List<string> _sumList;
    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();
        _sumList = new List<string>();
        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += SelectionChangedDriver;
        spreadsheetGrid.SelectionChanged += displaySelection;

        spreadsheetGrid.SetSelection(2, 3);
        cellName.Text = AddrToVar(2, 3);
        _data = new Spreadsheet(s => Regex.IsMatch(s, @"^[a-zA-Z][0-9][0-9]?$"), s => s.ToUpper(), "ps6");
    }

    /// <summary>
    /// help method to keep update event, thus we can do update when we want to
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangeUpdate(Object sender, EventArgs e)
    {
        this.sender = sender;
        this.e = e;
        changed = true;
    }

    /// <summary>
    /// help method, to drive function we want to at selection changed
    /// </summary>
    /// <param name="grid"></param>
    private void SelectionChangedDriver(ISpreadsheetGrid grid)
    {
        // auto evaluate function
        if (changed)
        {
            finishInput(sender, e);
            changed = false;
        }
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

        // update error message detail
        object t = _data.GetCellValue(AddrToVar(col, row));
        if (t is FormulaError)
        {
            cellValue.Text = "Error!";
            ToolTipProperties.SetText(cellValue, "#Error: " + ((FormulaError)t).Reason);
        }
        else
        {
            cellValue.Text = t.ToString();
            ToolTipProperties.SetText(cellValue, "Value");
        }

        // set content
        if (!_data.Cells.TryGetValue(AddrToVar(col, row), out Spreadsheet.Cell cell))
            cellContent.Text = "";
        else cellContent.Text = GetContentText(AddrToVar(col, row));
    }

    /// <summary>
    /// a help method to output correct form of content
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string GetContentText(string s)
    {
        object t = _data.GetCellContents(s);
        if (t is Formula)
            return "=" + t.ToString();
        return t.ToString();
    }

    public void HelpClicked(Object sender, EventArgs e)
    {
        _ = DisplayAlert("Help:",
        "# Cell Operations\r\n\r\n* Inputting Values and Formulas: To input data or a formula, simply double-click on the desired cell or select the input box. You can input numbers, text, or formulas.\r\n\r\n* Auto Calculation: Upon entering a formula within a cell, other relevant cells will automatically perform the calculation.\r\n\r\n* Summation: You can select the cell and press Click to add it to the box, click on the input box and enter. The result of the calculation will be displayed in the pop-up window.\r\n\r\n# File Management\r\n\r\n* Creating a New File: Click the \"New\" button at the top to create a new file.\r\n\r\n* Opening a File: Click on \"Open\" from the \"File\" dropdown menu to select and open an \r\n\r\n# existing file.\r\n\r\n* Saving a File: Click on \"Save\" from the \"File\" dropdown menu to save the current file.\r\n\r\n* Renaming: Click \"Rename\" to give a new name to the current file.\r\n\r\n# Error Alerts\r\n\r\n* Should you input an invalid formula or text, the system will automatically alert you.\r\n\r\n# Clear Operation\r\n\r\n* Clearing Data: Click on the \"clearButton\" at the top to clear the content of the selected cells."
        , "OK");

    }

    /// <summary>
    /// when finish input, add into _data, calculate result, update all change, and draw
    /// </summary>
    public void finishInput(Object sender, EventArgs e)
    {
        changed = false;
        try
        {
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

            // update save status
            if (_data.Changed)
            {
                status.Text = "Unsaved";
            }
        }
        catch (Exception ex)
        {
            if (ex is CircularException)
                _ = DisplayAlert("Circular Error", "there exist an circular error for your input, this change won't make any effect, try another value", "OK");
            else
                _ = DisplayAlert("Formula Format Error", "there exist an formula format error, this change won't make any effect, try another value", "OK");
        }
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
    private void VarToAddr(string v, out int col, out int row)
    {
        col = (((int)v[0]) - 65);
        row = (int.Parse(v.Substring(1)) - 1);
    }

    /// <summary>
    /// help method, mapping address of cell to variable in spreadsheet
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private string AddrToVar(int col, int row)
    {
        return ((char)(col + 65)).ToString() + (row + 1);
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
        if (status.Text == "Unsaved")
        {
            if (!(await DisplayAlert("Selection:", "Unsaved Yet!", accept: "Continue", cancel: "Cancel")))
            {
                return;
            }
        }
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();


            if (fileResult != null)
            {
                Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                Debug.WriteLine("File Full Path: " + fileResult.FullPath);
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
                    if (tempValue is FormulaError)
                        spreadsheetGrid.SetValue(col, row, "#Error!");
                    else
                        spreadsheetGrid.SetValue(col, row, tempValue.ToString());
                }
                FileLocation = PathWithOutName(fileResult.FullPath);
                fileName.Text = fileResult.FileName;
                status.Text = "Saved";
            }
            else
            {
                Debug.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error opening file:");
            Debug.WriteLine(ex.Message);
        }
    }

    private string PathWithOutName(string s)
    {
        int index = s.LastIndexOf('\\');
        return s.Remove(index);
    }

    private async void RenameClicked(Object sender, EventArgs e)
    {
        fileName.Text = await DisplayPromptAsync("Rename", "Enter new name");
        fileName.Text = fileName.Text + ".sprd";
    }

    private async void SaveClicked(Object sender, EventArgs e)
    {
        CancellationTokenSource c = new CancellationTokenSource();
        try
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName.Text);
            _data.Save(filePath);
            string jsonFile = File.ReadAllText(filePath);
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonFile));
            var path = await FileSaver.SaveAsync(fileName.Text, stream, c.Token);
        }



        catch (Exception ex)
        {
            Debug.WriteLine("Error with file:");

            Debug.WriteLine(ex.Message);

        }

    }
    private void SumComplete(Object sender, EventArgs e)
    {

        spreadsheetGrid.GetSelection(out int col, out int row);

        string target = AddrToVar(col, row);

        displaySelection(spreadsheetGrid);

        _sumList.Add(cellName.Text);

        string result = string.Join(", ", _sumList);

        Sum.Text = result;

    }
    private void sumSum(Object sender, EventArgs e)
    {

        double sum = 0;

        string result = string.Join(", ", _sumList);

        Sum.Text = result;
        try
        {
            foreach (var item in _sumList)
            {
                sum += (double)_data.GetCellValue(item);
            }

            DisplayAlert("Total", result + " = " + sum, "ok");

            _sumList.Clear();
        }
        catch (Exception)
        {

            DisplayAlert("Error", "Please Check the input", "OK");
        }
    }

    private void clearButtom(Object sender, EventArgs e)
    {

        _sumList.Clear();
        string result = string.Join(", ", _sumList);
        Sum.Text = result;

    }





}