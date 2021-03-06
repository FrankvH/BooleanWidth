﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// The Bron–Kerbosch algorithm is originally an algorithm used to find all maximal cliques in a graph, 
// but here it is modified in order to count all maximal independent sets.
/*************************/

using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public static class BronKerbosch
    {
        // Initial call
        public static int Compute(Graph graph)
        {
            return Compute(graph, new BitSet(0, graph.Size), graph.Vertices, new BitSet(0, graph.Size));
        }

        // Counts the number of independent sets in a graph, such that:
        // - All vertices in R are included in the independent set, initially empty
        // - Some of the vertices in P are included in the independent set, initially all vertices of the graph
        // - None of the vertices in X are included in the independent set, initially empty
        private static int Compute(Graph graph, BitSet R, BitSet P, BitSet X)
        {
            // Base case, when P and X are both empty we cannot expand the IS
            if (P.IsEmpty && X.IsEmpty)
                return 1;

            int count = 0;
            BitSet copy = P.Copy();

            // Foreach vertex v in P we include it in the IS and compute how many maximal IS will include v by going into recursion.
            foreach (int v in copy)
            {
                count += Compute(graph, R + v, P - graph.ClosedNeighborhood(v), X - graph.OpenNeighborhood(v));
                P.Remove(v);
                X.Add(v);
            }

            return count;
        }
    }
}
