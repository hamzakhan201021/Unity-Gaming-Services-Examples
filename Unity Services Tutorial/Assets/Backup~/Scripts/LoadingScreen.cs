using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class LoadingScreen : MonoBehaviour
{

    [Header("Loading Screen")]
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private GameObject _buttonObject;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private GameObject _content;

    public static LoadingScreen Instance;

    private Action _onCancel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            HideInternal();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _buttonObject.GetComponent<Button>().onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        _onCancel?.Invoke();
    }

    public static void Show(bool setText = false, string text = "", bool enableBT = false, string textBT = "", Action onCancel = null)
    {
        Instance.ShowInternal(setText, text, enableBT, textBT, onCancel);
    }

    public static void Hide()
    {
        Instance.HideInternal();
    }

    public void ShowInternal(bool setText, string text, bool useBT = false, string textBT = "", Action onCancel = null)
    {
        _content.SetActive(true);
        _buttonObject.SetActive(useBT);

        if (setText) _loadingText.text = text;
        _buttonText.text = textBT;

        _onCancel = onCancel;

        EventSystem.current.SetSelectedGameObject(null);

        if (useBT)
        {
            EventSystem.current.SetSelectedGameObject(_buttonObject);
        }
    }

    public void HideInternal()
    {
        _content.SetActive(false);
    }
}