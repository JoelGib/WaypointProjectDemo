using UnityEngine;
using UnityEngine.AI;
using Cinemachine;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent myNavMeshAgent;
    
    private Animator myAnim;
    private LineRenderer myLineRenderer;
    [SerializeField] private Transform CameraRoot;
    [SerializeField] private GameObject clickMarker ;
    [SerializeField] private Transform visualObjectsParent;
    [SerializeField] private GameObject characterSelectedMarker;
    public CinemachineVirtualCamera vCam;
    [SerializeField] private AudioScript AudioRef;
    private bool isSelected = false;
    private AudioSource audioSource;
    private bool isMoving = false;
    // private bool isReachedDestination = false;

    public delegate void DisableWaypoints();
    public static event DisableWaypoints OnDisableWaypoints;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();

        // Debug.Log("Remaining distance: " + Vector3.Distance(myNavMeshAgent.destination, transform.position));

        if(IsAtDestination()){
            HandleReachedDestination();
        }
        else if(myNavMeshAgent.hasPath){
            DrawPath();
        }
        
    }

    private void Initialize(){
        myNavMeshAgent = GetComponent<NavMeshAgent>();
        myAnim = GetComponent<Animator>();
        myLineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        clickMarker.SetActive(false);
        characterSelectedMarker.SetActive(false);

        myLineRenderer.startWidth = 0.15f;
        myLineRenderer.endWidth = 0.15f;
        myLineRenderer.positionCount = 0;
    }

    private void ClickToMove(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if(hasHit){
            SetDestination(hit.point);
            FollowPathToDestination();
        }
    }

    private void HandleMouseInput(){
        if(Input.GetMouseButtonDown(1) && isSelected){
            ClickToMove();
        }
    }

    private bool IsAtDestination(){
        return Vector3.Distance(myNavMeshAgent.destination, transform.position) <= myNavMeshAgent.stoppingDistance;
    }

    private bool hasPlayedNavigationStartSound = false;

    private void TogglePlayerMovement(bool canMove, float speed)
    {
        myNavMeshAgent.speed = speed;

        if (canMove)
        {
            clickMarker.transform.SetParent(visualObjectsParent);
            clickMarker.SetActive(true);

            if (!hasPlayedNavigationStartSound && speed == 0)
            {
                audioSource.loop = false;
                AudioRef.PlayNavigationStartAudio(audioSource);
                myAnim.SetBool("isRunning", false);
                hasPlayedNavigationStartSound = true;
            }
            else if (speed == 8)
            {
                audioSource.loop = true;
                AudioRef.PlayRunningFootstepAudio(audioSource);
                myAnim.SetBool("isRunning", true);
            }
            else if (hasPlayedNavigationStartSound && speed != 0)
            {
                // Stop the audio when conditions are met
                audioSource.Stop();
                hasPlayedNavigationStartSound = false;
            }
        }
        else
        {
            clickMarker.transform.SetParent(transform);
            clickMarker.SetActive(false);
            audioSource.loop = false;
            audioSource.Stop(); // Stop the audio when movement is toggled off
            hasPlayedNavigationStartSound = false; // Reset the flag
            myAnim.SetBool("isRunning", false);
        }
    }





    private void HandleReachedDestination(){
        // isReachedDestination = true;
        // clickMarker.transform.SetParent(transform);
        // clickMarker.SetActive(false);
        // if(isMoving){
        //     // Play NavEndAudio here
        //     AudioRef.PlayNavigationEndAudio(audioSource);  
        // }
        isMoving = false;
        // myAnim.SetBool("isRunning", false);
        TogglePlayerMovement(isMoving, 0);
        myLineRenderer.positionCount = 0;
        OnDisableWaypoints?.Invoke();
        // Debug.Log("DESTINATION REACHED!!! & Speed: "+myNavMeshAgent.speed);
        // Debug.Log("Stopped");
    }

    public void SetDestination(Vector3 target){
        
        // clickMarker.transform.SetParent(visualObjectsParent);
        // clickMarker.SetActive(true);
        isMoving = true;
        TogglePlayerMovement(isMoving, 0);
        myNavMeshAgent.SetDestination(target);
        myAnim.SetBool("isRunning", false);
        audioSource.Stop();
        audioSource.loop = false;
        // myNavMeshAgent.speed = 0;
        // isMoving = false;
        // myAnim.SetBool("isRunning", false);
        clickMarker.transform.position = myNavMeshAgent.destination;
        // Play NavStartAudio here
        
        // Debug.Log("Destination set: " + target);
    }

    public void FollowPathToDestination(){
        isMoving = true;
        TogglePlayerMovement(isMoving, 8);
        SetVCamFollowTarget(CameraRoot);
        // Play Footseps here
        // Debug.Log("FOOTSTEPS");
        // AudioRef.PlayRunningFootstepAudio(audioSource);
        
    }

    private void DrawPath(){
        // Draw the path that the player will take to reach its destination
        myLineRenderer.positionCount = myNavMeshAgent.path.corners.Length;
        myLineRenderer.SetPosition(0, transform.position);
        
        if(myNavMeshAgent.path.corners.Length < 2){
            return;
        }

        for (int i = 0; i < myNavMeshAgent.path.corners.Length; i++){
            Vector3 pointPosition = new Vector3(myNavMeshAgent.path.corners[i].x, myNavMeshAgent.path.corners[i].y, myNavMeshAgent.path.corners[i].z);
            myLineRenderer.SetPosition(i, pointPosition);
        }
    }

    private void OnMouseDown(){
        isSelected = true;
        characterSelectedMarker.SetActive(true);
    }

    public void SetVCamFollowTarget(Transform target){
        vCam.Follow = target;
        vCam.LookAt = target;
    }

    public float GetDistance(){
        return CalculateNavMeshDistance(transform.position, myNavMeshAgent.destination);
        // Debug.Log("DISTANCE: "+distance.ToString("F1")+" m");
    }

    private float CalculateNavMeshDistance(Vector3 start, Vector3 end){
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path)){
            float distance = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++) {
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return distance;
        }
        else{
            Debug.LogWarning("Path could not be calculated on the NavMesh.");
            return -1f;
        }
    }

    public void ShowDistance(TextMeshProUGUI Tmp){
        if (myNavMeshAgent.speed != 0)
        {
            Tmp.text = "DISTANCE: "+ GetDistance().ToString("F1") + " m";
            // Debug.Log("DISTANCE: "+ GetDistance().ToString("F1") + " m");
        }
        else
        {
            Tmp.text = "DISTANCE: "+ GetDistance().ToString() + " m";
            // Debug.Log("DISTANCE: "+ GetDistance().ToString() + " m");
        }
    }
    

}
