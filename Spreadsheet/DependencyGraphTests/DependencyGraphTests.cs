using DependencyGraph;
using System.ComponentModel;

namespace DependencyGraphTests
{
    [TestClass]
    public class DependencyGraphTests
    {
        Graph psTwoSample;

        public void CreateSample()
        {
            psTwoSample = new Graph();
            List<String> tempDependent = new List<String>();
            List<String> tempDependee = new List<String>();
            tempDependee.Add("A2");
            tempDependee.Add("A3");
            psTwoSample.WriteCell("A1", tempDependent, tempDependee);

            tempDependee.Clear();
            tempDependent.Add("A1");
            tempDependee.Add("A3");
            tempDependee.Add("A4");
            psTwoSample.WriteCell("A2", tempDependent, tempDependee);

            tempDependee.Clear();
            tempDependent.Add("A2");
            psTwoSample.WriteCell("A3", tempDependent, tempDependee);

            tempDependent.Clear();
            tempDependent.Add("A2");
            psTwoSample.WriteCell("A4", tempDependent, tempDependee);
        }

        [TestMethod]
        public void TestGraphMethod()
        {
            // Test SetEmptyCell and Size methods
            Graph sampleGraph = new Graph();
            Assert.AreEqual(0, sampleGraph.Size());
            sampleGraph.SetEmptyCell("A1");
            Assert.AreEqual(1, sampleGraph.Size());
            sampleGraph.SetEmptyCell("A2");
            Assert.AreEqual(2, sampleGraph.Size());
            sampleGraph.SetEmptyCell("A3");
            Assert.AreEqual(3, sampleGraph.Size());
            sampleGraph.SetEmptyCell("A4");
            Assert.AreEqual(4, sampleGraph.Size());

            // Test SetEmptyCell and ContainCell methods
            Assert.IsTrue(sampleGraph.ContainCell("A1"));
            Assert.IsTrue(sampleGraph.ContainCell("A2"));
            Assert.IsTrue(sampleGraph.ContainCell("A3"));
            Assert.IsTrue(sampleGraph.ContainCell("A4"));
            Assert.IsFalse(sampleGraph.ContainCell("A5"));
            Assert.IsFalse(sampleGraph.ContainCell("A0"));

            sampleGraph.Clear();
            Assert.AreEqual(0, sampleGraph.Size());
        }


        [TestMethod]
        public void WriteCellTest()
        {
            Graph sampleGraph = new Graph();
            string[] dependentsSet = { };
            string[] dependeesSet = { "A2", "A3" };

            sampleGraph.WriteCell("A1", dependentsSet.ToList(), dependeesSet.ToList());
            Assert.IsTrue(sampleGraph.ContainCell("A1"));
            Assert.IsTrue(sampleGraph.GetCell("A1").DependeeContain("A2"));
            Assert.IsTrue(sampleGraph.GetCell("A1").DependeeContain("A3"));
            Assert.IsFalse(sampleGraph.GetCell("A1").DependeeContain("A4"));
            Assert.AreEqual(1, sampleGraph.Size());
        }

        [TestMethod]
        public void CellClassMethodTest()
        {

        }
    }
}