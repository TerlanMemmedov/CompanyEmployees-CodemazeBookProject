﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    public class EmployeeParameters : RequestParameters
    {
        public EmployeeParameters() => OrderBy = "name";

        public uint MinAge { get; set; }
        public uint MaxAge { get; set; } = int.MaxValue;

        //for checking and if false return error
        public bool ValidAgeRange => MaxAge > MinAge;

        //for searching
        public string? SearchTerm { get; set; }
    }
}
