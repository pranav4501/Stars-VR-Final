

using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using TMPro;

public class Stars : MonoBehaviour
{
    public TextAsset csvFile; // Assign your CSV file in the Unity Inspector
    public GameObject starPrefab; // Assign your sphere prefab in the Unity Inspector
    public GameObject player;
    public TextMeshProUGUI timeElapsedText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI informationText;
    
    public TextAsset[] constFile;
    public AudioClip[] audioClips;
    public TextAsset ExoplanetFile;
    public GameObject cam;
    private Dictionary<int, Vector3> starPositionsByHIP = new Dictionary<int, Vector3>();
    public Dictionary<string, StarData> starList = new Dictionary<string, StarData>();
    public Dictionary<string, Vector3> constellationPointsDict = new Dictionary<string, Vector3>();
    public Dictionary<string, int> exoplanetData = new Dictionary<string, int>();

    public bool isExoplanetColorScheme = false;
    private List<GameObject> stars = new List<GameObject>();    
    private List<Renderer> starRenderers = new List<Renderer>();
    private GameObject instance;
    private Renderer rend;
    public float scaleRatioOfStars = 0.05f;
    public float distanceToRender = 50f;
    public GameObject ConstellationLines;
    public int activeConstellation = 0;
    public int direction = 0;
    float yearsPassed = 0;
    private float distance;
    private float prevDistance = 0;
    private float distanceError = 0.01f;
    private Vector3 origin = new Vector3(0, 0, 0);
    private bool isInformationActive = false;
    // Start is called before the first frame update
    void Start()
    {   
        InvokeRepeating("FaceCamera", 0.0f, 1f);
        // Check if the prefab and file are assigned
        if (starPrefab == null || csvFile == null)
        {
            Debug.LogError("Prefab or CSV File is not assigned!");
            return;
        }
        prevDistance = Vector3.Distance(gameObject.transform.position, new Vector3(0, 0, 0));
        addStars();
        CreateConstellations();
        AddExoplanetsData();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance();
        if(direction != 0){
            MoveStar();
            MoveConstellations();
        }
    }

