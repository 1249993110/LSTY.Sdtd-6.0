﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSTY.Sdtd.Data.Dtos
{
    public class PaginationResultDto
    {
        public uint Total { get; set; }

        public object Items { get; set; }
    }
}
