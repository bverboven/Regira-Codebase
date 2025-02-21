using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Services;
using Testing.Library.Contoso;

namespace Entities.DependencyInjection.Testing.Infrastructure;

public interface ICourseServiceA : IEntityService<Course, int, SearchObject<int>>;
public interface ICourseServiceB : IEntityService<Course, int, CourseSearchObject>;

// Repositories
public class CourseRepository1(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course>(readService, writeService), ICourseServiceA;

public class CourseRepository2(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int>(readService, writeService), ICourseServiceA;

public class CourseRepository3A(IEntityReadService<Course, int, SearchObject<int>> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, SearchObject<int>>(readService, writeService), ICourseServiceA;

public class CourseRepository3B(IEntityReadService<Course, int, CourseSearchObject> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, CourseSearchObject>(readService, writeService), ICourseServiceB;

public class CourseRepository4(IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, CourseSearchObject, CourseSortBy, CourseIncludes>(readService, writeService), ICourseServiceB;

public class CourseRepository5(IEntityReadService<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> readService, IEntityWriteService<Course, int> writeService)
    : EntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(readService, writeService), ICourseServiceB;

// Managers
public class CourseManager1(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course>(repo), ICourseServiceA;
public class CourseManager2(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course, int>(repo), ICourseServiceA;
public class CourseManager3A(IEntityRepository<Course, int, SearchObject<int>> repo)
    : EntityManager<Course, int, SearchObject<int>>(repo), ICourseServiceA;
public class CourseManager3B(IEntityRepository<Course, int, CourseSearchObject> repo)
    : EntityManager<Course, int, CourseSearchObject>(repo), ICourseServiceB;
public class CourseManager4(IEntityRepository<Course, CourseSearchObject, CourseSortBy, CourseIncludes> repo)
    : EntityManager<Course, CourseSearchObject, CourseSortBy, CourseIncludes>(repo), ICourseServiceB;
public class CourseManager5(IEntityRepository<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes> repo)
    : EntityManager<Course, int, CourseSearchObject, CourseSortBy, CourseIncludes>(repo), ICourseServiceB;

// Custom service

public class CourseServiceA(IEntityRepository<Course, int, SearchObject<int>> service)
    : EntityWrappingServiceBase<Course>(service), ICourseServiceA;
public class CustomCourseServiceA(ICourseServiceA service)
    : EntityWrappingServiceBase<Course>(service);

public class CourseServiceB(IEntityRepository<Course, int, CourseSearchObject> service)
    : EntityWrappingServiceBase<Course, int, CourseSearchObject>(service);
public class CustomCourseServiceB(ICourseServiceB service)
    : EntityWrappingServiceBase<Course, int, CourseSearchObject>(service);