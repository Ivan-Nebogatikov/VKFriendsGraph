using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.MDS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Color = Microsoft.Msagl.Drawing.Color;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Node = Microsoft.Msagl.Drawing.Node;

namespace VKFriendsGraph
{
    public static class GraphGenerator
    {
        private static string ResultDir =>
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "result");

        public static void ParseFields(this Graph graph, User user, Func<User, List<string>> dataType, double threshold, Color edgeColor, int possibleReplaces = 0)
        {
            foreach (User friend in user.Friends)
            {
                var fieldValues = friend.Friends
                    .SelectMany(dataType)
                    .GroupBy(x => x)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();
                if (!fieldValues.Any())
                {
                    continue;
                }
                int sum = fieldValues.Select(x => x.Count).Sum();
                if (dataType(friend).Count <= possibleReplaces && fieldValues.First().Count > sum * threshold)
                {
                    friend.WasEdited = true;
                    dataType(friend).Add(fieldValues.First().Name);
                }
            }

            AddNode(graph, user);
            foreach (User friend in user.Friends)
            {
                Node node = AddNode(graph, friend);
                string theirCommonField = dataType(friend).Intersect(dataType(user)).LastOrDefault();
                if (theirCommonField != null)
                {
                    Edge edge = graph.AddEdge(user.Name, friend.Name);
                    edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                    edge.Attr.LineWidth = 3;
                    edge.Attr.Color = edgeColor;
                    ++friend.Multiplexity;
                }
            }

            foreach (User friend in user.Friends)
            {
                foreach (User friendsFriend in friend.Friends)
                {
                    Node node = graph.FindNode(friendsFriend.Name);
                    if (!node.InEdges.Any(x => x.Source == friend.Name && x.Attr.Color == edgeColor))
                    {
                        string theirCommonField = dataType(friend).Intersect(dataType(friendsFriend)).LastOrDefault();
                        if (theirCommonField != null)
                        {
                            Edge edge = graph.AddEdge(friendsFriend.Name, friend.Name);
                            edge.Attr.LineWidth = 3;
                            edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                            edge.Attr.Color = edgeColor;
                        }
                    }
                }
            }
        }

        public static void SaveGraph(this Graph graph, long? userId, int graphWidth)
        {
            foreach (Node node in graph.Nodes)
            {
                var userInfo = (User) node.UserData;
                node.LabelText += $" {(userInfo.WasEdited ? "*" : "")}({userInfo.Multiplexity})";
            }

            graph.LayoutAlgorithmSettings = new MdsLayoutSettings();
            var renderer = new Microsoft.Msagl.GraphViewerGdi.GraphRenderer(graph);
            renderer.CalculateLayout();
            var bitmap = new Bitmap(graphWidth, (int)(graph.Height * (graphWidth / graph.Width)), PixelFormat.Format32bppPArgb);
            renderer.Render(bitmap);
            if (!Directory.Exists(ResultDir))
            {
                Directory.CreateDirectory(ResultDir);
            }
            bitmap.Save(Path.Combine(ResultDir, $"{userId ?? 0}_graph.png"));
        }

        private static Node AddNode(Graph graph, User user)
        {
            Node node = graph.FindNode(user.Name);
            if (node == null)
            {
                node = graph.AddNode(user.Name);
                node.UserData = user;
            }

            return node;
        }
    }
}
