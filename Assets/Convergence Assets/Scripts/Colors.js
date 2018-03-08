static function HSLtoColor(H : float, S : float, L : float) : Color
{
	var r : float = 0.0;
	var g : float = 0.0;
	var b : float = 0.0;
	var temp1 : float;
	var temp2 : float;
	
	if(L == 0.0) return new Color(0.0, 0.0, 0.0);
	
	if(S == 0.0) return new Color(L, L, L);
	
	temp2 = ((L <= 0.5) ? L * (1.0 + S) : L + S - (L * S));
	temp1 = 2.0 * L - temp2;
	
	var t3 : float[] = [H + 1.0 / 3.0, H, H - 1.0/3.0];
	var clr : float[] = [0.0, 0.0, 0.0];
	
	var i : int;
	for(i = 0; i < 3; ++i)
	{
		if(t3[i] < 0.0) t3[i] += 1.0;
		if(t3[i] > 1.0) t3[i] -= .0;
		
		if(6.0 * t3[i] < 1.0) clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
        else if(2.0 * t3[i] < 1.0) clr[i]=temp2;
        else if(3.0 * t3[i] < 2.0) clr[i]=(temp1+(temp2-temp1)*((2.0/3.0)-t3[i])*6.0);
        else clr[i]=temp1;
	}
	return new Color(clr[0], clr[1], clr[2]);
}