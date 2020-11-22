using Newtonsoft.Json.Converters;

namespace Charm.Application.Utils
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}