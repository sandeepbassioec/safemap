namespace SafeMap.TestCases
{
    // ------------------------------
    // Shared domain models used across scenarios
    // ------------------------------
    public class City 
    { 
        public string? Name { get; set; } 
    }

    public class Address 
    { 
        public City? City { get; set; } 
        public string? Zip { get; set; } 
    }

    public class Profile 
    { 
        public Address? Address { get; set; } 
        public DateTime? Birth { get; set; } 
        public int? Score { get; set; } 
    }
    
    public class Person 
    { 
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public Profile? Profile { get; set; } 
        public string? Email { get; set; } 
    }

    public class Trial 
    { 
        public DateTime? TrialEndAt { get; set; } 
        public int? StatusId { get; set; } 
    }
    
    public class Dealer 
    { 
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public Trial? Trial { get; set; } 
        public string? OrgKey { get; set; } 
    }
    
    public enum OrderStatus 
    { 
        Unknown = 0, 
        Pending = 1, 
        Shipped = 2, 
        Cancelled = 3 
    }
    
    public class FakeAvailabilityRequest
    {
        public List<string> PreferredOrgs { get; set; } = new List<string>();
    }

    public class FakePandaResponse
    {
        public Dictionary<string, List<string>>? PartNumberResponses { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }

    public class Customer 
    { 
        public Guid Id { get; set; } 
        public Profile? Profile { get; set; } 
    }
}