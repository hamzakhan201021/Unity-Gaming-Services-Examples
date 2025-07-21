using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ErrorScreen : MonoBehaviour
{

    [Header("Error Screen")]
    [SerializeField] private Button _okBT;
    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private TMP_Text _okBTText;
    [SerializeField] private GameObject _content;

    private static ErrorScreen _instance;

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

    private void Start()
    {
        _okBT.onClick.AddListener(OnClickOKBT);
    }

    private void OnClickOKBT()
    {
        HideInternal();
    }

    public static void Show(string error, string ok)
    {
        _instance?.ShowInternal(error, ok);
    }

    public void ShowInternal(string error, string ok)
    {
        _content.SetActive(true);

        _errorText.text = error;
        _okBTText.text = ok;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_okBT.gameObject);
    }

    public void HideInternal()
    {
        _content.SetActive(false);
    }
}