﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    class PaymentValueChanger : AbstractValueChanger<Payment>
    {
        public override string GetTargetTag()
        {
            return "PAYMENTS";
        }

        protected override string GetModelName(Payment model)
        {
            return model.Name;
        }
    }
}
