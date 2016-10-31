using System;

namespace SmartMirror
{
    //Class to hold information for the todays current weather.
    //ESimple POCO with the days weather info.
    public class TodaysWeather
    {
        private double _temperature;
        private double _highTemp;
        private double _lowTemp;
        private string _weather;
        private string _weatherIcon;
        private string _city;
        private string _state;
        private string _country;
        private DateTime _lastUpdated;

        public TodaysWeather()
        {
            SetDefaults();
        }

        public double GetTemerature()
        {
            return _temperature;
        }

        public void SetTemperature(double temp)
        {
            _temperature = temp;
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

        public string GetCity()
        {
            return _city;
        }

        public void SetCity(string city)
        {
            _city = city;
        }

        public string GetState()
        {
            return _state;
        }

        public void SetState(string state)
        {
            _state = state;
        }

        public string GetCountry(string country)
        {
            return _country;
        }

        public void SetCountry(string country)
        {
            _country = country;
        }

        public DateTime GetLastUpdated()
        {
            return _lastUpdated;
        }

        public void SetLastUpdated(DateTime lastUpdated)
        {
            _lastUpdated = lastUpdated;
        }

        public void SetDefaults()
        {
            _temperature = 78.0;
            _lowTemp = 72.0;
            _highTemp = 81.0;
            _weather = "Sunny";
            _city = "Orlando";
            _state = "FL";
            _country = "US";
            _lastUpdated = DateTime.Now;
        }
    }
}