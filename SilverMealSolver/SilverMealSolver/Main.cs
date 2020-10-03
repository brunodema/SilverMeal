using CommandLine;
using CommandLine.Text;
using SilverMeal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilverMealSolver
{
    static class SampleInstances // probably is a waste to have this as static, since initializes objects that probably won't be used
    {
        public static ISilverMeanInstance[] sample =
        {
            new SilverMealInstance(2, 80.00, new int[] { 18, 30, 42, 5, 20 }),
            new SilverMealInstance(0.11, 265.62, new int[] { 32, 26, 65, 12, 41 }),
            new SilverMealInstance(0.21, 234.69, new int[] { 42, 65, 44, 69, 57, 52, 92, 96, 22, 42 }),
            new SilverMealInstance(0.5, 25.00, new int[] { 20, 32, 30, 21 })
        };
    }

    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var result = Parser.Default.ParseArguments<Options>(args).WithParsed(options => RunAndReturnExitCode(options)).WithNotParsed(errs => { });
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return 1;
            }
        }

        static int RunAndReturnExitCode(Options options)
        {
            ISilverMeanInstance instance = null;
            // first option set
            if (options.filename != null)
            {
                instance = ReadInstanceFromFile(options.filename);
            }
            else if (options.sample_index != null)
            {
                if (options.sample_index < 0 || options.sample_index > 3)
                {
                    throw new Exception("Sample index value must be a value between 0 and 3");
                }
                Console.WriteLine($"Using sample instance number {options.sample_index}...");
                instance = SampleInstances.sample[(int)options.sample_index];
            }
            // second option set
            if (options.heuristic == true)
            {
                ISolver solver = new SilverMealHeuristicSolver(instance);
                solver.PrintInstance();
                solver.Run();
                solver.PrintSolution();
                return 0;
            }
            else if (options.exact == true)
            {
                ISolver solver = new SilverMealExactSolver(instance, new SilverMealExactSolverParams() {verbose = options.verbose });
                solver.PrintInstance();
                solver.Run();
                solver.PrintSolution();
                return 0;
            }
            else if (options.compare == true)
            {
                ISolver solver = new SilverMealHeuristicSolver(instance);
                solver.PrintInstance();
                solver.Run();
                solver.PrintSolution();
                solver = new SilverMealExactSolver(instance, new SilverMealExactSolverParams() { verbose = options.verbose });
                solver.PrintInstance();
                solver.Run();
                solver.PrintSolution();
                return 0;
            }
            return 1;
        }

        static ISilverMeanInstance ReadInstanceFromFile(string filename)
        {
            try
            {
                Console.WriteLine($"Attempting to read data from file '{filename}'...");
                StreamReader stream = new StreamReader(filename);
                int num_periods = Convert.ToInt32(stream.ReadLine());
                int[] demand = new int[num_periods];
                int i = 0;
                while (i < num_periods)
                {
                    demand[i] = Convert.ToInt32(stream.ReadLine());
                    ++i;
                }
                double holding_cost = Convert.ToDouble(stream.ReadLine());
                double setup_cost = Convert.ToDouble(stream.ReadLine());

                return new SilverMealInstance(holding_cost, setup_cost, demand);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
