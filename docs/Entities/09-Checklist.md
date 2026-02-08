# Checklist

## Setup

- [ ] Install required packages
- [ ] Create/configure **DbContext** (inherit from `DbContext`)
    - DbSets
    - Model configuration
- [ ] Configure interceptors on DbContext (if needed)
    - Primers
    - Normalizers
- [ ] Setup Entities using `.UseEntities()`
    - [ ] Configure **Mapping** library (AutoMapper/Mapster) when using DTOs
    - [ ] Register global filters, primers, preppers (optional)
- [ ] Configure the FileService in `.WithAttachments()` when using attachments


## Add & configure a new Entity

When implementing a new entity in an application:

*Required*
- [ ] Create entity **Model(s)** 
    - Use appropriate interfaces
    - Use Data annotations (*Required*, *MaxLength*, ...)
    - Prefer using `SetDecimalPrecisionConvention` in DbContext over setting precision on each property
- [ ] Configure **DbContext**
    - Add DbSet collection
    - Configure relationships
    - Prefer Data Annotations over Fluent API when possible
- [ ] Configure Entity in DI using `.For<TEntity>()`
- [ ] Create **Controller** *(when using API)*
    - Add custom actions only when necessary, otherwise rely on built-in CRUD actions
    - Prefer extending SearchObject to extend filtering over adding extra actions
- [ ] Create/apply database migration

*Recommended (when using API)*

- [ ] Create **DTOs** (output DTO, input DTO)
- [ ] Configure **Mapping** (+ Aftermappers when needed)

*Optional*

- [ ] Create SearchObject (+SortBy & Includes enums)
- [ ] Implement query filters
- [ ] Add Processors
- [ ] Add Preppers
- [ ] Add Primers
- [ ] Configure child properties with Related method

*Extra*
- [ ] Add Attachments
    - Ensure attachments are set up
    - Implement Owning entity interfaces
    - Implement DbContext
- [ ] Add Normalizers
    - Ensure normalizers are set up
    - Decorate entity properties with Normalized attribute
    - Or implement custom (global) normalizer for entity


## Overview

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. [Mapping](04-Mapping.md) - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. **[Checklist](09-Checklist.md)** - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
