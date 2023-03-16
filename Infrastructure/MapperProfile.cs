using AutoMapper;
using AutoMapper.Internal;
using Infrastructure;
using Infrastructure.Entity;

namespace BusinessCore.Entity
{
    public class MapperProfile : MapperConfigurationExpression
    {
        public MapperProfile()
        {
            #region AspNetUser
            CreateMap<AspNetUser, UserListDTO>().ReverseMap();
            CreateMap<AspNetUser, RegisterDTO>().ReverseMap();
            CreateMap<AspNetUser, UserSubmitDTO>().ReverseMap();
            CreateMap<AspNetUser, UserEditSubmitDTO>().ReverseMap();
            CreateMap<AspNetUser, UserExportDto>().ReverseMap();
            CreateMap<AspNetUser, UserimportDto>().ReverseMap();
            CreateMap<AspNetUser, UserimportTemplateDto>().ReverseMap();
            #endregion
        }
    }
}
