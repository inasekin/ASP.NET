using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pcf.GivingToCustomer.WebHost.GraphQL.Inputs
{
    /// <summary>
    /// Input тип для создания клиента
    /// </summary>
    public record CreateCustomerInput
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        public List<Guid> PreferenceIds { get; init; } = new();
    }

    /// <summary>
    /// Input тип для обновления клиента
    /// </summary>
    public record UpdateCustomerInput
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        public List<Guid> PreferenceIds { get; init; } = new();
    }

    /// <summary>
    /// Input тип для удаления клиента
    /// </summary>
    public record DeleteCustomerInput
    {
        [Required]
        public Guid Id { get; init; }
    }
} 