using Xunit;

namespace SafeMap.TestCases
{
    public class SafeMapXTests
    {
        // Group 1: Basic null checks and string fallbacks
        // ------------------------------
        // Scenario 1: Deep property retrieval (user city name)
        // Before: repeated null checks
        // After: Safe.Path expression
        // ------------------------------

        [Fact]
        public void Bad_DeepProperty_Classic()
        {
            Person? p = null;
            string city;

            if (p != null && p.Profile != null && p.Profile.Address != null && p.Profile.Address.City != null && p.Profile.Address.City.Name != null)
                city = p.Profile.Address.City.Name;
            else 
                city = "Unknown";
            
            Assert.Equal("Unknown", city);
        }

        [Fact]
        public void After_DeepProperty_SafePathExpr()
        {
            Person? p = null;
        
            var name = Safe
                .Path(p, (Person x) => x.Profile.Address.City.Name) ?? "Unknown";

            Assert.Equal("Unknown", name);
        }

        // Edge: object exists but inner city missing
        [Fact]
        public void Bad_DeepProperty_ObjectNoCity()
        {
            var p = new Person { Profile = new Profile { Address = new Address { City = null } } };
            string city;
        
            if (p != null && p.Profile != null && p.Profile.Address != null && p.Profile.Address.City != null && p.Profile.Address.City.Name != null)
                city = p.Profile.Address.City.Name;
            else 
                city = "Unknown";
            
            Assert.Equal("Unknown", city);
        }

        [Fact]
        public void After_DeepProperty_ObjectNoCity()
        {
            var p = new Person { Profile = new Profile { Address = new Address { City = null } } };
            var name = Safe
                .Path(p, "Profile.Address.City.Name") as string ?? "Unknown";
        
            Assert.Equal("Unknown", name);
        }

        // ------------------------------
        // Scenario 2: Trial expired calculation for dealer
        // Before: nested nullable handling
        // After: Safe.Guard + Map<DateTime> then Map<bool>
        // ------------------------------

        [Fact]
        public void Bad_TrialExpired_Classic()
        {
            Dealer? d = null;
            
            bool expired = d != null && d.Trial != null && d.Trial.TrialEndAt.HasValue ? d.Trial.TrialEndAt.Value < DateTime.UtcNow : false;
            
            Assert.False(expired);
        }

        [Fact]
        public void After_TrialExpired_Chain()
        {
            Dealer? d = null;
        
            var expired = Safe.Guard(d)
                .Map(t => t.Trial)
                .Map<DateTime>(t => t != null ? t.TrialEndAt : (DateTime?)null)
                .Map<bool>(dt => (bool?)(dt < DateTime.UtcNow))
                .Default(false)
                .Value();
            
            Assert.False(expired);
        }

        // Edge: trial exists and expired
        [Fact]
        public void Bad_TrialExpired_Expired()
        {
            Dealer d = new Dealer { Trial = new Trial { TrialEndAt = DateTime.UtcNow.AddDays(-2) } };
        
            bool expired = d != null && d.Trial != null && d.Trial.TrialEndAt.HasValue ? d.Trial.TrialEndAt.Value < DateTime.UtcNow : false;
            
            Assert.True(expired);
        }

        [Fact]
        public void After_TrialExpired_Expired()
        {
            Dealer d = new Dealer { Trial = new Trial { TrialEndAt = DateTime.UtcNow.AddDays(-2) } };
        
            var expired = Safe.Guard(d)
                .Map(t => t.Trial)
                .Map<DateTime>(t => t != null ? t.TrialEndAt : (DateTime?)null)
                .Map<bool>(dt => (bool?)(dt < DateTime.UtcNow))
                .Default(false)
                .Value();
            
            Assert.True(expired);
        }

        // ------------------------------
        // Scenario 3: Get customer's city with fallback to company HQ
        // Before: nested if + fallback
        // After: Safe.Guard.Fallback + Map chain
        // ------------------------------

        [Fact]
        public void Bad_CustomerCityWithFallback()
        {
            Person? customer = null;
            Address? hq = new Address { City = new City { Name = "HQCity" } };
            string city;
            
            if (customer != null && customer.Profile != null && customer.Profile.Address != null && customer.Profile.Address.City != null)
                city = customer.Profile.Address.City.Name;
            else if (hq != null && hq.City != null)
                city = hq.City.Name;
            else city = "Unknown";
            
            Assert.Equal("HQCity", city);
        }

