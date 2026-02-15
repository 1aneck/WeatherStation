using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherStation
{
    public class WeatherLog
    {
        public Guid Id
        {
            get;
            set;
        }
        public string? Data
        {
            get;
            set;
        } = null;
        public DateTimeOffset UploadedAt
        {
            get;
            set;
        } = DateTimeOffset.Now;
        public bool IsAvailable
        {
            get;
            set;
        }
        public string? ErrorMessage
        {
            get;
            set;
        } = null;
    }
}
