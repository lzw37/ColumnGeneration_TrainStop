/*********************************************
 * OPL 12.3 Model
 * Author: RCS-Liao
 * Creation Date: 2016-7-15 at ÏÂÎç05:56:11
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

{Train} TrainSet = ...;

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