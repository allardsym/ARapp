using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Video;

public class main : MonoBehaviour
{
    [SerializeField]
    private GameObject[] arObjectsToPlace;

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.01f, 0.01f, 0.01f);

    [SerializeField]
    private Camera arCamera;

    private ARTrackedImageManager m_TrackedImageManager;

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();

    private Vector2 touchPosition = default;

    private Touch pinch1;
    private Touch pinch2;
    private Vector2 pinchPosition1 = default;
    private Vector2 pinchPosition2 = default;
    private float distance = default;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        foreach (GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, new Vector3(90, 0, 0), Quaternion.identity);
            newARObject.name = arObject.name;
            newARObject.GetComponent<VideoPlayer>().frame = 1;
            arObjects.Add(arObject.name, newARObject);
        }
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            float newDistance;
            pinchPosition1 = Input.GetTouch(0).position;
            pinchPosition2 = Input.GetTouch(1).position;
            newDistance = Vector2.Distance(pinchPosition1, pinchPosition2);
            if (distance == default)
                distance = newDistance;
            touchPosition = pinchPosition1;

            Ray ray = arCamera.ScreenPointToRay(pinchPosition1);
            RaycastHit hitObject;
            if (Physics.Raycast(ray, out hitObject))
            {
                if (distance > newDistance)
                {
                    scaleFactor = new Vector3((scaleFactor.x * (float)0.95), (scaleFactor.y * (float)0.95), (scaleFactor.z * (float)0.95));
                    hitObject.transform.localScale = scaleFactor;
                }
                else if (distance < newDistance)
                {
                    scaleFactor = new Vector3((scaleFactor.x * (float)1.05), (scaleFactor.y * (float)1.05), (scaleFactor.z * (float)1.05));
                    hitObject.transform.localScale = scaleFactor;
                }
            }

            distance = newDistance;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    VideoPlayer videoObject = hitObject.transform.GetComponent<VideoPlayer>();
                    if (videoObject != null)
                    {
                        if (videoObject.isPaused)
                        {
                            videoObject.Play();
                        }
                        else
                        {
                            videoObject.Pause();
                        }
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            arObjects[trackedImage.name].SetActive(false);
            arObjects[trackedImage.name].GetComponent<VideoPlayer>().Stop();
        }
    }

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        try
        {
            GameObject goARObject = arObjects[trackedImage.referenceImage.name];
            VideoPlayer goVid = goARObject.GetComponent<VideoPlayer>();
            if (goVid.frame < 5)
            {
                goVid.Play();
                goARObject.transform.localScale = scaleFactor;
            }
            goARObject.SetActive(true);
            goARObject.transform.rotation = trackedImage.transform.rotation;
            goARObject.transform.position = trackedImage.transform.position;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
