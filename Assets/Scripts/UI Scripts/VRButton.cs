using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRButton : MonoBehaviour
{

    public Sprite _loadingSprite;
    public Sprite _onSprite;
    public Sprite _errorSprite;
    public Sprite _offSprite;


    private UnityEngine.UI.Button _vrButton;
    private UnityEngine.UI.Image _vrButtonImage;

    public enum State
    {
        Off,
        Loading,
        On,
        Error
    }

    // Start is called before the first frame update
    void Start()
    {
        _vrButton = GetComponent<UnityEngine.UI.Button>();
        _vrButtonImage = _vrButton.GetComponent<UnityEngine.UI.Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.Off:
                _vrButtonImage.sprite = _offSprite;
                break;
            case State.Loading:
                _vrButtonImage.sprite = _loadingSprite;
                break;
            case State.On:
                _vrButtonImage.sprite = _onSprite;
                break;
            case State.Error:
                _vrButtonImage.sprite = _errorSprite;
                break;
            default:
                break;
        }
    }
}
