/*********************************************
 * OPL 12.3 Model
 * Author: RCS-Liao
 * Creation Date: 2016-7-15 at PM05:56:11
 * This model file is the model of the main train stopping problem for column generation 
 *********************************************/

{int} StationSet = ...;

tuple OD
{
  int O;
  int D;
  }
{OD} ODSet = ...;

int ServiceRequire[ODSet] = ...;

float Dual[ODSet] = ...;

tuple Train
{
  int Id;
  int Stop[StationSet];
  int Service[ODSet]; 
  }

{Train} TrainSet = ...;/*The train set is to be add by the slave problem. When initial, we provide an initial solution.*/

dvar float+ n[TrainSet];

minimize 10*sum(f in TrainSet)n[f]+sum(f in TrainSet)(n[f] * sum(s in StationSet)f.Stop[s]);

subject to
{
  forall(s in ODSet)
    {
          ctFill:
          sum(f in TrainSet)n[f]*f.Service[s] >= ServiceRequire[s];    
      }
  }