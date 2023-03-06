using AppBroker.BusinessCore.Entity.DTO;
using AutoMapper;
using BusinessCore.Entity.DTO;
using Infrastructure.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            #endregion
        }
    }
}
