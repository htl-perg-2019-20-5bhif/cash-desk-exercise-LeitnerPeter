using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CashDesk
{
    public class Membership : IMembership
    {
        public int MembershipId { get; set; }

        [Required]
        public Member Member { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        public DateTime End
        {
            get { return End; }
            set
            {
                if (value <= Begin)
                {
                    throw new ArgumentException("Ende muss größer als Beginn sein");
                }
                End = value;
            }
        }
        IMember IMembership.Member => Member;
    }
}
