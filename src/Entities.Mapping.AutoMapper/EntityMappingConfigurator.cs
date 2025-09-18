//using Microsoft.Extensions.DependencyInjection;
//using Regira.Entities.DependencyInjection.Mapping.Models;

//namespace Regira.Entities.Mapping.AutoMapper;

//public class EntityMappingConfigurator(IServiceCollection services) : IMappingConfigurator
//{
//    public void ConfigureMapping(IEntityMappingDefinition mapping, IServiceProvider serviceProvider)
//    {
//        services.AddAutoMapper(c => c.CreateMap(mapping.SourceType, mapping.DestinationType));
//    }
//}