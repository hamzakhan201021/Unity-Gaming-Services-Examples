using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LoadingScreen : MonoBehaviour
{

    [Header("Loading Screen")]
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private GameObject _buttonObject;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private GameObject _content;

    private static LoadingScreen _instance;

    private Action _onBT;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            HideInternal();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_buttonObject.TryGetComponent(out Button bt))
        {
            bt.onClick.AddListener(OnClickButton);
        }
    }

    private void OnDestroy()
    {
        if (_buttonObject.TryGetComponent(out Button bt))
        {
            bt.onClick.RemoveListener(OnClickButton);
        }
    }

    private void OnClickButton()
    {
        _onBT?.Invoke();
    }

    public static void Show(bool setText = false, string text = "", bool enabledBT = false, string textBT = "", Action onBT = null)
    {
        _instance?.ShowInternal(setText, text, enabledBT, textBT, onBT);
    }

    public static void Hide()
    {
        _instance?.HideInternal();
    }

    public void ShowInternal(bool setText = false, string text = "", bool enabledBT = false, string textBT = "", Action onBT = null)
    {
        if (setText)
        {
            _loadingText.text = text;
        }
        _buttonText.text = textBT;

        _onBT = onBT;

        EventSystem.current.SetSelectedGameObject(null);

        if (enabledBT)
        {
            EventSystem.current.SetSelectedGameObject(_buttonObject);
        }

        _buttonObject.SetActive(enabledBT);
        _content.SetActive(true);
    }

    public void HideInternal()
    {
        _content.SetActive(false);
    }
}
