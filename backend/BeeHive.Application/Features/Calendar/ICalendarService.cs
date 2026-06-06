using BeeHive.Application.Features.Calendar.DTOs;

namespace BeeHive.Application.Features.Calendar;

public interface ICalendarService
{
    /// <summary>Returns calendar events (todos with due dates + diet feeding entries) visible to the current caller.</summary>
    Task<CalendarEventsDto> GetCalendarEventsAsync();
}
