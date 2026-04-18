using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public static class LogicLoop
{
    public const int Rate = 16;
    public const double Interval = 1.0 / Rate;

    private static readonly List<Action> callbacks = new();
    private static double lastTime;
    private static double accumulatedTime;

    static LogicLoop()
    {
        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        ref PlayerLoopSystem updateSystem = ref playerLoop.GetSubSystem<Update>();
        updateSystem.AddCallback(UpdateCallback);
        PlayerLoop.SetPlayerLoop(playerLoop);
        lastTime = GetTime();
    }

    private static void UpdateCallback()
    {
        double timeNow = GetTime();
        double delta = timeNow - lastTime;
        lastTime = timeNow;
        Update(delta);
    }

    private static void Update(double delta)
    {
        accumulatedTime += delta;
        while (accumulatedTime >= Interval)
        {
            accumulatedTime -= Interval;
            InvokeCallbacks();
        }
    }

    private static void InvokeCallbacks()
    {
        for (int i = 0; i < callbacks.Count; i++)
        {
            callbacks[i]();
        }
    }

    private static double GetTime()
    {
        DateTime now = DateTime.UtcNow;
        return now.Ticks / (double)TimeSpan.TicksPerSecond;
    }

    public static void Register(Action callback)
    {
        callbacks.Add(callback);
    }
}
