using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;


public class SlideShowUI : MonoBehaviour
{
    public event EventHandler OnNextSlideInput;
    public event EventHandler OnSkipSlideInput;
    public event EventHandler OnTriggerSlideChange;


    [Tooltip("This sets how frequently we will respond to button/key presses. By default this is set to 1 second, meaning after you press a button this class will not respond to another button/key press until one second has elapsed. This stops the event firing way too many times.")]
    [SerializeField, Min(0.5f)] private float _inputDelay = 1.0f;

    [Tooltip("This is the Image UI component that is used to display the current slide image in non-fullscreen modes.")]
    [SerializeField] private Image _slideImage;

    [Tooltip("This is the Image UI component that is used to display the background behind the slide image.")]
    [SerializeField] private Image _slideImageBackground;

    [Tooltip("This is the Image UI component that is used to fade the screen in and out.")]
    [SerializeField] private Image _screenFader;


    [SerializeField] private Transform _ButtonsContainer;
    [SerializeField] private Button _nextSlideButton;
    [SerializeField] private Button _skipSlideShowButton;
    [SerializeField] private Button _continueButton;


    IDisposable _inputListener;

    private float _inputTimer = 0f;

    private bool _mouseIsOverAnyButton;


    private void OnEnable()
    {
        if (_inputListener == null)            
            _inputListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
    }

    private void OnDisable()
    {
        _inputListener?.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetButtons();
    }

    // Update is called once per frame
    void Update()
    {
        _inputTimer += Time.deltaTime;
    }

    /// <summary>
    /// This function is called to initiate a transition from one slide to the next. The transition is determined by the TransitionType property of the current slide.
    /// </summary>
    public void StartSlideTransition(SlideShowStateInfo stateInfo)
    {
        if (IsTransitioning)
        {
            Debug.LogError("SlideShowUI.StartSlideTransition() was called while a slide transition is already in progress!");
            return;   
        }


        if (stateInfo.CurrentSlide.TransitionType != Slide.TransitionTypes.None)
        {
            StartCoroutine(DoSlideTransition(stateInfo));
        }
        else
        {
            // Trigger the player to change to the next slide.
            OnTriggerSlideChange?.Invoke(this, EventArgs.Empty);

            _screenFader.gameObject.SetActive(false);
        }
    }

    private IEnumerator DoSlideTransition(SlideShowStateInfo stateInfo)
    {
        float duration = stateInfo.GetCurrentSlideTransitionDuration();

        IsTransitioning = true;


        switch (stateInfo.CurrentSlide.TransitionType)
        {
            case Slide.TransitionTypes.SimpleFadeOutThenFadeIn:
                yield return StartCoroutine(DoSlideTransition_SimpleFadeOutThenFadeIn(duration));
                break;

            case Slide.TransitionTypes.CustomFadeOutThenFadeIn:
                yield return StartCoroutine(DoSlideTransition_CustomFadeOutThenFadeIn(stateInfo));
                break;


            default:
                yield return StartCoroutine(DoSlideTransition_SimpleFadeOutThenFadeIn(duration));
                break;

        } // end switch


        IsTransitioning = false;
    }

    public IEnumerator DoSlideTransition_SimpleFadeOutThenFadeIn(float duration)
    {
        float fadeTime = duration / 2f;

        // Fade out the current slide.
        yield return StartCoroutine(DoFade(Slide.FadeType.FadeOut, fadeTime));

        // Trigger the player to change to the next slide.
        OnTriggerSlideChange?.Invoke(this, EventArgs.Empty);

        // Fade in the next slide.
        yield return StartCoroutine(DoFade(Slide.FadeType.FadeIn, fadeTime));
    }

    public IEnumerator DoSlideTransition_CustomFadeOutThenFadeIn(SlideShowStateInfo stateInfo)
    {
        float fadeOutTime = stateInfo.GetCurrentSlideFadeOutTime();
        float fadeInTime = stateInfo.GetNextSlideFadeInTime();


        // Fade out the current slide.
        yield return StartCoroutine(DoFade(Slide.FadeType.FadeOut, fadeOutTime));

        // Trigger the player to change to the next slide.
        OnTriggerSlideChange?.Invoke(this, EventArgs.Empty);

        // Fade in the next slide.
        yield return StartCoroutine(DoFade(Slide.FadeType.FadeIn, fadeInTime));
    }

    public IEnumerator DoFade(Slide.FadeType fadeType, float fadeDuration)
    {
        IsTransitioning = true;

        _screenFader.gameObject.SetActive(true);


        float fadeStartTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime <= fadeDuration)
        {
            // Calculate how far through the fade we are.
            elapsedTime = (Time.time - fadeStartTime);
            float percentComplete = elapsedTime / fadeDuration;

            AdjustScreenFaderAlpha(fadeType, percentComplete);

            // Wait one frame.
            yield return null;

        } // end while


        AdjustScreenFaderAlpha(fadeType, 1.0f);


