﻿using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }

        public List<CustomerPreference> CustomerPreferences { get; set; } = new();
        public List<PromoCode> PromoCodes { get; set; } = new();
    }
}