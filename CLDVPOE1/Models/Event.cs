namespace CLDVPOE1.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventDescription { get; set; }
        public List<Booking> Booking { get; set; } = new();
        public required Venue Venue { get; set; }
        public int? EventTypeID { get; set; }
        public EventType? EventType { get; set; }
        public int? VenueId { get; internal set; }
    }
}
