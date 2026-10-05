using System;
using System.Collections;
using UnityEngine;

namespace Utility.UI
{
  /// <summary>
  /// Handles fade animations for UI components by updating their alpha channels.
  /// </summary>
  public class FadeGraphicComponent : MonoBehaviour
  {
    private const float TimeStep = 0.01f;

    #region Serialized Fields

    /// <summary>
    /// Array of <see cref="AlphaObject"/> used to configure fade animations.
    /// </summary>
    [Header("Fade Settings")]
    [SerializeField]
    private AlphaObject[] alphaObjects = Array.Empty<AlphaObject>();

    #endregion

    #region Private Fields

    /// <summary>
    /// Reference to the currently running fade coroutine.
    /// </summary>
    private Coroutine _fadeCoroutine;

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the fade sequence for all alpha objects.
    /// Initializes fade routines for each component if available.
    /// </summary>
    public void StartFadeSequence()
    {
      if (alphaObjects.Length == 0)
        return;

      StopFadeSequence();

      foreach (AlphaObject alphaObject in alphaObjects)
      {
        if (alphaObject.targetComp)
        {
          StartCoroutine(FadeLoop(alphaObject.targetComp, alphaObject.cycleTime));
        }
        else
        {
          Debug.LogWarning(
            $"{alphaObject.gameObject.name} does not have a CanvasGroup, Graphic, TMP, or SpriteRenderer to fade."
          );
        }
      }
    }

    /// <summary>
    /// Stops all fade sequences and resets the alpha values of all components.
    /// </summary>
    public void StopFadeSequence()
    {
      StopAllCoroutines();
      ResetAlpha();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes the alpha values for all alpha objects.
    /// </summary>
    private void Awake() => InitializeAlphaObjects();

    private void InitializeAlphaObjects()
    {
      for (int i = 0; i < alphaObjects.Length; i++)
      {
        if (alphaObjects[i].targetComp == null)
        {
          Component targetComp = alphaObjects[i].gameObject.FindAlphaComponent();
          if (targetComp != null)
          {
            alphaObjects[i].targetComp = targetComp;
            alphaObjects[i].targetComp.SetAlpha(1);
          }
          else
          {
            Debug.LogWarning(
              $"{alphaObjects[i].gameObject.name} does not have a CanvasGroup, Graphic, TMP, or SpriteRenderer to fade."
            );
          }
        }
        else
        {
          alphaObjects[i].targetComp.SetAlpha(1);
        }
      }
    }

    /// <summary>
    /// Continuously performs fade cycles for a given UI component.
    /// </summary>
    /// <param name="target">The component whose alpha channel is to fade in and out.</param>
    /// <param name="cycleTime">Total cycle time for one complete fade in/out cycle.</param>
    /// <returns>An IEnumerator for coroutine iteration.</returns>
    private IEnumerator FadeLoop(Component target, float cycleTime)
    {
      var halfTime = cycleTime / 2f;

      // Loop forever.
      while (true)
      {
        yield return FadeCycle(target, halfTime);
      }
    }

    /// <summary>
    /// Performs a single fade cycle by fading out then fading in the alpha value of a component.
    /// </summary>
    /// <param name="target">The component to fade.</param>
    /// <param name="halfTime">Time taken for each half (fade out or fade in) of the cycle.</param>
    /// <returns>An IEnumerator for coroutine iteration.</returns>
    private IEnumerator FadeCycle(Component target, float halfTime)
    {
      var startAlpha = target.GetAlpha();

      for (var t = 0f; t < halfTime; t += TimeStep)
      {
        var newAlpha = Mathf.Lerp(startAlpha, 0f, t / halfTime);
        target.SetAlpha(newAlpha);
        yield return new WaitForSecondsRealtime(TimeStep);
      }
      target.SetAlpha(0f);

      for (var t = 0f; t < halfTime; t += TimeStep)
      {
        var newAlpha = Mathf.Lerp(0f, startAlpha, t / halfTime);
        target.SetAlpha(newAlpha);
        yield return new WaitForSecondsRealtime(TimeStep);
      }
      target.SetAlpha(startAlpha);
    }

    /// <summary>
    /// Resets the alpha value of all target components to fully opaque.
    /// </summary>
    private void ResetAlpha()
    {
      foreach (AlphaObject alphaObject in alphaObjects)
      {
        if (alphaObject.targetComp)
        {
          alphaObject.targetComp.SetAlpha(1f);
        }
      }
    }

    /// <summary>
    /// Ensures all alpha values are reset when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
      ResetAlpha();
    }

    #endregion

    /// <summary>
    /// Serializable structure that stores configuration for each fade target.
    /// </summary>
    [Serializable]
    private struct AlphaObject
    {
      /// <summary>
      /// The game object that contains the component to fade.
      /// </summary>
      public GameObject gameObject;

      /// <summary>
      /// Total cycle time for fading in and out.
      /// </summary>
      public float cycleTime;

      /// <summary>
      /// The component associated with the game object that supports alpha changes.
      /// </summary>
      public Component targetComp;
    }
  }
}