        [Fact]
        public void After_CustomerCityWithFallback()
        {
            Person? customer = null;
            var hq = Safe.Guard(new Address { City = new City { Name = "HQCity" } });

            var city = Safe.Guard(customer)
                .Map(p => p.Profile)    // SafeValue<Profile>
                .Map(a => a.Address)    // SafeValue<Address>
                .Fallback(hq)           // Now both sides are SafeValue<Address>
                .Map(c => c.City)
                .Map(n => n.Name)
                .Default("Unknown")
                .Value();

            Assert.Equal("HQCity", city);
        }

        // Edge: both null
        [Fact]
        public void Bad_CustomerAndHqNull()
        {
            Person? customer = null;
            Address? hq = null;
            string city;
        
            if (customer != null && customer.Profile != null && customer.Profile.Address != null && customer.Profile.Address.City != null)
                city = customer.Profile.Address.City.Name;
            else if (hq != null && hq.City != null)
                city = hq.City.Name;
            else 
                city = "Unknown";
            
            Assert.Equal("Unknown", city);
        }
        
        [Fact]
        public void After_CustomerAndHqNull()
        {
            Person? customer = null;
            Address? hq = null;
            var hqSafe = Safe
                .Guard(hq); // SafeValue<Address>

            // FIRST map customer -> Address, then fallback to hqSafe (both SafeValue<Address>)
            var city = Safe
                .Guard(customer)
                .Map(p => p.Profile)           // SafeValue<Profile>
                .Map(a => a.Address)           // SafeValue<Address>
                .Fallback(hqSafe)              // SafeValue<Address> fallback with hqSafe
                .Map(c => c.City)              // SafeValue<City>
                .Map(n => n.Name)              // SafeValue<string>
                .Default("Unknown")
                .Value();

            Assert.Equal("Unknown", city);
        }

        // ------------------------------
        // Scenario 4: Convert DB status id to friendly string
        // Before: if null -> empty string
        // After: Map<int> then Map<string> and Default("")
        // ------------------------------

        [Fact]
        public void Bad_StatusMapping_Classic()
        {
            Dealer d = new Dealer { Trial = new Trial { StatusId = null } };
            string status;
            
            if (d != null && d.Trial != null && d.Trial.StatusId.HasValue) 
                status = d.Trial.StatusId.Value.ToString(); 
            else 
                status = "";

            Assert.Equal("", status);
        }
        
        [Fact]
        public void After_StatusMapping_Chain()
        {
            Dealer d = new Dealer { Trial = new Trial { StatusId = null } };

            var status = Safe.Guard(d)
                .Map(t => t.Trial)                                  // -> SafeValue<Trial>
                .Map<int>(t => t != null ? t.StatusId : (int?)null) // -> SafeValueStruct<int>
                .Map<string>(i => i.ToString())                     // projector receives int (non-nullable) when present
                .Default("")                                        // fallback when no value present
                .Value();

            Assert.Equal("", status);
        }

        // Edge: status present
        [Fact]
        public void Bad_StatusMapping_Present()
        {
            Dealer d = new Dealer { Trial = new Trial { StatusId = 5 } };
            string status;
            
            if (d != null && d.Trial != null && d.Trial.StatusId.HasValue) 
                status = d.Trial.StatusId.Value.ToString(); 
            else 
                status = "";
            
            Assert.Equal("5", status);
        }

        [Fact]
        public void After_StatusMapping_Present()
        {
            Dealer d = new Dealer { Trial = new Trial { StatusId = 5 } };

            var status = Safe.Guard(d)
                .Map(t => t.Trial)                                  // SafeValue<Trial>
                .Map<int>(t => t != null ? t.StatusId : (int?)null) // SafeValueStruct<int>
                .Map<string>(i => i.ToString())                     // projector receives int (non-nullable)
                .Default("")                                        // fallback when empty
                .Value();

            Assert.Equal("5", status);
        }

        // ------------------------------
        // Scenario 5: Safe collection projection when items may be null
        // Before: for loop with multiple null checks
        // After: SafeCollection.SafeSelect
        // ------------------------------

        [Fact]
        public void Bad_CollectionProjection_Loop()
        {
            var people = new List<Person?> { new Person { Profile = new Profile { Address = new Address { City = new City { Name = "A" } } } }, null, new Person() };
            var names = new List<string>();
        
            foreach (var p in people)
            {
                if (p == null) continue;
                if (p.Profile == null) continue;
                if (p.Profile.Address == null) continue;
                if (p.Profile.Address.City == null) continue;
            
                names.Add(p.Profile.Address.City.Name);
            }
            
            Assert.Equal(new[] { "A" }, names);
        }

