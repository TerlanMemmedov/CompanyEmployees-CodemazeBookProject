using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    //this is best for just json
    //[Serializable] //for a little support xml
    //public record EmployeeDto(Guid Id, string Name, int Age, string Position);


    //created that for support xml 
    public record EmployeeDto
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public int? Age { get; init; }
        public string? Position { get; init; }
    }
}
