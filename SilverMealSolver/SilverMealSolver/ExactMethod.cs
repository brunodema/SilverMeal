using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Gurobi;

namespace SilverMeal
{
    public class SilverMealGRBVariables
    {
        public GRBVar[] inventory_level { get; set; }
        public GRBVar[] period_has_production_setup { get; set; }
        public GRBVar[] batch_size { get; set; }
    }

    public class SilverMealGRBConstraints
    {
        public GRBConstr[] inventory_flow { get; set; }
        public GRBConstr[] production_if_allowed { get; set; }
        public GRBConstr[] must_meet_demands { get; set; }
    }

    public class SilverMealExactSolverParams : IExactSolverParams
    {
        public double time_limit { get; set; } = double.MaxValue;
        public int max_solutions { get; set; } = int.MaxValue;
    }

    public class SilverMealExactSolver : SilverMealSolver
    {
        private SilverMealGRBVariables variables = new SilverMealGRBVariables();
        private SilverMealGRBConstraints constraints = new SilverMealGRBConstraints();
        private GRBModel model = new GRBModel(new GRBEnv());
        private SilverMealExactSolverParams parameters;
        private GRBLinExpr OFvalue = new GRBLinExpr();

        public SilverMealExactSolver(ISilverMeanInstance instance, SilverMealExactSolverParams p_parameters) : base(instance) 
        { 
            num_periods = instance.num_periods;
            parameters = p_parameters;

        } // should check if num_periods of instance and solution match

        override public double GetTotalCosts()
        {
            return solution.CalculateTotalCosts();
        }

        public void SetRuntimeOptions()
        {
            model.Set(GRB.DoubleParam.TimeLimit, parameters.time_limit);
            model.Set(GRB.IntParam.SolutionLimit, parameters.max_solutions);
        }

        public override void Run()
        {
            CreateVariables();
            CreateConstraints();
            CreateObjectiveFunction();
            SetRuntimeOptions();
            model.Optimize();
            RetrieveSolutionFromModel();
        }

        private void CreateVariables()
        {
            CreateBatchSizeVariables();
            CreateInventoryLevelVariables();
            CreateProductionSetupVariables();
        }

        private void CreateBatchSizeVariables()
        {
            variables.batch_size = model.AddVars(num_periods, GRB.INTEGER);
        }

        private void CreateInventoryLevelVariables()
        {
            variables.inventory_level = model.AddVars(num_periods, GRB.INTEGER);
        }

        private void CreateProductionSetupVariables()
        {
            variables.period_has_production_setup = model.AddVars(num_periods, GRB.BINARY);
        }

        private void CreateConstraints()
        {
            constraints.inventory_flow = new GRBConstr[num_periods];
            constraints.must_meet_demands = new GRBConstr[num_periods];
            constraints.production_if_allowed = new GRBConstr[num_periods];
            for (int i = 0; i < num_periods; i++)
            {
                CreateInventoryFlowConstraints(i);
                CreateProductionIfAllowedCostraints(i);
                CreateMustMeetDemandConstraint(i);
            }
        }

        private void CreateInventoryFlowConstraints(int i)
        {
            GRBLinExpr previous_period_iventory = i == 0 ? (GRBLinExpr)0.00 : variables.inventory_level[i - 1];
            constraints.inventory_flow[i] = model.AddConstr(variables.inventory_level[i] == previous_period_iventory + variables.batch_size[i] - instance.demand[i], "");
        }

        private void CreateProductionIfAllowedCostraints(int i)
        {
            constraints.production_if_allowed[i] = model.AddConstr(variables.batch_size[i] <= 1000 * variables.period_has_production_setup[i], ""); // I'm using a big M value here
        }

        private void CreateMustMeetDemandConstraint(int i)
        {
            constraints.must_meet_demands[i] = model.AddConstr(variables.inventory_level[i] >= 0.00, "");
        }

        private void CreateObjectiveFunction()
        {
            for (int i = 0; i < num_periods; i++)
            {
                OFvalue += variables.period_has_production_setup[i] * instance.setup_cost + variables.inventory_level[i] * instance.holding_cost;
            }
            model.SetObjective(OFvalue, GRB.MINIMIZE);
        }

        private void RetrieveSolutionFromModel()
        {
            solution.batch_size = variables.batch_size.Select(var => (int)var.X).ToArray();
            solution.inventory_level = variables.inventory_level.Select(var => (int)var.X).ToArray();
        }
    }
}
