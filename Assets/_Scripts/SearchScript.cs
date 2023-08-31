using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SearchScript : MonoBehaviour
{
    [SerializeField] private GameObject ContentHolder;
    [SerializeField] private GameObject SearchObjectPrefab;
    [SerializeField] private GameObject SearchBar;
    [SerializeField] private GameObject WayPointsHolder;
    [SerializeField] private Button GoButton;
    [SerializeField] private PlayerController PlayerRef;
    [SerializeField] private TextMeshProUGUI DistanceText;
    [SerializeField] private GameObject SearchCanvas;
    [SerializeField] private GameObject DirectionCanvas;
    [SerializeField] private GameObject IntroCanvas;
    private int totalElements;
    private int totalWaypoints;
    // private int waypointIndex = 0;

    [System.Serializable]
    private struct WayPointElement
    {
        public GameObject element;
        public GameObject waypoint;
    }

    [SerializeField] private WayPointElement[] wayPointElements;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    [System.Obsolete]
    void Update()
    {
        PlayerRef.ShowDistance(DistanceText);
        if (Input.GetKeyDown(KeyCode.F))
        {
            SearchCanvas.SetActive(!SearchCanvas.activeSelf);
            if (!DirectionCanvas.active)
            {
                DirectionCanvas.SetActive(true);
                IntroCanvas.SetActive(false);
            }
        }
    }

    private void OnEnable() {
        PlayerController.OnDisableWaypoints += DisableWaypoints;
    }

    private void OnDisable() {
        PlayerController.OnDisableWaypoints -= DisableWaypoints;
    }

    private void Initialize(){
        SearchCanvas.SetActive(false);
        DirectionCanvas.SetActive(false);
        IntroCanvas.SetActive(true);
        GoButton.interactable = false;
        totalWaypoints = WayPointsHolder.transform.childCount;
        wayPointElements = new WayPointElement[totalWaypoints];
        // LocationPointer.SetActive(false);
        for (int i = 0; i < totalWaypoints; i++)
        {
            int index = i;
            wayPointElements[i].waypoint = WayPointsHolder.transform.GetChild(i).gameObject;
            Transform wpTransform = wayPointElements[i].waypoint.transform;
            GameObject waypointRootInstance = Instantiate(new GameObject(), wpTransform.position, Quaternion.Euler(new Vector3(45, 0, 0)));
            waypointRootInstance.transform.SetParent(wpTransform);
            wayPointElements[i].waypoint.SetActive(false);
            GameObject elementPrefabInstance = Instantiate(SearchObjectPrefab, ContentHolder.transform);
            elementPrefabInstance.name = "Element" + i;
            elementPrefabInstance.GetComponent<Button>().onClick.AddListener(() => LocateObject(index));
            wayPointElements[i].element = elementPrefabInstance;
            wayPointElements[i].element.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = wayPointElements[i].waypoint.name;
            // RotateWaypointToFaceCamera(i);
        }
        
    }

    public void Search(){
        string searchText = SearchBar.GetComponent<TMP_InputField>().text;
        int searchTxtLength = searchText.Length;

        foreach (WayPointElement wayPointElement in wayPointElements){
            GameObject element = wayPointElement.element;

            if (element != null){
                TextMeshProUGUI textMesh = element.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

                if (textMesh != null && textMesh.text.Length >= searchTxtLength && searchText.ToLower() == textMesh.text.Substring(0, searchTxtLength).ToLower())
                {
                    element.SetActive(true);
                }
                else
                {
                    element.SetActive(false);
                }
            }
        }

        if(string.IsNullOrEmpty(searchText)){
            searchText = "Search...";
        }
    }

    public void LocateObject(int index){
        ResetWaypoints(index);
        GoButton.interactable = true;
        Transform location =  wayPointElements[index].waypoint.transform;
        PlayerRef.SetDestination(location.position);
        location.position = new Vector3(location.position.x, 1f, location.position.z);
        PlayerRef.SetVCamFollowTarget(location.GetChild(0).transform);
        RotateWaypointToFaceCamera(index);
        // Debug.Log("Waypoint Located! " + location);
        // LocationPointer.transform.SetParent(WayPointsHolder.transform);
        // if(LocationPointer.transform.parent == null){
            
        // }
        
        // LocationPointer.transform.position = new Vector3(location.x, location.y + 1f, location.z);

        wayPointElements[index].waypoint.SetActive(true);
        // LocationPointer.SetActive(true);

    }

    private void ResetWaypoints(int index){
        for (int i = 0; i < wayPointElements.Length; i++)
        {
            if(i == index){
                wayPointElements[i].waypoint.SetActive(true);
            }
            else{
                wayPointElements[i].waypoint.SetActive(false);
            }
        }
    }

    private void DisableWaypoints(){
        foreach (Transform waypoint in WayPointsHolder.transform)
        {
            waypoint.gameObject.SetActive(false);
        }
    }
    
    public void FollowPath(){
        PlayerRef.FollowPathToDestination();
        SearchCanvas.SetActive(false);
    }

    private void RotateWaypointToFaceCamera(int waypointIndex)
    {
        if (waypointIndex >= 0 && waypointIndex < wayPointElements.Length)
        {
            WayPointElement waypointElement = wayPointElements[waypointIndex];
            Vector3 directionToCamera = PlayerRef.vCam.transform.position - waypointElement.waypoint.transform.position;
            directionToCamera.y = 0; // If you want to ignore the vertical component

            if (directionToCamera != Vector3.zero)
            {
                Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera, Vector3.up);
                // Quaternion adjustedRotation = Quaternion.Euler(45f, rotationToCamera.eulerAngles.y, rotationToCamera.eulerAngles.z);
                waypointElement.waypoint.transform.rotation = rotationToCamera;
            }
        }
        else
        {
            Debug.LogWarning("Invalid waypoint index: " + waypointIndex);
        }
    }
}
