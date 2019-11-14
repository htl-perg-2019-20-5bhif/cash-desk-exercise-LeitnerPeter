using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private CashDeskDataContext context;

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if (context != null)
            {
                throw new InvalidOperationException();
            }
            context = new CashDeskDataContext();
            return Task.CompletedTask;
        }


        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            if (context == null)
            {
                throw new InvalidOperationException();
            }
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || birthday == null)
            {
                throw new ArgumentException("Vorname, Nachname oder Geburtstag ist null oder leer");
            }
            if (context.Members.Where(curMember => curMember.LastName.Equals(lastName)).Count() > 0)
            {
                throw new DuplicateNameException("Nachname " + lastName + " existiert bereits");
            }
            Member member = new Member { FirstName = firstName, LastName = lastName, Birthday = birthday };
            context.Members.Add(member);
            await context.SaveChangesAsync();
            return member.MemberNumber;
        }


        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber) 
        {
            if (context == null)
            {
                throw new InvalidOperationException("Datenbank nicht gefunden");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("MemberNumber unbekannt");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);
            context.Members.Remove(member);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException();
            }

            var membershipExists = context.Memberships.Where(membership => membership.Member.MemberNumber == memberNumber && membership.End == null).Count() > 0;
            if (membershipExists)
            {
                throw new AlreadyMemberException();
            }
            Member member;
            try
            {
                member = context.Members.Single(curMember => curMember.MemberNumber == memberNumber);
            }
            catch (Exception ex)
            {
                throw new ArgumentException();
            }
            Membership membership = new Membership { Member = member, Begin = DateTime.Now };
            context.Memberships.Add(membership);
            await context.SaveChangesAsync();
            return membership;
        }

        /// <inheritdoc />
        public Task<IMembership> CancelMembershipAsync(int memberNumber) => throw new NotImplementedException();

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            if (context == null)
            {
                throw new InvalidOperationException();
            }

            Membership membership;
            try
            {
                membership = context.Memberships.Single(membership => membership.Member.MemberNumber == memberNumber && membership.End == null);
            }
            catch (Exception ex)
            {
                throw new NoMemberException();
            }
            try
            {
                context.Members.Single(curMember => curMember.MemberNumber == memberNumber);
            }
            catch (Exception ex)
            {
                throw new ArgumentException();
            }

            if (amount <= 0)
            {
                throw new ArgumentException();
            }

            Deposit deposit = new Deposit
            {
                Amount = amount,
                Membership = membership
            };
            context.Deposits.Add(deposit);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            if (context == null)
            {
                throw new InvalidOperationException();
            }

            var depositStatistics = from deposit in context.Deposits
                                    group deposit by new { deposit.Membership.Begin.Year, deposit.Membership.Member } into g
                                    select new DepositStatistics { Year = g.Key.Year, Member = g.Key.Member, TotalAmount = g.Sum(deposit => deposit.Amount) };


            return depositStatistics;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }
    }
}
