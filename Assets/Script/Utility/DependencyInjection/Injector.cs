using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

namespace Utility.DependencyInjection
{
  /// <summary>
  /// Attribute to mark fields or methods for dependency injection.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
  public sealed class InjectAttribute : Attribute { }

  /// <summary>
  /// Attribute to mark methods that provide dependencies.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public sealed class ProvideAttribute : Attribute { }

  /// <summary>
  /// Injector class responsible for managing dependency injection.
  /// </summary>
  [DefaultExecutionOrder(-1000)]
  [ExcludeFromCoverage]
  [ExcludeFromCodeCoverage]
  public class Injector : Singleton<Injector>
  {
    private const BindingFlags KBindingFlags =
      BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {
      base.Awake();

      // Find all classes that implement IDependencyProvider
      var providers = FindMonoBehaviours().OfType<IDependencyProvider>();
      foreach (var provider in providers)
      {
        RegisterProvider(provider);
      }

      // Find all injectable objects and inject them
      var injectables = FindMonoBehaviours().Where(IsInjectable);
      foreach (var injectable in injectables)
      {
        Inject(injectable);
      }
    }

    /// <summary>
    /// Resolves an instance of the specified type from the registry.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The resolved instance or null if not found.</returns>
    private object Resolve(Type type)
    {
      _registry.TryGetValue(type, out var value);
      return value;
    }

    /// <summary>
    /// Injects dependencies into the specified object.
    /// </summary>
    /// <param name="value">The object to inject dependencies into.</param>
    private void Inject(object value)
    {
      var type = value.GetType();
      var injectableFields = type.GetFields(KBindingFlags)
        .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

      foreach (var field in injectableFields)
      {
        var fieldType = field.FieldType;
        var resolvedInstance = Resolve(fieldType);

        if (resolvedInstance is null)
        {
          throw new Exception($"Failed to resolve {fieldType.Name}");
        }

        field.SetValue(value, resolvedInstance);
#if UNITY_EDITOR
        Debug.Log($"Injected {fieldType.Name} to {type.Name}");
#endif
      }

      var injectableMethods = type.GetMethods(KBindingFlags)
        .Where(m => Attribute.IsDefined(m, typeof(InjectAttribute)));

      foreach (var method in injectableMethods)
      {
        var parameters = method.GetParameters();
        var resolvedParameters = parameters.Select(parameter => parameter.ParameterType).ToArray();

        var resolvedInstances = resolvedParameters.Select(Resolve).ToArray();

        if (resolvedInstances.Any(resolvedInstance => resolvedInstance is null))
        {
          throw new Exception($"Failed to resolve parameters for {method.Name}");
        }

        method.Invoke(value, resolvedInstances);
#if UNITY_EDITOR
        Debug.Log($"Injected parameters to {method.Name} in {type.Name}");
#endif
      }

      var injectableProperties = type.GetProperties(KBindingFlags)
        .Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)));

      foreach (var property in injectableProperties)
      {
        var propertyType = property.PropertyType;
        var resolvedInstance = Resolve(propertyType);

        if (resolvedInstance is null)
        {
          throw new Exception($"Failed to resolve {propertyType.Name}");
        }

        property.SetValue(value, resolvedInstance);
#if UNITY_EDITOR
        Debug.Log($"Injected {propertyType.Name} to {type.Name}");
#endif
      }
    }

    /// <summary>
    /// Determines if a MonoBehaviour object has injectable members.
    /// </summary>
    /// <param name="obj">The MonoBehaviour object to check.</param>
    /// <returns>True if the object has injectable members, otherwise false.</returns>
    static bool IsInjectable(MonoBehaviour obj)
    {
      var member = obj.GetType().GetMembers(KBindingFlags);
      return member.Any(m => Attribute.IsDefined(m, typeof(InjectAttribute)));
    }

    /// <summary>
    /// Registers a dependency provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    private void RegisterProvider(IDependencyProvider provider)
    {
      var methods = provider.GetType().GetMethods(KBindingFlags);

      foreach (var method in methods)
      {
        if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
          continue;

        var returnType = method.ReturnType;
        var providerInstance = method.Invoke(provider, null);
        if (providerInstance is not null)
        {
          _registry[returnType] = providerInstance;
#if UNITY_EDITOR
          Debug.Log($"Registered provider {returnType.Name}");
#endif
        }
        else
        {
          throw new Exception($"Provider method {method.Name} returned null");
        }
      }
    }

    /// <summary>
    /// Finds all MonoBehaviour objects in the scene.
    /// </summary>
    /// <returns>An enumerable of MonoBehaviour objects.</returns>
    static IEnumerable<MonoBehaviour> FindMonoBehaviours()
    {
      return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
    }
  }
}