        [Fact]
        public void After_CollectionProjection_SafeSelect()
        {
            var people = new List<Person?> { new Person { Profile = new Profile { Address = new Address { City = new City { Name = "A" } } } }, null, new Person() };
        
            var names = people
                .ToSafeCollection()
                .SafeSelect(p => p.Profile.Address.City.Name)
                .ToList();
            
            Assert.Equal(new[] { "A" }, names);
        }

        // Edge: projector may throw for a specific item — should be swallowed
        [Fact]
        public void Bad_CollectionProjection_ProjectorThrows()
        {
            var people = new List<Person?> { new Person { Profile = new Profile { Address = new Address { City = new City { Name = "A" } } } } };
        
            // manual try/catch around loop
            var result = new List<string>();
            foreach (var p in people)
            {
                try 
                { 
                    result.Add(p.Profile.Address.City.Name); 
                } 
                catch 
                { 
                    /* swallow */ 
                }
            }
            Assert.Equal(new[] { "A" }, result);
        }

        [Fact]
        public void After_CollectionProjection_ProjectorSwallowed()
        {
            var people = new List<Person?> { new Person { Profile = new Profile { Address = new Address { City = new City { Name = "A" } } } } };

            var names = people
                .ToSafeCollection()
                .SafeSelect(p => 
                    { 
                        if (p.Profile == null) 
                            throw new Exception("boom"); 
                        return p.Profile.Address.City.Name; 
                    }).ToList();

            Assert.Equal(new[] { "A" }, names);
        }

        // ------------------------------
        // Scenario 6: Repo orchestration sync — get order -> customer -> profile
        // Before: nested null check between repo calls
        // After: DeepNavigator sync Steps
        // ------------------------------

        [Fact]
        public void Bad_RepoOrchestration_Sync()
        {
            // Simulate simple repository methods
            Func<Guid, Order?> repoGetOrder = id => null;
            Order? order = repoGetOrder(Guid.NewGuid());
            Customer? customer = null;
            
            if (order != null) 
                customer = repoGetCustomer(order);
            
            Assert.Null(customer);

            Order? repoGetOrderLocal(Guid id) => null;
            Customer? repoGetCustomer(Order o) => null;
        }

        [Fact]
        public void After_DeepNavigator_Sync()
        {
            // Simulate simple repository methods as lambdas
            Func<Guid, Order?> repoGetOrder = id => null;
            Func<Order, Customer?> repoGetCustomer = o => null;

            // Start from an order instance (null)
            var nav = DeepNavigator<Order>
                .Start(null as Order)
                .Step(o => repoGetCustomer(o))
                .Finish();
            
            Assert.Null(nav);
        }

        // Edge: order exists but customer missing
        [Fact]
        public void Bad_RepoOrchestration_OrderExists_CustomerMissing()
        {
            var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            Func<Guid, Order?> repoGetOrder = id => order;
            Func<Order, Customer?> repoGetCustomer = o => null;

            var o = repoGetOrder(order.Id);
            Customer? c = null;
        
            if (o != null) 
                c = repoGetCustomer(o);
            
            Assert.Null(c);
        }

        [Fact]
        public void After_DeepNavigator_OrderExists_CustomerMissing()
        {
            var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            Func<Guid, Order?> repoGetOrder = id => order;
            Func<Order, Customer?> repoGetCustomer = o => null;

            var start = DeepNavigator<Order>
                .Start(order);
            
            var next = start
                .Step(o => repoGetCustomer(o))
                .Finish();
            
            Assert.Null(next);
        }

        // ------------------------------
        // Scenario 7: Async repo orchestration (get by id)
        // Before: multiple awaits & null checks
        // After: DeepNavigator StepAsync
        // ------------------------------

        [Fact]
        public async Task Bad_RepoOrchestration_Async()
        {
            // fake async repo
            Func<Guid, Task<Order?>> repoGetOrderAsync = async id => { await Task.Delay(1); return null; };

            var order = await repoGetOrderAsync(Guid.NewGuid());

            Assert.Null(order);
        }

        [Fact]
        public async Task After_DeepNavigator_Async()
        {
            Func<Guid, Task<Order?>> repoGetOrderAsync = async id => { await Task.Delay(1); return null; };
            Func<Order, Task<Customer?>> repoGetCustomerAsync = async o => { await Task.Delay(1); return null; };

            // Cannot start from id directly in our simple DeepNavigator; show StepAsync usage on an instance
            var start = DeepNavigator<Order>
                .Start(null as Order);

            var step = await start
                .StepAsync(async o => await repoGetCustomerAsync(o));
            
            Assert.Null(step.Finish());
        }

