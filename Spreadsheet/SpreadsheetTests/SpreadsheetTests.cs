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
            smallSpredShet.SetCellContents("A1", 1.0);
            smallSpredShet.SetCellContents("A2", 2.0);
            smallSpredShet.SetCellContents("A3", 3.0);
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
            smallSpredShet.SetCellContents("A1", 1.0);
            smallSpredShet.SetCellContents("A2", 2.0);
            smallSpredShet.SetCellContents("A3", 3.0);
            string[] cellsName = smallSpredShet.GetNamesOfAllNonemptyCells().ToArray();
            Assert.AreEqual(3, cellsName.Length);
            Assert.IsTrue(cellsName.Contains("A1"));
            Assert.IsTrue(cellsName.Contains("A2"));
            Assert.IsTrue(cellsName.Contains("A3"));

            Assert.AreEqual(1.0, smallSpredShet.GetCellContents("A1"));
            Assert.AreEqual("", smallSpredShet.GetCellContents("A10"));
        }

        /// <summary>
        /// test three SetCellContents() methods
        /// including invalid case
        /// </summary>
        [TestMethod]
        public void TestSetCellContents()
        {
            Spreadsheet smallSpredShet = new Spreadsheet();

            // Contents is double
            smallSpredShet.SetCellContents("A1", 1.0);

            // Contents is string
            smallSpredShet.SetCellContents("A2", "2");

            // contents is Formula
            Formula simpleFormula = new Formula("1+1+2+3");
            smallSpredShet.SetCellContents("A3", simpleFormula);

            Assert.IsTrue(smallSpredShet.GetNamesOfAllNonemptyCells().ToArray().Length == 3);

            // invalid name, throws an InvalidNameException
            Assert.ThrowsException<InvalidNameException>(() => smallSpredShet.SetCellContents("1A", 1.0));
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
                Assert.ThrowsException<InvalidNameException>(() => testSpredShet.SetCellContents(i, 3.0));
            }

            // not throws for valid name
            foreach (var i in validName)
            {
                testSpredShet.SetCellContents(i, 3.0);
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
            smallSpredShet.SetCellContents("B1", new Formula("A1 * 2"));
            smallSpredShet.SetCellContents("C1", new Formula("B1 * A1"));
            List<string> ans = (List<string>)smallSpredShet.SetCellContents("A1", 1.0);

            Assert.AreEqual(3, ans.Count);
            Assert.IsTrue(ans.Contains("A1"));
            Assert.IsTrue(ans.Contains("B1"));
            Assert.IsTrue(ans.Contains("C1"));

            // circular throws
            Spreadsheet testSpredShet = new Spreadsheet();
            testSpredShet.SetCellContents("A1", new Formula("B1"));
            testSpredShet.SetCellContents("B1", new Formula("C1"));
            testSpredShet.SetCellContents("C1", new Formula("D1"));
            Assert.ThrowsException<CircularException>(() => testSpredShet.SetCellContents("D1", new Formula("A1")));
        }

        /// <summary>
        /// more corner case to test Set
        /// </summary>
        [TestMethod]
        public void TestMoreToSetCellContents()
        {
            Spreadsheet testSpredShet = new Spreadsheet();
            Assert.AreEqual(1, testSpredShet.SetCellContents("B1", new Formula("C1")).Count);
            Assert.AreEqual(2, testSpredShet.SetCellContents("C1", new Formula("D1")).Count);
            Assert.AreEqual(1, testSpredShet.SetCellContents("A1", new Formula("B1")).Count);
            Assert.AreEqual(4, testSpredShet.SetCellContents("D1", new Formula("E1")).Count);
            Assert.ThrowsException<CircularException>(() => testSpredShet.SetCellContents("D1", new Formula("A1")));
        }
    }
}