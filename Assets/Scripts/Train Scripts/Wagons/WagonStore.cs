using UnityEngine;

public class WagonStore : IWagonID
{
    private int id;
    public int Id => id;

    public WagonStore(int Id)
    {
        this.id = Id;
    }


}
