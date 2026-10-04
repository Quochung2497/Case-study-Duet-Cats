using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DanielTran.Utility
{
  public static class GraphicComponentExtensions
  {
    /// <summary>
    /// Searches the GameObject for a component that supports alpha (CanvasGroup, Graphic, or TMP).
    /// Returns the first one found, or null if none is present.
    /// </summary>
    public static Component FindAlphaComponent(this GameObject go)
    {
      if (go == null)
        return null;
      if (go.TryGetComponent<CanvasGroup>(out var canvasGroup))
        return canvasGroup;
      if (go.TryGetComponent<Graphic>(out var graphic))
        return graphic;
      if (go.TryGetComponent<TextMeshProUGUI>(out var tmpText))
        return tmpText;
      if (go.TryGetComponent<Image>(out var image))
        return image;

      if (go.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        return spriteRenderer;

      return null;
    }

    /// <summary>
    /// Gets the alpha value from the component.
    /// </summary>
    public static float GetAlpha(this Component comp)
    {
      if (comp is CanvasGroup cg)
        return cg.alpha;
      else if (comp is Graphic graphic)
        return graphic.color.a;
      else if (comp is TextMeshProUGUI tmp)
        return tmp.color.a;
      else if (comp is SpriteRenderer sr)
        return sr.color.a;

      Debug.LogWarning(
        $"GetAlpha: Component type {comp.GetType().Name} does not support alpha. Returning 1f."
      );
      return 1f;
    }

    /// <summary>
    /// Sets the alpha value on the component.
    /// </summary>
    public static void SetAlpha(this Component comp, float alpha)
    {
      if (comp is CanvasGroup cg)
      {
        cg.alpha = alpha;
      }
      else if (comp is Graphic graphic)
      {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
      }
      else if (comp is TextMeshProUGUI tmp)
      {
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
      }
      else if (comp is SpriteRenderer sr)
      {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
      }
      else
      {
        Debug.LogWarning($"SetAlpha: Component type {comp.GetType().Name} does not support alpha.");
      }
    }

    /// <summary>
    /// Gets the color from the component.
    /// </summary>
    public static Color GetColor(this Component comp)
    {
      if (comp is Graphic graphic)
        return graphic.color;
      else if (comp is TextMeshProUGUI tmp)
        return tmp.color;
      else if (comp is SpriteRenderer sr)
        return sr.color;
      else
      {
        Debug.LogWarning(
          $"GetColor: Component type {comp.GetType().Name} does not support color. Returning white."
        );
        return Color.white;
      }
    }

    /// <summary>
    /// Sets the color on the component.
    /// </summary>
    public static void SetColor(this Component comp, Color newColor)
    {
      if (comp is Graphic graphic)
      {
        graphic.color = newColor;
      }
      else if (comp is TextMeshProUGUI tmp)
      {
        tmp.color = newColor;
      }
      else if (comp is SpriteRenderer sr)
      {
        sr.color = newColor;
      }
      else
      {
        Debug.LogWarning($"SetColor: Component type {comp.GetType().Name} does not support color.");
      }
    }
  }
}
