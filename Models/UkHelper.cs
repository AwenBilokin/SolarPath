namespace SolarPath.Web.Models;

public static class UkHelper
{
    public static string DifficultyUk(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy   => "Легкий",
        DifficultyLevel.Medium => "Середній",
        DifficultyLevel.Hard   => "Складний",
        DifficultyLevel.Expert => "Експерт",
        _                      => d.ToString()
    };

    public static string BookingStatusUk(BookingStatus s) => s switch
    {
        BookingStatus.Pending          => "Очікує",
        BookingStatus.Paid             => "Оплачено",
        BookingStatus.Confirmed        => "Підтверджено",
        BookingStatus.InProgress       => "В процесі",
        BookingStatus.Completed        => "Завершено",
        BookingStatus.Cancelled        => "Скасовано",
        BookingStatus.CancelledByGuide => "Скасовано гідом",
        BookingStatus.RefundRequested  => "Запит повернення",
        BookingStatus.Refunded         => "Повернено",
        _                              => s.ToString()
    };
}
