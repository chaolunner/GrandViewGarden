using System.Collections.Generic;
using UnityEngine;

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
        RealtimeData.Add(timePointData);
    }

    public void Forecast(TimePointData timePointData, int limit)
    {
        ForecastData.Clear();
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
        ForecastData.Add(timePointData);
    }
}
