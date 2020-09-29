using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace SilverMealSolver
{
    class Options // next time, split different group of commands in different classes, with a standard 'run' method
    {
        [Option('f', "file", Group = "instance", HelpText = "Path to file which contains the instance data. Must have the correct structure (the program will not check for exceptions", Required = true)]
        public string filename { get; set; }

        [Option("sample", Default = 0, Group = "instance", HelpText = "Uses a sample instance to be solved by the chosen algorithm. Must be a value between 0 <= x <= 3.")]
        public int? sample_index { get; set; }

        [Option("heuristic", HelpText = "Uses the Silver-Meal heuristic to solve the instance.", Group = "method")]
        public bool heuristic { get; set; }

        [Option("exact", HelpText = "Uses a MIP formulation to solve the instance.", Group = "method")]
        public bool exact { get; set; }

        [Option("compare", HelpText = "Runs both the heuristic and the exact method, one after the other.", Group = "method")]
        public bool compare { get; set; }
    }
}
