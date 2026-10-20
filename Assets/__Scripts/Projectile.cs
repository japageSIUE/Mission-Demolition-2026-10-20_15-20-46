using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    const int LOOKBACK_COUNT = 10;
    static List<Projectile> PROJECTILES = new List<Projectile>();

    [SerializeField]
    private bool _awake = true;
    public bool awake
    {
        get{ return _awake; }
        private set{ _awake = value; }
    }

    private Vector3 prevPos;
    // this private list stores the history of projectile's move distance
    private List<float> deltas = new List<float>();
    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        awake = true;
        prevPos = new Vector3(1000,1000,0);
        deltas.Add(1000);

        PROJECTILES.Add(this);
    }

    void FixedUpdate()
    {
        if(rigid.isKinematic || !awake) return;

        Vector3 deltaV3 = transform.position - prevPos;
        deltas.Add(deltaV3.magnitude);
        prevPos = transform.position;

        // limit lookback; while shouldn't be used very frequently if at all
        while(deltas.Count > LOOKBACK_COUNT)
        {
            deltas.RemoveAt(0);
        }

        //Iterate over deltas and find the greatest one
        float maxDelta = 0;
        foreach(float f in deltas)
        {
            if(f > maxDelta) maxDelta = f;
        }

        // if the projectile hasn't moved more than the sleepThreshold
        if(maxDelta <= Physics.sleepThreshold)
        {
            // set awake to false and put the rigidbody to sleep
            awake = false;
            rigid.Sleep();
        }
    }

    private void OnDestroy()
    {
        PROJECTILES.Remove(this);
    }

    static public void DESTROY_PROJECTILES()
    {
        foreach(Projectile p in PROJECTILES)
        {
            Destroy(p.gameObject);
        }
    }
}
