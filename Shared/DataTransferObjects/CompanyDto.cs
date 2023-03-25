using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    //this is best for just json
    //[Serializable] //for a little support xml
    //public record CompanyDto(Guid Id, string Name, string FullAddress); 

    //created that for support xml 
    public record CompanyDto
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? FullAddress { get; init; }
    }
}
