﻿using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Abstract
{
    public interface ICityRepository
    {
        bool Save(LocationModel model);
        LocationModel Get(int id);
    }
}
