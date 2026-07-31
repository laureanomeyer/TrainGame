using System;

[Flags]
public enum TravelAxis
{
    None = 0,
    X = 1 << 0, // 1
    Y = 1 << 1, // 2
    Z = 1 << 2, // 4
    XY = X | Y, // 3
    XZ = X | Z, // 5
    YZ = Y | Z, // 6
    All = X | Y | Z // 7
}   


