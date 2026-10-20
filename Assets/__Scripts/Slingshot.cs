using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    //Inscribed fields are set in the unity inspector pane
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    // slingshot band variables
    public float bandWidth = 0.15f;
    public Material bandMat;
    // audio stuff
    public AudioClip bandSnapFX;
    private AudioSource audioSource;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    // transforms tell us where the ends of the slingshot arms are
    private Transform leftArm;
    private Transform rightArm;
    // slingshot band line renderers
    private LineRenderer leftBand;
    private LineRenderer rightBand;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");

        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // rubber band shenanigans below here
        leftArm = transform.Find("LeftArm");
        rightArm = transform.Find("RightArm");

        // each side of the band gets its own straight LineRenderer, a single
        // 3 point line segment has to build a corner at the pouch, and when the two
        // segments fold back on each other on screen that corner twists

        leftBand = MakeBandSegment("LeftBand");
        rightBand = MakeBandSegment("RightBand");
        UpdateBand(launchPos);

        // set up audio component
        audioSource = GetComponent<AudioSource>();
    }

    LineRenderer MakeBandSegment(string bandName)
    {
        GameObject bandGO = new GameObject(bandName);
        bandGO.transform.SetParent(transform, false);

        LineRenderer lineRend = bandGO.AddComponent<LineRenderer>();
        lineRend.material = bandMat;
        lineRend.startWidth = lineRend.endWidth = bandWidth;
        // making numCapVertices greater than zero makes the band appear round
        lineRend.numCapVertices = 4;
        lineRend.useWorldSpace = true;
        lineRend.positionCount = 2;
        return lineRend;
    }

    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        // the player has pressed the mouse button while over slingshot
        //print("Slingshot:OnMouseDown()");
        aimingMode = true;
        // instantiate a projectile
        projectile = Instantiate(projectilePrefab) as GameObject;
        // start it at the launchpoint
        projectile.transform.position = launchPos;
        // set it to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    Vector3 ArmTip(Transform arm)
    {
        return arm.position + arm.up * arm.lossyScale.y;
    }

    void UpdateBand(Vector3 projPos)
    {
        // slingshotBand.SetPosition(0, ArmTip(leftArm));
        // slingshotBand.SetPosition(1, projPos);
        // slingshotBand.SetPosition(2, ArmTip(rightArm));

        leftBand.SetPosition(0, ArmTip(leftArm));
        leftBand.SetPosition(1, projPos);
        rightBand.SetPosition(0, ArmTip(rightArm));
        rightBand.SetPosition(1, projPos);
    }

    void Update()
    {
        // if slingshot is not in aimingMode, do not run this code
        if(!aimingMode) return;

        // get the current mouse position in 2d screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D -launchPos;
        // limit mouseDelta to the radius of the slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if(mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;
        // update band (duh)
        UpdateBand(projPos);


        if(Input.GetMouseButtonUp(0))
        {
            // the mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            // play bandSnapFX
            audioSource.PlayOneShot(bandSnapFX);

            // switch to slingshot view immediately before setting POI
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);

            FollowCam.POI = projectile; // set the _MainCamera POI
            // add a ProjectileLine to the Projectile
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            // this move the band back to the launch postion
            UpdateBand(launchPos);
            MissionDemolition.SHOT_FIRED();
        }
    }
}
