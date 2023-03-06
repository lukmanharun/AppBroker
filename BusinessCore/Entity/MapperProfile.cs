using AppBroker.BusinessCore.Entity.DTO;
using AutoMapper;
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
            CreateMap<AspNetUser, RegisterDTO>().ReverseMap();
        }
    }
}
