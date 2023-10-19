// write by Zelin Yi for CS 3500
// Last updated: September 2023

using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// test get name of all non empty cells in the spreadsheet
        /// </summary>
        [TestMethod]
        public void TestGetNamesOfAllNonemptyCells()
        {
            // test empty spreadsheet
            Spreadsheet emptySpredShet = new Spreadsheet();
            Assert.IsTrue(emptySpredShet.GetNamesOfAllNonemptyCells().ToArray().Length == 0);

            // test a simple spreadsheet
            Spreadsheet smallSpredShet = new Spreadsheet();
            smallSpredShet.SetContentsOfCell("A1", "1.0");
            smallSpredShet.SetContentsOfCell("A2", "2.0");
            smallSpredShet.SetContentsOfCell("A3", "3.0");
            smallSpredShet.Save("C:\\Users\\86\\Desktop\\New.json");
            string[] cellsName = smallSpredShet.GetNamesOfAllNonemptyCells().ToArray();
            Assert.AreEqual(3, cellsName.Length);
            Assert.IsTrue(cellsName.Contains("A1"));
            Assert.IsTrue(cellsName.Contains("A2"));
            Assert.IsTrue(cellsName.Contains("A3"));
        }

        /// <summary>
        /// test three GetCellContents() methods
        /// including invalid case
        /// </summary>
        [TestMethod]
        public void TestGetCellContents()
        {
            // test a simple spreadsheet
            Spreadsheet smallSpredShet = new Spreadsheet();
            smallSpredShet.SetContentsOfCell("A1", "1.0");
            smallSpredShet.SetContentsOfCell("A2", "2.0");
            smallSpredShet.SetContentsOfCell("A3", "3.0");
            string[] cellsName = smallSpredShet.GetNamesOfAllNonemptyCells().ToArray();
            Assert.AreEqual(3, cellsName.Length);
            Assert.IsTrue(cellsName.Contains("A1"));
            Assert.IsTrue(cellsName.Contains("A2"));
            Assert.IsTrue(cellsName.Contains("A3"));

            Assert.AreEqual(1.0, smallSpredShet.GetCellContents("A1"));
            Assert.AreEqual("", smallSpredShet.GetCellContents("A10"));
        }

        /// <summary>
        /// test three SetContentsOfCell() methods
        /// including invalid case
        /// </summary>
        [TestMethod]
        public void TestSetContentsOfCell()
        {
            Spreadsheet smallSpredShet = new Spreadsheet();

            // Contents is double
            smallSpredShet.SetContentsOfCell("A1", "1.0");

            // Contents is string
            smallSpredShet.SetContentsOfCell("A2", "hahahaha");

            // contents is Formula
            smallSpredShet.SetContentsOfCell("A3", "=1+1+2+3");

            Assert.IsTrue(smallSpredShet.GetNamesOfAllNonemptyCells().ToArray().Length == 3);

            // invalid name, throws an InvalidNameException
            Assert.ThrowsException<InvalidNameException>(() => smallSpredShet.SetContentsOfCell("1A", "1.0"));
        }

        /// <summary>
        /// test help method ValidName()
        /// Using indirect test
        /// </summary>
        [TestMethod]
        public void TestValidName()
        {
            Spreadsheet testSpredShet = new Spreadsheet();
            string[] invalidName = { "25", "2x", "&", "3a", "-", "", " " };
            string[] validName = { "x", "_", "x2", "y_15", "___", "X2", "X", "X_15" };

            // throws exception with invalid name
            foreach (var i in invalidName)
            {
                Assert.ThrowsException<InvalidNameException>(() => testSpredShet.SetContentsOfCell(i, "3.0"));
            }

            // not throws for valid name
            foreach (var i in validName)
            {
                testSpredShet.SetContentsOfCell(i, "3.0");
            }
            Assert.AreEqual(validName.Length, testSpredShet.GetNamesOfAllNonemptyCells().Count());
        }

        /// <summary>
        /// test help method GetCellsToRecalculate()
        /// Using indirect test
        /// </summary>
        [TestMethod]
        public void TestVisit()
        {
            Spreadsheet smallSpredShet = new Spreadsheet();
            smallSpredShet.SetContentsOfCell("B1", "= A1 * 2");
            smallSpredShet.SetContentsOfCell("C1", "= B1 * A1");
            List<string> ans = (List<string>)smallSpredShet.SetContentsOfCell("A1", "1.0");

            Assert.AreEqual(3, ans.Count);
            Assert.IsTrue(ans.Contains("A1"));
            Assert.IsTrue(ans.Contains("B1"));
            Assert.IsTrue(ans.Contains("C1"));

            // circular throws
            Spreadsheet testSpredShet = new Spreadsheet();
            testSpredShet.SetContentsOfCell("A1", "= B1");
            testSpredShet.SetContentsOfCell("B1", "= C1");
            testSpredShet.SetContentsOfCell("C1", "= D1");
            Console.WriteLine(testSpredShet.Cells["A1"].stringForm);
            Assert.ThrowsException<CircularException>(() => testSpredShet.SetContentsOfCell("D1", "= A1"));
        }

        /// <summary>
        /// more corner case to test Set
        /// </summary>
        [TestMethod]
        public void TestMoreToSetContentsOfCell()
        {
            Spreadsheet testSpredShet = new Spreadsheet();
            Assert.AreEqual(1, testSpredShet.SetContentsOfCell("B1", "=C1").Count);
            Assert.AreEqual(2, testSpredShet.SetContentsOfCell("C1", "=D1").Count);
            Assert.AreEqual(1, testSpredShet.SetContentsOfCell("A1", "=B1").Count);
            Assert.AreEqual(4, testSpredShet.SetContentsOfCell("D1", "=E1").Count);
            Assert.ThrowsException<CircularException>(() => testSpredShet.SetContentsOfCell("D1", "=A1"));
        }

        /// <summary>
        /// test GetCellValue()
        /// </summary>
        [TestMethod]
        public void TestValue()
        {
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "2.4");
            ss.SetContentsOfCell("A2", "1.8");
            ss.SetContentsOfCell("A3", "=A1+A2");
            Assert.IsTrue(double.Abs((double)ss.GetCellValue("A3") - 4.2) < 1e-8);
            Assert.AreEqual("", ss.GetCellValue("C1"));
        }

        /// <summary>
        /// test Save() and four parameter constructor
        /// </summary>
        [TestMethod]
        public void TestSaveAndLoad()
        {
            // empty case
            Spreadsheet testSpredShet = new Spreadsheet();
            testSpredShet.Save("test1.txt");
            Spreadsheet temp = new Spreadsheet("test1.txt", s => true, s => s, "default");
            // save error
            Assert.ThrowsException<SpreadsheetReadWriteException>(()=>testSpredShet.Save("$//h"));

            // test load
            Spreadsheet ss = new Spreadsheet();
            ss.SetContentsOfCell("A1", "2.4");
            ss.SetContentsOfCell("A2", "1.8");
            ss.SetContentsOfCell("A3", "=A1+A2");
            ss.Save("test1.txt");

            Spreadsheet loadss = new Spreadsheet("test1.txt", s => true, s => s, "default");

            Assert.IsTrue(double.Abs((double)loadss.GetCellValue("A3") - 4.2) < 1e-8);
            Assert.AreEqual("", loadss.GetCellValue("C1"));
            Assert.AreEqual(2.4, loadss.GetCellValue("A1"));
            Assert.AreEqual(1.8, loadss.GetCellValue("A2"));


            // test update value
            ss.SetContentsOfCell("A4", "=A3+A2");
            ss.SetContentsOfCell("A5", "=A4+A1");
            ss.SetContentsOfCell("A6", "tnohu9");
            ss.SetContentsOfCell("A7", "=A6+A1");
            ss.SetContentsOfCell("A1", "99");
            ss.SetContentsOfCell("A2", "=A1");
            Assert.IsTrue(ss.GetCellValue("A7") is FormulaError);

            // set to empty
            ss.SetContentsOfCell("A1", "");
            Assert.AreEqual("", ss.GetCellValue("A1"));

            // version don't match
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("test1.txt", s => true, s => s, "error version"));
        }
    }
}