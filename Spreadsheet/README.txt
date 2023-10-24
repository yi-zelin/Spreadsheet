

# Spreadsheet Solution

## Author Information
- **Name:** Zelin Yi and Rishen Cao
- **Date:** 23/10/2023

## Summary
This project encompasses a GUI-based spreadsheet solution crafted using MAUI and .NET. Not only does it offer rudimentary spreadsheet functionalities, but it's also supplemented by features such as Summation.

## Additional Features
- **Tooltip for Error Messages:** 
  Each content box integrates TooltipProperties. This feature ensures users are immediately informed of errors or additional content when they hover over a content box.
  
- **Rename:**
  We allow the user to change the filename and will automatically add the correct suffix if the user does not use it.

- **Summation of Selected Cells:** 
  Users can choose multiple cells and combine their values in the 'sum' box. Our solution initializes a list in the Spreadsheet, with selected cells being added to this list. A 'foreach' loop then computes the total of each cell's value.

- **File Saving and Opening Functionality:** 
  'Save' and 'Save As' options utilize FileSaver. For this, the 'community.Toolkit.Muai' package is essential. FileSaver empowers users to select their file path and name.

- **AutoEnter:**
   We want to minimize the user's frequent use of Enter to assign values. Add a selectionChanged listener so that the selection will be updated automatically after it is changed. This greatly accelerates the user's efficiency and experience.

## Special Instructions for Use
1. Press the 'New' button to start a new spreadsheet.
2. Manage your files efficiently with the 'Open' and 'Save' buttons.
3. Discover the content and computed values of cells by clicking on them.
4. Aggregate values of selected cells using the 'add' feature.

## External Code Resources
- The solution relies on 'CommunityToolkit.Maui' for alerts and storage functions.

## Implementation Notes
- We've employed event-driven programming, ensuring real-time updates with cell value alterations.
- To ensure smooth File IO operations, we've integrated the async/await paradigm.

## Problems Faced
Challenges arose when addressing formulaException and formula formatException. After some in-depth probing, we identified discrepancies in our earlier Dependent graph. Rigorous debugging helped in isolating and fixing these issues.
- We plan to introduce a 'Drag and Drop' feature for easier cell value transfers.However，it is difficlut for
to finish, so we give up.