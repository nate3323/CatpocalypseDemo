using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "New Slide Show", menuName = "Cut Scenes/Slide Show")]
public class SlideShow : ScriptableObject
{
    [Header("Fade Settings")]

    [Tooltip("The time (in seconds) that it takes a slide to fade in. Individual slides can override this value.")]
    [SerializeField, Min(0f)]
    private float _defaultFadeInTime = 1.5f;
    [Tooltip("The time (in seconds) that it takes a slide to fade out. Individual slides can override this value.")]
    [SerializeField, Min(0f)]
    private float _defaultFadeOutTime = 1.5f;

    [Tooltip("This is the duration of slide transitions in seconds. Individual slides can override this value. This value is ignored by custom FadeOut/In transitions, which use the FadeIn/Out time values instead.")]
    [SerializeField, Min(0f)]
    private float _defaultTransitionDuration = 2f;

    [Tooltip("The color to use when this slide fades out.")]
    [SerializeField]
    private Color _fadeColor = Color.black;

    [Tooltip("This option determines whether the slide show will begin with a fade in effect and end with a fade out effect. If enabled, the duration is controlled by the DefaultFadeInTime and DefaultFadeOutTime settings.")]
    [SerializeField]
    private bool _fadeSlideShowInAndOut = true;


    [Header("General Settings")]
    
    [Tooltip("Determines when the slide show can advance to the next slide.")]
    [SerializeField]
    private AdvancementModes _advancementMode = AdvancementModes.WhenAnyKeyPressed;

    [Tooltip("To have the slide show player load another scene when this slide show ends, just enter the name of that scene in this field. You can leave this field blank if you don't want another scene to load when the slide show finishes playing.")]
    [SerializeField]
    private string _sceneToLoadOnSlideShowComplete;

    [Tooltip("This is the list of images in this slide show.")]
    [SerializeField]
    private List<Slide> _slides;



    public enum AdvancementModes
    {
        WhenAnyKeyPressed,
        WhenAnyKeyPressedOrSlideDurationExpires,
        WhenSlideDurationExpires,
    }



    public void OnEnable()
    {
        CalculateDuration();
    }


    public Slide GetSlide(int slideIndex)
    { 
        return _slides[slideIndex]; 
    }

    /// <summary>
    /// Gets the total display time of the specified slide, including fade in and fade out.
    /// </summary>
    /// <param name="slideIndex">The index of the slide.</param>
    /// <returns>The total display time of the slide.</returns>
    public float GetSlideDisplayTime(int slideIndex)
    {
        Slide slide = _slides[slideIndex];

        if (slide.TransitionType == Slide.TransitionTypes.CustomFadeOutThenFadeIn)
        {
            // Get the fade in and out times. If the slide has them set to negative values, then use the default value of this slide show.
            float fadeInTime = slide.FadeInTimeOverride >= 0f ? slide.FadeInTimeOverride : _defaultFadeInTime;
            float fadeOutTime = slide.FadeOutTimeOverride >= 0f ? slide.FadeOutTimeOverride : _defaultFadeOutTime;

            return slide.DisplayTime + fadeInTime + fadeOutTime;
        }
        else
        {
            return slide.DisplayTime + GetSlideTransitionDuration(slideIndex);
        }
    }

    /// <summary>
    /// Gets the fade in time of the specified slide.
    /// </summary>
    /// <param name="slideIndex">The index of the slide.</param>
    /// <returns>If the slide overrides the fade in time, it returns that value. Otherwise the default value from this slide show is returned.</returns>
    public float GetSlideFadeInTime(int slideIndex)
    {
        Slide slide = _slides[slideIndex];

        if (slide.TransitionType == Slide.TransitionTypes.None)
            return 0;


        if (slide.TransitionType == Slide.TransitionTypes.CustomFadeOutThenFadeIn)
        {
            return slide.FadeInTimeOverride >= 0f ? slide.FadeInTimeOverride : _defaultFadeInTime;
        }
        else
        {
            return GetSlideTransitionDuration(slideIndex) / 2f;
        }
    }

    /// <summary>
    /// Gets the fade out time of the specified slide.
    /// </summary>
    /// <param name="slideIndex">The index of the slide.</param>
    /// <returns>If the slide overrides the fade out time, it returns that value. Otherwise the default value from this slide show is returned.</returns>
    public float GetSlideFadeOutTime(int slideIndex)
    {
        Slide slide = _slides[slideIndex];

        if (slide.TransitionType == Slide.TransitionTypes.None)
            return 0;


        if (slide.TransitionType == Slide.TransitionTypes.CustomFadeOutThenFadeIn)
        {
            return slide.FadeOutTimeOverride >= 0f ? slide.FadeOutTimeOverride : _defaultFadeOutTime;
        }
        else
        {
            return GetSlideTransitionDuration(slideIndex) / 2f;
        }
    }

    /// <summary>
    /// Gets the transition duration of the specified slide.
    /// </summary>
    /// <param name="slideIndex">The index of the slide.</param>
    /// <returns>If the slide overrides the transition duration, it returns that value. Otherwise the default value from this slide show is returned.</returns>
    public float GetSlideTransitionDuration(int slideIndex)
    {
        Slide slide = _slides[slideIndex];

        if (slide.TransitionType == Slide.TransitionTypes.None)
            return 0;


        float prevSlideFadeOutTime = slideIndex > 0 ? GetSlideFadeOutTime(slideIndex - 1) : _defaultFadeOutTime;

        if (slide.TransitionType == Slide.TransitionTypes.CustomFadeOutThenFadeIn)
        {
            return prevSlideFadeOutTime + GetSlideFadeInTime(slideIndex);
        }
        else
        { 
            return slide.TransitionDurationOverride >= 0f ? slide.TransitionDurationOverride : _defaultTransitionDuration;
        }
    }

    /// <summary>
    /// Calculates the length of this slide show in seconds.
    /// </summary>
    private void CalculateDuration()
    {
        float duration = 0f;


        if (_slides == null || _slides.Count == 0)
            return;


        // Add up the display time for all slides in this slide show.
        foreach (Slide slide in _slides)
        {
            duration += slide.DisplayTime;
        }


        // Take into account the fade in/out times for all slides.
        duration += (_defaultFadeInTime + _defaultFadeOutTime) * _slides.Count;

        // Update the Duration property.
        SlideShowLength = duration;
    }



    /// <summary>
    /// Returns the length of this slide show in seconds.
    /// </summary>
    public float SlideShowLength { get; private set; }


    public float DefaultFadeInTime { get { return _defaultFadeInTime; } }
    public float DefaultFadeOutTime { get { return _defaultFadeOutTime; } }

    public Color FadeColor { get { return _fadeColor; } }

    public bool FadeSlideShowInAndOut { get { return _fadeSlideShowInAndOut; } }

    public int SlideCount { get { return _slides.Count; } }
    public string SceneToLoadOnSlideShowComplete { get { return _sceneToLoadOnSlideShowComplete; } }
    public AdvancementModes AdvancementMode { get { return _advancementMode; } }
    public float TransitionDuration { get { return _defaultTransitionDuration; } }
}
