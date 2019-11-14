using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk
{
    public class Deposit : IDeposit
    {
        public int DepositId { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        public decimal Amount
        {
            get { return Amount; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Anzahl muss größer 0 sein");
                }
                Amount = value;
            }
        }

        IMembership IDeposit.Membership => Membership;
    }
}
