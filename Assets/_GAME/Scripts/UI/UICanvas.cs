using UnityEngine;

public class UICanvas:MonoBehaviour
{
    [SerializeField] bool IsDestroyOnClose = false;

    private void Awake()
    {
        RectTransform rect = GetComponent<RectTransform>();

        float ratio = (float)Screen.width / (float)Screen.height;

        //Xu li tai tho
        if(ratio > 2.1f)
        {
            Vector2 leftBottom = rect.offsetMin;

            Vector2 rightTop = rect.offsetMax;

            leftBottom.y = 0f;
            rightTop.y = -100f;

            rect.offsetMin = leftBottom;

            rect.offsetMax = rightTop;
        }
    }


    /// <summary>
    /// Call beforce canvas is active
    /// </summary>
    public virtual void SetUp()
    {
        
    }

    /// <summary>
    /// Call after canvas is active
    /// </summary>
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Open(UICanvas uICanvas)
    {
        gameObject.SetActive(true);
    }


    /// <summary>
    /// Close canvas after time(s)
    /// </summary>
    /// <param name="time"></param>
    public virtual void Close(float time)
    {
        Invoke(nameof(CloseDirectly), time);
    }

    /// <summary>
    /// Close canvas directly
    /// </summary>
    public virtual void CloseDirectly()
    {
        if (IsDestroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
        
    }
}