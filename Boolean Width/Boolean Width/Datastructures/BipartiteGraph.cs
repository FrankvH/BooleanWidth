﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// A bipartite graph is a graph consisting of a left and right part of vertices,
// and all edges have one endpoint in the left and one endpoint in the right part.
/*************************/

using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public class BipartiteGraph : Graph
    {
        /*************************/
        // Basic attributes
        /*************************/

        // Left set of vertices
        public BitSet Left { get { return left.Copy(); } }
        private readonly BitSet left;

        // Right set of vertices
        public BitSet Right { get { return right.Copy(); } }
        private readonly BitSet right;

        /*************************/
        // Constructors
        /*************************/

        // Basic constructor that initializes empty sets for the Left and Right part
        private BipartiteGraph(int size) : this(new BitSet(0, size), new BitSet(0, size), size) { }

        // Basic constructor 
        private BipartiteGraph(BitSet _left, BitSet _right, int size) : base(size)
        {
            left = _left.Copy();
            right = _right.Copy();
        }

        // This constructor returns a new bipartite graph by putting all vertices in 'left' on one side, and 'right' on the other side
        // There will be an edge between two vertices if there was an edge in the original graph
        public BipartiteGraph(Graph graph, BitSet _left, BitSet _right)
            : this(_left, _right, _left.Count + _right.Count)
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();
            int i = 0;
            
            foreach (int v in left + right)
                mapping[v] = i++;

            foreach (int v in left)
                foreach (int w in graph.OpenNeighborhood(v) * right)
                    Connect(mapping[v], mapping[w]);
        }
    }
}
