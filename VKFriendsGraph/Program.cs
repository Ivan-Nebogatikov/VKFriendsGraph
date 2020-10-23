using CommandLine;
using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;

namespace VKFriendsGraph
{
    public class Options
    {
        [Option('l', "login", Required = false, HelpText = "Email/phone number.")]
        public string Login { get; set; }

        [Option('p', "password", Required = false, HelpText = "Password.")]
        public string Password { get; set; }


        [Option('i', "id", Required = false, HelpText = "User id to collect data. Default is the user with token.")]
        public long? Id { get; set; }

        [Option('t', "threshold", Required = false, HelpText = "Threshold to replace activity when not specified")]
        public double Threshold { get; set; } = 0.7;

        [Option('n', "nocache", Required = false, HelpText = "Do not use cache to draw a graph")]
        public bool NoCache { get; set; }


        [Option('w', "width", Required = false, HelpText = "Graph width")]
        public int Width { get; set; } = 4000;
    }

    class Program
    {
        private const byte DefaultAlpha = 100;
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(options =>
                   {
                       try
                       {
                           if (!string.IsNullOrEmpty(options.Login) && !string.IsNullOrEmpty(options.Password))
                           {
                               ApiHelper.SetToken(options.Login, options.Password);
                           }

                           User user = ApiHelper.GetUserFriendsGraph(options.Id, !options.NoCache);
                           var graph = new Graph();
                           var r = Color.Red;
                           var g = Color.Green;
                           var b = Color.Blue;
                           var bl = Color.Black;
                           r.A = DefaultAlpha;
                           g.A = DefaultAlpha;
                           b.A = DefaultAlpha;
                           bl.A = DefaultAlpha;
                           graph.ParseFields(user, x => new List<string> { x.Sex.ToString() }, options.Threshold, bl, -1);
                           graph.ParseFields(user, x => x.Schools, options.Threshold, g);
                           graph.ParseFields(user, x => x.Works, options.Threshold, b);
                           graph.ParseFields(user, x => x.Universities, options.Threshold, r, 1);
                           graph.SaveGraph(options.Id, options.Width);
                           Console.WriteLine("Processing done!");
                       }
                       catch (Exception e)
                       {
                           Console.WriteLine(e.Message);
                       }
                   });
        }
    }
}
