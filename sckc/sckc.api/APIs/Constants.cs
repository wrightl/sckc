namespace sckc.api.APIs
{
    public static class Constants
    {
        public const string BookingsEmailAddress = "bookings@sheffieldcitykayakclub.co.uk";
        public const string BookingsEmailName = "Booking Request";

        public const string BookingRequestEmailAddress = "bookingrequest@sheffieldcitykayakclub.co.uk";
        public const string BookingRequestEmailName = "Booking Request";


        public const string ContactUsEmailAddress = "contactus@sheffieldcitykayakclub.co.uk";
        public const string ContactUsEmailName = "Contact Us";

        // Submitted trip reports are sent here for someone to accept or reject. They are sent from the contactus
        // address, a sender on the club's verified Resend domain.
        public const string TripReportsEmailAddress = "admin@sheffieldcitykayakclub.co.uk";
        public const string TripReportsFromAddress = ContactUsEmailAddress;
        public const string TripReportsEmailName = "Trip Reports";

        // Stripe payment notifications (StripeWebhookController) go here, sent from a verified Resend sender
        public const string PaymentsEmailAddress = "payment@sheffieldcitykayakclub.co.uk";
        public const string PaymentsFromAddress = BookingsEmailAddress;
        public const string PaymentsEmailName = "Payments";

        // Used to build the accept/reject links in review emails (never taken from the request, which a sender controls)
        public const string SiteBaseUrl = "https://www.sheffieldcitykayakclub.co.uk/";

        public const string GoogleCalendarId = "sheffieldcitykayakclub@gmail.com";

        public const string AdminSiteBaseUrl = "https://sckc.azurewebsites.net/api/";
        public const string AdminSiteBookingsApi = "bookings";
    }
}