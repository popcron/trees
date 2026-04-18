using System;

[Flags]
public enum ControlFlags
{
    Move = 1,
    OrientHead = 2,
    OrientBody = 4,
    Jump = 8,
}