using System;

namespace Fitness.Business.DTOs
{
    public record ReservationDto(
        int ReservationId,
        DateTime Date,
        string Equipment,
        string TimeSlot
    );
}
