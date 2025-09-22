using AutoMapper;
using PropertyManagement.Application.DTOs;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Owner mappings
        //CreateMap<Owner, OwnerDto>();
        CreateMap<Owner, OwnerDto>()
            .ForMember(dest => dest.PhotoBase64, opt => opt.MapFrom(src =>
                src.Photo != null ? Convert.ToBase64String(src.Photo) : null))
            .ForMember(dest => dest.PropertyCount, opt => opt.MapFrom(src =>
                src.Properties.Count(p => p.Enabled)))
            .ForMember(dest => dest.Properties, opt => opt.MapFrom(src =>
                src.Properties.Where(p => p.Enabled)));

        CreateMap<CreateOwnerDto, Owner>()
            .ForMember(dest => dest.IdOwner, opt => opt.Ignore())
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.PhotoBase64) ? Convert.FromBase64String(src.PhotoBase64) : null))
            .ForMember(dest => dest.Properties, opt => opt.Ignore());

        CreateMap<UpdateOwnerDto, Owner>()
            .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.PhotoBase64) ? Convert.FromBase64String(src.PhotoBase64) : null))
            .ForMember(dest => dest.Properties, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Property mappings
        CreateMap<Property, PropertyDto>()
            .ForMember(dest => dest.PropertyImages, opt => opt.MapFrom(src =>
                src.PropertyImages.Where(pi => pi.Enabled)))
            .ForMember(dest => dest.PropertyTraces, opt => opt.MapFrom(src =>
                src.PropertyTraces.OrderByDescending(pt => pt.DateSale)));

        CreateMap<CreatePropertyDto, Property>()
            .ForMember(dest => dest.IdProperty, opt => opt.Ignore())
            .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.PropertyImages, opt => opt.Ignore())
            .ForMember(dest => dest.PropertyTraces, opt => opt.Ignore());

        // PropertyImage mappings
        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(dest => dest.FileBase64, opt => opt.MapFrom(src =>
                src.File != null ? Convert.ToBase64String(src.File) : null));

        CreateMap<PropertyImageDto, PropertyImage>()
            .ForMember(dest => dest.File, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.FileBase64) ? Convert.FromBase64String(src.FileBase64) : null))
            .ForMember(dest => dest.Property, opt => opt.Ignore());

        // PropertyTrace mappings
        CreateMap<PropertyTrace, PropertyTraceDto>();
        CreateMap<PropertyTraceDto, PropertyTrace>()
            .ForMember(dest => dest.Property, opt => opt.Ignore());
        CreateMap<CreatePropertyTraceDto, PropertyTrace>()
            .ForMember(dest => dest.IdPropertyTrace, opt => opt.Ignore())
            .ForMember(dest => dest.Property, opt => opt.Ignore());

        // Mapeo entre DTOs de aplicación y modelos de dominio
        CreateMap<PropertyFilterDto, PropertyFilter>();

        // Mapeo entre modelos de dominio y DTOs de aplicación
        CreateMap<PagedResult<Property>, PagedResultDto<PropertyDto>>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

        // Mapeo genérico para resultados paginados
        CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>))
            .ForMember("Data", opt => opt.MapFrom("Data"))
            .ForMember("TotalCount", opt => opt.MapFrom("TotalCount"))
            .ForMember("PageNumber", opt => opt.MapFrom("PageNumber"))
            .ForMember("PageSize", opt => opt.MapFrom("PageSize"));


    }
}