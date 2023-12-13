using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CourseSelect : MonoBehaviour {
    
    public Image closeButton;
    public GameObject[] Courses;
    public string[] courseNames;
    public GameObject loadingUI;
    void Start() {
        EventTrigger trigger = closeButton.GetComponent<EventTrigger> ();
        EventTrigger.Entry entry = new EventTrigger.Entry ();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((BaseEventData eventData)=>{
            this.gameObject.SetActive(false);
        });
        trigger.triggers.Add(entry);

        loadingUI.SetActive(false);

        if(courseNames.Length != Courses.Length) {
            Debug.LogError("[CourseSelect]courseNames.Length != Courses.Length");
        }

        for(int courseId = 0; courseId < Courses.Length; courseId++) {
            var course = Courses[courseId];
            EventTrigger courseTrigger = course.GetComponent<EventTrigger> ();
            EventTrigger.Entry courseEntry = new EventTrigger.Entry ();
            courseEntry.eventID = EventTriggerType.PointerClick;
            courseEntry.callback.AddListener((BaseEventData eventData)=>{
                var courseScene = courseNames[courseId];
                StartCoroutine(LoadCourse(courseId));
            });
            courseTrigger.triggers.Add(courseEntry);
        }
    }


    private void OnSelectCourse() {
        Debug.Log("Select Course");
    }

    private IEnumerator LoadCourse(int courseId) {
        loadingUI.SetActive(true);

        var course = courseNames[courseId];
        var courseLoad = SceneManager.LoadSceneAsync(course);
        while(!courseLoad.isDone) {
            yield return null;
        }
        loadingUI.SetActive(false);
    }

}
