using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOS
{
    public class SmsRequest
    {
        public string RecipientPhoneNumber { get; set; }
        public string Body { get; set; }
    }
}
