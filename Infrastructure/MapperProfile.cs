using AutoMapper;
using Infrastructure;
using Infrastructure.Entity;

namespace BusinessCore.Entity
{
    public class MapperProfile : MapperConfigurationExpression
    {
        public MapperProfile()
        {
            #region User
            CreateMap<AspNetUser, RegisterDTO>().ReverseMap();
            CreateMap<AspNetUser, UserListDTO>().ReverseMap();
            CreateMap<AspNetUser, UserSubmitDTO>().ReverseMap();
            CreateMap<AspNetUser, UserEditSubmitDTO>().ReverseMap();
            CreateMap<AspNetUser, UserExportDto>().ReverseMap();
            #endregion
        }
    }
}
