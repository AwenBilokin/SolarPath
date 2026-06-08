namespace SolarPath.Web.Models;

public enum DifficultyLevel { Easy, Medium, Hard, Expert }
public enum BookingStatus
{
    Pending, Paid, Confirmed, InProgress, Completed,
    Cancelled, CancelledByGuide, RefundRequested, Refunded
}
public enum RouteStatus { Draft, Published, Archived }
public enum PointType  { Start, Checkpoint, Highlight, Finish }
public enum PaymentMethod { Card, BankTransfer, Cash }
