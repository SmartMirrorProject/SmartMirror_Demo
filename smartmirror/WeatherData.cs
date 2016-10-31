using System;

namespace SmartMirror
{
    //A number of classes used for parsing JSON info from OpenWeatherMaps APIs.
    //Primary class for Today's weather
    public class WeatherData
    {
        public TempInfo main { get; set; }
        public WeatherInfo[] weather { get; set; }
        public long dt { get; set; }
        public SysInfo sys { get; set; }
        public string name { get; set; }
    }

    //Primary class for Tomorrows Weather
    public class TomorrowsWeatherData
    {
        public CityInfo city { get; set; }
        public int cnt { get; set; }
        public TomorrowsWeatherList[] list { get; set; }
    }

    //Primary class for the weeks weather forecast
    public class WeeksWeatherData
    {
        public CityInfo city { get; set; }
        public int cnt { get; set; }
        public WeeksForecastList[] list { get; set; }
    }

    //------------------------------------------------------------------------------------------------------------
    //-------------------------------------Subclasses to match JSON layout----------------------------------------
    //------------------------------------------------------------------------------------------------------------
    public class TomorrowsWeatherList
    {
        public long dt { get; set; }
        public TempInfo main { get; set; }
        public WeatherInfo[] weather { get; set; }
        public string dt_text { get; set; }
    }

    public class WeeksForecastList
    {
        public long dt { get; set; }
        public WeeksTempInfo temp { get; set; }
        public WeatherInfo[] weather { get; set; }
    }

    public class DaysForecast
    {
        public long dt { get; set; }
        public TempInfo temp { get; set; }
        public WeatherInfo weather { get; set; }
    }

    public class WeatherInfo
    {
        public string main { get; set; }
        public string icon { get; set; }
    }

    public class TempInfo
    {
        public double temp { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }

    public class WeeksTempInfo
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class SysInfo
    {
        public string country { get; set; }
    }

    public class CityInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
    }
}