        if (fadeType == Slide.FadeType.FadeIn)
            _screenFader.gameObject.SetActive(false);


        IsTransitioning = false;
    }

    private void AdjustScreenFaderAlpha(Slide.FadeType fadeType, float slideShowPercentComplete)
    {
        // Calculate the alpha amount.
        int alpha = Mathf.Clamp(Mathf.RoundToInt(255f * slideShowPercentComplete),
                                0, 255);

        //Debug.Log($"{(Time.time - fadeStartTime)} / {fadeDuration}    {percentComplete}    {alpha}");

        // Set the alpha value of the screen fader. If we are fading out, set it to (255 - alpha) rather than just alpha.
        Color32 color = _screenFader.color;
        color.a = fadeType == Slide.FadeType.FadeOut ? (byte)alpha : (byte)(255 - alpha);
        _screenFader.color = color;
    }

    public void ResetSlideDisplay(Color fadeColor)
    {
        _screenFader.color = fadeColor;
        _screenFader.gameObject.SetActive(true);

        ResetButtons();

        // Enable the slide show UI canvas.
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables the slide show UI. It is automatically re-enabled when a cut scene starts playing.
    /// </summary>
    public void DisableSlideShowDisplay()
    {
        _slideImage.gameObject.SetActive(false);
        _screenFader.gameObject.SetActive(false);
        EnableSlideDisplay(false);

        // Disable the canvas.
        gameObject.SetActive(false);
    }

    public void EnableSlideDisplay(bool state)
    {
        _slideImageBackground.gameObject.SetActive(state);
        _slideImage.gameObject.SetActive(state);
        _ButtonsContainer.gameObject.SetActive(state);        
    }

    public void UpdateSlideDisplay(Slide slide)
    {
        _slideImageBackground.color = slide.BackgroundColor;

        DisplaySlideImage(slide);
    }

    private void DisplaySlideImage(Slide slide)
    {
        bool fullScreen = slide.IsFullscreenImage;
        bool preserveAspectRatio = slide.PreserveAspectRatio;


        // Configure the image UI component and set the image to it.
        _slideImage.preserveAspect = false;
        
        Vector2 imageSize = Vector2.zero;

        // If the slide is using stretch full screen mode, then get the image size.
        if (slide.ImageDisplayMode == Slide.ImageDisplayModes.StretchFullScreen)
        {
            RectTransform parentRectTrans = _slideImage.transform.parent.GetComponent<RectTransform>();
            imageSize.x = parentRectTrans.rect.size.x;
            imageSize.y = parentRectTrans.rect.size.y;
        }

        // If the slide is using custom size mode, then get the custom image size.
        if (slide.IsCustomSizeImage)
        {
            imageSize.x = slide.CustomImageSize.x;
            imageSize.y = slide.CustomImageSize.y;
        }


        // Set the screen alignment of the image.
        _slideImageBackground.GetComponent<HorizontalLayoutGroup>().childAlignment = slide.ImageScreenAlignment;


        // Set the image UI element to the image's native size if appropriate.
        if (slide.ImageDisplayMode == Slide.ImageDisplayModes.NativeSize)
        {
            _slideImage.SetNativeSize();
        }
        else
        {
            _slideImage.GetComponent<RectTransform>().sizeDelta = imageSize;
        }


        // Set the preserveAspect property on the UI image component if native size mode is enabled.
        _slideImage.preserveAspect = preserveAspectRatio;
        _slideImage.sprite = slide.Image;

    }

    public void ShowContinueButton()
    {
        _nextSlideButton.gameObject.SetActive(false);
        _skipSlideShowButton.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(true);
    }

    public void ResetButtons()
    {
        _nextSlideButton.gameObject.SetActive(true);
        _skipSlideShowButton.gameObject.SetActive(true);
        _continueButton.gameObject.SetActive(false);
    }

    private void OnAnyButtonPressed(InputControl control)
    {
        if (_inputTimer >= _inputDelay)
        {
            // If the user clicked on a button, then ignore this input since the button is already handling it.
            // Otherwise, run the code in this if statement to handle it.
            if (!(control.device.displayName == "Mouse" && _mouseIsOverAnyButton))                 
            {
                _inputTimer = 0f;

                //Debug.Log($"InputTimer: {_inputTimer}    InputDelay: {_inputDelay}    KeyPressed: {control.displayName}");
                OnNextSlideInput?.Invoke(this, EventArgs.Empty);
            }
        }

    }

    public void OnNextSlideButtonClicked()
    {
        OnNextSlideInput?.Invoke(this, EventArgs.Empty);
    }

    public void OnSkipButtonClicked()
    {
        OnSkipSlideInput?.Invoke(this, EventArgs.Empty);
    }
    
    public void OnMouseEnteredButtonArea()
    {
        _mouseIsOverAnyButton = true;
    }

    public void OnMouseExitedButtonArea()
    {
        _mouseIsOverAnyButton = false;
    }



    public bool IsTransitioning { get; private set; }

}