        // Edge: order exists and chain returns customer
        [Fact]
        public async Task Bad_RepoOrchestration_Async_OrderExists()
        {
            var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
        
            Func<Guid, Task<Order?>> repoGetOrderAsync = async id => { await Task.Delay(1); return order; };
            
            var fetched = await repoGetOrderAsync(order.Id);
            
            Assert.NotNull(fetched);
        }

        [Fact]
        public async Task After_DeepNavigator_Async_OrderExists()
        {
            var order = new Order { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            Func<Guid, Task<Order?>> repoGetOrderAsync = async id => { await Task.Delay(1); return order; };
            Func<Order, Task<Customer?>> repoGetCustomerAsync = async o => { await Task.Delay(1); return new Customer { Id = o.CustomerId }; };

            var fetched = await repoGetOrderAsync(order.Id);
        
            if (fetched == null) 
                Assert.True(false);
            
            var nav = DeepNavigator<Order>
                .Start(fetched);
            
            var next = await nav
                .StepAsync(async o => await repoGetCustomerAsync(o));
            
            Assert.NotNull(next.Finish());
        }

        // ------------------------------
        // Scenario 8: Convert incoming string to enum safely
        // Before: Enum.TryParse plus fallback
        // After: EnumSafe.Parse and EnumSafe.Convert usage
        // ------------------------------

        [Fact]
        public void Bad_EnumParsing_Classic()
        {
            string raw = "BadStatus";
            OrderStatus s;

            if (!Enum.TryParse<OrderStatus>(raw, true, out s)) 
                s = OrderStatus.Unknown;

            Assert.Equal(OrderStatus.Unknown, s);
        }

        [Fact]
        public void After_EnumSafe_Parse()
        {
            var parsed = EnumSafe.Parse<OrderStatus>("BadStatus");
        
            Assert.False(parsed.TryGet(out var value));
        }

        // Edge: numeric string
        [Fact]
        public void Bad_EnumFromNumber_Classic()
        {
            object raw = 2;
        
            bool ok = Enum.IsDefined(typeof(OrderStatus), raw) && (OrderStatus)raw == OrderStatus.Shipped;
            
            Assert.False(ok); // wrong style
        }

        [Fact]
        public void After_EnumFromNumber()
        {
            object raw = 2;
        
            var parsed = EnumSafe.Convert<OrderStatus>(raw);
            var got = parsed.Value(); // Value() will throw if not present; Convert should return present for 2
            
            Assert.Equal(OrderStatus.Shipped, got);
        }

        // ------------------------------
        // Scenario 9: Email fallback & normalization (trim/lower)
        // Before: manual string checks
        // After: Safe.Guard + Map + Default
        // ------------------------------

        [Fact]
        public void Bad_EmailNormalize_Classic()
        {
            Person? p = null;
            string email;
            
            if (p != null && !string.IsNullOrWhiteSpace(p.Email)) 
                email = p.Email.Trim().ToLowerInvariant(); 
            else 
                email = "unknown@example.com";
            
            Assert.Equal("unknown@example.com", email);
        }

        [Fact]
        public void After_EmailNormalize_Chain()
        {
            Person? p = null;
        
            var email = Safe.Guard(p)
                .Map(x => x.Email)
                .Map(s => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant())
                .Default("unknown@example.com")
                .Value();
            Assert.Equal("unknown@example.com", email);
        }

        // Edge: email present but with spaces
        [Fact]
        public void Bad_EmailPresent_Classic()
        {
            Person p = new Person { Email = " USER@EX.COM " };
        
            string email = !string.IsNullOrWhiteSpace(p.Email) ? p.Email.Trim().ToLowerInvariant() : "unknown@example.com";
            
            Assert.Equal("user@ex.com", email);
        }

        [Fact]
        public void After_EmailPresent_Chain()
        {
            var p = new Person { Email = " USER@EX.COM " };
        
            var email = Safe
                .Guard(p)
                .Map(x => x.Email)
                .Map(s => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant())
                .Default("unknown@example.com")
                .Value();
            
            Assert.Equal("user@ex.com", email);
        }

        // ------------------------------
        // Scenario 10: Aggregate profile score from DB calls
        // Before: multiple repository calls and null checks
        // After: SafeAsync and chaining projects into safe result
        // ------------------------------

        [Fact]
        public async Task Bad_AggregateScore()
        {
            Func<Guid, Task<Profile?>> repoGetProfile = async id => { await Task.Delay(1); return null; };
            
            var profile = await repoGetProfile(Guid.NewGuid());
            int total = profile != null && profile.Score.HasValue ? profile.Score.Value : 0;
            
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task After_AggregateScore_SafeAsync()
        {
            Func<Guid, Task<Profile?>> repoGetProfile = async id => { await Task.Delay(1); return null; };
        
            var safe = await Safe
                .GuardAsync(repoGetProfile(Guid.NewGuid())).MapAsync(p => p.Score.HasValue ? p.Score.Value.ToString() : null);
            
            var val = safe.Value();
            
            Assert.Null(val);
        }

        // Edge: profile exists with score
        [Fact]
        public async Task Bad_AggregateScore_ProfilePresent()
        {
            Func<Guid, Task<Profile?>> repoGetProfile = async id => { await Task.Delay(1); return new Profile { Score = 7 }; };
        
            var profile = await repoGetProfile(Guid.NewGuid());
            int total = profile != null && profile.Score.HasValue ? profile.Score.Value : 0;
            
            Assert.Equal(7, total);
        }

        [Fact]
        public async Task After_AggregateScore_ProfilePresent()
        {
            Func<Guid, Task<Profile?>> repoGetProfile = async id => { await Task.Delay(1); return new Profile { Score = 7 }; };
        
            var safe = await Safe
                .GuardAsync(repoGetProfile(Guid.NewGuid()))
                .MapAsync(p => p.Score.HasValue ? p.Score.Value.ToString() : null);
            
            var val = safe.Value();
            
            Assert.Equal("7", val);
        }

        // ------------------------------
        // Scenario 11: DeepPath with method call (trim or transform within expression)
        // Before: manual extraction then transform
        // After: Safe.Path with expression that contains method call (basic support)
        // ------------------------------

        [Fact]
        public void Bad_DeepPathMethod_Classic()
        {
            Person? p = null;
            string? name = null;
            
            if (p != null && p.Profile != null && p.Profile.Address != null && p.Profile.Address.City != null)
                name = p.Profile.Address.City.Name?.Trim();
            
            Assert.Null(name);
        }

        [Fact]
        public void After_DeepPathMethod_Expr()
        {
            Person? p = null;
            
            var v = Safe
                .Path(p, (Person x) => x.Profile.Address.City.Name);
            
            Assert.Null(v);
        }

        // Edge: city exists with whitespace
        [Fact]
        public void Bad_DeepPathMethod_Present()
        {
            var p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = " ABC " } } } };

