using System;
using System.ComponentModel.DataAnnotations;
using Charm.Application.Utils;
using Newtonsoft.Json;

namespace Charm.Application.Dto
{
    public class GistRequest
    {
        [Required]
        public string? Message { get; set; }

        [Required]
        [JsonConverter(typeof(DateFormatConverter), "dd-MM-yyyyTHH:mm")]
        public DateTimeOffset? Deadline { get; set; }

        [JsonConverter(typeof(DateFormatConverter), "dd-MM-yyyyTHH:mm")]
        public TimeSpan? Advance { get; set; }
    }
}