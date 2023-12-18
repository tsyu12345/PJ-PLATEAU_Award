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
    public Sprite[] CoursesImages;
    public GameObject loadingUI;
    public Image MapImage;
    public Button SelectButton;
    public AudioClip cardSelectSE;
    public AudioClip selectBtnSE;
    private int nowSelectedCourseIdx;
    private string nowSelectCourseName;
    [SerializeField] private Slider _slider;
    private AudioSource _audioSource;
    void Start() {
        EventTrigger trigger = closeButton.GetComponent<EventTrigger> ();
        _audioSource = GetComponent<AudioSource>();

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

        SelectButton.onClick.AddListener(()=>{
            _audioSource.PlayOneShot(selectBtnSE);
            loadingUI.SetActive(true);
            StartCoroutine(LoadCourse(nowSelectCourseName));
        });
        AddClickCardEvent();

    }


    private void AddClickCardEvent() {
        for(int courseId = 0; courseId < Courses.Length; courseId++) {
            var course = Courses[courseId];
            EventTrigger courseTrigger = course.GetComponent<EventTrigger> ();
            EventTrigger.Entry courseEntry = new EventTrigger.Entry ();
            courseEntry.eventID = EventTriggerType.PointerClick;
            int currentCourseId = courseId;
            courseEntry.callback.AddListener((BaseEventData eventData)=>{
                Debug.Log("Click INdex" + currentCourseId);
                DeleteAllCoursesOutLine();
                //カード選択時のコールバック関数
                _audioSource.PlayOneShot(cardSelectSE);
                Outline outline = course.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.red;
                outline.effectDistance = new Vector2(3, -3);
                nowSelectedCourseIdx = currentCourseId;
                nowSelectCourseName = courseNames[currentCourseId];
                MapImage.sprite = CoursesImages[currentCourseId];
            });
            courseTrigger.triggers.Add(courseEntry);
        }
    }


    private void DeleteAllCoursesOutLine() {
        for(int courseId = 0; courseId < Courses.Length; courseId++) {
            if(Courses[courseId].GetComponent<Outline>() != null) {
                Destroy(Courses[courseId].GetComponent<Outline>());
            }
        }
    }


    private IEnumerator LoadCourse(string sceneName) {
        var courseLoad = SceneManager.LoadSceneAsync(sceneName);
        while(!courseLoad.isDone) {
            _slider.value = courseLoad.progress;
            yield return null;
        }
        loadingUI.SetActive(false);
        _audioSource.Stop();
    }

}
