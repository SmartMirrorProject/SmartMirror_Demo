using System;

namespace SmartMirror
{
    //Class to hold information for the weeks forecast.
    //Each instance of this class is to represent a single
    //days weather forecast.  Simple POCO with each days weather info.
    public class DaysWeatherForecast
    {
        private double _highTemp;
        private double _lowTemp;
        private string _weather;
        private string _weatherIcon;
        private string _date;        

        public DaysWeatherForecast(DateTime date)
        {
            _date = date.ToString("M/d");
            SetDefaults();
        }

        public double GetHighTemperature()
        {
            return _highTemp;
        }

        public void SetHighTemperature(double high)
        {
            _highTemp = high;
        }

        public double GetLowTemperature()
        {
            return _lowTemp;
        }

        public void SetLowTemperature(double low)
        {
            _lowTemp = low;
        }

        public string GetWeather()
        {
            return _weather;
        }

        public void SetWeather(string weather)
        {
            _weather = weather;
        }

        public string GetWeatherIcon()
        {
            return _weatherIcon;
        }

        public void SetWeatherIcon(string weatherIcon)
        {
            _weatherIcon = weatherIcon;
        }

        public string GetDate()
        {
            return _date;
        }

        public void SetDefaults()
        {
            _lowTemp = 72.0;
            _highTemp = 81.0;
            _weather = "Sunny";
        }
    }
}