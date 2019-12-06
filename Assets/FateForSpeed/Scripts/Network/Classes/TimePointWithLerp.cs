using System.Collections.Generic;
using UnityEngine;
using Common;

public class TimePointWithLerp
{
    public bool IsPlaying;
    public int TickId;
    public float From;
    public float To;
    public List<TimePointData> RealtimeData = new List<TimePointData>();
    public List<TimePointData> ForecastData = new List<TimePointData>();
    public List<IUserInputResult[]> RollbackData = new List<IUserInputResult[]>();

    private int forecastCount;
    private Fix64 realtimeTotalTime;
    private Fix64 forecastTotalTime;

    public float TotalTime
    {
        get
        {
            return (float)(realtimeTotalTime + forecastTotalTime);
        }
    }

    public List<TimePointData> TimePoints
    {
        get
        {
            var timePoints = new List<TimePointData>();
            timePoints.AddRange(RealtimeData);
            timePoints.AddRange(ForecastData);
            return timePoints;
        }
    }

    public void Begin(float deltaTime, float fixedDeltaTime)
    {
        if (fixedDeltaTime > 0)
        {
            From = To;
            To = Mathf.Clamp01((To * fixedDeltaTime + deltaTime) / fixedDeltaTime);
        }
        else
        {
            From = 0;
            To = 1;
        }
    }

    public void End()
    {
        if (To >= 1)
        {
            From = 0;
            To = 0;
            realtimeTotalTime = Fix64.Zero;
            RealtimeData.Clear();
            IsPlaying = false;
        }
        else
        {
            IsPlaying = true;
        }
    }

    public void AddRealtimeData(Fix64 deltaTime, TimePointData timePointData)
    {
        realtimeTotalTime += deltaTime;
        RealtimeData.Add(timePointData);
    }
    public void Forecast(Fix64 deltaTime, TimePointData timePointData, int limit)
    {
        ForecastData.Clear();
        forecastTotalTime = Fix64.Zero;
        if (RealtimeData.Count > 0)
        {
            forecastCount = Mathf.Clamp(forecastCount - RealtimeData.Count + 1, 0, limit);
            for (int i = 0; i < forecastCount; i++)
            {
                AddForecastData(deltaTime, timePointData, i + 1);
            }
        }
        else if (forecastCount < limit)
        {
            forecastCount++;
            AddForecastData(deltaTime, timePointData, forecastCount);
        }
    }

    private void AddForecastData(Fix64 deltaTime, TimePointData timePointData, int count)
    {
        timePointData.ForecastCount = count;
        forecastTotalTime += deltaTime;
        ForecastData.Add(timePointData);
    }

    public void Rollback()
    {
        for (int i = RollbackData.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < RollbackData[i].Length; j++)
            {
                RollbackData[i][j].Rollback();
            }
        }
        RollbackData.Clear();
    }
}
