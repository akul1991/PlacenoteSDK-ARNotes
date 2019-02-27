using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

// Classes to hold Notes information.

[System.Serializable]
public class NoteInfo
{
    public float px;
    public float py;
    public float pz;
    public float qx;
    public float qy;
    public float qz;
    public float qw;
    public string note;
}

[System.Serializable]
public class NotesList
{
    // List of all notes stored in the current Place.
    public NoteInfo[] notes;
}


// Main class for managing notes.
public class NotesManager : MonoBehaviour {

    public List<NoteInfo> mNotesInfoList = new List<NoteInfo>();
    public List<GameObject> mNotesObjList = new List<GameObject>();

    // Prefab for the Note
    public GameObject mNotePrefab;

    private GameObject mCurrNote;
    private NoteInfo mCurrNoteInfo;
    
    // Use this for initialization
    void Start () {
		
	}

    // The HitTest to add a Note
    bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
    {
        List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
    
        if (hitResults.Count > 0) 
        {
            foreach (var hitResult in hitResults) 
            {
                Debug.Log("Got hit!");

                Vector3 position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                Quaternion rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);

                // Transform to Placenote frame of reference (planes are detected in ARKit frame of reference)
                Matrix4x4 worldTransform = Matrix4x4.TRS(position, rotation, Vector3.one);
                Matrix4x4? placenoteTransform = LibPlacenote.Instance.ProcessPose(worldTransform);

                Vector3 hitPosition = PNUtility.MatrixOps.GetPosition(placenoteTransform.Value);
                Quaternion hitRotation = PNUtility.MatrixOps.GetRotation(placenoteTransform.Value);

                // Create note
                InstantiateNote(hitPosition);

                return true;
            }
        }

        return false;
    }

    // Update checks for hit test, and if we're not editing a note.
    void Update ()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    Debug.Log("Not touching a UI button, moving on.");

                    // Test if you are hitting an existing marker
                    RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);

                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log("Selected an existing note.");
                        mCurrNote = hit.transform.gameObject;

                        int index = mCurrNote.GetComponent<NoteID>().index;
                        mCurrNoteInfo = mNotesInfoList[index];

                        // Show the Edit and Delete buttons
                        mCurrNote.transform.Find("EditButton").gameObject.SetActive(true);
                        mCurrNote.transform.Find("DeleteButton").gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Creating new note.");
                        // Add new note.
                        var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                        ARPoint point = new ARPoint
                        {
                            x = screenPosition.x,
                            y = screenPosition.y
                        };

                        ARHitTestResultType[] resultTypes =
                        {
                        ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                        };

                        foreach (ARHitTestResultType resultType in resultTypes)
                        {
                            if (HitTestWithResultType(point, resultType))
                            {
                                Debug.Log("Found a hit test result");
                                return;
                            }
                        }
                    }
                }
            }
        }
    }


    public void InstantiateNote(Vector3 notePosition)
    {
        Debug.Log("Creating new note");
        // Instantiate new note prefab and set transform.
        GameObject note = Instantiate(mNotePrefab);
        note.transform.position = notePosition;
        note.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        // Turn the note to point at the camera
        Vector3 targetPosition = new Vector3(Camera.main.transform.position.x,
                                             Camera.main.transform.position.y,
                                             Camera.main.transform.position.z);
        note.transform.LookAt(targetPosition);
        note.transform.Rotate(0f, -180f, 0f);
        note.SetActive(true);

        // Set currently selected note
        mCurrNote = note;
        
        mCurrNoteInfo = new NoteInfo
        {
            px = note.transform.position.x,
            py = note.transform.position.y,
            pz = note.transform.position.z,
            qx = note.transform.rotation.x,
            qy = note.transform.rotation.y,
            qz = note.transform.rotation.z,
            qw = note.transform.rotation.w,
            note = ""
        };

        EditCurrNote();
    }

    private void EditCurrNote()
    {
        Debug.Log("Editing selected note.");

        // Activate input field
        InputField input = mCurrNote.GetComponentInChildren<InputField>();
        input.interactable = true;
        input.ActivateInputField();

        input.onEndEdit.AddListener(delegate { OnNoteClosed(input); });
    }

    private void OnNoteClosed(InputField input)
    {
        Debug.Log("No longer editing current note!");

        // Save input text, and set input field as non interactable
        mCurrNoteInfo.note = input.text;
        input.DeactivateInputField();
        input.interactable = false;

        mCurrNote.transform.Find("DeleteButton").gameObject.SetActive(false);
        mCurrNote.transform.Find("EditButton").gameObject.SetActive(false);

        int index = mCurrNote.GetComponent<NoteID>().index;
        if (index < 0)
        {
            // New note being saved!
            mCurrNote.GetComponent<NoteID>().index = mNotesObjList.Count;
            Debug.Log("Saving note with ID " + mNotesObjList.Count);
            mNotesInfoList.Add(mCurrNoteInfo);
            mNotesObjList.Add(mCurrNote);
        }
        else
        {
            // Need to re-save the object.
            mNotesObjList[index] = mCurrNote;
            mNotesInfoList[index] = mCurrNoteInfo;
        }
    }

    public GameObject NoteFromInfo(NoteInfo info)
    {
        GameObject note = Instantiate(mNotePrefab);
        note.transform.position = new Vector3(info.px, info.py, info.pz);
        note.transform.rotation = new Quaternion(info.qx, info.qy, info.qz, info.qw);
        note.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        note.SetActive(true);

        note.GetComponentInChildren<InputField>().text = info.note;

        return note;
    }

    public void OnEditButtonClick()
    {
        Debug.Log("Edit button clicked!");
        EditCurrNote();
    }

    public void OnDeleteButtonClick()
    {
        Debug.Log("Delete button clicked!");
        DeleteCurrentNote();
    }

    private void DeleteCurrentNote()
    {
        Debug.Log("Deleting current note!");
        int index = mCurrNote.GetComponent<NoteID>().index;

        if (index >= 0)
        {
            Debug.Log("Index is " + index);
            mNotesObjList.RemoveAt(index);
            mNotesInfoList.RemoveAt(index);

            // Refresh Note indices
            for (int i = 0; i < mNotesObjList.Count; ++i)
            {
                mNotesObjList[i].GetComponent<NoteID>().index = i;
            }
        }

        Destroy(mCurrNote);
    }

    public void ClearNotes()
    {
        foreach (var obj in mNotesObjList)
        {
            Destroy(obj);
        }

        mNotesObjList.Clear();
        mNotesInfoList.Clear();
    }

    public JObject Notes2JSON()
    {
        NotesList notesList = new NotesList
        {
            notes = new NoteInfo[mNotesInfoList.Count]
        };

        for (int i = 0; i < mNotesInfoList.Count; ++i)
        {
            notesList.notes[i] = mNotesInfoList[i];
        }

        return JObject.FromObject(notesList);
    }

    public void LoadNotesJSON(JToken mapMetadata)
    {
        ClearNotes();

        if (mapMetadata is JObject && mapMetadata["notesList"] is JObject)
        {
            NotesList notesList = mapMetadata["notesList"].ToObject<NotesList>();

            if (notesList.notes == null)
            {
                Debug.Log("No notes created!");
                return;
            }
            
            foreach (var noteInfo in notesList.notes)
            {
                GameObject note = NoteFromInfo(noteInfo);
                note.GetComponent<NoteID>().index = mNotesObjList.Count;

                mNotesObjList.Add(note);
                mNotesInfoList.Add(noteInfo);
            }
        }
    }
}
