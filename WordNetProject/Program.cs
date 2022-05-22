using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WordNetProject
{
    class Program
    {
        public static Dictionary<int, List<String>> Synsets_dic = new Dictionary<int, List<String>>();
        // fun that return parents of a node 
        public static void Parent(int node, Dictionary<int, List<int>> p)
        {
            try
            {
                foreach (int x in p[node])
                {
                    Console.WriteLine(x);
                }
            }
            catch (Exception e)
            {
               Console.WriteLine("it's the root");
            }
        }
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                Console.WriteLine("WordNet Noun Semantic:\n[1] Sample test cases\n[2] Complete testing");
                Console.Write("\nEnter your choice [1-2]: ");
                char choice = (char)Console.ReadLine()[0];
                switch (choice)
                {
                    case '1':
                        SampleTest();

                        break;
                    case '2':
                        CompleteTest();
                        break;
                }
                var w = stopwatch.Elapsed;
                Console.WriteLine("Time of run : " + w.ToString());
            }

        }
        //find the nouns (synonyms) that belong to the given synset ID 

        // sysnset map key = id, value = list of nouns of the same meaning  
        public static Dictionary<string, SortedSet<int>> nounsID = new Dictionary<string, SortedSet<int>>();
        public static Dictionary<int, HashSet<string>> synsets = new Dictionary<int, HashSet<string>>();
        public static void InitNouns(string Synsetsfile)
        {
            using (var reader = new StreamReader(Synsetsfile))
            {
                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    var data = dataLine?.Split(','); // Split id, nouns , gloss
                    var nounId = -1;
                    int.TryParse(data?[0], out nounId); // Get Noun ID
                    var nouns = data?[1].Split(' '); //Get Nouns 
                    //Debug fail if nounId = -1
                    if (nounId == -1 || nouns == null)
                        throw new InvalidDataException("Invalid Data format");

                    //TODO it's better to combine both noun to id map and id to noun map here, for code redundancy
                    // sysnset map key = id, value = list of nouns of the same meaning
                    synsets.Add(nounId, new HashSet<string>(nouns));
                    foreach (var noun in nouns)
                    {
                        SortedSet<int> nounIds;
                        if (!nounsID.TryGetValue(noun, out nounIds))
                        {
                            nounIds = new SortedSet<int>();
                        }
                        nounIds.Add(nounId);
                        if (!nounsID.ContainsKey(noun))
                            nounsID.Add(noun, nounIds);
                        else
                            nounsID[noun] = nounIds;
                    }
                    //Finished adding nouns and mapping to ids
                }
            
            }
        }
        public static Dictionary<int, List<int>> Graph = new Dictionary<int, List<int>>();
        public static void ConstructGraph(string Hypernyms)
        {
            using (var reader = new StreamReader(Hypernyms))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();//read id of synset,parents 
                    var data = line?.Split(',');//Split the synset id, id numbers of the synset's hypernyms. 
                    int chiled = Int32.Parse(data?[0]);
                    List<int> parents = new List<int>();
                    for (int i = 1; i < data.Count(); i++)
                    {
                        parents.Add(Int32.Parse(data[i]));
                    }
                    Graph.Add(chiled, parents);
                }
            }
        }
        public static SortedSet<int> a = new SortedSet<int>();
        public static SortedSet<int> b = new SortedSet<int>();
        public static List<int> dis = new List<int>();
        public static List<int> sca = new List<int>();
        public static SortedSet<int> GetSynset(string noun)
        {
            return nounsID[noun];
        }
        public static HashSet<String> GetNouns(int SynsetID)
        {

            return synsets[SynsetID];
        }
        public static HashSet<string>get_sca(string noun1, string noun2, out int shortest)
        {
            a = GetSynset(noun1);
            b = GetSynset(noun2);
            int distance = 0;
            int min;
            int id_sca = 0;
            for (int i = 0; i < a.Count(); i++)
            {
                for (int j = 0; j < b.Count(); j++)
                {
                    sca.Add(BiDirectionalBfs(a.ElementAt(i), b.ElementAt(j), out distance));
                    dis.Add(distance);
                }
            }
            min = dis.Min();
            for (int i = 0; i < dis.Count(); i++)
            {
                if (min == dis[i])
                {
                    id_sca = sca[i];
                    break;
                }
                else
                    continue;

            }
            shortest = min;
            a = new SortedSet<int>();
            b = new SortedSet<int>();
            dis = new List<int>();
            sca = new List<int>();
            var node = synsets[id_sca];
            return node;
        }
        public static int qPeek;
        public static void bfs(Dictionary<int, bool> rout_visit, Dictionary<int, int> rout_dist, Queue<int> q)
        {
            qPeek = q.Peek();
            for (var i = 0; i < Graph[qPeek].Count; i++)
            {
                if (!rout_visit.ContainsKey(Graph[qPeek].ElementAt(i)))
                {
                    rout_visit.Add(Graph[qPeek].ElementAt(i), true);
                    if (rout_dist.ContainsKey(Graph[qPeek].ElementAt(i)))
                        rout_dist[Graph[qPeek].ElementAt(i)] = rout_dist[qPeek] + 1;
                    else
                    {
                        rout_dist.Add(Graph[qPeek].ElementAt(i), rout_dist[qPeek] + 1);
                    }
                }
                q.Enqueue(Graph[qPeek].ElementAt(i));
            }
            q.Dequeue();
        }
        public static int BiDirectionalBfs(int pNode1, int pNode2, out int pathLength)
        {
            Queue<int> q1 = new Queue<int>();
            Dictionary<int, bool> _1Visited = new Dictionary<int, bool>();
            Dictionary<int, int> _1Distance = new Dictionary<int, int>();
            Queue<int> q2 = new Queue<int>();
            Dictionary<int, bool> _2Visited = new Dictionary<int, bool>();
            Dictionary<int, int> _route2Distance = new Dictionary<int, int>();
            int sca = -1;
            int minDistance = int.MaxValue;
            _1Distance.Add(pNode1, 0);
            _1Visited.Add(pNode1, true);
            q1.Enqueue(pNode1);
            while (q1.Count > 0)
            {
                bfs(_1Visited, _1Distance, q1);
            }
            q2.Enqueue(pNode2);
            _route2Distance.Add(pNode2, 0);
            _2Visited.Add(pNode2, true);
            while (q2.Count > 0)
            {
                bfs(_2Visited, _route2Distance, q2);
                if (_1Visited.ContainsKey(qPeek))
                {
                    var dist = _1Distance[qPeek] + _route2Distance[qPeek];
                    if (dist < minDistance)
                    {
                        sca = qPeek;
                        minDistance = dist;
                    }
                }
            }
            pathLength = minDistance;
            return sca;
        }
        public static string out_cast(List<string> l)
        { 
            int s;
            HashSet<string> sc;
            int max = 0;
            int counter = 0;
            string node = string.Empty;
            int[] sum = new int[l.Count];
            for (int i = 0; i < l.Count(); i++)
            {
                for (int j = 0; j < l.Count(); j++)
                {
                    sc = get_sca(l[i], l[j], out s); 
                    sum[i] += s;
                }
            }
            max = sum.Max();
            for (int i = 0; i < sum.Count(); i++)
            {
                if (sum[i] == max)
                {
                    node = l[i];
                    break;
                }
                else
                    continue;
            }
            return node;
        }

        public static Dictionary<int, List<string>> Queries = new Dictionary<int, List<string>>();
        public static void RelationsQueries(String relationsQueries)
        {
            using (var sr = new StreamReader(relationsQueries))
            {
                int cases = int.Parse(sr.ReadLine());
                for (int i = 0; i < cases; i++)
                {
                    String line = sr.ReadLine();
                    string[] lineParts = line.Split(',');
                    List<string> a = new List<string>(2);
                    a.Add(lineParts[0]);
                    a.Add(lineParts[1]);
                    Queries.Add(i, a);
                }
            }

        }
        public static Dictionary<int, List<string>> outQueries = new Dictionary<int, List<string>>();
        public static void OutcastQueries(String qutcastQueries)
        {
            using (var sr = new StreamReader(qutcastQueries))
            {
                try
                {
                    if (sr != null)
                    {
                        int cases = int.Parse(sr.ReadLine());
                        for (int i = 0; i < cases; i++)
                        {
                            String line = sr.ReadLine();
                            string[] lineParts = line.Split(',');
                            List<String> h = new List<string>();
                            for (int j = 0; j < lineParts.Length; j++)
                            {
                                h.Add(lineParts[j]);
                            }
                            outQueries.Add(i, h);

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
           // outQueries = new Dictionary<int, List<string>>();
        }
        #region TestCase
        public static void SampleTest()
        {
            Console.Write("Choice Cases:\n1)Case1\n2)Case2\n3)Case3\n4)Case4\n5)Other special cases");
            Console.Write("\nEnter your choice [1-2-3-4-5]: ");
            char choice1 = (char)Console.ReadLine()[0];
            Console.WriteLine("----------------------------------");
            switch (choice1)
            {
                case '1':
                    InitNouns(@"Sample\Case1\Input\1synsets.txt");
                    ConstructGraph(@"Sample\Case1\Input\2hypernyms.txt");
                    RelationsQueries(@"Sample\Case1\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Sample\Case1\Input\4OutcastQueries.txt");
                    StreamWriter sw1 = new StreamWriter(@"Sample Result\Case1\Case1Output1.txt");
                    StreamWriter sw2 = new StreamWriter(@"Sample Result\Case1\Case1Output2.txt");
                    HashSet<string> d = new HashSet<string>();
                        int k;
                        for (int i = 0; i < Queries.Count; i++)
                        {
                            d = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out k);
                            sw1.Write(k+",");
                            for (int j = 0; j < d.Count; j++)
                            {
                                sw1.Write(d.ElementAt(j));
                            }
                        sw1.WriteLine();
                        }
                    sw1.Close();
                    for (int j=0;j<outQueries.Count;j++)
                        {
                           List<string> f = new List<string>();
                           for(int o=0;o<outQueries[j].Count;o++)
                           {
                            f.Add(outQueries[j].ElementAt(o));
                           }
                            String n = out_cast(f);
                        sw2.WriteLine(n);
                        }
                    sw2.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                    break;
                case '2':
                    InitNouns(@"Sample\Case2\Input\1synsets.txt");
                    ConstructGraph(@"Sample\Case2\Input\2hypernyms.txt");
                    RelationsQueries(@"Sample\Case2\Input\3RelationsQueries.txt");
                    StreamWriter sw3 = new StreamWriter(@"Sample Result\Case2\Case2Output1.txt");
                    HashSet<string> l = new HashSet<string>();
                    int s;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        l = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out s);
                        sw3.Write(s + ",");
                        for (int j = 0; j < l.Count; j++)
                        {
                            sw3.Write(l.ElementAt(j));
                        }
                        sw3.WriteLine();
                    }
                    sw3.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    break;
                case '3':
                    InitNouns(@"Sample\Case3\Input\1synsets.txt");
                    ConstructGraph(@"Sample\Case3\Input\2hypernyms.txt");
                    RelationsQueries(@"Sample\Case3\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Sample\Case3\Input\4OutcastQueries.txt");
                    StreamWriter sw4 = new StreamWriter(@"Sample Result\Case3\Case3Output1.txt");
                    StreamWriter sw5 = new StreamWriter(@"Sample Result\Case3\Case3Output2.txt");
                    HashSet<string> g = new HashSet<string>();
                    int z;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        g = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out z);
                        sw4.Write(z + ",");
                        for (int j = 0; j < g.Count; j++)
                        {
                            sw4.Write(g.ElementAt(j));
                        }
                        sw4.WriteLine();
                    }
                    sw4.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw5.WriteLine(n);
                    }
                    sw5.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                    break;
                case '4':
                    InitNouns(@"Sample\Case4\Input\1synsets.txt");
                    ConstructGraph(@"Sample\Case4\Input\2hypernyms.txt");
                    RelationsQueries(@"Sample\Case4\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Sample\Case4\Input\4OutcastQueries.txt");
                    StreamWriter sw6 = new StreamWriter(@"Sample Result\Case4\Case4Output1.txt");
                    StreamWriter sw7 = new StreamWriter(@"Sample Result\Case4\Case4Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw6.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw6.Write(w.ElementAt(j));
                        }
                        sw6.WriteLine();
                    }
                    sw6.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw7.WriteLine(n);
                    }
                    sw7.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                    break;
                case '5':
                    Console.WriteLine("1)2 commons cases\n2)Many to Many");
                    Console.Write("\nEnter your choice [1-2]: ");
                    char choice2 = (char)Console.ReadLine()[0];
                    if (choice2 == '1')
                    {
                        InitNouns(@"Sample\Other special cases\2 commons case (Bidirectional)\Input\1synsets.txt");
                        ConstructGraph(@"Sample\Other special cases\2 commons case (Bidirectional)\Input\2hypernyms.txt");
                        RelationsQueries(@"Sample\Other special cases\2 commons case (Bidirectional)\Input\3RelationsQueries.txt");
                        StreamWriter sw = new StreamWriter(@"Sample Result\Other special cases\Case5CommonsCaseOutput1.txt");
                        HashSet<string> p = new HashSet<string>();
                        int u;
                        for (int i = 0; i < Queries.Count; i++)
                        {
                            p = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out u);
                            sw.Write(u + ",");
                            for (int j = 0; j < p.Count; j++)
                            {
                                sw.Write(p.ElementAt(j));
                            }
                            sw.WriteLine();
                        }
                        sw.Close();
                        Queries = new Dictionary<int, List<string>>();
                        nounsID = new Dictionary<string, SortedSet<int>>();
                        synsets = new Dictionary<int, HashSet<string>>();
                        Graph = new Dictionary<int, List<int>>();
                    }
                    else
                    {
                        InitNouns(@"Sample\Other special cases\Many-Many (Noun in more than 1 synset)\Input\1synsets.txt");
                        ConstructGraph(@"Sample\Other special cases\Many-Many (Noun in more than 1 synset)\Input\2hypernyms.txt");
                        RelationsQueries(@"Sample\Other special cases\Many-Many (Noun in more than 1 synset)\Input\3RelationsQueries.txt");
                        StreamWriter sw = new StreamWriter(@"Sample Result\Other special cases\Case5ManyToManyOutput1.txt");
                        HashSet<string>  r= new HashSet<string>();
                        int t;
                        for (int i = 0; i < Queries.Count; i++)
                        {
                            r = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out t);
                            sw.Write(t + ",");
                            for (int j = 0; j < r.Count; j++)
                            {
                                sw.Write(r.ElementAt(j));
                            }
                            sw.WriteLine();
                        }
                        sw.Close();
                        Queries = new Dictionary<int, List<string>>();
                        nounsID = new Dictionary<string, SortedSet<int>>();
                        synsets = new Dictionary<int, HashSet<string>>();
                        Graph = new Dictionary<int, List<int>>();

                    }
                    break;
            }
        }
        public static void CompleteTest()
        {
            Console.Write("Choice Cases:\n1)Small\n2)Medium\n3)Large");
            Console.Write("\nEnter your choice [1-2-3-4]: ");
            char choice3 = (char)Console.ReadLine()[0];
            Console.WriteLine("----------------------------------");
            if (choice3 == '1')
            {
                Console.WriteLine("1)Case1\n2)Case2");
                Console.Write("\nEnter your choice [1-2]: ");
                char choice4 = (char)Console.ReadLine()[0];
                if (choice4 == '1')
                {
                    InitNouns(@"Complete\1-Small\Case1_100_100\Input\1synsets.txt");
                    ConstructGraph(@"Complete\1-Small\Case1_100_100\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\1-Small\Case1_100_100\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\1-Small\Case1_100_100\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Small\Case1\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Small\Case1\Output2.txt");
                    HashSet<string> d = new HashSet<string>();
                    int k;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        d = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out k);
                        sw.Write(k + ",");
                        for (int j = 0; j < d.Count; j++)
                        {
                            sw.Write(d.ElementAt(j)+" ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
                else
                {
                    InitNouns(@"Complete\1-Small\Case2_1000_500\Input\1synsets.txt");
                    ConstructGraph(@"Complete\1-Small\Case2_1000_500\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\1-Small\Case2_1000_500\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\1-Small\Case2_1000_500\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Small\Case2\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Small\Case2\Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
            }
            else if (choice3 == '2')
            {
                Console.WriteLine("1)Case1\n2)Case2");
                Console.Write("\nEnter your choice [1-2]: ");
                char choice4 = (char)Console.ReadLine()[0];
                if (choice4 == '1')
                {
                    InitNouns(@"Complete\2-Medium\Case1_10000_5000\Input\1synsets.txt");
                    ConstructGraph(@"Complete\2-Medium\Case1_10000_5000\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\2-Medium\Case1_10000_5000\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\2-Medium\Case1_10000_5000\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Meduim\Case1\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Meduim\Case1\Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
                else
                {
                    InitNouns(@"Complete\2-Medium\Case2_10000_50000\Input\1synsets.txt");
                    ConstructGraph(@"Complete\2-Medium\Case2_10000_50000\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\2-Medium\Case2_10000_50000\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\2-Medium\Case2_10000_50000\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Meduim\Case2\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Meduim\Case2\Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
            }
            else if (choice3 == '3')
            {
                Console.WriteLine("1)Case1\n2)Case2\n3)Case3");
                Console.Write("\nEnter your choice [1-2-3]: ");
                char choice4 = (char)Console.ReadLine()[0];
                if (choice4 == '1')
                {
                    InitNouns(@"Complete\3-Large\Case1_82K_100K_5000RQ\Input\1synsets.txt");
                    ConstructGraph(@"Complete\3-Large\Case1_82K_100K_5000RQ\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\3-Large\Case1_82K_100K_5000RQ\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\3-Large\Case1_82K_100K_5000RQ\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Large\Case1\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Large\Case1\Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
                else if (choice4 == '2')
                {
                    InitNouns(@"Complete\3-Large\Case2_82K_300K_1500RQ\Input\1synsets.txt");
                    ConstructGraph(@"Complete\3-Large\Case2_82K_300K_1500RQ\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\3-Large\Case2_82K_300K_1500RQ\Input\3RelationsQueries.txt");
                    OutcastQueries(@"Complete\3-Large\Case2_82K_300K_1500RQ\Input\4OutcastQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Large\Case2\Output1.txt");
                    StreamWriter sw1 = new StreamWriter(@"Complete Result\Large\Case2\Output2.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    for (int j = 0; j < outQueries.Count; j++)
                    {
                        List<string> f = new List<string>();
                        for (int o = 0; o < outQueries[j].Count; o++)
                        {
                            f.Add(outQueries[j].ElementAt(o));
                        }
                        String n = out_cast(f);
                        sw1.WriteLine(n);
                    }
                    sw1.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                    outQueries = new Dictionary<int, List<string>>();
                }
                else
                {
                    InitNouns(@"Complete\3-Large\Case3_82K_300K_5000RQ\Input\1synsets.txt");
                    ConstructGraph(@"Complete\3-Large\Case3_82K_300K_5000RQ\Input\2hypernyms.txt");
                    RelationsQueries(@"Complete\3-Large\Case3_82K_300K_5000RQ\Input\3RelationsQueries.txt");
                    StreamWriter sw = new StreamWriter(@"Complete Result\Large\Case3\Output1.txt");
                    HashSet<string> w = new HashSet<string>();
                    int a;
                    for (int i = 0; i < Queries.Count; i++)
                    {
                        w = get_sca(Queries[i].ElementAt(0), Queries[i].ElementAt(1), out a);
                        sw.Write(a + ",");
                        for (int j = 0; j < w.Count; j++)
                        {
                            sw.Write(w.ElementAt(j) + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.Close();
                    Queries = new Dictionary<int, List<string>>();
                    nounsID = new Dictionary<string, SortedSet<int>>();
                    synsets = new Dictionary<int, HashSet<string>>();
                    Graph = new Dictionary<int, List<int>>();
                }
            }
        }
#endregion
    }

}