    private void addStars()
    {
        string[] lines = csvFile.text.Split('\n');
        

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            StarData star = new StarData();
            try
                {
                star.absoluteMag = float.Parse(values[6]);
                star.relativeMag = float.Parse(values[5]);
                star.dist = float.Parse(values[1]);
                star.position = new Vector3(float.Parse(values[2]), float.Parse(values[4]), float.Parse(values[3]));
                star.originalPosition = star.position;
                star.vx = float.Parse(values[7]) * 1.02269E-6f;
                star.vy = float.Parse(values[8]) * 1.02269E-6f;
                star.vz = float.Parse(values[9]) * 1.02269E-6f;
                star.spectralType = values[10].Trim();
                star.instance = Instantiate(starPrefab, star.position, Quaternion.LookRotation(star.position)) as GameObject;
                starList.Add(values[0].Substring(0, values[0].Length - 2), star);
                star.instance.transform.localScale *= scaleRatioOfStars * star.relativeMag;
                star.instance.GetComponent<Renderer>().material.color = GetSpectColor(star.spectralType);

                float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

                star.instance.GetComponent<MeshRenderer>().enabled = distanceToCamera <= distanceToRender;
                }
                
                catch (System.IndexOutOfRangeException)
                {
                    continue;
                }
        }
        Debug.Log("Stars added"+ stars.Count.ToString());
        Debug.Log(starList.Count.ToString());
    }

    private void CreateConstellations(){
        for (int j=0; j<constFile.Length; j++)
        {
            string[] lines = constFile[j].text.Split('\n');

            GameObject constellationParent = new GameObject("Constellations");
            constellationParent.transform.parent = ConstellationLines.transform;

            foreach (string line in lines)
            {
                string[] parts = line.Trim().Split(' ');
                if (parts.Length < 2) continue;

                string constellationName = parts[0];
                int starCount = int.Parse(parts[1]);

                for (int i = 3; i < parts.Length - 1; i += 2)
                {
                    string starId1 = parts[i];
                    string starId2 = parts[i + 1];
                    starId1 = starId1.Trim();
                    starId2 = starId2.Trim();


                    Vector3 position1 = GetStarPosition(starId1);
                    Vector3 position2 = GetStarPosition(starId2);

                    constellationPointsDict[starId1] = position1;
                    constellationPointsDict[starId2] = position2;

                    // Create a line between the stars
                    GameObject lineObject = new GameObject("Line");
                    lineObject.name = starId1 + "-" + starId2;
                    lineObject.transform.parent = constellationParent.transform;
                    LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, position1);
                    lineRenderer.SetPosition(1, position2);
                    lineRenderer.startWidth = 0.05f;
                    lineRenderer.endWidth = 0.05f;
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.startColor = Color.white;
                    lineRenderer.endColor = Color.white;
                }   
            }
        

        constellationParent.SetActive(j==activeConstellation);
        }
    }

    public void ToggleConstellation(int constellationIndex)
    {
        for (int i = 0; i < ConstellationLines.transform.childCount; i++)
        {
            ConstellationLines.transform.GetChild(i).gameObject.SetActive(i == constellationIndex);
        }
    }

    Vector3 GetStarPosition(string starId)
    {
        Vector3 starPosition = Vector3.zero;
        if (starList.ContainsKey(starId))
            starPosition = starList[starId].position;
        else
            Debug.Log("Star not found: " + starId);
        return starPosition;
    }
    

    public void UpdateStarColors(bool isExoplanetColorScheme)
    {   
        isExoplanetColorScheme = isExoplanetColorScheme;
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            if(isExoplanetColorScheme)
            {
                star.instance.GetComponent<Renderer>().material.color = GetExoColor(keyValuePair.Key);
            }
            else
            {
                star.instance.GetComponent<Renderer>().material.color = GetSpectColor(star.spectralType);
        }
    }
    }

    void MoveStar()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;

            float distanceToCamera = Vector3.Distance(star.position, player.transform.position);

            Vector3 velocity = new Vector3(star.vx, star.vy, star.vz) * direction;

            star.position += velocity * Time.deltaTime * 1000;

            constellationPointsDict[keyValuePair.Key] = star.position;

            if (distanceToCamera <= 25f)
            {
                star.instance.transform.position = star.position;
            }
        }
        yearsPassed += 1000 * Time.deltaTime * direction;
        timeElapsedText.text = "Time Elapsed:\n" + (int) yearsPassed + " years";
    }

    void MoveConstellations()
    {
        foreach (Transform constellation in ConstellationLines.transform)
        {
            if (constellation.gameObject.activeInHierarchy)
            {
                foreach (Transform constellationLine in constellation.gameObject.transform)
                {
                    string lineName = constellationLine.gameObject.name;
                    var stars = lineName.Split('-');
                    var starHip1 = stars[0];
                    var starHip2 = stars[1];
                    Debug.Log(starHip1);
                    

                    Vector3 star1Pos = constellationPointsDict[starHip1];
                    Vector3 star2Pos = constellationPointsDict[starHip2];

                    constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(0, star1Pos);
                    constellationLine.gameObject.GetComponent<LineRenderer>().SetPosition(1, star2Pos);
                }
                
            }
        }
    }

    public void SetDirection(int dir){
        direction = dir;
    }   

    public void ResetTime(){
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            star.position = star.originalPosition;
            star.instance.transform.position = star.position;
            constellationPointsDict[keyValuePair.Key] = star.position;
        }
        yearsPassed = 0;
        timeElapsedText.text = "Time Elapsed:\n" + (int) yearsPassed + " years";
        MoveConstellations();
    }   

    public void ResetCameraOrientation(){
        cam.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void UpdateDistance(){
        distance = Vector3.Distance(player.transform.position, origin);
        if (Mathf.Abs(distance - prevDistance)> distanceError){
            distanceText.text = "Distance from Sol:\n" + distance.ToString("F2") + " Parsecs";
            prevDistance = distance;
        }
    }

    void FaceCamera()
    {
        foreach (var keyValuePair in starList)
        {
            var star = keyValuePair.Value;
            float distanceToCamera = Vector3.Distance(star.instance.transform.position, cam.transform.position);

            if(distanceToCamera <= distanceToRender)
                star.instance.transform.LookAt(cam.transform);
        }
    }
    private void AddExoplanetsData()
    {
        string[] lines = ExoplanetFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            string starId = values[0];
            int exoplanetCount = int.Parse(values[1]);
            exoplanetData[starId] = exoplanetCount;
            
        }
        Debug.Log("Exoplanets added");
    }   
    public Color GetSpectColor(string spectralType)
    {
        spectralType = spectralType.ToUpper();

        if (spectralType.StartsWith("O"))
            return new Color(0.6f, 0.6f, 1.0f); 

        if (spectralType.StartsWith("B"))
            return new Color(0.8f, 0.8f, 1.0f); 

        if (spectralType.StartsWith("A"))
            return new Color(1.0f, 1.0f, 1.0f); 

        if (spectralType.StartsWith("F"))
            return new Color(1.0f, 1.0f, 0.8f); 

        if (spectralType.StartsWith("G"))
            return new Color(1.0f, 1.0f, 0.6f); 

        if (spectralType.StartsWith("K"))
            return new Color(1.0f, 0.8f, 0.6f); 

        if (spectralType.StartsWith("M"))
            return new Color(1.0f, 0.6f, 0.6f); 

        return Color.white;
    }

    public Color GetExoColor(string starID){
        float exoplanetCount = -1;
        if (exoplanetData.ContainsKey(starID))
        {
            exoplanetCount = exoplanetData[starID];
        }
        Color[] colors = {Color.white, Color.green, Color.yellow, Color.red, Color.blue};

        if (exoplanetCount < 0)
        {
            return colors[0];
        }
        else if (exoplanetCount < 1)
        {
            return colors[1];
        }
        else if (exoplanetCount < 2)
        {
            return colors[2];
        }
        else if (exoplanetCount < 3)
        {
            return colors[3];
        }
        else
        {
            return colors[4];
        }
    }

    public void UpdateAudio(int audioIndex)
    {
        AudioSource audio = player.GetComponent<AudioSource>();
        audio.clip = audioClips[audioIndex];
        audio.Play();
    }

    public void ToggleInformation(){
        if(isInformationActive){
            player.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else{
            player.transform.rotation = Quaternion.Euler(0, 15, 0);
        }
        informationText.gameObject.SetActive(!informationText.gameObject.activeSelf);
        isInformationActive = !isInformationActive;

    }

}