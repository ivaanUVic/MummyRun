using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class TrackGenerator : MonoBehaviour
{

    public List<GameObject> tracks;



    [HideInInspector]
    public List<GameObject> trackPool = new List<GameObject>();

    //private List<GameObject> currentSelectedTracks = new List<GameObject>();

    private bool tutorialTrackPlaced;
    private float trackCurrentZ;
    private float trackRenderDistance = 300;

    public GameObject fixTrack;
    private GameObject player;

    [HideInInspector]
    public GameObject tracksRoot, trackObjectsRoot;
    [HideInInspector]
    public Vector3 rootPos = new Vector3(0, 0, -5000);



    private void Awake()
    {

        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.SetInt("tutorial", 1);

        player = GameObject.Find("Player");
        tracksRoot = GameObject.Find("TracksRoot");


        // Clonnig Regular Tracks
        foreach (GameObject track in tracks)
        {

            if (track != null)
            {
                GameObject cloneTrack = (GameObject)GameObject.Instantiate(track, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                cloneTrack.transform.parent = tracksRoot.transform;
                cloneTrack.transform.position = rootPos;
                cloneTrack.gameObject.SetActive(false);

                disposeEditorTrackItems(cloneTrack);

                trackPool.Add(cloneTrack);
            }
           
        }

  
        Reset();

    }

    public void Reset()
    {
    
        trackCurrentZ = 0;
        disposeAllTracks();

    }

    private void disposeEditorTrackItems(GameObject track)
    {
        Transform currentTrackObjectsRoot = track.transform.Find("trackObjects");
        if (currentTrackObjectsRoot != null)
        {
            foreach (Transform trackItem in currentTrackObjectsRoot.transform)
            {
                foreach (Transform child in trackItem) Destroy(child.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        StartCoroutine(GenerateTracks());
    }

    public IEnumerator GenerateTracks()
    {

        float forwardTargetDistance = player.transform.position.z + trackRenderDistance;


        if (trackCurrentZ < forwardTargetDistance)
        {
            StartCoroutine(clearTracksAndObjectsBehind());
        }

        while (trackCurrentZ < forwardTargetDistance)
        {

            // Getting an empty track
            GameObject currentTrack = null;
         
         
            currentTrack = getRandomTrack();


            currentTrack.transform.position = (Vector3)(Vector3.forward * trackCurrentZ);
            trackCurrentZ += currentTrack.GetComponent<Track>().zSize;

        }

        yield break;

    }

    private GameObject getRandomTrack()
    {
    
        float playerZ = player.transform.position.z;

        if (playerZ < 0) playerZ = 0;

        List<GameObject> possibleTracks = new List<GameObject>();


        for (int i = 0; i < trackPool.Count; i++)
        {
            Track item = trackPool[i].GetComponent<Track>();

            possibleTracks.Add(item.gameObject);
            // Adding Regular Tracks
            if (item.positioned == false && item.CanSpawnAtEveryWhere == false && item.CanSpawnAtMaximum == false)
            {
                if ((item.spawnStartPos <= playerZ) && (playerZ < item.spawnEndPos))
                {
                    possibleTracks.Add(item.gameObject);
                }
            }

        }



        if (possibleTracks.Count > 0)
        {
            GameObject createdFixTrack = (GameObject)GameObject.Instantiate(possibleTracks[Random.Range(0, possibleTracks.Count)], new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
            createdFixTrack.transform.parent = tracksRoot.transform;
            createdFixTrack.gameObject.SetActive(true);
            return createdFixTrack;

        }
        else
        {
            GameObject createdFixTrack = (GameObject)GameObject.Instantiate(fixTrack, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
            createdFixTrack.transform.parent = tracksRoot.transform;

            return createdFixTrack;
        }

    }

    IEnumerator clearTracksAndObjectsBehind()
    {
    
       // disposeTrackPoolBehind(trackPool);

        yield return null;
    }

    private void disposeTrackPoolBehind(List<GameObject> tPool)
    {

        List<int> injectedTracksForDeleting = new List<int>();

       


        for (int i = 0; i < tPool.Count; i++)
            {
                Track item = tPool[i].GetComponent<Track>();

                if (item != null)
                {

                    if (player.transform.position.z > item.gameObject.transform.position.z + item.zSize + 50.0f)
                    {
                        // Disposing Tracks
                        disposeTrackAndTrackObjects(item, true);

                        injectedTracksForDeleting.Add(i);
                       // Destroy(item.gameObject);

                    }

                }

            }
       
        // Destroying Injected Tracks
        for (int i = 0; i < injectedTracksForDeleting.Count; i++)
        {
            try
            {
                Destroy(trackPool[injectedTracksForDeleting[i]]);
                trackPool.RemoveAt(injectedTracksForDeleting[i]);
            }
            catch (System.Exception)
            {
                //print(e.Message);
            }
        }

    }

    private void disposeAllTracks()
    {

        // Disposing All Tracks
        for (int i = 0; i < trackPool.Count; i++)
        {
            Track item = trackPool[i].GetComponent<Track>();
            if (item != null)
            {
                disposeTrackAndTrackObjects(item, true);
            }
        }

    }

    public void disposeTrackAndTrackObjects(Track track, bool disposeTrackGeometry)
    {
    
        if (disposeTrackGeometry == true)
        {

            track.positioned = false;
            track.gameObject.transform.position = rootPos;
            track.gameObject.SetActive(false);
        }

    }

    public List<GameObject> getActiveTracks()
    {
        List<GameObject> result = new List<GameObject>();

        foreach (Transform track in tracksRoot.transform)
        {
            if (track.gameObject.activeInHierarchy == true)
            {
                result.Add(track.gameObject);
            }
        }

        return result;
    }




}
