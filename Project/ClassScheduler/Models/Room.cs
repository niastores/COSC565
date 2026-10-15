namespace ClassScheduler.Models
{
    /*
    Class Name: Room
    Module Name: Domain Model
    Description: Physical room. Pseudo-rooms such as "Online" or "Dept" are exempt from conflict checks.
    Method List:
    None (data container only; Key and IsConflictExempt are computed properties)
    */
    public class Room
    {
        private static readonly HashSet<string> ExemptNames = new() { "online", "dept", "tba", "asynchronous" };

        public string Building { get; set; } = "";
        public string Number { get; set; } = "";
        public int Capacity { get; set; }

        // Identity used to decide whether two bookings are in the same room.
        public string Key => $"{Building}|{Number}".Trim().ToUpperInvariant();

        // True for shared or virtual locations that many sections may use at once.
        public bool IsConflictExempt =>
            Building.Length == 0 && ExemptNames.Contains(Number.Trim().ToLowerInvariant().Replace("-", ""));
    }
}