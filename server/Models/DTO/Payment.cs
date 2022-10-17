using System;

namespace project.Models.DTO
{
    public class Payment
    {
        public string Consecutive { set; get; }
        public string Code { get; set; }
        public double PaymentValue { get; set; }
        public DateTime FechaRecaudo { get; set; }
    }
}
