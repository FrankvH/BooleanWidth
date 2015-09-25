﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// Exact algorithm for finding an optimal linear decomposition of a graph, using dynamic programming.
/*************************/

using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public static class ExactDecomposition
    {
        /*************************/
        // Tables
        /*************************/

        // Pre-computed boolean-dimensions of all possible cuts of the graph
        private static Dictionary<BitSet, long> Cuts = new Dictionary<BitSet, long>();

        // Actual value that can be obtained for a certain cut after previously having done other cuts
        private static Dictionary<BitSet, long> Width = new Dictionary<BitSet, long>();

        // Saves all actual neighborhoods at each cut
        private static Dictionary<BitSet, Set<BitSet>> Neighborhoods = new Dictionary<BitSet, Set<BitSet>>();

        // Optimal split for a parent node, saved so that we can easily reconstruct the decomposition
        private static Dictionary<BitSet, BitSet> OptimalChild = new Dictionary<BitSet, BitSet>();

        /*************************/
        // Initialization
        /*************************/
        private static void Initialize(int n)
        {
            // Clear all the static tables
            Clear();

            // Base cases
            BitSet emptySet = new BitSet(0, n);
            Width[emptySet] = 1;
            Cuts[emptySet] = 1;
            Neighborhoods[emptySet] = new Set<BitSet>() { emptySet };
        }

        // Clears all static tables
        private static void Clear()
        {
            Width = new Dictionary<BitSet, long>();
            Cuts = new Dictionary<BitSet, long>();
            Neighborhoods = new Dictionary<BitSet, Set<BitSet>>();
        }

        /*************************/
        // Computing Exact Decompositions
        /*************************/
        public static Decomposition Compute(Graph graph)
        {
            Initialize(graph.Size);

            // Construct the table with the correct width values
            ConstructTree(graph, graph.Vertices);

            // Retrieve a sequence that will result in our bound not being exceeded
            Tree tree = RetrieveTree(graph);

            return new Decomposition(graph, tree);
        }

        // Constructs the actual width values 
        private static void ConstructTree(Graph graph, BitSet A)
        {

            long min = A.Count == 1 ? 0 : long.MaxValue;
            int n = graph.Size;
            int v = -1;     // v is the vertex that if we remove it from A, we have the smallest number of neighbors
            BitSet optimal = new BitSet(0, n);

            Set<BitSet> subsets = new Set<BitSet>(new BitSet(0, n));
            foreach (int a in A)
            {
                Set<BitSet> newSubsets = new Set<BitSet>();
                foreach (BitSet j in subsets)
                {
                    BitSet subset = j + a;
                    BitSet inverse = A - subset;

                    if (subset.Equals(A)) continue; // only consider strict subsets

                    if (!Width.ContainsKey(subset))
                        ConstructTree(graph, subset);

                    if (!Width.ContainsKey(inverse))
                        ConstructTree(graph, inverse);

                    newSubsets.Add(subset); // add this for the next iteration

                    long max = Math.Max(Width[subset], Width[inverse]); // either S or A\S will be the bottleneck

                    if (max < min)
                    {
                        min = max;
                        optimal = subset; // it doesn't matter if we take j + a or A - (j + a), since when retrieving the tree we split them anyway

                        if (inverse.Count == 1)
                            v = inverse.First();
                    }
                }

                subsets.AddRange(newSubsets);
            }

            v = v == -1 ? A.First() : v;
            BitSet nv = graph.OpenNeighborhood(v) * (graph.Vertices - (A - v));
            Set<BitSet> un = new Set<BitSet>();
            foreach (BitSet _base in Neighborhoods[A - v])
            {
                un.Add(_base - v);          // previous neighbor without v is a possible new neighborhood
                un.Add((_base - v) + nv);   // previous neighbor without v, unioned with the neighborhood of v is a possible new neighborhood
            }
            Neighborhoods[A] = un;
            Cuts[A] = Neighborhoods[A].Count;

            Width[A] = Math.Max(min, Cuts[A]);  // Actual possible width to get to this cut
            OptimalChild[A] = optimal;
        }


        // Retreives the actual ordering of a certain ordering that will not validate our bound
        private static Tree RetrieveTree(Graph graph)
        {
            Tree tree = new Tree();

            Queue<BitSet> queue = new Queue<BitSet>();
            queue.Enqueue(graph.Vertices);

            while (queue.Count != 0)
            {
                BitSet A = queue.Dequeue();
                tree.Insert(A);

                if (OptimalChild[A].IsEmpty) continue;

                queue.Enqueue(OptimalChild[A]);
                queue.Enqueue(A - OptimalChild[A]);
            }

            return tree;
        }

        /*************************/
        // Computing Exact Linear Decompositions
        /*************************/

        public static LinearDecomposition ExactlinearDecomposition(Graph graph)
        {
            Initialize(graph.Size);

            // Construct the table with the correct width values
            ConstructSequence(graph, graph.Vertices);

            // Retrieve a sequence that will result in our bound not being exceeded
            List<int> sequence = RetrieveSequence(graph, graph.Vertices);

            return new LinearDecomposition(graph, sequence);
        }

        // Constructs the actual width values 
        // The idea is the following: If we want to know the optimal width for a certain cut A, thus Width[A], then we can obtain this by
        // either taking the max(Cuts[A], the minimum over all widths of Width[A - a]), which is our recurrence relation.
        private static void ConstructSequence(Graph graph, BitSet A)
        {
            // min saves the minimum size of all neighborhoods of the cut [A - a], where a can be any vertex in A
            long min = long.MaxValue;

            // v will be the optimal choice to leave out in the previous iteration in order to obtain A's full neighborhood
            int v = -1;

            foreach (int a in A)
            {
                BitSet previous = A - a;

                // If we have not constructed the previous step yet, then go in recursion and do so
                if (!Neighborhoods.ContainsKey(previous))
                    ConstructSequence(graph, previous);

                // Save the minimum value
                if (Width[previous] < min)
                {
                    min = Width[previous];
                    v = a;
                }
            }

            // Obtain the neighborhood of v
            BitSet nv = graph.OpenNeighborhood(v) * (graph.Vertices - A);

            // We save the full set of neighborhood vertices at cut A. It does not matter that v was chosen arbitrarely; we always end up in the same collection of neighboring vertices for the set A
            Set<BitSet> un = new Set<BitSet>();
            foreach (BitSet _base in Neighborhoods[A - v])
            {
                un.Add(_base - v);          // previous neighbor without v is a possible new neighborhood
                un.Add((_base - v) + nv);   // previous neighbor without v, unioned with the neighborhood of v is a possible new neighborhood
            }

            // Save all entries
            Neighborhoods[A] = un;              // List of all neighbors at cut A
            Cuts[A] = Neighborhoods[A].Count;   // Dimension at this cut
            Width[A] = Math.Max(min, Cuts[A]);  // Actual possible width to get to this cut

        }

        // Retreives the actual ordering of a certain ordering that will not validate our bound
        private static List<int> RetrieveSequence(Graph graph, BitSet A)
        {
            List<int> sequence = new List<int>();

            // We know that the value saved at Width[graph.Vertices] will be the best value that we can get
            // Furthermore, we know that there is a possibility to remove a vertex and get a smaller or equal Width value, in other words Width[A - a] <= Width[A]
            // This way we navigate through the table and find a sequence that never will exceed the bound that is set, and we know for sure
            // that such a path exists.
            foreach (int a in A)
            {
                BitSet previous = A - a;
                if (Width[previous] <= Width[graph.Vertices])
                {
                    sequence.Add(a);
                    sequence.AddRange(RetrieveSequence(graph, previous));
                    break;
                }
            }
            
            return sequence;
        }

    }
}
