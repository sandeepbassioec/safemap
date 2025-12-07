using System;

namespace SafeMap.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {

        }

    }

    public class Person
    {
        public Address? Address { get; set; }
        public int? Age { get; set; }
    }

    public class Address
    {
        public City? City { get; set; }
    }

    public class City
    {
        public string? Name { get; set; }
    }

    public class Dealer
    {
        public Trial? Trial { get; set; }
    }

    public class Trial
    {
        public DateTime? TrialEndAt { get; set; }
        public int? StatusId { get; set; }
    }
}