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

    public void AddRealtimeData(TimePointData timePointData)
    {
        realtimeTotalTime += timePointData.RealTime;
        RealtimeData.Add(timePointData);
    }

    public void Forecast(TimePointData timePointData, int limit)
    {
        ForecastData.Clear();
        forecastTotalTime = Fix64.Zero;
        if (RealtimeData.Count > 0)
        {
            forecastCount = 0;
        }
        else if (forecastCount < limit)
        {
            forecastCount++;
            AddForecastData(timePointData, forecastCount);
        }
    }

    private void AddForecastData(TimePointData timePointData, int count)
    {
        timePointData.ForecastCount = count;
        forecastTotalTime += timePointData.RealTime;
        ForecastData.Add(timePointData);
    }
}
