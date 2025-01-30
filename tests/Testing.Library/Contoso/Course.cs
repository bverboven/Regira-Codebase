using System.ComponentModel.DataAnnotations;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Testing.Library.Contoso;

public class Course : IEntityWithSerial, IHasNormalizedTitle, IHasAttachments, IHasAttachments<CourseAttachment>
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }

    [StringLength(64, MinimumLength = 3)]
    public string? Title { get; set; }
    [MaxLength(64)]
    [Normalized(SourceProperty = nameof(Title))]
    public string? NormalizedTitle { get; set; }

    [Range(0, 5)]
    public int Credits { get; set; }

    public Department? Department { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
    public ICollection<Instructor>? Instructors { get; set; }

    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.ToArray();
        set => Attachments = value?.Cast<CourseAttachment>().ToArray();
    }
    public ICollection<CourseAttachment>? Attachments { get; set; }

    public bool? HasAttachment { get; set; }
}

public class CourseAttachment : EntityAttachment
{
    public CourseAttachment()
    {
        ObjectType = nameof(Course);
    }

    public string? Description { get; set; }
}