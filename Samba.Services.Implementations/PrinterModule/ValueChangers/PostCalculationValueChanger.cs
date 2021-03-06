﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    class PostCalculationValueChanger : AbstractValueChanger<Calculation>
    {
        public override string GetTargetTag()
        {
            return "SERVICES";
        }

        protected override string GetModelName(Calculation model)
        {
            return model.Name;
        }
    }
}
