using System;

namespace MeteoApp
{
    public class MeteoData
    {
        public DateTime DateTime { get; set; }
        public float Humidity { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }

        public MeteoData()
        {

        }
    }
}
