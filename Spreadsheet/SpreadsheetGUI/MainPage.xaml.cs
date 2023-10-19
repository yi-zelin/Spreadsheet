using SS;
using System.Diagnostics;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    // easier to update, for column in PS6 is only from A to Z, if need more in future, just update this
    Func<int, string> getCol;

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
        getCol = (col) => ((char)(col + 65)).ToString();
    }

    private void setContent(Object sender, EventArgs e)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        Debug.WriteLine("Setcontent: " + cellContent.Text);
        spreadsheetGrid.SetValue(col, row, cellContent.Text);
        spreadsheetGrid.GetValue(col, row, out string value);
        cellValue.Text = value;
    }

    private void displaySelection(ISpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        // set name
        cellName.Text = getCol(col) + (row+1);
        spreadsheetGrid.GetValue(col, row, out string value);

        //spreadsheetGrid.SetValue(col,row, value);

        cellValue.Text = value;
        cellContent.Text = spreadsheetGrid.GetCurrentContent();
        //cellContent.Text = "=hahah";
    }

    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();
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
        Console.WriteLine( "Successfully chose file: " + fileResult.FileName );
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

    private void cellContent_Completed(object sender, EventArgs e)
    {

    }
}
