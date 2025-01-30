namespace Regira.Entities.Models.Abstractions;

public interface IHasStartDate
{
    DateTime? StartDate { get; set; }
}

public interface IHasEndDate
{
    DateTime? EndDate { get; set; }
}
public interface IHasStartEndDate : IHasStartDate, IHasEndDate;