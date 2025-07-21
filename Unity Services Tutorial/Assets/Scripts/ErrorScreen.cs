using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _okBT.onClick.AddListener(OnClickOKBT);
    }

    private void OnDestroy()
    {
        _okBT.onClick.RemoveListener(OnClickOKBT);
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
        _errorText.text = error;
        _okBTText.text = ok;

        _content.SetActive(true);

        _okBT.Select();
    }

    private void HideInternal()
    {
        _content.SetActive(false);
    }
}
