﻿using System.ComponentModel.DataAnnotations;
using Entities.TestApi.Infrastructure.Departments;

namespace Entities.TestApi.Infrastructure.Persons;

public class PersonInputDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    public string? GivenName { get; set; }
    [Required]
    [MaxLength(64)]
    public string? LastName { get; set; }
    public string? Description { get; set; }
    [MaxLength(32)]
    public string? Phone { get; set; }
    [MaxLength(256)]
    public string? Email { get; set; }

    public int? SupervisorId { get; set; }
    public ICollection<DepartmentInputDto>? Departments { get; set; }
}