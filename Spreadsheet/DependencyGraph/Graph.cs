using System.Collections;
using System.Numerics;

namespace DependencyGraph
{
    /// <summary>
    /// Graph is based on a Dictionary and inner class called SignedEdges
    /// </summary>
    public class Graph
    {
        private Dictionary<String, SignedEdges> VariablesMap = new Dictionary<string, SignedEdges>();
        private class SignedEdges
        {
            // depend to
            internal List<String> Dependents;

            // depend on
            internal List<String> Dependees;

            internal SignedEdges()
            {
                Dependents = new List<String>();
                Dependees = new List<String>();
            }

            internal SignedEdges(List<String> Dependents, List<String> Dependees)
            {
                this.Dependents = Dependents;
                this.Dependees = Dependees;
            }

        }

        public bool Contain (String s) { return VariablesMap.ContainsKey(s); }

        public int Size() { return VariablesMap.Count; }

        public void Clear() { VariablesMap.Clear(); }

        public void WriteCell(String name, List<String> s1, List<String> s2) { VariablesMap.Add(name,new SignedEdges(s1, s2)); }

        public bool AddDependTo(String variable, String item) { 
            if (!VariablesMap.ContainsKey(variable)) { return false; }
            VariablesMap[variable].Dependents.Add(item);
            return true;
        }

        public bool AddDependOn(String variable, String item) {
            if (!VariablesMap.ContainsKey(variable)) { return false; }
            VariablesMap[variable].Dependees.Add(item); 
            return true;
        }

        public bool IfDependTo(String variable, String item) { return VariablesMap[variable].Dependents.Contains(item); }

        public bool IfDependOn(String variable, String item) { return VariablesMap[variable].Dependees.Contains(item); }

        /// <summary>
        /// Remove item in Dependents
        /// Have to check if contain before use
        /// </summary>
        /// <param name="variable">variable in graph</param>
        /// <param name="item">item we want to remove in variable_Dependents</param>
        /// <returns>true if variable are in graph, false if variable are not in graph</returns>
        public bool RemoveDependTo (String variable,String item) {
            if (!VariablesMap.ContainsKey(variable)) { return false; }
            VariablesMap[variable].Dependents.Remove(item);
            return true;
        }

        /// <summary>
        /// Remove item in Dependees
        /// Have to check if contain before use
        /// </summary>
        /// <param name="variable">variable in graph</param>
        /// <param name="item">item we want to remove in variable_Dependents</param>
        /// <returns>true if variable are in graph, false if variable are not in graph</returns>
        public bool RemoveDependOn(String variable, String item) {
            if (!VariablesMap.ContainsKey(variable)) { return false; }
            VariablesMap[variable].Dependees.Remove(item); 
            return true;
        }
    }
}