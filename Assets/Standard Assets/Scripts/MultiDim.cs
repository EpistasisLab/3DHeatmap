using UnityEngine;
public class MultiDim {


    public static int[,] IntArray (int a, int b) {
        return new int[a,b];
    }

    public static int[,,] IntArray (int a, int b, int c) {
        return new int[a,b,c];
    }

    public static float[,] FloatArray (int a, int b) {
        return new float[a,b];
    }
   
    public static float[,,] FloatArray (int a, int b, int c) {
        return new float[a,b,c];
    }
   
    public static float[,,,] FloatArray (int a, int b, int c, int d) {
        return new float[a,b,c,d];
    }
   
    public static string[,] StringArray (int a, int b) {
        return new string[a,b];
    }

    public static string[,,] StringArray (int a, int b, int c) {
        return new string[a,b,c];
    }
    
    public static ParticleEmitter[,,,] ParticleEmitterArray (int a, int b, int c, int d) {
    	ParticleEmitter[,,,] p = new ParticleEmitter[a, b, c, d];
    	//for(int ia = 0; ia < a; ++ia)
    	//for(int ib = 0; ib < b; ++ib)
    	//for(int ic = 0; ic < c; ++ic)
    	//for(int id = 0; id < d; ++id) p[ia,ib,ic,id] = new ParticleEmitter();
    	return p;
    }
   
    public static int[][] JaggedInt (int a) {
        return new int[a][];
    }

    public static float[][] JaggedFloat (int a) {
        return new float[a][];
    }
   
    public static string[][] JaggedString (int a) {
        return new string[a][];
    }
}
