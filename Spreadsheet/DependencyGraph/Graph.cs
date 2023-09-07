using System.Collections;
using System.Numerics;

namespace DependencyGraph
{
    /// <summary>
    /// Graph is based on a Dictionary and inner class called SignedEdges
    /// </summary>
    public class Graph
    {
        private Dictionary<String, Cell> CellsMap = new Dictionary<string, Cell>();
        public class Cell
        {
            // depend to
            private HashSet<String> Dependent;

            // depend on
            private HashSet<String> Dependee;

            internal Cell()
            {
                Dependent = new HashSet<string>();
                Dependee = new HashSet<string>();
            }

            internal Cell(List<String> Dependents, List<String> Dependees)
            {
                this.Dependent = new HashSet<string> (Dependents);
                this.Dependee = new HashSet<string>(Dependees);
            }

            public bool DependentContain(String CellName) { return Dependent.Contains (CellName); }
            public bool DependeeContain(String CellName) { return Dependee.Contains (CellName); }
            public bool DependentAdd(String CellName) { return Dependent.Add (CellName); }
            public bool DependeeAdd(String CellName) { return Dependee.Add (CellName); }
            public bool DependentRemove(String CellName) { return Dependent.Remove (CellName); }
            public bool DependeeRemove(String CellName) { return Dependee.Remove (CellName); }
        }


        public bool ContainCell(String s) { return CellsMap.ContainsKey(s); }

        public int Size() { return CellsMap.Count; }

        public void Clear() { CellsMap.Clear(); }

        public void WriteCell(String name, List<String> s1, List<String> s2) { CellsMap.Add(name, new Cell(s1, s2)); }

        public void SetEmptyCell(String name) { CellsMap.Add(name, new Cell()); }

        public Cell GetCell (String name) { return CellsMap[name]; }
    }
}