            var name = p.Profile.Address.City.Name?.Trim();

            Assert.Equal("ABC", name);
        }

        [Fact]
        public void After_DeepPathMethod_Present()
        {
            var p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = " ABC " } } } };

            var v = Safe
                .Path(p, (Person x) => x.Profile.Address.City.Name) as string;
            
            var trimmed = v?.Trim();

            Assert.Equal("ABC", trimmed);
        }

        // ------------------------------
        // Scenario 12: Aggregating list of orders -> first shipped order
        // Before: nested checks + LINQ risk
        // After: SafeCollection.SafeFirstOrDefault
        // ------------------------------

        [Fact]
        public void Bad_FirstShipped_Classic()
        {
            var orders = new List<Order?> { new Order { Status = OrderStatus.Pending }, new Order { Status = OrderStatus.Shipped } };
            Order? found = null;
            
            foreach (var o in orders)
            {
                if (o == null) continue;
                if (o.Status == OrderStatus.Shipped) { found = o; break; }
            }
            
            Assert.NotNull(found);
        }

        [Fact]
        public void After_FirstShipped_SafeCollection()
        {
            var orders = new List<Order?> { new Order { Status = OrderStatus.Pending }, new Order { Status = OrderStatus.Shipped } };
        
            var found = orders
                .ToSafeCollection()
                .SafeFirstOrDefault(o => o.Status == OrderStatus.Shipped ? o.Id.ToString() : null);
            
            Assert.NotNull(found);
        }

        // Edge: empty list
        [Fact]
        public void Bad_FirstShipped_Empty()
        {
            var orders = new List<Order?>();
            Order? found = null;
        
            foreach (var o in orders)
            {
                if (o == null) continue;
                if (o.Status == OrderStatus.Shipped) { found = o; break; }
            }
            
            Assert.Null(found);
        }

        [Fact]
        public void After_FirstShipped_Empty()
        {
            var orders = new List<Order?>();
        
            var found = orders
                .ToSafeCollection()
                .SafeFirstOrDefault(o => o.Status == OrderStatus.Shipped ? o.Id.ToString() : null);
            
            Assert.Null(found);
        }

        // ------------------------------
        // Scenario 13: Validate member group string format
        // Before: manual splitting + argument exception
        // After: Safe.Path + safer validation or return default
        // ------------------------------

        [Fact]
        public void Bad_ValidateMemberGroup_Classic()
        {
            List<string>? groups = new List<string> { "X-Name", "BadGroup" };
            
            foreach (var g in groups)
            {
                var parts = g.Trim().ToUpper().Split(new[] { '-' }, 2);
                if (parts.Length != 2) 
                { 
                    Assert.True(true); 
                    return; 
                } // indicates failure
            }

            Assert.False(false);
        }

        [Fact]
        public void After_ValidateMemberGroup_SafeWay()
        {
            List<string>? groups = new List<string> { "X-Name", "A-B" };
        
            var good = groups.Select(g =>
            {
                var parts = g?.Trim().ToUpper().Split(new[] { '-' }, 2);
            
                return parts != null && parts.Length == 2 ? parts[0] : null;
            }).Where(x => x != null).ToList();
            
            Assert.Equal(new[] { "X", "A" }, good);
        }

        // Edge: null list
        [Fact]
        public void Bad_ValidateMemberGroup_NullList()
        {
            List<string>? groups = null;
            var ok = false;
        
            if (groups != null) 
                foreach (var g in groups) 
                { 
                    ok = true; 
                }
            
            Assert.False(ok);
        }

        [Fact]
        public void After_ValidateMemberGroup_NullList()
        {
            List<string>? groups = null;
        
            var safe = Safe
                .Guard(groups)
                .Value();
            
            Assert.Null(safe);
        }

        // ------------------------------
        // Scenario 14: Convert multiple optional properties into DTO
        // Before: repeated assignments with null checks
        // After: Safe pipeline mapping into DTO
        // ------------------------------

        [Fact]
        public void Bad_MapToDto_Classic()
        {
            Person p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = "C" }, Zip = "123" }, Birth = new DateTime(1990, 1, 1) } };
            
            var dtoCity = p != null && p.Profile != null && p.Profile.Address != null && p.Profile.Address.City != null ? p.Profile.Address.City.Name : null;
            var dtoZip = p != null && p.Profile != null && p.Profile.Address != null ? p.Profile.Address.Zip : null;
            
            Assert.Equal("C", dtoCity);
            Assert.Equal("123", dtoZip);
        }

        [Fact]
        public void After_MapToDto_Pipeline()
        {
            Person p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = "C" }, Zip = "123" }, Birth = new DateTime(1990, 1, 1) } };
            
            var city = Safe
                .Guard(p)
                .Map(x => x.Profile)
                .Map(a => a.Address)
                .Map(c => c.City)
                .Map(n => n.Name).Value();
            
            var zip = Safe
                .Guard(p)
                .Map(x => x.Profile)
                .Map(a => a.Address)
                .Map(z => z.Zip)
                .Value();
            
            Assert.Equal("C", city);
            Assert.Equal("123", zip);
        }

        // Edge: missing zip
        [Fact]
        public void Bad_MapToDto_MissingZip()
        {
            Person p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = "C" } } } };
        
            var zip = p.Profile != null && p.Profile.Address != null ? p.Profile.Address.Zip : null;
            
            Assert.Null(zip);
        }

        [Fact]
        public void After_MapToDto_MissingZip()
        {
            Person p = new Person { Profile = new Profile { Address = new Address { City = new City { Name = "C" } } } };
        
            var zip = Safe
                .Guard(p)
                .Map(x => x.Profile)
                .Map(a => a.Address)
                .Map(z => z.Zip)
                .Value();
            
            Assert.Null(zip);
        }

        // ------------------------------
        // Scenario 15: Handling optional configuration flags (bool?)
        // Before: HasValue + Value style
        // After: Safe.GuardStruct + Default
        // ------------------------------

        [Fact]
        public void Bad_ConfigFlag_Classic()
        {
            bool? feature = null;
            
            bool enabled = feature.HasValue ? feature.Value : false;
            
            Assert.False(enabled);
        }

        [Fact]
        public void After_ConfigFlag_SafeStruct()
        {
            bool? feature = null;
        
            var enabled = Safe
                .GuardStruct(feature)
                .Default(false)
                .Value();
            
            Assert.False(enabled);
        }

        // Edge: feature true
        [Fact]
        public void Bad_ConfigFlag_True()
        {
            bool? feature = true;
        
            bool enabled = feature.HasValue ? feature.Value : false;
            
            Assert.True(enabled);
        }

        [Fact]
        public void After_ConfigFlag_True()
        {
            bool? feature = true;
        
            var enabled = Safe
                .GuardStruct(feature)
                .Default(false)
                .Value();
            
            Assert.True(enabled);
        }

        // ------------------------------
        // Scenario 16: Multi-tenant locate favorites (list fallback)
        // Before: mutate request object and if-else
        // After: functional mapping with fallback
        // ------------------------------

        [Fact]
        public void Bad_MultiTenantLocate_Mutate()
        {
            var includeFavorites = true;
            var req = new FakeAvailabilityRequest();
            
            if (includeFavorites) 
                req.PreferredOrgs = new List<string> { "A" };
            
            Assert.Single(req.PreferredOrgs);
        }

        [Fact]
        public void After_MultiTenantLocate_Fallback()
        {
            var includeFavorites = true;
            var preferred = includeFavorites ? new List<string> { "A" } : null;
        
            var reqPreferred = Safe
                .Guard(preferred)
                .Default(new List<string>())
                .Value();
            
            Assert.Single(reqPreferred);
        }

        [Fact]
        public void Bad_MultiTenantLocate_NoFavorites()
        {
            var includeFavorites = false;
            var req = new FakeAvailabilityRequest();
        
            if (includeFavorites) 
                req.PreferredOrgs = new List<string> { "A" };
            
            Assert.Empty(req.PreferredOrgs);
        }

        [Fact]
        public void After_MultiTenantLocate_NoFavorites()
        {
            var includeFavorites = false;
            var preferred = includeFavorites ? new List<string> { "A" } : null;
        
            var reqPreferred = Safe
                .Guard(preferred)
                .Default(new List<string>())
                .Value();
            
            Assert.Empty(reqPreferred);
        }

        // ------------------------------
        // Scenario 17: Determine nearest preferred org distance conversion based on culture
        // Before: if-culture and inline conversion
        // After: Safe chain with LocateHelper stub
        // ------------------------------
        
        
        [Fact]
        public void Bad_DistanceConversion_Classic()
        {
            var culture = "IN";
            double distance = 100;
            double km = culture == "US" ? distance : distance; // placeholder
            
            Assert.Equal(100, km);
        }

        [Fact]
        public void After_DistanceConversion_Safe()
        {
            var culture = "IN";
            double? distance = 100;

            var km = Safe
                .GuardStruct(distance)
                .Default(0)
                .Value();
            
            Assert.Equal(100, km);
        }

        [Fact]
        public void Bad_Distance_NoValue()
        {
            double? distance = null;
        
            double km = distance.HasValue ? distance.Value : 0;
            
            Assert.Equal(0, km);
        }
        
        [Fact]
        public void After_Distance_NoValue()
        {
            double? distance = null;
            
            var km = Safe
                .GuardStruct(distance)
                .Default(0)
                .Value();
            
            Assert.Equal(0, km);
        }

        // Basic IsNullOrWhiteSpace replacement
        
        [Fact]
        public void Bad_IsNullOrWhiteSpace_Classic()
        {
            string? name = "   ";
            string res;
          
            if (string.IsNullOrWhiteSpace(name))
                res = "UNKNOWN";
            else
                res = name.Trim();
            
            Assert.Equal("UNKNOWN", res);
        }

        [Fact]
        public void After_IsNullOrWhiteSpace_SafeText()
        {
            string? name = "   ";
            
            var res = SafeText
                .From(name)
                .Trim()
                .OrIfWhitespace("UNKNOWN");
            
            Assert.Equal("UNKNOWN", res);
        }

        // Trim + uppercase
        [Fact]
        public void Bad_TrimAndUpper_Classic()
        {
            string? name = " bob ";
            string res;
          
            if (string.IsNullOrEmpty(name))
                res = "N/A";
            else
                res = name.Trim().ToUpperInvariant();
            
            Assert.Equal("BOB", res);
        }

        [Fact]
        public void After_TrimAndUpper_SafeText()
        {
            string? name = " bob ";
            
            var res = SafeText
                .From(name)
                .Trim()
                .ToUpper()
                .OrDefault("N/A");
            
            Assert.Equal("BOB", res);
        }

        // Replace multiple invalid tokens
        [Fact]
        public void Bad_InvalidTokens_Classic()
        {
            string? code = "---";
            string outv = (code == null || code == "" || code == " " || code == "---") ? "INVALID" : code.Trim();
          
            Assert.Equal("INVALID", outv);
        }

        [Fact]
        public void After_InvalidTokens_SafeText()
        {
            string? code = "---";
            
            var outv = SafeText
                .From(code)
                .Trim()
                .OrIfInvalid(new[] { "", " ", "---" }, "INVALID");
            
            Assert.Equal("INVALID", outv);
        }

        // Normalize and collapse spaces
        [Fact]
        public void Bad_Normalize_Classic()
        {
            string? s = " A   B ";
            var got = (s == null) ? null : s.Trim().Replace("  ", " ");
          
            Assert.Equal("A B", got);
        }

        [Fact]
        public void After_Normalize_SafeText()
        {
            string? s = " A   B ";
            
            var got = SafeText
                .From(s)
                .Normalize()
                .Value();
            
            Assert.Equal("A B", got);
        }

        // ContainsIgnoreCase
        [Fact]
        public void Bad_ContainsLowerManual()
        {
            string hay = "Hello Friend";
            string needle = "friend";
          
            var found = hay != null && hay.ToLowerInvariant().Contains(needle.ToLowerInvariant());
            Assert.True(found);
        }

        [Fact]
        public void After_ContainsIgnoreCase()
        {
            var hay = "Hello Friend";
            
            var ok = SafeText
                .From(hay)
                .ContainsIgnoreCase("friend");

            Assert.True(ok);
        }

        // Map with custom projector that may throw (should swallow)
        [Fact]
        public void Bad_ProjectorThrows_Manual()
        {
            string? s = "abc";
            string? outv;
          
            try 
            { 
                outv = ((Func<string>)(() => throw new Exception("boom")))(); 
            } 
            catch 
            { 
                outv = null; 
            }
            
            Assert.Null(outv);
        }

        [Fact]
        public void After_ProjectorThrows_Swallowed()
        {
            var s = SafeText.From("abc");
        
            var got = s
                .Map(x => throw new Exception("boom")).Value();
            
            Assert.Null(got);
        }

        // ToLower + fallback
        [Fact]
        public void Bad_ToLower_Fallback()
        {
            string? email = null;
          
            string result = !string.IsNullOrWhiteSpace(email) ? email.Trim().ToLowerInvariant() : "unknown@example.com";
            
            Assert.Equal("unknown@example.com", result);
        }

        [Fact]
        public void After_ToLower_Fallback()
        {
            string? email = null;
        
            var result = SafeText
                .From(email)
                .Trim()
                .ToLower()
                .OrDefault("unknown@example.com");
            
            Assert.Equal("unknown@example.com", result);
        }

        // RemoveSpaces for codes
        [Fact]
        public void Bad_RemoveSpaces_Classic()
        {
            string? c = "A B C";
            string cleaned = c == null ? null : c.Replace(" ", "");
          
            Assert.Equal("ABC", cleaned);
        }

        [Fact]
        public void After_RemoveSpaces_SafeText()
        {
            string? c = "A B C";
        
            var cleaned = SafeText
                .From(c)
                .RemoveSpaces()
                .Value();
            
            Assert.Equal("ABC", cleaned);
        }

        // OrIfNotMatch predicate
        [Fact]
        public void Bad_PredicateManual()
        {
            string? p = "abc";
            string outv = (p != null && p.Length >= 3) ? p : "SHORT";
          
            Assert.Equal("abc", outv);
        }

        [Fact]
        public void After_PredicateSafe()
        {
            string? p = "abc";
        
            var outv = SafeText
                .From(p)
                .OrIfNotMatch(s => s.Length >= 3, "SHORT");
            
            Assert.Equal("abc", outv);
        }

        // Null source fallback via ToSafeText from Safe.Guard
        [Fact]
        public void After_IntegrateWithSafeGuard()
        {
            Person? p = null;
            var email = Safe
                .Guard(p)
                .ToSafeText()
                .Trim()
                .OrDefault("unknown@example.com");
          
            Assert.Equal("unknown@example.com", email);
        }

        // Regex replace safely
        [Fact]
        public void After_ReplaceRegex_Safe()
        {
            var s = "abc-123";
            var res = SafeText
                .From(s)
                .ReplaceRegex(@"-\d+", "")
                .Value();
          
            Assert.Equal("abc", res);
        }

        // Collapse spaces (multiple spaces)
        [Fact]
        public void After_CollapseSpaces()
        {
            var s = "X   Y     Z";
            var got = SafeText
                .From(s)
                .CollapseSpaces()
                .Value();
            
            Assert.Equal("X Y Z", got);
        }

        // Map then OrIfWhitespace
        [Fact]
        public void After_MapThenOrIfWhitespace()
        {
            var s = "  ";
            var got = SafeText
                .From(s)
                .Trim()
                .Map(x => x.Length > 0 ? x + "!" : null)
                .OrIfWhitespace("EMPTY");
          
            Assert.Equal("EMPTY", got);
        }
    }
}