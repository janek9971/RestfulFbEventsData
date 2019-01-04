using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Test
{
    public class EventsData
    {
        public string TimeRange { get; set; }
        public string DayOfTheWeek { get; set; }
        public int Guests { get; set; } //text
        public string Title { get; set; } //name[1]
        public string LocalLocation { get; set; }

        public string GlobalLocation { get; set; }
        public string BuyTicketUrl { get; set; }
        public string EventId { get; set; }
    }
    public class Deserialize
    {
        
        class Data
        {
            [JsonProperty("data")]
            public Page Page { get; set; }
        }

        class Page
        {
            [JsonProperty("page")]
            public Events Events { get; set; }
        }

        class Events
        {
            [JsonProperty("upcoming_events")]
            public Edges Edges { get; set; }

        }

        class Edges
        {
            public List<EdgesCodes> EdgesCodes { get; set; }

        }
        class EdgesCodes
        {
            [JsonProperty("is_hidden_on_profile_calendar")]
            public string IsHiddenOnProfileCalendar { get; set; }
            [JsonProperty("is_added_to_profile_calendar")]
            public string IsAddedToProfileCalendar { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
        
        }
    }
}
