using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using System.ComponentModel.DataAnnotations;

namespace Testing.Library.Contoso;

public class OfficeAssignment : IEntityWithSerial
{
    public int Id { get; set; }
    public int? InstructorId { get; set; }
    [MaxLength(64)]
    public string? Location { get; set; }
    [MaxLength(64)]
    [Normalized(SourceProperty = nameof(Location))]
    public string? NormalizedLocation { get; set; }

    public Instructor? Instructor { get; set; }
}