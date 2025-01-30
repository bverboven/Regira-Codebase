﻿using Microsoft.AspNetCore.Mvc;
using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Models;

namespace Regira.Entities.Web.Controllers.Abstractions;

/// <summary>
/// Basic EntityController without extra models
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class EntityControllerBase<TEntity> : EntityControllerBase<TEntity, SearchObject, TEntity, TEntity>
    where TEntity : class, IEntity<int>;
/// <summary>
/// Basic EntityController with DTO models
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TInputDto"></typeparam>
public abstract class EntityControllerBase<TEntity, TDto, TInputDto> : EntityControllerBase<TEntity, SearchObject, TDto, TInputDto>
    where TEntity : class, IEntity<int>;
/// <summary>
/// Default EntityController with custom <see cref="ISearchObject"/> and DTO models
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TSearchObject"></typeparam>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TInputDto"></typeparam>
public abstract class EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto> : EntityControllerBase<TEntity, int, TSearchObject, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>;

/// <summary>
/// Default EntityController with custom <see cref="ISearchObject{TKey}"/> and DTO models
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TInputDto"></typeparam>
/// <typeparam name="TSearchObject"></typeparam>
public abstract class EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto> : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>
{
    // Details
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] TKey id)
        => await this.Details<TEntity, TKey, TDto>(id) ?? NotFound();

    // List
    [HttpGet]
    public virtual Task<ActionResult<ListResult<TDto>>> List([FromQuery] TSearchObject so, [FromQuery] PagingInfo pagingInfo)
        => this.List<TEntity, TKey, TSearchObject, TDto>(so, pagingInfo);

    // Save
    [HttpPost("save")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Save([FromBody] TInputDto model)
        => await this.Save<TEntity, TKey, TDto, TInputDto>(model) ?? NotFound();
    // Add
    [HttpPost]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Create([FromBody] TInputDto model)
        => (await this.Save<TEntity, TKey, TDto, TInputDto>(model))!;
    // Modify
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Modify([FromRoute] TKey id, [FromBody] TInputDto model)
        => await this.Save<TEntity, TKey, TDto, TInputDto>(model, id) ?? NotFound();

    // Delete
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] TKey id)
        => await this.Delete<TEntity, TKey, TDto>(id) ?? NotFound();
}
/// <summary>
/// Complex EntityController
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TSo"></typeparam>
/// <typeparam name="TSortBy"></typeparam>
/// <typeparam name="TIncludes"></typeparam>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TInputDto"></typeparam>
public abstract class EntityControllerBase<TEntity, TSo, TSortBy, TIncludes, TDto, TInputDto> : EntityControllerBase<TEntity, int, TSo, TSortBy, TIncludes, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TSo : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
/// <summary>
/// Complex EntityController
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TSo"></typeparam>
/// <typeparam name="TSortBy"></typeparam>
/// <typeparam name="TIncludes"></typeparam>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TInputDto"></typeparam>
public abstract class EntityControllerBase<TEntity, TKey, TSo, TSortBy, TIncludes, TDto, TInputDto> : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSo : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // Details
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] TKey id)
        => await this.Details<TEntity, TKey, TDto>(id) ?? NotFound();

    // List
    [HttpGet]
    public virtual Task<ActionResult<ListResult<TDto>>> List([FromQuery] TSo so,
        [FromQuery] PagingInfo pagingInfo, [FromQuery] TIncludes[] includes, [FromQuery] TSortBy[] sortBy)
        => List([so], pagingInfo, includes, sortBy);

    [HttpPost("list")]
    public virtual Task<ActionResult<ListResult<TDto>>> List([FromBody] TSo[] so,
        [FromQuery] PagingInfo pagingInfo, [FromQuery] TIncludes[] includes, [FromQuery] TSortBy[] sortBy)
        => this.List<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(so, pagingInfo, includes, sortBy);

    // Search
    [HttpGet("search")]
    public virtual Task<ActionResult<SearchResult<TDto>>> Search([FromQuery] TSo so,
        [FromQuery] PagingInfo pagingInfo, [FromQuery] TIncludes[] includes,
        [FromQuery] TSortBy[] sortBy)
        => Search([so], pagingInfo, includes, sortBy);

    [HttpPost("search")]
    public virtual Task<ActionResult<SearchResult<TDto>>> Search([FromBody] TSo[] so,
        [FromQuery] PagingInfo pagingInfo, [FromQuery] TIncludes[] includes, [FromQuery] TSortBy[] sortBy)
        => this.Search<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(so, pagingInfo, includes, sortBy);

    // Save
    [HttpPost("save")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Save([FromBody] TInputDto model)
        => await this.Save<TEntity, TKey, TDto, TInputDto>(model) ?? NotFound();
    // Add
    [HttpPost]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Create([FromBody] TInputDto model)
        => (await this.Save<TEntity, TKey, TDto, TInputDto>(model))!;
    // Modify
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Modify([FromRoute] TKey id, [FromBody] TInputDto model)
        => await this.Save<TEntity, TKey, TDto, TInputDto>(model, id) ?? NotFound();

    // Delete
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] TKey id)
        => await this.Delete<TEntity, TKey, TDto>(id) ?? NotFound();
}