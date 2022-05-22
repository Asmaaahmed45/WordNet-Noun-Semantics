using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace WordNetProject
{
    class DirectedGragh
    {
        //Dictionaray that assigen each vetrice(synset) to it's parents
        public Dictionary<int, HashSet<int>> Graph = new Dictionary<int, HashSet<int>>();
        //fun that represent data to a Graph
        public DirectedGragh(string Synsets, string Hypernyms)
        {
            ConstructGraph(Hypernyms);
        }
        private void ConstructGraph(string Hypernyms)
        {
            using (var reader = new StreamReader(Hypernyms))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    Debug.WriteLine($"Line Input {line}");
                    var v = line?.Split(',');// Split the synset id, id numbers of the synset's hypernyms.
                    var child = v?[0];
                    var list = new HashSet<int>();
                    for (var i = 1; i < v.Count(); i++)
                    {
                        list.Add(int.Parse(v[i]));
                    }
                    //convert the string representation of child to its integer
                    var Child = int.Parse(child);
                    if (Graph.ContainsKey(Child))
                    {
                        Graph[Child].UnionWith(list);
                        foreach (var i in Graph[Child])
                        {
                            Debug.WriteLine($"SET UNION ---> {i}");
                        }
                        Debug.WriteLine("@@@@@@@@@@@@@@@@@@@");
                    }
                    else
                    {
                        Graph.Add(Child, list);
                    }
                    //TODO report progress

                }
                Debug.WriteLine("Done");
            }
        }
        //fun that return all vertices in graph
        public IEnumerable<int> Vertices()
        {
            return Graph.Keys;
        }
        //fun that return all parents of a vertices
        public HashSet<int> GetParents(int child)
        {
            if (Graph.ContainsKey(child))
            {
                return Graph[child];
            }
            else
            {
                return new HashSet<int>();
            }
        }
        //fun that find the IDs of all synsets that the given noun belongs to. 
        public Dictionary<string, SortedSet<int>> _nounMap = new Dictionary<string, SortedSet<int>>();
        public Dictionary<int, HashSet<string>> _synsetsMap = new Dictionary<int, HashSet<string>>();
        public void InitNouns(string Synsetsfile)
        {
            using (var reader = new StreamReader(Synsetsfile))
            {
                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    var data = dataLine?.Split(','); // Split id, nouns , gloss
                    var nounId = data?[0];
                    var NounId = int.Parse(nounId);//Get noun id
                    var nouns = data?[1].Split(' '); //Get Nouns 
                    //find the nouns (synonyms) that belong to the given synset ID
                    // sysnset map key = id, value = list of nouns of the same meaning
                    _synsetsMap.Add(NounId, new HashSet<string>(nouns));
                    foreach (var noun in nouns)
                    {
                        SortedSet<int> nounIds;
                        if (!_nounMap.TryGetValue(noun, out nounIds))
                        {
                            nounIds = new SortedSet<int>();
                        }
                        nounIds.Add(NounId);
                        if (!_nounMap.ContainsKey(noun))
                            _nounMap.Add(noun, nounIds);
                        else
                            _nounMap[noun] = nounIds;
                    }
                    //Finished adding nouns and mapping to ids
                }
            }
        }
    }
}

