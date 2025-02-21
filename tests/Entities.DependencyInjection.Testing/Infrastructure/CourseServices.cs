using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Services;
using Testing.Library.Contoso;

namespace Entities.DependencyInjection.Testing.Infrastructure;

public interface ICourseService : IEntityService<Course, int, SearchObject<int>>;

// Repositories
public class CourseRepository1(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course>(readService, writeService), ICourseService;

public class CourseRepository2(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int>(readService, writeService), ICourseService;

public class CourseRepository3A(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, SearchObject<int>>(readService, writeService), ICourseService;

public class CourseRepository3B(IEntityReadService<Course, int, CourseSearchObject> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, CourseSearchObject>(readService, writeService);

public class CourseRepository4(IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, CourseSearchObject, CourseSortBy, CourseIncludes>(readService, writeService);

public class CourseRepository5(IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(readService, writeService);

// Managers
public class CourseManager1(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course>(repo);
public class CourseManager2(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course, int>(repo);
public class CourseManager3A(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course, int, SearchObject<int>>(repo);
public class CourseManager3B(IEntityRepository<Course, int, CourseSearchObject> repo)
    : EntityManager<Course, int, CourseSearchObject>(repo);
public class CourseManager4(IEntityRepository<Course, CourseSearchObject, CourseSortBy, CourseIncludes> repo)
    : EntityManager<Course, CourseSearchObject, CourseSortBy, CourseIncludes>(repo);
public class CourseManager5(IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> repo)
    : EntityManager<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(repo);

// Custom service

public class CourseService1(IEntityRepository<Course, int, SearchObject<int>> service)
    : EntityWrappingServiceBase<Course>(service);
public class CustomCourseService1(ICourseService service)
    : EntityWrappingServiceBase<Course>(service);