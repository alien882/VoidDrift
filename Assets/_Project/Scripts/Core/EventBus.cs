using System;
using System.Collections.Generic;

/// <summary>
/// Sistema de comunicación central entre sistemas del juego.
/// Uso: EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied) en Start/OnEnable
///      EventBus.Publish(new PlayerDiedEvent { score = 100 }) cuando ocurre algo
///      EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied) en OnDestroy/OnDisable
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> subscribers = new();

    // Suscribirse a un evento
    public static void Subscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (!subscribers.ContainsKey(type))
            subscribers[type] = new List<Delegate>();

        subscribers[type].Add(handler);
    }

    // Desuscribirse de un evento
    public static void Unsubscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (subscribers.ContainsKey(type))
            subscribers[type].Remove(handler);
    }

    // Publicar un evento — notifica a todos los suscriptores
    public static void Publish<T>(T eventData)
    {
        Type type = typeof(T);
        if (!subscribers.ContainsKey(type)) return;

        // Iteramos sobre una copia para evitar errores si alguien se desuscribe durante el publish
        foreach (Delegate handler in subscribers[type].ToArray())
            (handler as Action<T>)?.Invoke(eventData);
    }

    // Limpia todos los suscriptores — llamar al reiniciar la escena
    public static void Clear()
    {
        subscribers.Clear();
    }
}