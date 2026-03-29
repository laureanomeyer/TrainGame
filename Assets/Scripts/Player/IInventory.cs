using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IInventory
{
    float DepositGold();
    void DepositCoal();
    void CollectCoal();
    bool HasCoal {  get; }
    
}

