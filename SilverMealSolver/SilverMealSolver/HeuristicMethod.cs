using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using SilverMeal;

namespace SilverMeal
{
    public class SilverMealHeuristicSolver : SilverMealSolver
    {
        public SilverMealHeuristicSolver(ISilverMeanInstance instance) : base(instance) { num_periods = instance.num_periods; } // should check if num_periods of instance and solution match. 'verbose' is not used here

        override public double GetTotalCosts()
        {
            return solution.CalculateTotalCosts();
        }

        private double ClosedIntervalAvgCostSummation(int begin, int end)
        {
            double ret = instance.setup_cost;
            for (int i = begin; i <= end; i++)
            {
                ret += (i - begin + 1) * instance.demand[i] * instance.holding_cost;
            }

            if (begin == end)
            {
                return ret;
            }
            return ret/(end - begin + 1);
        }

        private bool CurrentAvgCostIsHigherThanPrevious(int i)
        {
            return solution.avg_holding_cost[i] > solution.avg_holding_cost[i - 1];
        }

        private bool CustomerMeetsPeriodsDemand(int i)
        {
            return solution.inventory_level[i] >= instance.demand[i];
        }

        public override void Run()
        {
            Console.WriteLine("Running heuristic method...");
            InitializeSolution();
            int floor_period = 0;
            for (int i = 1; i < num_periods; i++)
            {
                if (CurrentAvgCostIsHigherThanPrevious(i))
                {
                    SetSingleSetupScheduleForClosedInterval(floor_period, i - 1);
                    floor_period = i;
                    SetSingleSetupScheduleForClosedInterval(floor_period, num_periods - 1);
                    CalculateInventoryLevelUntilEnd();
                }
            }
        }

        private void InitializeSolution()
        {
            SetSingleSetupScheduleForClosedInterval(0, num_periods - 1);
            CalculateInventoryLevelUntilEnd();
        }

        private void SetSingleSetupScheduleForClosedInterval(int begin, int end)
        {
            for (int i = begin; i <= end; i++)
            {
                solution.batch_size[i] = CalculateBatchSizeGivenClosedInterval(begin, end, i);
                solution.avg_holding_cost[i] = ClosedIntervalAvgCostSummation(begin, i);
            }
        }

        private int CalculateBatchSizeGivenClosedInterval(int begin, int end, int i)
        {
            if (i == begin)
            {
                return instance.demand.Skip(begin).Take(end - begin + 1).Sum(); // skips to beggining, gets elements until end position
            }
            else
            {
                return 0;
            }
        }

        private void CalculateInventoryLevelUntilEnd()
        {
            solution.inventory_level[0] = solution.batch_size[0] - solution.demand[0];
            for (int i = 1; i < num_periods; i++)
            {
                solution.inventory_level[i] = solution.inventory_level[i - 1] - instance.demand[i] + solution.batch_size[i] ;
            }
        }
    }
}
