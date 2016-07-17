/*********************************************
 * OPL 12.3 Model
 * Author: RCS-Liao
 * Creation Date: 2016-7-15 at ����05:56:58
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
  //һ�г���಻��ͣ����3վ
  sum(s in StationSet)x[s]<=3;
  
  forall(s in ODSet)
    {
      //��x[s.O]ͬʱΪx[s.D]1ʱ��k[s]Ϊ1������Ϊ0
      k[s]>=1+M*(x[s.O]+x[s.D]-2);
      k[s]<=M*(x[s.O]+x[s.D]);
      k[s]<=M*(1+x[s.O]-x[s.D]);
      k[s]<=M*(1+x[s.D]-x[s.O]);
      }
  }