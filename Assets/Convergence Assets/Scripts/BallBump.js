static var minMag : float = 12.0;
static var bigMag : float = 60.0;

function OnCollisionEnter (collision : Collision) {
	var mag : float = collision.relativeVelocity.magnitude;
	if(mag > minMag)
	{
		if(mag > bigMag) mag = bigMag;
		GetComponent.<AudioSource>().volume = mag / bigMag;
		GetComponent.<AudioSource>().Play();
		//Debug.Log("collision " + mag);
	}
	//Debug.Log("Had a collision");
}