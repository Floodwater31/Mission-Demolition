using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    // Add this for the sound
    public AudioClip rubberBandSound;


    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    // Add LineRenderer for rubber band
    private LineRenderer _band;

    // AudioSource for playing sound
    private AudioSource audioSource;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // Initialize _band
        _band = gameObject.AddComponent<LineRenderer>();
        _band.positionCount = 2; // The line has 2 points (start and end)
        _band.startWidth = 0.05f; // Adjust the thickness of the line
        _band.endWidth = 0.05f; // Same thickness for the end
        _band.material = new Material(Shader.Find("Sprites/Default")); // Use a basic material
        _band.startColor = Color.yellow; // The color of the line
        _band.endColor = Color.yellow; // The color at the end of the line

        // Initialize AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = rubberBandSound; // Assign the rubber band sound
    }

    void OnMouseEnter()
    {
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;

        // Instantiate a Projectile
        projectile = Instantiate(projectilePrefab) as GameObject;
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        // Show the rubber band
        _band.enabled = true;
    }

    void Update()
    {
        if (!aimingMode) return;

        // Get the current mouse position in 2D screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;

        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // Move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update _band to show the rubber band
        _band.SetPosition(0, launchPos); // Start of the rubber band at the slingshot
        _band.SetPosition(1, projPos);   // End of the rubber band at the projectile

        if (Input.GetMouseButtonUp(0))
        {
            // The mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            // Play the rubber band release sound
            audioSource.Play();

            // Switch to slingshot view immediately before setting POI
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile; // Set the _MainCamera POI
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;

            MissionDemolition.SHOT_FIRED();

            // Hide the rubber band
            _band.enabled = false;
        }
    }
}