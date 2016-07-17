/*********************************************
 * OPL 12.3 Model
 * Author: RCS-Liao
 * Creation Date: 2016-7-15 at 下午05:56:58
 *********************************************/
int M=50000;

{int} StationSet = ...;

tuple OD
{
  int O;
  int D;
  }
{OD} ODSet = ...;

float Dual[ODSet] = ...;

dvar boolean x[StationSet];
dvar boolean k[ODSet]; 

minimize 10+sum(s in StationSet)(x[s])-(sum(s in ODSet)(Dual[s]*k[s]));

subject to
{
  //一列车最多不能停超过3站
  sum(s in StationSet)x[s]<=3;
  
  forall(s in ODSet)
    {
      //当x[s.O]同时为x[s.D]1时，k[s]为1，否则为0
      k[s]>=1+M*(x[s.O]+x[s.D]-2);
      k[s]<=M*(x[s.O]+x[s.D]);
      k[s]<=M*(1+x[s.O]-x[s.D]);
      k[s]<=M*(1+x[s.D]-x[s.O]);
      }
  }