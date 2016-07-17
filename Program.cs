using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILOG.Concert;
using ILOG.CPLEX;
using ILOG.OPL;

namespace CG_Stop_Frmk
{
    class Program
    {
        static void Main(string[] args)
        {
            StopOptimization so = new StopOptimization();
            so.Optimize();
            Console.ReadKey();
        }
    }
    class StopOptimization
    {
        public int Optimize()
        {
            int status = 127;
            const string DATADIR = "../../../..";
            const double RC_EPS = 0.000001;
            try
            {
                OplFactory.DebugMode = true;
                OplFactory oplF = new OplFactory();
                OplErrorHandler errHandler = oplF.CreateOplErrorHandler();
                OplSettings settings = oplF.CreateOplSettings(errHandler);
                // Make master model 
                Cplex masterCplex = oplF.CreateCplex();
                masterCplex.SetOut(null);

                OplRunConfiguration masterRC0 = oplF.CreateOplRunConfiguration(DATADIR + "/CG_StopArrangement/CG_Stop_Master.mod", DATADIR + "/CG_StopArrangement/CG_Stop_Master.dat");
                masterRC0.Cplex = masterCplex;
                OplDataElements masterDataElements = masterRC0.OplModel.MakeDataElements();

                // prepare sub model source, definition and engine
                OplModelSource subSource = oplF.CreateOplModelSource(DATADIR + "/CG_StopArrangement/CG_Stop_Slave.mod");
                OplModelDefinition subDef = oplF.CreateOplModelDefinition(subSource, settings);
                Cplex subCplex = oplF.CreateCplex();
                subCplex.SetOut(null);

                const int serviceNumber = 10;
                IIntRange serviceItems = masterCplex.IntRange(0, serviceNumber - 1);
                const int stationNumber = 5;
                IIntRange stationItems = masterCplex.IntRange(0, stationNumber - 1);
                double best;
                double currentObj = double.MaxValue;
                string resultTrainPlan = "";


                do
                {
                    best = currentObj;

                    masterCplex.ClearModel();

                    OplRunConfiguration masterRC = oplF.CreateOplRunConfiguration(masterRC0.OplModel.ModelDefinition, masterDataElements);
                    masterRC.Cplex = masterCplex;
                    masterRC.OplModel.Generate();

                    ITupleSet its = masterDataElements.GetElement("TrainSet").AsTupleSet();

                    Console.Out.WriteLine("Solve master.");
                    if (masterCplex.Solve())//求解主问题
                    {
                        currentObj = masterCplex.ObjValue;
                        Console.Out.WriteLine("OBJECTIVE: " + currentObj);
                        status = 0;//主问题有最优解
                    }
                    else
                    {
                        Console.Out.WriteLine("No solution!");
                        status = 1;//主问题无解
                    }
                    //Output Current Master Solution
                    ITupleSet resultTrainSet = masterDataElements.GetElement("TrainSet").AsTupleSet();
                    INumVarMap resultTrainNumVarSet = masterRC.OplModel.GetElement("n").AsNumVarMap();
                    resultTrainPlan = "";
                    foreach (ITuple i in resultTrainSet)
                    {
                        IIntMap stopPlan = i.GetIntMapValue("Stop");
                        double rst = masterCplex.GetValue(resultTrainNumVarSet.Get(i));
                        resultTrainPlan += stopPlan.ToString() + "  Num: " +rst.ToString()+"\n ";
                    }
                    // prepare sub model data
                    OplDataElements subDataElements = oplF.CreateOplDataElements();
                    subDataElements.AddElement(masterDataElements.GetElement("StationSet"));
                    subDataElements.AddElement(masterDataElements.GetElement("ODSet"));
                    subDataElements.AddElement(masterDataElements.GetElement("Dual"));
                    // get reduced costs and set them in sub problem 这里的GetDual是C(BV)*B(0)(-1)，是对偶问题的解
                    INumMap dual = subDataElements.GetElement("Dual").AsNumMap();

                    ITupleSet ODSet = subDataElements.GetElement("ODSet").AsTupleSet();
                    IIntSet StationSet = subDataElements.GetElement("StationSet").AsIntSet();
                    foreach (ITuple it in ODSet)
                    {
                        IForAllRange forAll = (IForAllRange)masterRC.OplModel.GetElement("ctFill").AsConstraintMap().Get(it);
                        dual.Set(it, masterCplex.GetDual(forAll));
                    }
                    //make sub model
                    OplModel subOpl = oplF.CreateOplModel(subDef, subCplex);
                    subOpl.AddDataSource(subDataElements);
                    subOpl.Generate();

                    Console.Out.WriteLine("Solve sub.");
                    if (subCplex.Solve())//求解子问题
                    {
                        Console.Out.WriteLine("OBJECTIVE: " + subCplex.ObjValue);
                        status = 0;
                    }
                    else
                    {
                        Console.Out.WriteLine("No solution!");
                        status = 1;
                    }

                    if (subCplex.ObjValue > -RC_EPS)
                    {
                        break;
                    }

                    // Add variable in master model
                    IIntMap stationFill = masterCplex.IntMap(StationSet);
                    IIntMap serviceFill = masterCplex.IntMap(ODSet);
                    foreach (int i in StationSet)
                    {
                        int coef = (int)subCplex.GetValue(subOpl.GetElement("x").AsIntVarMap().Get(i));
                        stationFill.Set(i, coef);
                    }
                    foreach (ITuple i in ODSet)
                    {
                        int coef = (int)subCplex.GetValue(subOpl.GetElement("k").AsIntVarMap().Get(i));
                        serviceFill.Set(i, coef);
                    }
                    ITupleBuffer buf = masterDataElements.GetElement("TrainSet").AsTupleSet().MakeTupleBuffer(-1);
                    buf.SetIntValue("Id", masterDataElements.GetElement("TrainSet").AsTupleSet().Size);
                    buf.SetIntMapValue("Stop", stationFill);
                    buf.SetIntMapValue("Service", serviceFill);
                    buf.Commit();
                    subOpl.End();
                    masterRC.End();
                } while (/*best != currentObj &&*/ status == 0);
                oplF.End();
                Console.WriteLine(resultTrainPlan);
            }
            catch (ILOG.OPL.OplException ex)
            {
                Console.WriteLine(ex.Message);
                status = 2;
            }
            catch (ILOG.Concert.Exception ex)
            {
                Console.WriteLine(ex.Message);
                status = 3;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                status = 4;
            }

            Console.WriteLine("--Press <Enter> to exit--");
            Console.ReadLine();
            return status;
        }
    }
}
