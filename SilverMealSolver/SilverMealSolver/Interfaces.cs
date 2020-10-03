using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace SilverMeal
{
    public interface ISilverMeanInstance
    {
        public int num_periods { get; set; }
        public double holding_cost { get; set; }
        public double setup_cost { get; set; }

        public int[] demand { get; set; }

        public void Print()
        {
            Console.WriteLine("Printing instance data:");
            Console.WriteLine($"Periods: {num_periods}, Setup Cost: {setup_cost}, Holding Cost: {holding_cost}");
            Console.WriteLine("{0,10} {1,10} {2,10} {3,10} {4,15} {5,15}", "Period", "Demand", "Batch Size", "Inventory", "Setup Cost", "Holding Cost");
            for (int i = 0; i < num_periods; i++)
            {
                Console.WriteLine("{0,10} {1,10} {2,10} {3,10} {4,15:N2} {5,15:N2}", i + 1, demand[i], "-", "-", "-", "-");
            }
        }
    }

    public interface ISolver
    {
        void Run();
        void PrintInstance();
        void PrintSolution();
    }

    public interface IExactSolverParams
    {
        double time_limit { get; set; }
        int max_solutions { get; set; }
    }

    public class SilverMealInstance : ISilverMeanInstance
    {
        public int num_periods { get; set; }
        public double holding_cost { get; set; }
        public double setup_cost { get; set; }

        public int[] demand { get; set; }

        public SilverMealInstance(double p_holding_cost, double p_setup_cost, int[] p_demand)
        {
            num_periods = p_demand.Length;
            holding_cost = p_holding_cost;
            setup_cost = p_setup_cost;
            demand = p_demand;
        }
    }

    public class SilverMealSolution : ISilverMeanInstance
    {
        public int num_periods { get; set; }
        public double holding_cost { get; set; }
        public double setup_cost { get; set; }

        public int[] demand { get; set; }
        public int[] inventory_level { get; set; }
        public int[] batch_size { get; set; }
        public double[] avg_holding_cost { get; set; }

        public SilverMealSolution(ISilverMeanInstance p_instance)
        {
            num_periods = p_instance.num_periods;
            holding_cost = p_instance.holding_cost;
            setup_cost = p_instance.setup_cost;
            demand = p_instance.demand;

            avg_holding_cost = new double[p_instance.num_periods];
            batch_size = new int[p_instance.num_periods];
            inventory_level = new int[p_instance.num_periods];
        }

        public void Print() // probably could implement something using a default method for the interface
        {
            Console.WriteLine("Printing solution data:");
            Console.WriteLine($"Periods: {num_periods}, Setup Cost: {setup_cost}, Holding Cost: {holding_cost}");
            Console.WriteLine("{0,10} {1,10} {2,10} {3,10} {4,15} {5,15}", "Period", "Demand", "Batch Size", "Inventory", "Setup Cost", "Holding Cost");
            for (int i = 0; i < num_periods; i++)
            {
                Console.WriteLine("{0,10} {1,10} {2,10} {3,10} {4,15:N2} {5,15:N2}", i, demand[i], batch_size[i], inventory_level[i], batch_size[i] > 0 ? setup_cost : 0, inventory_level[i] * holding_cost);
            }
            Console.WriteLine($"Total Cost: {CalculateTotalCosts()}");
        }
        public double CalculateTotalCosts()
        {
            double total_costs = inventory_level.Sum(ei => ei * holding_cost) + batch_size.Count(bz => bz > 0) * setup_cost;
            return total_costs;
        }
    }

    public abstract class SilverMealSolver : ISolver
    {
        protected int num_periods;
        protected readonly ISilverMeanInstance instance;
        protected SilverMealSolution solution;

        public abstract double GetTotalCosts();
        public abstract void Run();
        public void PrintInstance() { instance.Print(); }
        public void PrintSolution() { solution.Print(); }

        public SilverMealSolver(ISilverMeanInstance p_instance)
        {
            instance = p_instance;
            solution = new SilverMealSolution(p_instance);
        }
    }
}
