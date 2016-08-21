using UnityEngine;
using System.Collections;

public static class Helper
{
    public static float[] ToArray(this Vector3 inp)
    {
        return new float[] {inp.x, inp.y, inp.z};
    }
}
