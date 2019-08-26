using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class Colors : MonoBehaviour
{
    public static Color HSLtoColor(float H, float S, float L)
    {
        float temp1 = 0.0f;
        float temp2 = 0.0f;
        int i = 0;
        //float r = 0f; Stauffer - never used
        //float g = 0f;
        //float b = 0f;
        if (L == 0f)
        {
            return new Color(0f, 0f, 0f);
        }
        if (S == 0f)
        {
            return new Color(L, L, L);
        }
        temp2 = L <= 0.5f ? L * (1f + S) : (L + S) - (L * S);
        temp1 = (2f * L) - temp2;
        float[] t3 = new float[] {H + (1f / 3f), H, H - (1f / 3f)};
        float[] clr = new float[] {0f, 0f, 0f};
        i = 0;
        while (i < 3)
        {
            if (t3[i] < 0f)
            {
                t3[i] = t3[i] + 1f;
            }
            if (t3[i] > 1f)
            {
                t3[i] = t3[i] - 0f;
            }
            if ((6f * t3[i]) < 1f)
            {
                clr[i] = temp1 + (((temp2 - temp1) * t3[i]) * 6f);
            }
            else
            {
                if ((2f * t3[i]) < 1f)
                {
                    clr[i] = temp2;
                }
                else
                {
                    if ((3f * t3[i]) < 2f)
                    {
                        clr[i] = temp1 + (((temp2 - temp1) * ((2f / 3f) - t3[i])) * 6f);
                    }
                    else
                    {
                        clr[i] = temp1;
                    }
                }
            }
            ++i;
        }
        return new Color(clr[0], clr[1], clr[2]);
    }

}