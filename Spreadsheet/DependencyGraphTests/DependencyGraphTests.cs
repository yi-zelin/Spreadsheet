using DependencyGraph;

namespace DependencyGraphTests
{
    [TestClass]
    public class DependencyGraphTests
    {

        [TestMethod]
        public void TestBasicMethod()
        {
            Graph a = new Graph();
            Assert.AreEqual(0, a.Size());

            List<String> a1Dependent = new List<String>();
            List<String> a1Dependee = new List<String>();
            List<String> a2Dependent = new List<String>();
            List<String> a2Dependee = new List<String>();
            List<String> a3Dependent = new List<String>();
            List<String> a3Dependee = new List<String>();
            List<String> a4Dependent = new List<String>();
            List<String> a4Dependee = new List<String>();

            a1Dependee.Add("A2");
            a1Dependee.Add("A3");

            a2Dependent.Add("A1");
            a2Dependee.Add("A3");
            a2Dependee.Add("A4");

            a3Dependent.Add("A1");
            a3Dependent.Add("A2");

            a4Dependent.Add("A2");

            a.WriteCell("A1",a1Dependent,a1Dependee);
            Assert.AreEqual(1, a.Size());
            a.WriteCell("A2",a2Dependent,a2Dependee);
            Assert.AreEqual(2, a.Size());
            a.WriteCell("A3",a3Dependent,a3Dependee);
            Assert.AreEqual(3, a.Size());
            a.WriteCell("A4",a4Dependent,a4Dependee);
            Assert.AreEqual(4, a.Size());

            TestSameGraph(a);

            a.Clear();
            Assert.AreEqual(0, a.Size());
            Assert.IsFalse(a.Contain("A1"));
            Assert.IsFalse(a.Contain("A2"));
            Assert.IsFalse(a.Contain("A3"));
            Assert.IsFalse(a.Contain("A4"));
            Assert.IsFalse(a.Contain("A5"));
            Assert.IsFalse(a.Contain("A0"));
        }

        private void TestSameGraph(Graph a) { 
            Assert.IsTrue(a.Contain("A1"));
            Assert.IsTrue(a.Contain("A2"));
            Assert.IsTrue(a.Contain("A3"));
            Assert.IsTrue(a.Contain("A4"));
            Assert.IsFalse(a.Contain("A5"));
            Assert.IsFalse(a.Contain("A0"));
        }


        [TestMethod]
        public void TestOtherMethods()
        {
            Graph a = new Graph();

            a.AddEmptyCell("A1");
            a.AddEmptyCell("A2");
            a.AddEmptyCell("A3");
            a.AddEmptyCell("A4");

            a.DependOn("A1", "A2");
            a.DependOn("A1", "A3");
            a.DependOn("A2", "A3");
            a.DependOn("A2", "A4");

            a.DependTo("A2", "A1");
            a.DependTo("A3", "A1");
            a.DependTo("A3", "A4");
            a.DependTo("A4", "A2");

            TestSameGraph(a);

            a.DependTo("A1", "A5");
            a.DependTo("A1", "A6");
            a.DependTo("A1", "A7");

            Assert.IsTrue(a.IfDependTo("A1", "A5"));
            Assert.IsTrue(a.IfDependTo("A1", "A6"));
            Assert.IsTrue(a.IfDependTo("A1", "A7"));

            a.RemoveDependTo("A1", "A5");
            a.RemoveDependTo("A1", "A6");
            a.RemoveDependTo("A1", "A7");

            Assert.IsFalse(a.IfDependTo("A1", "A5"));
            Assert.IsFalse(a.IfDependTo("A1", "A6"));
            Assert.IsFalse(a.IfDependTo("A1", "A7"));
            // at here, graph shold not changed

            a.DependOn("A3", "A5");
            a.DependOn("A3", "A6");
            a.DependOn("A3", "A7");
            Assert.IsTrue(a.IfDependOn("A3", "A5"));
            Assert.IsTrue(a.IfDependOn("A3", "A6"));
            Assert.IsTrue(a.IfDependOn("A3", "A7"));
            a.RemoveDependOn("A3", "A5");
            a.RemoveDependOn("A3", "A6");
            a.RemoveDependOn("A3", "A7");

            TestSameGraph(a);


            Assert.IsTrue(a.IfDependOn("A1", "A2"));
            Assert.IsTrue(a.IfDependOn("A1", "A3"));
            Assert.IsFalse(a.IfDependOn("A1", "A4"));
            Assert.IsFalse(a.IfDependOn("A1", "A5"));
        }

        [TestMethod]
        public void TestBrantch()
        {
            Graph a = new Graph();

            a.AddEmptyCell("A1");
            a.AddEmptyCell("A2");
            a.AddEmptyCell("A3");
            a.AddEmptyCell("A4");

            a.DependOn("A1", "A2");
            a.DependOn("A1", "A3");
            a.DependOn("A2", "A3");
            a.DependOn("A2", "A4");

            a.DependTo("A2", "A1");
            a.DependTo("A3", "A1");
            a.DependTo("A3", "A4");
            a.DependTo("A4", "A2");

            Assert.IsFalse()
        }
}