// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)

namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph
{
    private Dictionary<String, Cell> CellsMap = new Dictionary<string, Cell>();
    private int pair;
    private class Cell
    {
        // depend to
        internal HashSet<String> Dependent;

        // depend on
        internal HashSet<String> Dependee;

        public Cell()
        {
            Dependent = new HashSet<string>();
            Dependee = new HashSet<string>();
        }
    }

    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        CellsMap = new Dictionary<string, Cell>();
        pair = 0;
    }


    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get { return pair; }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s)
    {
        if (CellsMap.ContainsKey(s)) { return CellsMap[s].Dependee.Count; }
        return 0;
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        if (!CellsMap.ContainsKey(s)) { return false; }
        return CellsMap[s].Dependent.Count != 0;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        if (!CellsMap.ContainsKey(s)) { return false; }
        return CellsMap[s].Dependee.Count != 0;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        if (!CellsMap.ContainsKey(s)) { return Enumerable.Empty<string>(); }
        return CellsMap[s].Dependent;
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        if (!CellsMap.ContainsKey(s)) { return Enumerable.Empty<string>(); }
        return CellsMap[s].Dependee;
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        // both s and t are not in the dictionary
        if (!CellsMap.ContainsKey(s) && !CellsMap.ContainsKey(t))
        {
            CellsMap.Add(s, new Cell());
            CellsMap.Add(t, new Cell());
        }
        // s not in the dictionary, but t is in dictionary
        else if (!CellsMap.ContainsKey(s) && CellsMap.ContainsKey(t))
        {
            CellsMap.Add(s, new Cell());
        }
        // t is not in dictionary, s in dictionary
        else if (!CellsMap.ContainsKey(t))
        {
            CellsMap.Add(t, new Cell());
        }

        // at here, s and t both in dictionary.
        // if pair (s, t) not exist, add pair (s, t)
        if (!CellsMap[s].Dependent.Contains(t))
        {
            CellsMap[s].Dependent.Add(t);
            CellsMap[t].Dependee.Add(s);
            pair++;
        }
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        if ((CellsMap.ContainsKey(s) && CellsMap[s].Dependent.Contains(t)))
        {
            CellsMap[s].Dependent.Remove(t);
            CellsMap[t].Dependee.Remove(s);
            pair--;
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        // remove all existing ordered pars of form (s,r)
        foreach (string item in CellsMap.Keys)
        {
            RemoveDependency(s, item);
        }
        // adds ordered pair (s,t) for each t in newDependents
        foreach (string item in newDependents)
        {
            AddDependency(s, item);
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        // remove all existing ordered pairs of the form (r,s)
        foreach (string item in CellsMap.Keys)
        {
            RemoveDependency(item, s);
        }
        // adds the ordered pair (t,s) for each t in newDependees
        foreach (string item in newDependees)
        {
            AddDependency(item, s);
        }
    }
}